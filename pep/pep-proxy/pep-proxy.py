from werkzeug.wrappers       import Request, Response
from werkzeug.routing        import Map, Rule
from pdp.token_pdp           import TokenPDP
from pdp.vp_pdp              import VPPDP
from pdp.openfga_pdp         import OpenFGA_PDP
from proxy.http_proxy        import HTTPProxy
from urllib.parse import urlencode
import os
import json
import sys
import secrets



class Handler():
    def __init__(self):
        conf_file = os.getenv('PEP_CONF_FILE', 'conf/proxy.conf')
        with open(conf_file) as f:
            try:
                self.conf = json.load(f)
            except json.decoder.JSONDecodeError as error:
                print(error)
                sys.exit("Cannot parse the configuration file")
        self.url_map = Map()
        for key in self.conf['resources']:
            if key == "default": continue
            self.url_map.add(rulefactory=Rule(key, endpoint=key))
        self.vp_pdp = VPPDP(True)
        self.token_pdp = TokenPDP()
        self.http_proxy = HTTPProxy()
        self.openFGAPDP = OpenFGA_PDP(debug=True)

    def wsgi_app(self, environ, start_response):
        req      = Request(environ)
        code     = 401
        resource = {}
        output   = ''
        output_header = {}
        headers = {}
        auth    = req.headers.get('Authorization')
        is_client_authorized = False
        ver_output = "Invalid or missing input parameters"
        # Check if the requested path is included in the configuration file
        # If it is not, check if there is default rule
        try:
            endpoint, _ = self.url_map.bind_to_environ(environ).match()
            resource = self.conf['resources'][endpoint]
        except Exception as e:
            print("not found")
            if ('default' in self.conf['resources']):
                resource = self.conf['resources']["default"]
        if ('authorization' in resource):
            auth_type = ""
            auth_grant = ""
            if (auth):
                auth_type, auth_grant = auth.split(" ",1)
            
            #*********Token***********
            if ((resource['authorization']['type'] == "token") ):
                if (not auth_type): # the user has not provided any token
                    token_endpoint = resource['authorization']['token_endpoint']
                    if (token_endpoint['type'] == "vc_verifier"): # send authorization request 
                        # https://openid.net/specs/openid-4-verifiable-presentations-1_0.html
                        query_parameter = {
                            'scope' : 'excid-io.github.io.imperial',
                            'nonce' : secrets.token_urlsafe(16),
                            'response_type': 'vp_token',
                            'response_mode' : 'direct_post',
                            'response_uri': 'http://localhost:9000/authorize',
                            'state': secrets.token_urlsafe(16),
                            'client_metadata': json.dumps({
                                "client_name":"Demo verifier"
                            })
                        }
                        self.token_pdp.append_authorization_table(query_parameter['state'], {})
                        code = 301
                        output_header['Location'] = "http://localhost:8002/Credentials/Authorize?" + urlencode(query_parameter)
                elif(auth_type == "Bearer"):
                    object = req.path.split('/')[-1]
                    is_client_authorized, ver_output = self.token_pdp.get_info(auth_grant)
                    if is_client_authorized:
                        is_client_authorized, ver_output= self.openFGAPDP.check(ver_output['user'], "access","resource:"+object, ver_output['relations'])
                       
                    #is_client_authorized, ver_output = self.token_pdp.decide(auth_grant, object)

            #*********VP***********
            if ((resource['authorization']['type'] == "vp") ):
                is_client_authorized, ver_output = self.vp_pdp.decide(request=req)
                if(is_client_authorized):
                    state = ver_output["state"]
                    vcs = ver_output["vcs"]
                    print("authorized for state",state, "with vcs:", vcs) 

            #*********IMPERIAL***********
            if ((resource['authorization']['type'] == "imperial") ):
                is_client_authorized, ver_output = self.vp_pdp.decide(request=req)
                if(is_client_authorized):
                    #token_payload = json.loads(decoded_token.objects['payload'].decode())
                    #relations = token_payload["vc"]["credentialSubject"]["relationships"]
                    #user = token_payload["sub"]
                    state = ver_output["state"]
                    vcs = ver_output["vcs"]
                    if state not in self.token_pdp.authorization_table:
                        self.token_pdp.append_authorization_table(state, {})
                    self.token_pdp.authorization_table[state]['relations']=[]
                    firstVC = True
                    for vc in vcs:
                        user = vc['vc']['credentialSubject']['id'].split(":")[-1]
                        relationships = vc['vc']['credentialSubject']['relationships']
                        relationship = []
                        for relation in relationships:
                            key = list(relation.keys())[0]
                            value = relation[key]
                            if (value=="authorized"):
                                company=key.split(":")[-1]
                                relationship.append("employee:"+user)
                                relationship.append(value)
                                relationship.append("company:"+company)
                                self.token_pdp.authorization_table[state]['relations'].append(relationship)
                                if firstVC:
                                     # The first VC includes the user
                                    self.token_pdp.authorization_table[state]['user']= "employee:"+user
                            if (value=="access"):
                                relationship.append("company:"+user+"#authorized")
                                relationship.append(value)
                                relationship.append("resource:"+key)
                                self.token_pdp.authorization_table[state]['relations'].append(relationship)
                                if firstVC:
                                     # The first VC includes the user
                                    self.token_pdp.authorization_table[state]['user']= "company:"+user
                            firstVC= False
                    #self.token_pdp.authorization_table[ver_output["state"]]={"user":ver_output["user"], "relations":ver_output["relations"]}
                    print(self.token_pdp.authorization_table)
                    print("authorized for state",state, "with vcs:", vcs) 

            elif('authorization' not in resource):
                is_client_authorized = True
            
        if (is_client_authorized):
            if ('proxy' in  resource):
                code, output, headers = self.http_proxy.forward(environ, resource['proxy']['proxy_pass'], resource['proxy'].get('header_rewrite'))
            else:
                code = "200"
                output = "Authorized request!"
        else:
            output = ver_output
        if not 'Content-Type' in headers:
            headers['Content-Type'] = "text/html"
        response = Response(output.encode(), status=code, mimetype=headers['Content-Type'])
        if output_header:
            for key,value in output_header.items():
                response.headers.add(key, value)
        return response(environ, start_response)
        
    def __call__(self, environ, start_response):
        return self.wsgi_app(environ, start_response)


def main(): 
    app = Handler()
    '''
    Using werkzeug simple server. For production consider Waitress, or mod_wsgi
    '''
    from werkzeug.serving import run_simple
    address = os.getenv('PEP_ADDRESS', 'localhost')
    port = int(os.getenv('PEP_PORT', 9000))
    run_simple(address, port, app)

if __name__ == '__main__':
    main()

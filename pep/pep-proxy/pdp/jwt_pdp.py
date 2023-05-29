import json
import time
import re
from jwcrypto import jwt, jwk, jws
from jsonpath_ng import jsonpath
from jsonpath_ng.ext import parse
from jwcrypto.common import base64url_decode


class JWTPDP:
    def decide(self, token, configuration): 
        try:
            filter = None
            trusted_issuers  = configuration['trusted_issuers']
            if ('filters' in configuration):
                filter = configuration['filters']
            decoded_token = jwt.JWS()
            decoded_token.deserialize(token)
            token_payload = json.loads(decoded_token.objects['payload'].decode())
            # check token validity times
            now = int(time.time())
            if 'nbf' in token_payload:
                if now < token_payload['nbf']:
                    return False, "Error: JWT is not valid yet"
            if 'exp' in token_payload:
                if now > token_payload['exp']:
                    return False, "Error: JWT has expired"
            # Read the iss claim
            iss = token_payload['iss']
            # Check if iss is trusted
            if (iss not in trusted_issuers): # Not trusted issuer
                return False, "Issuer is not trusted"
            # Retrieve the issuer key    
            ver_key = None
            issuer_key_type = trusted_issuers[iss]['issuer_key_type']
            issuer_key = trusted_issuers[iss]['issuer_key']
            if (issuer_key_type == "pem_file"):
                with open(issuer_key, mode='rb') as file: 
                    pem_file = file.read()
                ver_key = jwk.JWK.from_pem(pem_file)
            if (issuer_key_type == "jwt"):
                ver_key = jwk.JWK.from_json(json.dumps(issuer_key))
            # Verify the JWS
            decoded_token.verify(ver_key)
            # Check for filters
            if(filter):
                if(not self._filter(json.loads(decoded_token.payload.decode()), filter)):
                    return False, "Filter verification failed" #Filter failed
            token_payload = decoded_token.payload.decode()

            return True, token_payload
        except Exception as e:
            return False, str(e) #Token cannot be decoded

    def _filter(self, json_obj, filters): 
        for filter in filters:
            jsonpath_expr = parse(filter[0])
            found = False
            for match in jsonpath_expr.find(json_obj):
                if len(filter) == 2 and isinstance(filter[1], list):
                    if match.value in filter[1]:
                        found = True
                elif len(filter) == 2:
                    if match.value == filter[1]:
                        found = True
                else: #no value is required
                    found = True
            if not found:
                return False 
        return True
from werkzeug.wrappers import Request, Response
from werkzeug.datastructures import Headers
import requests

class HTTPProxy:
    def forward(self, environ, target, header_rewrite = None):
        path    = environ.get('PATH_INFO')
        req     = Request(environ)
        query   = req.query_string.decode()
        accept  = req.headers.get('Accept')
        content = req.headers.get('Content-Type')
        headers = {}
        if(accept):
            headers['Accept'] = accept
        if(content):
            headers['Content-Type'] = content
        if(header_rewrite):
            headers.update(header_rewrite)
        if (req.method == "GET"):
            response  = requests.get(target + path +"?" + query, headers = headers)
        elif (req.method == "PUT"): 
            put_data = req.data
            response  = requests.put(target + path, headers = headers, data = put_data.decode())
        elif (req.method == "POST"): 
            post_data = req.data
            response  = requests.post(target + path, headers = headers, data = post_data.decode())
        code = response.status_code
        output = response.text
        headers = response.headers
        return code, output, headers
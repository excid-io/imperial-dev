import os
import http.server
import socketserver
from http import HTTPStatus


class Handler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        self.send_response(HTTPStatus.OK)
        self.end_headers()
        self.wfile.write(b'GET request received')

    def do_PUT(self):
        self.send_response(HTTPStatus.OK)
        self.end_headers()
        self.wfile.write(b'PUT request received')

    def do_POST(self):
        self.send_response(HTTPStatus.OK)
        self.end_headers()
        self.wfile.write(b'POST request received')

address = os.getenv('HTTP_SERVER_ADDRESS', 'localhost')
port = int(os.getenv('HTTP_SERVER_PORT', 8080))
print("\n * Protected resource on http://" + address + ":" + str(port) + "/")
httpd = socketserver.TCPServer((address, port), Handler)
httpd.serve_forever()

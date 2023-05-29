import time
import json
from jwcrypto.common import base64url_encode
from pdp.openfga_pdp           import OpenFGA_PDP

class TokenPDP:

    def __init__(self):
        self.authorization_table = {}
        self.openfga_pdp = OpenFGA_PDP()
    '''
    Section 6.1 of https://datatracker.ietf.org/doc/html/draft-ietf-oauth-dpop
    '''
    def generate_key_bound_token(self, jkt):
        current_time = int(time.time())
        jwt_claims = {
            "iat": current_time,
            "exp": current_time,
            "jkt": jkt
        }
        return base64url_encode(json.dumps(jwt_claims))

    def append_authorization_table(self, jti, token):
        self.authorization_table[jti] = token
        print(self.authorization_table)

    def decide(self, token_id, resource):
        relations = self.authorization_table[token_id]
        print("Will check if ", relations["user"], " has read relationship with ", resource, " based on ", relations["relations"])
        result = self.openfga_pdp.check(relations["user"], resource, relations["relations"] )
        if result.allowed:
            return True,""
        return False, "Relationship not found"

if __name__ == '__main__':
    tokenPDP = TokenPDP()
    print(tokenPDP.generate_key_bound_token("hello world"))
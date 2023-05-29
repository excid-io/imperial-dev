import json 
from jwcrypto import jwt

class VPPDP:
    def decide(self, configuration, request): 
        try:
            vp_token = request.form.get('vp_token', "")
            state = request.form.get('state', "")
            decoded_token = jwt.JWS()
            decoded_token.deserialize(vp_token)
            token_payload = json.loads(decoded_token.objects['payload'].decode())
            relations = token_payload["vc"]["credentialSubject"]["relationships"]
            user = token_payload["sub"]

            return True, {"state":state, "user":user, "relations":relations}
        except Exception as e:
            return False, str(e) #Token cannot be decoded

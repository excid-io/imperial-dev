
import pytest
import requests
import json 
from issuer import Issuer
from jwcrypto import jwt, jwk

class TestJWT:
    def test_valid_vp(self):
        key_dict = { "crv": "P-256", "d": "yObukdNY2tUF1dy_Qb4GjTifxhMGm7VFL4MrGECmhX8",  "kty": "EC", "x": "uSbAriX2Bi34gWR1mxchmARMt85mhmfUzRSPdhW4Zs0", "y": "1rhtzeLXBh93FNM_qHn6wqUjmGSod-VCd1Hhxw0nAk0"}
        key = jwk.JWK.from_json(json.dumps(key_dict))
        myDID = "did:self:Yq8km4WxWxHjoKHXRdCUcHHKYfz80ROcrSIs4oUDbAM"

        vc = Issuer().issue_valid_vc(myDID)
        jwt_header = {
            "typ": "jwt",
            "alg": "ES256",
        }
        vp = {
            "vp": {
            "@context": ["https://www.w3.org/2018/credentials/v1"],
            "type": ["VerifiablePresentation"],
            "verifiableCredential": [vc]
            }
        }
        vp_jwt = jwt.JWT(header=jwt_header, claims=vp)
        vp_jwt.make_signed_token(key)
        headers = {}
        data={"state":"teststate", "vp_token":vp_jwt.serialize()}
        response  = requests.post("http://localhost:9000/authorize", headers = headers, data=data)
        print(response.text)
        assert(response.status_code == 200)
    '''
    def test_valid_authorization_post(self):
        token = Issuer().issue_valid_vc()
        headers = {'Authorization':'Bearer ' + token, 'Accept': 'application/json', 'Content-Type': 'application/json'}
        data = {'on': False}
        response  = requests.post("http://localhost:9000/secure/jwt", headers = headers, data = json.dumps(data))
        print(response.text)
        assert(response.status_code == 200)

    def test_valid_authorization_get_with_exp(self):
        token = Issuer().issue_valid_vc_with_exp()
        headers = {'Authorization':'Bearer ' + token, 'Accept': 'application/json'}
        response  = requests.get("http://localhost:9000/secure/jwt", headers = headers)
        print(response.text)
        assert(response.status_code == 200)

    def test_expired(self):
        token = Issuer().issue_expired()
        headers = {'Authorization':'Bearer ' + token, 'Accept': 'application/json'}
        response  = requests.get("http://localhost:9000/secure/jwt", headers = headers)
        print(response.text)
        assert(response.status_code == 401)
    '''

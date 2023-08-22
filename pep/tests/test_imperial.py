
import pytest
import requests
import json 
from issuer import Issuer
from jwcrypto import jwt, jwk

class TestJWT:

    def test_valid_vp_with_non_revoked_vc(self):
        key_dict = { "crv": "P-256", "d": "yObukdNY2tUF1dy_Qb4GjTifxhMGm7VFL4MrGECmhX8",  "kty": "EC", "x": "uSbAriX2Bi34gWR1mxchmARMt85mhmfUzRSPdhW4Zs0", "y": "1rhtzeLXBh93FNM_qHn6wqUjmGSod-VCd1Hhxw0nAk0"}
        key = jwk.JWK.from_json(json.dumps(key_dict))
        myDID = "did:self:Yq8km4WxWxHjoKHXRdCUcHHKYfz80ROcrSIs4oUDbAM"
        myCompanyDID = "did:self:xWxHjoKHXRdCUcHHKYfz80ROcrSIs4oUDbAMYq8km4W"
        accessVC = Issuer().issue_valid_access_vc(myCompanyDID,"Camera1")
        authorizedVC = Issuer().issue_valid_authorized_vc(myDID,myCompanyDID)
        jwt_header = {
            "typ": "jwt",
            "alg": "ES256",
        }
        vp = {
            "vp": {
            "@context": ["https://www.w3.org/2018/credentials/v1"],
            "type": ["VerifiablePresentation"],
            "verifiableCredential": [authorizedVC,accessVC]
            }
        }
        vp_jwt = jwt.JWT(header=jwt_header, claims=vp)
        vp_jwt.make_signed_token(key)
        headers = {}
        data={"state":"teststate", "vp_token":vp_jwt.serialize()}
        response  = requests.post("http://localhost:9000/authorize", headers = headers, data=data)
        headers = {"Authorization": "Bearer teststate"}
        response  = requests.post("http://localhost:9000/secure/Camera1", headers = headers)
        print(response.text)
        assert(response.status_code == 200)

import pytest
import requests
import json 
from issuer import Issuer
from jwcrypto import jwt, jwk

class TestJWT:
    '''
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
        response  = requests.post("http://localhost:9000/vptest", headers = headers, data=data)
        print(response.text)
        assert(response.status_code == 200)
    '''
    '''
    def test_valid_vp_with_non_revoked_vc(self):
        key_dict = { "crv": "P-256", "d": "yObukdNY2tUF1dy_Qb4GjTifxhMGm7VFL4MrGECmhX8",  "kty": "EC", "x": "uSbAriX2Bi34gWR1mxchmARMt85mhmfUzRSPdhW4Zs0", "y": "1rhtzeLXBh93FNM_qHn6wqUjmGSod-VCd1Hhxw0nAk0"}
        key = jwk.JWK.from_json(json.dumps(key_dict))
        myDID = "did:self:Yq8km4WxWxHjoKHXRdCUcHHKYfz80ROcrSIs4oUDbAM"

        vc = Issuer().issue_non_revoked_vc(myDID)
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
        response  = requests.post("http://localhost:9000/vptest", headers = headers, data=data)
        print(response.text)
        assert(response.status_code == 200)
    '''
    '''
    def test_valid_vp_with_revoked_vc(self):
        key_dict = { "crv": "P-256", "d": "yObukdNY2tUF1dy_Qb4GjTifxhMGm7VFL4MrGECmhX8",  "kty": "EC", "x": "uSbAriX2Bi34gWR1mxchmARMt85mhmfUzRSPdhW4Zs0", "y": "1rhtzeLXBh93FNM_qHn6wqUjmGSod-VCd1Hhxw0nAk0"}
        key = jwk.JWK.from_json(json.dumps(key_dict))
        myDID = "did:self:Yq8km4WxWxHjoKHXRdCUcHHKYfz80ROcrSIs4oUDbAM"

        vc = Issuer().issue_revoked_vc(myDID)
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
        response  = requests.post("http://localhost:9000/vptest", headers = headers, data=data)
        print(response.text)
        assert(response.status_code == 401)
    '''
    def test_valid_vp_with_non_revoked_vc(self):
        key_dict = { "crv": "P-256", "d": "yObukdNY2tUF1dy_Qb4GjTifxhMGm7VFL4MrGECmhX8",  "kty": "EC", "x": "uSbAriX2Bi34gWR1mxchmARMt85mhmfUzRSPdhW4Zs0", "y": "1rhtzeLXBh93FNM_qHn6wqUjmGSod-VCd1Hhxw0nAk0"}
        key = jwk.JWK.from_json(json.dumps(key_dict))
        myDID = "did:self:Yq8km4WxWxHjoKHXRdCUcHHKYfz80ROcrSIs4oUDbAM"

        vc = Issuer().issue_non_revoked_vc(myDID)
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
        response  = requests.post("http://localhost:9000/vptest", headers = headers, data=data)
        print(response.text)
        assert(response.status_code == 200)
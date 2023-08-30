import json
import time
from jwcrypto import jwt, jwk
from jwcrypto.common import base64url_decode, base64url_encode
import base64


class Issuer:
    
    def __init__(self):
        '''
        >>> key = jwk.JWK.generate(kty='EC', crv='P-256')
        >>> print (key.export(as_dict=True))
        '''
        key_dict = {'kty': 'EC', 'crv': 'P-256', 'x': 'z30WuxpsPow8KpH0N93vW24nA0HD48_MluqgdEUvtU4', 'y': 'VcKco12BZFPu5HU2LBLotTD9NitdlNxnBLngD-eTapM', 'd': 'UCe_iiyGTQf13KyLPhLgjVCT3gSx4APgNSbS7uyLxN8'}
        self.key = jwk.JWK.from_json(json.dumps(key_dict))

    def issue_valid_vc(self, credentialSubjectId):
        jwt_header = {
            "typ": "jwt",
            "alg": "ES256",
            "jwk":  self.key.export_public(as_dict=True)
        }
        jwt_claims = {
            "iss": "http://testscript",
            "vc": {
                "@context": [
                "https://www.w3.org/2018/credentials/v1",
                "https://excid-io.github.io/imperial/contexts/v1"
                ],
                "type": ["VerifiableCredential","RelationshipsCredential"],
                "credentialSubject": {
                    "id": credentialSubjectId,
                    "relationships": [{
                       "ACME":"authorized"
                    }]
                }
            }
        }
        vc = jwt.JWT(header=jwt_header, claims=jwt_claims)
        vc.make_signed_token( self.key)
        return vc.serialize()

    def issue_non_revoked_vc(self, credentialSubjectId):
        jwt_header = {
            "typ": "jwt",
            "alg": "ES256",
            "jwk":  self.key.export_public(as_dict=True)
        }
        jwt_claims = {
            "iss": "http://testscript",
            "vc": {
                "@context": [
                "https://www.w3.org/2018/credentials/v1",
                "https://excid-io.github.io/imperial/contexts/v1"
                ],
                "type": ["VerifiableCredential","RelationshipsCredential"],
                "credentialSubject": {
                    "id": credentialSubjectId,
                    "relationships": [{
                       "ACME":"authorized"
                    }]
                },
                "credentialStatus":{
                    "type": "RevocationList2021Status",
                    "statusListIndex": "10",
                    "statusListCredential": "https://dev-issuer.excid.io/credential/teststatus"
                }
            }
        }
        vc = jwt.JWT(header=jwt_header, claims=jwt_claims)
        vc.make_signed_token( self.key)
        return vc.serialize()
    
    def issue_revoked_vc(self, credentialSubjectId):
        jwt_header = {
            "typ": "jwt",
            "alg": "ES256",
            "jwk":  self.key.export_public(as_dict=True)
        }
        jwt_claims = {
            "iss": "http://testscript",
            "vc": {
                "@context": [
                "https://www.w3.org/2018/credentials/v1",
                "https://excid-io.github.io/imperial/contexts/v1"
                ],
                "type": ["VerifiableCredential","RelationshipsCredential"],
                "credentialSubject": {
                    "id": credentialSubjectId,
                    "relationships": [{
                       "ACME":"authorized"
                    }]
                },
                "credentialStatus":{
                    "type": "RevocationList2021Status",
                    "statusListIndex": "9",
                    "statusListCredential": "https://dev-issuer.excid.io/credential/teststatus"
                }
            }
        }
        vc = jwt.JWT(header=jwt_header, claims=jwt_claims)
        vc.make_signed_token( self.key)
        return vc.serialize()
    
    def issue_non_revoked_vc(self, credentialSubjectId):
        jwt_header = {
            "typ": "jwt",
            "alg": "ES256",
            "jwk":  self.key.export_public(as_dict=True)
        }
        jwt_claims = {
            "iss": "http://testscript",
            "vc": {
                "@context": [
                "https://www.w3.org/2018/credentials/v1",
                "https://excid-io.github.io/imperial/contexts/v1"
                ],
                "type": ["VerifiableCredential","RelationshipsCredential"],
                "credentialSubject": {
                    "id": credentialSubjectId,
                    "relationships": [{
                       "ACME":"authorized"
                    }]
                },
                "credentialStatus":{
                    "type": "RevocationList2021Status",
                    "statusListIndex": "10",
                    "statusListCredential": "https://dev-issuer.excid.io/credential/teststatus"
                }
            }
        }
        vc = jwt.JWT(header=jwt_header, claims=jwt_claims)
        vc.make_signed_token( self.key)
        return vc.serialize()
    
    def issue_valid_authorized_vc(self, credentialSubjectId, companyId):
        jwt_header = {
            "typ": "jwt",
            "alg": "ES256",
            "jwk":  self.key.export_public(as_dict=True)
        }
        jwt_claims = {
            "iss": "http://testscript",
            "vc": {
                "@context": [
                "https://www.w3.org/2018/credentials/v1",
                "https://excid-io.github.io/imperial/contexts/v1"
                ],
                "type": ["VerifiableCredential","RelationshipsCredential"],
                "credentialSubject": {
                    "id": credentialSubjectId,
                    "relationships": [{
                       companyId:"authorized"
                    }]
                }
            }
        }
        vc = jwt.JWT(header=jwt_header, claims=jwt_claims)
        vc.make_signed_token( self.key)
        return vc.serialize()

    def issue_valid_vc_hmac(self, credentialSubjectId):
            '''
            key = jwk.JWK.generate(kty='oct', size=256)
            print (key.export(as_dict=True))
            '''
            key_dict = {'kty': 'oct', 'k': '4XsWWgfADFS_daMpZSTpiJMTXNORRKqC9CgsuUsNggE'}
            key = jwk.JWK.from_json(json.dumps(key_dict))
            jwt_header = {
                "typ": "jwt",
                "alg": "HS256",
            }
            jwt_claims = {
                "iss": "http://testscript",
                "vc": {
                    "@context": [
                    "https://www.w3.org/2018/credentials/v1",
                    "https://excid-io.github.io/imperial/contexts/v1"
                    ],
                    "type": ["VerifiableCredential","RelationshipsCredential"],
                    "credentialSubject": {
                        "id": credentialSubjectId,
                        "relationships": [{
                        "ACME":"authorized"
                        }]
                    }
                }
            }
            vc = jwt.JWT(header=jwt_header, claims=jwt_claims)
            vc.make_signed_token(key)
            return vc.serialize()
    
    def issue_subject_keys(self):
        import hmac
        import hashlib
        import secrets
        key = base64url_decode('4XsWWgfADFS_daMpZSTpiJMTXNORRKqC9CgsuUsNggE')
        keyid = secrets.token_bytes(32)
        secret = hmac.digest( key, keyid, hashlib.sha256 )
        return  base64url_encode(keyid), base64url_encode(secret)

if __name__ == '__main__':
    issuer = Issuer()
    kid, secret = issuer.issue_subject_keys()
    print(kid, secret)


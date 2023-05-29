
import pytest
import requests
import json 
from issuer import Issuer


class TestJWT:

    def test_valid_authorization(self):
        token = Issuer().issue_valid_vc()
        headers = {'Authorization':'Bearer ' + token, 'Accept': 'application/json'}
        response  = requests.get("http://localhost:9000/secure/jwt-filter", headers = headers)
        print(response.text)
        assert(response.status_code == 200)

    def test_invalid_aud(self):
        token = Issuer().issue_without_aud()
        headers = {'Authorization':'Bearer ' + token, 'Accept': 'application/json'}
        response  = requests.get("http://localhost:9000/secure/jwt-filter", headers = headers)
        print(response.text)
        assert(response.status_code == 401)

# PEP Proxy
## About
A PEP proxy that transparently protects APIs and microservices

## Usage
### Installation
Install prerequisites

```
python3 -m pip install Werkzeug
python3 -m pip install jsonpath-ng
python3 -m pip install jwcrypto
python3 -m pip install requests
```
In order to  support OpenFGA install openfga sdk

```
python3 -m pip install openfga_sdk
```

### Configuration
The core configuration file of PEP proxy is `conf/proxy.conf`. There the protected resources are described. 
The file contains a mapping from a (Werkzeug URL pattern)[https://werkzeug.palletsprojects.com/en/2.2.x/routing/#werkzeug.routing.Rule] to authentication and proxy configurations.
Examples patterns are

- "/secure/": Maps to http:/example.com/secure/
- "/secure/\<user>": Maps to e.g.,  http:/example.com/secure/Alice
- "/secure/\<record>/\<int:id>": Maps to e.g.,  http:/example.com/secure/abd43f/1
- "/secure/\<path:path>": Maps to e.g.,  http:/example.com/secure/user/22/edit

The authentication entry of a resource contains the following fields:

- **type**: It can be `vp`, `token`
- **filters**: A list of json-path queries for validating the provided tokens. 
- **trusted_issuers**: A list of objects that map issuer ids (i.e., the `iss` claim) to the following:
  - **issuer_key_type**: The format of the issuer public key, it can be `jwk` or `pem_file`
  - **issuer_key**: if issuer_key_type is jwk then this is the jwk, if issuer_key_type is pem_file the this is the path to the pem file

A guideline for constructing json-path queries can be found [here](https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html).

### Run
Execute
```
python3 pep-proxy/pep-proxy.py
```
## Docker
You can create a docker image of pep-proxy by building the provided docker file:

```
docker build -t pep-proxy -f pep-proxy.dockerfile .
```

Then run it using the following command:

```
docker run -d --rm --name pep-proxy \
  -v ./conf/proxy.conf:/conf/proxy.conf \
   -e PEP_ADDRESS='' \
  -p 9000:9000 pep-proxy
```
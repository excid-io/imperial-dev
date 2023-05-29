import requests
from urllib.parse import urlparse, parse_qs

# 1. Unauthorized request
response  = requests.get("http://localhost:9000/secure/RP1", allow_redirects=False)
if (response.status_code != 301): # This shouldn't happen
    print(response.status_code)
    exit() 
url = response.headers["Location"]
parseResult = urlparse(url)
token = parse_qs(parseResult.query)["state"][0]
print("Ctrl+Click to open: ", url)
input("Press any key when authorization is completed...")

# 2. Authorized request
headers = {'Authorization':'Bearer ' + token}
response  = requests.get("http://localhost:9000/secure/RP1",  headers = headers)
if (response.status_code == 200):
    print(response.text)
else:
    print("Access denied: "+ response.text)

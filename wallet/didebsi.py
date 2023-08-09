import base58
import jcs
from jwcrypto.common import base64url_decode, base64url_encode


key_dict = {
  "crv": "P-256",
  "kty": "EC",
  "x": "XY5DZh4py8xWGNzUeOe2FOszU8jP4KVALsaNLv_WAHI",
  "y": "Wrxy8lndBXYq8xjq8cUNMFd5OOZk1_E8NOm9b-Ynr24"
}
data = jcs.canonicalize(key_dict)
did = "did:key:z" + base58.b58encode( b'\xd1\xd6\x03' + data).decode()
print(did)


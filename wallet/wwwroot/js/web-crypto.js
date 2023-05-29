function b64urltoa(buf) {
    return btoa(buf)
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=/g, "");
}

function Uint8Tob64url(buf) {
    return btoa(String.fromCharCode.apply(null, buf))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=/g, "");
}

function ab2str(buf) {
    return String.fromCharCode.apply(null, new Uint8Array(buf));
}

function str2ab(str) {
    const buf = new ArrayBuffer(str.length);
    const bufView = new Uint8Array(buf);
    for (let i = 0, strLen = str.length; i < strLen; i++) {
      bufView[i] = str.charCodeAt(i);
    }
    return buf;
}

async function _getPublicKeyJWT() {
    var response = await fetch('/Publickey/getPublicKeyJWT', {
        method: 'POST', // or 'PUT'
        body: '{"Name":"key-1"}',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
    var jsonResponse = await response.json();
    const objResponse = JSON.parse(jsonResponse);
    console.log(jsonResponse)
    //TODO add checks
    return JSON.stringify(objResponse['key'])
}

async function createKey()
{
    let keyPair = await window.crypto.subtle.generateKey(
        {
          name: "ECDSA",
          namedCurve: "P-256"
        },
        true,
        ["sign", "verify"]
      );
    
    const exportedPrv = await window.crypto.subtle.exportKey( "pkcs8", keyPair.privateKey);
    const exportedAsString = ab2str(exportedPrv);
    const exportedAsBase64 = window.btoa(exportedAsString);
    document.getElementById('PrivateKey').value = exportedAsBase64;
    const exportedPub = await window.crypto.subtle.exportKey("jwk", keyPair.publicKey);
    document.getElementById('PublicKeyJWK').value = JSON.stringify(exportedPub, null, " ");
}

async function sign()
{
    const pemContents = document.getElementById('password').value
    // base64 decode the string to get the binary data
    const binaryDerString = window.atob(pemContents);
    // convert from a binary string to an ArrayBuffer
    const binaryDer = str2ab(binaryDerString);
    const privateKey =  await window.crypto.subtle.importKey(
        "pkcs8",
        binaryDer,
        {
            name: "ECDSA",
            namedCurve: "P-256"
        },
        true,
        ["sign"]
    );
    const publicKeyJWK = await _getPublicKeyJWT()
    const protectedHeader = `{
        "alg": "ES256",
        "typ": "JWT",
        "jwk": ${publicKeyJWK}
    }`;
    const textToSign = document.getElementById('textToSign').value
    const payload = b64urltoa(textToSign)
    const header = b64urltoa(protectedHeader)
    let encoded = new TextEncoder().encode(header + "." + payload);
    let signature = await window.crypto.subtle.sign(
      {
        name: "ECDSA",
        hash: {name: "SHA-256"},
      },
      privateKey,
      encoded
    );
    signatureArray = new Uint8Array(signature)
    document.getElementById('signature').innerHTML = header + "." + payload + "." + Uint8Tob64url(signatureArray)
}
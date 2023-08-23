# VC issuer
## About
A VC Issuer that can be accessed using [OpenID for Verifiable Credential Issuance](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)

## Usage

### Prerequisites
Specify in `appsettings.json` a JSON web key that can
be used for singing tokens. You can generate such a jwk in python using jwcrypto
and the following script

```python
from jwcrypto import jwt, jwk, jws
key = jwk.JWK.generate(kty='EC', crv='P-256')
print (key.export(as_dict=True))
```
For example:

```
"jwk": "{'kty': 'EC', 'kid': 'bZll1NPj1dEI1qmcgM1fML0pszfHxjvfD-psfjY4K50', 'crv': 'P-256', 'x': 'sCp_6IGfDeom0_9TxtLC_4elxsyOe6WLMpRYZDcvNtk', 'y': 'iwgCFXsk5yDXRvoCxMdkzTCI-uGm5lOA8c6zfMPsHi0', 'd': '...'}",
```

Finally, you have to specify in `appsettings.json` your issuer identifier (e.g., the
URL of your issuer).

### Create database

**NOTE** The following will delete any existing tables.

From the project folder run:

```
dotnet-ef migrations add InitialCreate
```

If `ef` is not available, install it using  the command `dotnet tool install --global dotnet-ef`

Then execute

```
dotnet ef database update
```



### Compile and run
You can open the source code in Visual Studio or you can use .net sdk to compile it.
Instructions for compiling and running the project follow. In order to compile
the source code, from the project folder execute:

```bash
dotnet build
```

In order to run the compiled file, from the project folder execute:

```bash
dotnet run
```


**ΝΟΤΕ**

VC issuer should be installed behind a proxy, which will support HTTPS (see
for example the instructions [here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache)).

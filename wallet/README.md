# Cloud Wallet
## About
A Cloud Wallet for storing [DIDs](https://www.w3.org/TR/did-core/) and [VCs](https://www.w3.org/TR/vc-data-model/). 
It supports [OpenID for Verifiable Credential Issuance](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)
and [OpenID for Verifiable Presentation](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)

## Usage


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

Cloud Wallet should be installed behind a proxy, which will support HTTPS (see
for example the instructions [here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache)).
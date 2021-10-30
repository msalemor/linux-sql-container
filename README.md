# Linux container pre-login handshake issue with a SQL VM

## Scenario

- Linux container 
-   .Net Core 5.0 
-   VM running SQL (2012 & 2016)
- Error when connecting:
  - A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: Shared Memory Provider, error: 0 - No process is on the other end of the pipe.) (Microsoft SQL Server, Error: 233)

## Microsoft.Data.SqlClient issue

Server Certificate validation when TLS encryption is enforced by the target Server

If the target server is configured to enforce encryption, then this version of the client requires the server certificate to be installed locally or enforce use of SSL for the database connection. Notice that Azure SQL Database is always configured to enforce encryption. So when using this version against Azure SQL Database (or any SQL Server enforcing encryption), you must at least add this setting to your connection string (to enforce use of SSL):

TrustServerCertificate=true

If you do not update your connection string (or install the server certificate) you will get this error:

"A connection was successfully established with the server, but then an error occurred during the pre-login handshake."


> Reference: https://erikej.github.io/sqlclient/efcore/2020/06/22/sqlclient-2-breaking-changes.html

## Dockerfile

```bash
# First stage
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . .

RUN dotnet restore

# Copy everything else and build website
RUN dotnet publish -c release -o /out/sqlapp

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:5.0

# Refresh the certifcates
RUN update-ca-certificates --fresh
# Set TLS minimum to 1.0
RUN sed -i 's/TLSv1.2/TLSv1.0/g' /etc/ssl/openssl.cnf
# Create a run user
RUN adduser \
  --disabled-password \
  --home /app \
  --gecos '' app \
  && chown -R app /app

USER app
WORKDIR /app
COPY --from=build /out/sqlapp ./

# Run app as an unpriviledged user (if port is exposed it should be > 1024)
ENTRYPOINT ["dotnet", "sqlapp.dll"]
```

## Open SSL update

- Note the line above:  ```RUN sed -i 's/TLSv1.2/TLSv1.0/g' /etc/ssl/openssl.cnf```
- It changes the ```/etc/ssl/openssl.cnf``` to:

```text
[system_default_sect]
MinProtocol = TLSv1.0
CipherString = DEFAULT@SECLEVEL=2
```

## Correct Solution

The correct solution to this issue is to ensure target SQL Server supports TLS 1.2 protocol by installing all latest updates. If your server supports and is enabled with TLS 1.2, it will be negotiated.
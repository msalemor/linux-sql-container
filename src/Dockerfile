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

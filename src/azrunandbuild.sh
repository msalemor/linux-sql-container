ver=v1
arcname=<NAME>

echo Building Version: $ver

az acr build --registry $acrname --file Dockerfile --image sqlapp:$ver .

echo Running Version: $ver

kubectl delete po/sqlapp

kubectl run sqlapp --image=aleacr.azurecr.io/sqlapp:$ver --restart=Never --env SQL_CONNECTION="Server=tcp:vmsql,1433;Initial Catalog=<CATALOG>;Persist Security Info=False;User ID=<USER>;Password=<PASSWORD>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"

kubectl get pods --watch

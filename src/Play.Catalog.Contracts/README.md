# Play.Catalog

play around with microservices

## Create and publish package

```
version="1.0.2"
owner="samsonprojects"
gh_pat="[PAT HERE]"

dotnet pack src/Play.Catalog.Contracts/ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Catalog -o ../packages


dotnet nuget push ../packages/Play.Catalog.$version.nupkg --api-key $gh_pat --source "github"

```

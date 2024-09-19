dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./Coverage/
reportgenerator -reports:./Coverage/coverage.opencover.xml -targetdir:Coverage

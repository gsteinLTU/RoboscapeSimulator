FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine

RUN apk update && apk add gcc build-base gcompat make
RUN apk add --no-cache python3
RUN apk add nodejs npm

COPY src/node/package.json App/src/node/
COPY src/node/package-lock.json App/src/node/
COPY src/node/index.js App/src/node/
COPY src/RoboScapeSimulator/bin/Release/net7.0/publish/ App/

WORKDIR /App/src/node

RUN npm ci

WORKDIR /App

ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "RoboScapeSimulator.dll"]
EXPOSE 9001

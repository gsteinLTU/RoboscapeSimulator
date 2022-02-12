FROM mcr.microsoft.com/dotnet/runtime:6.0 

COPY src/RoboScapeSimulator/bin/Release/net6.0/publish/ App/
WORKDIR /App
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "RoboScapeSimulator.dll"]
EXPOSE 9001

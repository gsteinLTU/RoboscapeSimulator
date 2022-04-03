build:
	dotnet build
	cd src/node && npm install
clean:
	dotnet clean
restore:
	dotnet restore
	cd src/node && npm install
watch:
	dotnet watch --project src/RoboScapeSimulator/RoboScapeSimulator.csproj run
test:
	dotnet test
start:
	dotnet run -c Release --project src/RoboScapeSimulator/RoboScapeSimulator.csproj
start-dev:
	dotnet run -c Debug --project src/RoboScapeSimulator/RoboScapeSimulator.csproj
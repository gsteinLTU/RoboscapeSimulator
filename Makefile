build:
	dotnet build
clean:
	dotnet clean
restore:
	dotnet restore
watch:
	dotnet watch --project src/RoboScapeSimulator/RoboScapeSimulator.csproj run
test:
	dotnet test
start:
	dotnet run -c Release --project src/RoboScapeSimulator/RoboScapeSimulator.csproj
start-dev:
	dotnet run -c Debug --project src/RoboScapeSimulator/RoboScapeSimulator.csproj
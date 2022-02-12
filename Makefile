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
	dotnet run --project src/RoboScapeSimulator/RoboScapeSimulator.csproj
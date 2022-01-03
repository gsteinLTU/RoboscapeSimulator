# RoboScape Online Server

This repository contains a server for a simple simulation of [RoboScape](https://www.netsblox.org/roboscape) robots written in C#. This is intended for use with the [RoboScape Online NetsBlox extension](https://github.com/NetsBlox/extensions).

## Requirements
The only dependency that must be pre-installed is the [.NET runtime](https://github.com/dotnet/runtime) >6.0. The other dependencies are either (for pre-built binaries) included or (if building locally) acquired during the build process.  

## Configuration
In appsettings.json, a few options can be configured:

 - `RoboScapeSimSettings`
   - `RoboScapeHost` - Host of RoboScape instance to communicate with
   - `RoboScapePort` - Port to communicate with RoboScape instance on
   - `MaxRooms` - Maximum number of rooms to allow users to create

## Current Scenarios

 - Default: One robot and three boxes on a large plane
 - Demo 2021: Two robots and three boxes on a plane, with barriers

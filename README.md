# RoboScape Online Server

This repository contains a server for a simple simulation of [RoboScape](https://www.netsblox.org/roboscape) robots written in C#. This is intended for use with the [RoboScape Online NetsBlox extension](https://extensions.netsblox.org).

## Requirements
The only dependency that must be pre-installed is the [.NET runtime](https://github.com/dotnet/runtime) >6.0. The other dependencies are either (for pre-built binaries) included or (if building locally) acquired during the build process.  

## Configuration
In appsettings.json, a few options can be configured:

 - `RoboScapeSimSettings`
   - `RoboScapeHost` - Host of RoboScape instance to communicate with
   - `RoboScapePort` - Port to communicate with RoboScape instance on
   - `IoTScapePort` - Port to communicate with IoTScape instance on
   - `MaxRooms` - Maximum number of rooms to allow users to create

## Current Scenarios

 - Default: One robot and three boxes on a large plane
 - Wall: Robot with a large wall
 - Square Driving: Robot and a cube to drive around
 - Four Color Robots: Four robots of different colors
 - Obstacle Course: Robots must drive through gaps in walls and push a block out of the way to reach the end
 - Table with `N` Boxes and `M` Robots (With Lidar?): `M` Robots on a raised platform with `N` boxes to push off, optionally with LIDAR sensors
 - LIDAR Road (`difficulty`): A path robots must navigate autonomously using LIDAR sensors
 - Waypoint Navigation (wait for `N` s?): Random waypoints appear on the ground, the robot must navigate to them
 - Final Challenge (mini?): Two robots must work together to complete a complex task

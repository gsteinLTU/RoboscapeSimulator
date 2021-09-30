import nengi from 'nengi';
import * as BABYLON from 'babylonjs';
import * as CANNON from 'cannon';

const red = new BABYLON.Color4(1, 0, 0);
const blue = new BABYLON.Color4(0, 0, 1);
const faceColors = [red, blue, blue, blue, blue, blue];

class Robot {
    constructor(physics = false) {
        // this.mesh = BABYLON.MeshBuilder.CreateBox('robot', { size: 1, faceColors });
        // this.mesh.position.y = this.mesh.position.y - 0.5;

        // var wheelXoffset = 0.5;
        // var wheelYoffset = 0.4;
        // var wheelZoffset = 0.6;

        // this.wheelFL = BABYLON.MeshBuilder.CreateCylinder('wheel', {diameter: 0.5, height: 0.1, tessellation: 16});
        // this.wheelRR = BABYLON.MeshBuilder.CreateCylinder('wheel', {diameter: 0.5, height: 0.1, tessellation: 16});
        // this.wheelFR = BABYLON.MeshBuilder.CreateCylinder('wheel', {diameter: 0.5, height: 0.1, tessellation: 16});
        // this.wheelRL = BABYLON.MeshBuilder.CreateCylinder('wheel', {diameter: 0.5, height: 0.1, tessellation: 16});
        
        // this.wheelFL.rotation.x = Math.PI / 2;
        // this.wheelFR.rotation.x = Math.PI / 2;
        // this.wheelRL.rotation.x = Math.PI / 2;
        // this.wheelRR.rotation.x = Math.PI / 2;
        
        // this.wheelFL.position = this.mesh.position.add(new BABYLON.Vector3(-wheelXoffset, -wheelYoffset, -wheelZoffset));
        // this.wheelFR.position = this.mesh.position.add(new BABYLON.Vector3(wheelXoffset, -wheelYoffset, -wheelZoffset));
        // this.wheelRL.position = this.mesh.position.add(new BABYLON.Vector3(-wheelXoffset, -wheelYoffset, wheelZoffset));
        // this.wheelRR.position = this.mesh.position.add(new BABYLON.Vector3(wheelXoffset, -wheelYoffset, wheelZoffset));

        // this.wheelFL.parent = this.mesh;
        // this.wheelFR.parent = this.mesh;
        // this.wheelRL.parent = this.mesh;
        // this.wheelRR.parent = this.mesh;
        //CAR!
        var width = 8;
        var depth = 8;
        var height = 0.3;

        var wheelDiameter = 5;
        var wheelDepthPosition = (depth + wheelDiameter) / 2;

        var axisWidth = width + wheelDiameter;

        var centerOfMassAdjust = new CANNON.Vec3(0, -wheelDiameter, 0);

        var chassis = BABYLON.MeshBuilder.CreateBox("chassis", {
            width: width,
            height: height,
            depth: depth
        });
        chassis.position.y = wheelDiameter + height / 2;

        this.mesh = chassis;
        
        if(physics == true){

            chassis.physicsImpostor = new BABYLON.PhysicsImpostor(chassis, BABYLON.PhysicsImpostor.BoxImpostor, { mass: 10, restitution: 0.9 });
            // chassis.physicsImpostor = new BABYLON.PhysicsImpostor(chassis, BABYLON.PhysicsEngine.BoxImpostor, {
            //     mass: 10
            // }, scene)
            //camera.target = (chassis);
            var wheels = [0, 1, 2, 3].map(function(num) {
                var wheel = BABYLON.MeshBuilder.CreateSphere("wheel" + num, {
                    segments: 4,
                    diameter: wheelDiameter
                });
                var a = (num % 2) ? -1 : 1;
                var b = num < 2 ? 1 : -1;
                wheel.position.copyFromFloats(a * axisWidth / 2, wheelDiameter / 2, b * wheelDepthPosition)
                wheel.scaling.x = 0.4;
                wheel.physicsImpostor = new BABYLON.PhysicsImpostor(wheel, BABYLON.PhysicsImpostor.SphereImpostor, { mass: 3, restitution: 0.9 });
                return wheel;
            });

            var vehicle = new CANNON.RigidVehicle({
                chassisBody: chassis.physicsImpostor.physicsBody
            });


            var down = new CANNON.Vec3(0, -1, 0);

            vehicle.addWheel({
                body: wheels[0].physicsImpostor.physicsBody,
                position: new CANNON.Vec3(axisWidth / 2, 0, wheelDepthPosition).vadd(centerOfMassAdjust),
                axis: new CANNON.Vec3(1, 0, 0),
                direction: down
            });

            vehicle.addWheel({
                body: wheels[1].physicsImpostor.physicsBody,
                position: new CANNON.Vec3(-axisWidth / 2, 0, wheelDepthPosition).vadd(centerOfMassAdjust),
                axis: new CANNON.Vec3(-1, 0, -0),
                direction: down
            });

            vehicle.addWheel({
                body: wheels[2].physicsImpostor.physicsBody,
                position: new CANNON.Vec3(axisWidth / 2, 0, -wheelDepthPosition).vadd(centerOfMassAdjust),
                axis: new CANNON.Vec3(1, 0, 0),
                direction: down
            });

            vehicle.addWheel({
                body: wheels[3].physicsImpostor.physicsBody,
                position: new CANNON.Vec3(-axisWidth / 2, 0, -wheelDepthPosition).vadd(centerOfMassAdjust),
                axis: new CANNON.Vec3(-1, 0, 0),
                direction: down
            });

            // Some damping to not spin wheels too fast
            for (var i = 0; i < vehicle.wheelBodies.length; i++) {
                vehicle.wheelBodies[i].angularDamping = 0.4;
            }

            //add the constraints to the world
            var world = wheels[3].physicsImpostor.physicsBody.world;

            for (var i = 0; i < vehicle.constraints.length; i++) {
                world.addConstraint(vehicle.constraints[i]);
            }
            // this.wheelFL.physicsImpostor = new BABYLON.PhysicsImpostor(this.wheelFL, BABYLON.PhysicsImpostor.CylinderImpostor, { mass: 0.25, restitution: 0.2 });
            // this.wheelRL.physicsImpostor = new BABYLON.PhysicsImpostor(this.wheelRL, BABYLON.PhysicsImpostor.CylinderImpostor, { mass: 0.25, restitution: 0.2 });
            // this.wheelFR.physicsImpostor = new BABYLON.PhysicsImpostor(this.wheelFR, BABYLON.PhysicsImpostor.CylinderImpostor, { mass: 0.25, restitution: 0.2 });
            // this.wheelRR.physicsImpostor = new BABYLON.PhysicsImpostor(this.wheelRR, BABYLON.PhysicsImpostor.CylinderImpostor, { mass: 0.25, restitution: 0.2 });
            // this.mesh.physicsImpostor = new BABYLON.PhysicsImpostor(this.mesh, BABYLON.PhysicsImpostor.BoxImpostor, { mass: 1, restitution: 0.2 });
        


            // var vehicle = new CANNON.RigidVehicle({
            //     chassisBody: this.mesh.physicsImpostor.physicsBody
            // });

            // var down = new CANNON.Vec3(0, -1, 0);

            // vehicle.addWheel({
            //     body: this.wheelFL.physicsImpostor.physicsBody,
            //     position: new CANNON.Vec3(wheelXoffset, wheelYoffset, wheelZoffset),
            //     axis: new CANNON.Vec3(1, 0, 0),
            //     direction: down
            // });
        }
        //this.mesh.checkCollisions = true;
    }

    get x() { return this.mesh.position.x; }
    set x(value) { this.mesh.position.x = value; }

    get y() { return this.mesh.position.y; }
    set y(value) { this.mesh.position.y = value; }

    get z() { return this.mesh.position.z; }
    set z(value) { this.mesh.position.z = value; }

    get rotationX() { 
        if(!this.mesh.rotationQuaternion){
            return this.mesh.rotation.x;
        }

        return this.mesh.rotationQuaternion.toEulerAngles().x; 
    }
    set rotationX(value) { this.mesh.rotationQuaternion = BABYLON.Quaternion.FromEulerAngles(value, this.rotationY, this.rotationZ); }

    get rotationY() {
        if(!this.mesh.rotationQuaternion){
            return this.mesh.rotation.y;
        }

        return this.mesh.rotationQuaternion.toEulerAngles().y; 
    }
    set rotationY(value) { this.mesh.rotationQuaternion = BABYLON.Quaternion.FromEulerAngles(this.rotationX, value, this.rotationZ); }

    get rotationZ() {
        if(!this.mesh.rotationQuaternion){
            return this.mesh.rotation.z;
        }

        return this.mesh.rotationQuaternion.toEulerAngles().z; 
    }
    set rotationZ(value) { this.mesh.rotationQuaternion = BABYLON.Quaternion.FromEulerAngles(this.rotationX, this.rotationY, value); }
}

Robot.protocol = {
    x: { type: nengi.Float32, interp: true },
    y: { type: nengi.Float32, interp: true },
    z: { type: nengi.Float32, interp: true },
    rotationX: { type: nengi.RotationFloat32, interp: true },
    rotationY: { type: nengi.RotationFloat32, interp: true },
    rotationZ: { type: nengi.RotationFloat32, interp: true }
};

export default Robot;

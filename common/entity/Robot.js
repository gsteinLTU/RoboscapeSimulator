import nengi from 'nengi';
import * as BABYLON from 'babylonjs';
import * as CANNON from 'cannon';
import { generateRandomMAC } from '../util';

const red = new BABYLON.Color4(1, 0, 0);
const blue = new BABYLON.Color4(0, 0, 1);
const faceColors = [red, blue, blue, blue, blue, blue];

class Robot {
    constructor(physics = false) {
        this.mesh = BABYLON.MeshBuilder.CreateBox('robot', { size: 1, faceColors });
        this.mesh.position.y = this.mesh.position.y - 0.5;

        var wheelXoffset = 0.5;
        var wheelYoffset = -0.15;
        var wheelZoffset = 0.4;

        this.wheelFL = BABYLON.MeshBuilder.CreateCylinder('wheel', {diameter: 0.5, height: 0.1, tessellation: 16});
        this.wheelFR = BABYLON.MeshBuilder.CreateCylinder('wheel', {diameter: 0.5, height: 0.1, tessellation: 16});
        // this.wheelRR = BABYLON.MeshBuilder.CreateCylinder('wheel', {diameter: 0.5, height: 0.1, tessellation: 16});
        // this.wheelRL = BABYLON.MeshBuilder.CreateCylinder('wheel', {diameter: 0.5, height: 0.1, tessellation: 16});
        
        this.wheelFL.rotation.z = Math.PI / 2;
        this.wheelFR.rotation.z = Math.PI / 2;
        // this.wheelRL.rotation.x = Math.PI / 2;
        // this.wheelRR.rotation.x = Math.PI / 2;
        
        this.wheelFL.position = this.mesh.position.add(new BABYLON.Vector3(-wheelXoffset, -wheelYoffset, wheelZoffset));
        this.wheelFR.position = this.mesh.position.add(new BABYLON.Vector3(wheelXoffset, -wheelYoffset, wheelZoffset));
        // this.wheelRL.position = this.mesh.position.add(new BABYLON.Vector3(-wheelXoffset, -wheelYoffset, -wheelZoffset));
        // this.wheelRR.position = this.mesh.position.add(new BABYLON.Vector3(wheelXoffset, -wheelYoffset, -wheelZoffset));

        this.wheelFL.parent = this.mesh;
        this.wheelFR.parent = this.mesh;
        // this.wheelRL.parent = this.mesh;
        // this.wheelRR.parent = this.mesh;
        
        if(physics == true){
            this.mac = generateRandomMAC();
            console.log(`New robot with MAC ${this.mac}`);


            // this.wheelFL.physicsImpostor = new BABYLON.PhysicsImpostor(this.wheelFL, BABYLON.PhysicsImpostor.CylinderImpostor, { mass: 0.25, restitution: 0.2 });
            // this.wheelRL.physicsImpostor = new BABYLON.PhysicsImpostor(this.wheelRL, BABYLON.PhysicsImpostor.CylinderImpostor, { mass: 0.25, restitution: 0.2 });
            // this.wheelFR.physicsImpostor = new BABYLON.PhysicsImpostor(this.wheelFR, BABYLON.PhysicsImpostor.CylinderImpostor, { mass: 0.25, restitution: 0.2 });
            // this.wheelRR.physicsImpostor = new BABYLON.PhysicsImpostor(this.wheelRR, BABYLON.PhysicsImpostor.CylinderImpostor, { mass: 0.25, restitution: 0.2 });
            this.mesh.physicsImpostor = new BABYLON.PhysicsImpostor(this.mesh, BABYLON.PhysicsImpostor.BoxImpostor, { mass: 10, restitution: 0.1 });
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

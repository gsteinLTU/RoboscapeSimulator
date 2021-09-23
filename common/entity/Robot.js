import nengi from 'nengi';
import * as BABYLON from 'babylonjs';

const red = new BABYLON.Color4(1, 0, 0);
const blue = new BABYLON.Color4(0, 0, 1);
const faceColors = [red, blue, blue, blue, blue, blue];

class Robot {
    constructor(physics = false) {
        this.mesh = BABYLON.MeshBuilder.CreateBox('robot', { size: 1, faceColors });

        if(physics == true){
            this.physicsImpostor = new BABYLON.PhysicsImpostor(this.mesh, BABYLON.PhysicsImpostor.BoxImpostor, { mass: 1, restitution: 0.2 });
        }

        this.mesh.checkCollisions = true;
    }

    get x() { return this.mesh.position.x; }
    set x(value) { this.mesh.position.x = value; }

    get y() { return this.mesh.position.y; }
    set y(value) { this.mesh.position.y = value; }

    get z() { return this.mesh.position.z; }
    set z(value) { this.mesh.position.z = value; }

    get rotationX() { return this.mesh.rotation.x; }
    set rotationX(value) { this.mesh.rotation.x = value; }

    get rotationY() { return this.mesh.rotation.y; }
    set rotationY(value) { this.mesh.rotation.y = value; }

    get rotationZ() { return this.mesh.rotation.z; }
    set rotationZ(value) { this.mesh.rotation.z = value; }
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

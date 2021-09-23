import nengi from 'nengi';
import * as BABYLON from 'babylonjs';

class Ground {
    constructor(physics = false) {
        this.mesh = BABYLON.MeshBuilder.CreateGround('ground', { width: 30, height: 30 });
        this.y = -1;

        if(physics == true){
            this.physicsImpostor = new BABYLON.PhysicsImpostor(this.mesh, BABYLON.PhysicsImpostor.BoxImpostor, { mass: 0, restitution: 0.9 });
        }

        this.mesh.checkCollisions = true;
    }

    get x() { return this.mesh.position.x; }
    set x(value) { this.mesh.position.x = value; }

    get y() { return this.mesh.position.y; }
    set y(value) { this.mesh.position.y = value; }

    get z() { return this.mesh.position.z; }
    set z(value) { this.mesh.position.z = value; }
}

Ground.protocol = {
    x: { type: nengi.Float32, interp: false },
    y: { type: nengi.Float32, interp: false },
    z: { type: nengi.Float32, interp: false },
};

export default Ground;

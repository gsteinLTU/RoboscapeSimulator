import * as BABYLON from 'babylonjs';
//import 'babylonjs-materials'

class BABYLONRenderer {
    constructor() {
        const canvas = document.getElementById('main-canvas');
        this.engine = new BABYLON.Engine(canvas, true);
        this.engine.enableOfflineSupport = false;

        canvas.addEventListener('resize', function(){
            this.engine.resize();
        });

        this.scene = new BABYLON.Scene(this.engine);
        this.scene.collisionsEnabled = true;

        this.camera = new BABYLON.FreeCamera('camera', new BABYLON.Vector3(0, 0, 0), this.scene);
        this.camera.speed = 0.3;
        this.camera.minZ = 0.01;
        this.camera.maxZ = 250;
        this.camera.attachControl(document.getElementById('main-canvas'), true);

        const light = new BABYLON.DirectionalLight('light1', new BABYLON.Vector3(0, -1, 0.5), this.scene);
        light.position = new BABYLON.Vector3(0, 15, -30);
        light.intensity = 1;

        this.shadowGenerator = new BABYLON.CascadedShadowGenerator(512, light);
        this.shadowGenerator.setDarkness(0.25);
        this.shadowGenerator.bias = 0;

        this.scene.executeWhenReady(() => { console.log('SCENE READY'); });

        // needed for certain shaders, though none in this simple demo
        this.engine.runRenderLoop(() => { });
    }

    drawHitscan(message, color) {
        // shows a red ray for a few frames.. should maybe make a tube or something more visible
        const { sourceId, x, y, z, tx, ty, tz } = message;
        const rayhelper = new BABYLON.RayHelper(new BABYLON.Ray(
            new BABYLON.Vector3(x, y, z),
            new BABYLON.Vector3(tx, ty, tz),
        ));

        rayhelper.show(this.scene, color);

        setTimeout(() => {
            rayhelper.dispose();
        }, 128);
    }

    update(delta) {
        this.scene.render();
    }
}

export default BABYLONRenderer;

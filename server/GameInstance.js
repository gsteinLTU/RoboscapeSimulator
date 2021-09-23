import nengi from 'nengi';
import nengiConfig from '../common/nengiConfig';
import Robot from '../common/entity/Robot';
import Identity from '../common/message/Identity';
import niceInstanceExtension from './niceInstanceExtension';
import applyCommand from '../common/applyCommand';
import followPath from './followPath';
import lagCompensatedHitscanCheck from './lagCompensatedHitscanCheck';
import CANNON from 'cannon';

import * as BABYLON from 'babylonjs';
import Ground from '../common/entity/Ground';

//import 'babylonjs-loaders' // mutates something globally
global.XMLHttpRequest = require('xhr2').XMLHttpRequest;

var lastRobot;

class GameInstance {
    constructor() {
        this.engine = new BABYLON.NullEngine();
        this.engine.enableOfflineSupport = false;
        this.scene = new BABYLON.Scene(this.engine);
        this.scene.collisionsEnabled = true;
        
        this.scene.enablePhysics(new BABYLON.Vector3(0,-9.81, 0), new BABYLON.CannonJSPlugin(true, 10, CANNON));
	
        const camera = new BABYLON.ArcRotateCamera('Camera', 0, 0.8, 100, BABYLON.Vector3.Zero(), this.scene);

        this.instance = new nengi.Instance(nengiConfig, { port: 8079 });
        niceInstanceExtension(this.instance);

        // game-related state
        //this.obstacles = setupObstacles(this.instance);
        // (the rest is just attached to client objects when they connect)
        this.ground = new Ground(true);
        this.instance.addEntity(this.ground);

        this.instance.on('connect', ({ client, callback }) => {
            // PER player-related state, attached to clients

            // create a entity for this client
            // const rawEntity = new Robot();
            // rawEntity.mesh.checkCollisions = true;

            // make the raw entity only visible to this client
            // const channel = this.instance.createChannel();
            // channel.subscribe(client);
            // channel.addEntity(rawEntity);
            // this.instance.addEntity(rawEntity)
            // client.channel = channel;

            // smooth entity is visible to everyone
            // const smoothEntity = new Robot();
            // smoothEntity.mesh.checkCollisions = false;
            // this.instance.addEntity(smoothEntity);
            const robot = new Robot(true);
            robot.mesh.checkCollisions = true;
            robot.x = Math.random() * 10.0;
            robot.y = Math.random() * 10.0;
            robot.z = Math.random() * 10.0;
            robot.rotationX = Math.random();
            robot.rotationZ = Math.random();
            this.instance.addEntity(robot);
            
            robot.mesh.computeWorldMatrix(true);
            lastRobot = robot;

            // tell the client which entities it controls
            //this.instance.message(new Identity(rawEntity.nid, smoothEntity.nid), client);

            // establish a relation between this entity and the client
            // rawEntity.client = client;
            // client.rawEntity = rawEntity;
            // smoothEntity.client = client;
            // client.smoothEntity = smoothEntity;
            // client.positions = [];

            // define the view (the area of the game visible to this client, all else is culled)
            // there is no 3D view culler in nengi yet, so we just use a big view (there will be one soon tho)
            client.view = {
                x: 0,
                y: 0,
                z: 0,
                halfWidth: 50,
                halfHeight: 50,
                halfDepth: 50
            };

            // accept the connection
            callback({ accepted: true, text: 'Welcome!' });
        });

        this.instance.on('disconnect', client => {
            // clean up per client state
            // client.rawEntity.mesh.dispose();			
            // client.smoothEntity.mesh.dispose();
            // this.instance.removeEntity(client.rawEntity);
            // this.instance.removeEntity(client.smoothEntity);
            // client.channel.destroy();
        });

        // this.instance.on('command::MoveCommand', ({ command, client, tick }) => {
        //     // move this client's entity
        //     const entity = client.rawEntity;

        //     applyCommand(entity, command, this.obstacles);
        //     client.positions.push({
        //         x: entity.x,
        //         y: entity.y,
        //         z: entity.z,
        //         rotation: entity.rotation
        //     });
        // });
    }

    update(delta, tick, now) {
        this.instance.emitCommands();

        // for each player ...
        // this.instance.clients.forEach(client => {
        //     const { rawEntity, smoothEntity } = client;

        //     // center client's network view on the entity they control
        //     client.view.x = rawEntity.x;
        //     client.view.y = rawEntity.y;
        //     client.view.z = rawEntity.z;

        //     // smooth entity will follow raw entity's path at *up to* 110% movement speed
        //     // confused? stop by the nengi discord server https://discord.gg/7kAa7NJ 
        //     const movementBudget = rawEntity.speed * 1.1 * delta;
        //     followPath(smoothEntity, client.positions, movementBudget);
        //     smoothEntity.rotationX = rawEntity.rotationX;
        //     smoothEntity.rotationY = rawEntity.rotationY;
        // });

        // TECHNICALLY we should call scene.render(), but as this game is so simple and
        // computeWorldMatrix is called on every object after it is moved, i skipped it.
        // that's all scene.render() was going to do for us

        // when instance.updates, nengi sends out snapshots to every client
        for (let i = 0; i < delta * 5 / (1 / 60); i++) {
            this.scene.render();            
        }

        if(lastRobot){
            lastRobot.rotationY += 0.0001;
            lastRobot.rotationX += 0.0001;
            lastRobot.rotationZ += 0.0001;
        }

        this.instance.update();
    }
}

export default GameInstance;

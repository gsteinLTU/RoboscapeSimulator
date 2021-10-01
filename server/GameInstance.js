import nengi from 'nengi';
import nengiConfig from '../common/nengiConfig';
import Robot from '../common/entity/Robot';
import Identity from '../common/message/Identity';
import niceInstanceExtension from './niceInstanceExtension';
import applyCommand from '../common/applyCommand';
import followPath from './followPath';
import lagCompensatedHitscanCheck from './lagCompensatedHitscanCheck';
import CANNON from 'cannon';
import {createSocket} from 'dgram';

import * as BABYLON from 'babylonjs';
import Ground from '../common/entity/Ground';
import { debug } from 'debug';

//import 'babylonjs-loaders' // mutates something globally
global.XMLHttpRequest = require('xhr2').XMLHttpRequest;

var lastRobot;

class GameInstance {
    constructor() {
        this.engine = new BABYLON.NullEngine();
        this.engine.enableOfflineSupport = false;
        this.scene = new BABYLON.Scene(this.engine);
        this.scene.collisionsEnabled = true;

        this.updateEntities = [];
        
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
            robot.x = Math.random() * 5.0;
            robot.y = Math.random() * 1.0 + 0.5;
            robot.z = Math.random() * 5.0;
            robot.rotationY = Math.random();
            this.instance.addEntity(robot);          
            this.updateEntities.push(robot);

            robot.mesh.computeWorldMatrix(true);
            lastRobot = robot;
            
            robot.setSpeed = {
                left: 0,
                right: 0
            };

            robot.settings = {};
            robot.commandHandlers = {
                S: (msg) => {
                    let v1 = msg.readInt16LE(1);
                    let v2 = msg.readInt16LE(3);

                    robot.setSpeed = {
                        left: v1 / 100,
                        right: v2 / 100
                    };
                }
            };

            robot.onUpdate = (delta) => {
                const width = 10;
                robot.mesh.translate(BABYLON.Vector3.Forward(), (robot.setSpeed.left + robot.setSpeed.right) * delta);
                robot.mesh.rotate(BABYLON.Vector3.Up(), Math.atan2(robot.setSpeed.left - robot.setSpeed.right, width));
            };

            // Connect to RoboScape server to get commands
            //robot.debug = debug('roboscapesim:robot:' + robot.mac);
            robot.debug = console.log;
            robot.socket = createSocket('udp4');
            robot.socket.on('message', (msg) => msgHandler(robot, msg));

            // Start heartbeat
            robot.heartbeatInterval = setInterval(() => sendToServer(robot, 'I'), 1000);

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

        // if(lastRobot){
        //     lastRobot.rotationY += 0.0001;
        //     lastRobot.rotationX += 0.0001;
        //     lastRobot.rotationZ += 0.0001;
        // }

        for (const updateEntity of this.updateEntities) {
            updateEntity.onUpdate(delta, tick, now);
        }

        this.instance.update();
    }
}

const msgHandler = function(robot, msg) {
    // Handle known commands
    if (msg.length > 0) {
        let msgType = String.fromCharCode(msg[0]);
        if (Object.keys(robot.commandHandlers).indexOf(msgType) !== -1) {
            robot.commandHandlers[msgType](msg);

            //sendToServer(robot, msg);

            // Keep robot from timing out if in use
            robot.lastCommandTime = Date.now();
        } else {
            robot.debug(`Unknown message type: ${msgType}`);
        }
    }

    if (robot.settings.debugMessages) {
        robot.debug(msg);
    }
};

/**
 * Send a message as this robot to the server
 * @param {String | Buffer} msg Message to send
 */
const sendToServer = function(robot, msg) {
    let msgBuff;

    // String needs preallocated space
    if (typeof msg == typeof '') {
        msgBuff = Buffer.alloc(10 + msg.length);
    } else if (msg instanceof Buffer) {
        msgBuff = Buffer.alloc(10);
    } else {
        throw 'Attempt to send message which is not Buffer or string type';
    }

    let macParts = robot.mac.split(':');
    for (let i = 0; i < macParts.length; i++) {
        msgBuff.writeUInt8(Number.parseInt(macParts[i], 16), i);
    }
    msgBuff.writeUInt32BE(process.uptime(), 6);

    // Write string to position, or combine buffers
    if (typeof msg == typeof '') {
        msgBuff.write(msg, 10);
    } else {
        msgBuff = Buffer.concat([msgBuff, msg]);
    }

    if (robot.settings.debugMessages) {
        robot.debug(msgBuff);
    }

    // Send complete message to server
    try {
        robot.socket.send(msgBuff, settings.port, settings.server);
    } catch (error) {
        robot.debug(`Error sending message ${error}`);
    }
};

const settings = {
    server: 'dev.netsblox.org',
    port: 1970
};


export default GameInstance;

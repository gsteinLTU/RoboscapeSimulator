const Matter = require('matter-js');
const Bodies = Matter.Bodies,
    Body = Matter.Body,
    Vector = Matter.Vector,
    World = Matter.World;
const _ = require('lodash');
const dgram = require('dgram');

const { generateRandomMAC } = require('../util');

/**
 * Represents an abstract robot
 */
class Robot {
    constructor(mac = null, position = null, engine = null, settings = {}) {
        // Generate random new MAC address to use if none provided
        this.mac = mac || generateRandomMAC(mac);
        this.settings = _.defaults(settings, Robot.defaultSettings);

        this.debug = require('debug')(`roboscape-sim:Robot-${this.mac}`);

        position = position || {
            x: Math.random() * (settings.maxX - settings.minX) + settings.minX,
            y: Math.random() * (settings.maxY - settings.minY) + settings.minY
        };

        // Create physics object
        this.mainBody = Bodies.rectangle(position.x, position.y, settings.width, settings.height, { label: `${this.mac}_main`, friction: 0.6, frictionAir: 0.45, frictionStatic: 0 });
        this.body = Body.create({
            label: this.mac,
            position: { x: position.x, y: position.y },
            parts: [this.mainBody],
            friction: 0.6,
            frictionAir: 0.45,
            frictionStatic: 0
        });

        this.mainBody.width = settings.width;
        this.mainBody.height = settings.height;
        this.mainBody.image = settings.image;
        this.body.width = this.mainBody.width;
        this.body.height = this.mainBody.height;
        this.body.image = this.mainBody.image;
        this.setSpeed = { left: 0, right: 0 };

        this.commandHandlers = {
            S: this.updateSpeed.bind(this)
        };

        // Connect to RoboScape server to get commands
        this.socket = dgram.createSocket('udp4');
        this.socket.on('message', this.msgHandler.bind(this));

        // Start heartbeat
        this.heartbeatInterval = setInterval(this.sendToServer.bind(this, 'I'), 1000);

        // Start driving
        this.driveInterval = setInterval(this.drive.bind(this), 1000 / 60);

        // Keep track of last use
        this.lastCommandTime = Date.now();

        // Add to world
        World.add(engine.world, [this.body]);

        this.debug(`Robot with MAC ${this.mac} created`);
    }

    /**
     * Send a message as this robot to the server
     * @param {String | Buffer} msg Message to send
     */
    sendToServer(msg) {
        let msgBuff;

        // String needs preallocated space
        if (typeof msg == typeof '') {
            msgBuff = Buffer.alloc(10 + msg.length);
        } else if (msg instanceof Buffer) {
            msgBuff = Buffer.alloc(10);
        } else {
            throw 'Attempt to send message which is not Buffer or string type';
        }

        let macParts = this.mac.split(':');
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

        if (this.settings.debugMessages) {
            this.debug(msgBuff);
        }

        // Send complete message to server
        this.socket.send(msgBuff, this.settings.port, this.settings.server);
    }

    /**
     * Handle an incoming message
     * @param {Buffer} msg Message from server to this robot
     */
    msgHandler(msg) {
        // Handle known commands
        if (msg.length > 0) {
            let msgType = String.fromCharCode(msg[0]);
            if (Object.keys(this.commandHandlers).indexOf(msgType) !== -1) {
                this.commandHandlers[msgType](msg);

                //this.sendToServer(msg);

                // Keep robot from timing out if in use
                this.lastCommandTime = Date.now();
            } else {
                this.debug(`Unknown message type: ${msgType}`);
            }
        }

        if (this.settings.debugMessages) {
            this.debug(msg);
        }
    }

    /**
     * Handle an incoming "set speed" message
     * @param {Buffer} msg Message from server to this robot
     */
    updateSpeed(msg) {
        let v1 = msg.readInt16LE(1);
        let v2 = msg.readInt16LE(3);
        let boost = (2 * Math.abs(v1 - v2)) / (Math.abs(v1) + Math.abs(v2)) + 1;

        if (Math.abs(v1) + Math.abs(v2) === 0) {
            boost = 1;
        }

        this.setSpeed = {
            left: (boost * (Math.sign(v1) * Math.pow(Math.abs(v1), 0.6))) / 10000,
            right: (boost * (Math.sign(v2) * Math.pow(Math.abs(v2), 0.6))) / 10000
        };
    }

    /**
     * Applies force of wheels to robot
     */
    drive() {
        let v1 = this.setSpeed.left;
        let v2 = this.setSpeed.right;
        let vleft = Vector.create(1, 0);
        vleft = Vector.rotate(vleft, this.body.angle);
        let vup = Vector.create(0, 1);
        vup = Vector.rotate(vup, this.body.angle);

        // Apply force
        Body.applyForce(this.body, Vector.add(this.body.position, vleft), Vector.mult(vup, v1));
        Body.applyForce(this.body, Vector.sub(this.body.position, vleft), Vector.mult(vup, v2));
    }

    /**
     * Shut down this robot
     */
    close() {
        this.debug('Stopping robot...');
        clearInterval(this.driveInterval);
        clearInterval(this.heartbeatInterval);
        this.socket.close();
    }
}

Robot.defaultSettings = {
    server: process.env.SERVER || '52.73.65.98',
    port: 1973,
    width: 20,
    height: 40,
    minX: 100,
    maxX: 700,
    minY: 100,
    maxY: 700,
    debugMessages: false
};

module.exports = Robot;

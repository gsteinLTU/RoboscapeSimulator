const Matter = require('matter-js');
const Bodies = Matter.Bodies;
const _ = require('lodash');
const dgram = require('dgram');

const { generateRandomMAC } = require('../util');

const defaultSettings = {
    server: '52.73.65.98',
    port: 1973,
    width: 20,
    height: 40,
    minX: 100,
    maxX: 700,
    minY: 100,
    maxY: 700
};
class Robot {
    constructor(mac = null, position = null, settings = {}) {
        // Generate random new MAC address to use if none provided
        this.mac = mac || generateRandomMAC(mac);
        this.settings = _.defaults(settings, defaultSettings);

        position = position || {
            x: Math.random() * (settings.maxX - settings.minX) + settings.minX,
            y: Math.random() * (settings.maxY - settings.minY) + settings.minY
        };

        // Create physics object
        this.body = Bodies.rectangle(position.x, position.y, settings.width, settings.height, { label: this.mac, friction: 0.6, frictionAir: 0.45, frictionStatic: 0 });
        this.body.width = settings.width;
        this.body.height = settings.height;
        this.setSpeed = { left: 0, right: 0 };

        // Connect to RoboScape server to get commands
        this.socket = dgram.createSocket('udp4');
        this.socket.on('message', this.msgHandler.bind(this));

        // Start heartbeat
        this.heartbeatInterval = setInterval(this.sendToServer.bind(this, 'I'), 1000);

        // Start driving
        this.driveInterval = setInterval(this.drive.bind(this), 1000 / 60);

        // Keep track of last use
        this.lastCommandTime = Date.now();

        console.log(`Robot with MAC ${this.mac} created`);
    }

    /**
     * Send a message as this robot to the server
     * @param {String | Buffer} msg Message to send
     */
    sendToServer(msg) {
        let msgBuff = Buffer.alloc(10 + msg.length);
        let macParts = this.mac.split(':');
        for (let i = 0; i < macParts.length; i++) {
            msgBuff.writeUInt8(Number.parseInt(macParts[i], 16), i);
        }
        msgBuff.writeUInt32BE(process.uptime(), 6);
        msgBuff.write(msg, 10);
        // Tell server robot is alive
        this.socket.send(msgBuff, this.settings.port, this.settings.server);
    }

    /**
     * Handle an incoming message
     * @param {Buffer} msg Message from server to this robot
     */
    msgHandler(msg) {
        // Set speed command
        if (String.fromCharCode(msg[0]) == 'S') {
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

            //this.sendToServer(msg);
        }

        console.log(msg);
        // Keep robot from timing out if in use
        this.lastCommandTime = Date.now();
    }

    /**
     * Applies force of wheels to robot
     */
    drive() {
        let v1 = this.setSpeed.left;
        let v2 = this.setSpeed.right;
        let vleft = Matter.Vector.create(1, 0);
        vleft = Matter.Vector.rotate(vleft, this.body.angle);
        let vup = Matter.Vector.create(0, 1);
        vup = Matter.Vector.rotate(vup, this.body.angle);

        // Apply force
        Matter.Body.applyForce(this.body, Matter.Vector.add(this.body.position, vleft), Matter.Vector.mult(vup, v1));
        Matter.Body.applyForce(this.body, Matter.Vector.sub(this.body.position, vleft), Matter.Vector.mult(vup, v2));
    }
}

module.exports = Robot;

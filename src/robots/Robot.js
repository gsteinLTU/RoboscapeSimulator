const Matter = require('matter-js');
const Bodies = Matter.Bodies;
const dgram = require('dgram');

const { generateRandomMAC } = require('../util');

const defaultSettings = {
    server: '52.73.65.98',
    port: 1973,
    width: 20,
    height: 40
};
class Robot {
    constructor(mac = null, settings = {}) {
        // Generate random new MAC address to use if none provided
        this.mac = mac || generateRandomMAC(mac);

        this.settings = { ...defaultSettings, ...settings };

        // Create physics object
        this.body = Bodies.rectangle(400, 200, settings.width, settings.height, { label: mac });
        this.body.width = settings.width;
        this.body.height = settings.height;

        // Connect to RoboScape server to get commands
        this.socket = dgram.createSocket('udp4');
        this.socket.on('message', this.msgHandler.bind(this));

        // Start heartbeat
        this.heartbeatInterval = setInterval(this.sendToServer.bind(this, 'I'), 1000);

        console.log(`Robot with MAC ${this.mac} created`);
    }

    /**
     * Send a message as this robot to the server
     * @param {String | Buffer} msg Message to send
     */
    sendToServer(msg) {
        let msgBuff = Buffer.alloc(10 + msg.length);
        let macParts = this.mac.split(':');
        for (let i in macParts) {
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
            // Apply force
        }
    }
}

module.exports = Robot;

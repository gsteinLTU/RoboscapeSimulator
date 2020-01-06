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
        let initMsg = Buffer.alloc(11);
        let macParts = this.mac.split(':');
        for (let i in macParts) {
            initMsg.writeUInt8(Number.parseInt(macParts[i], 16), i);
        }
        initMsg.writeUInt32BE(process.uptime(), 6);
        initMsg.write('I', 10);

        // Tell server robot is alive
        this.socket.send(initMsg, this.settings.port, this.settings.server);

        console.log(`Robot with MAC ${this.mac} created`);
    }
}

module.exports = Robot;

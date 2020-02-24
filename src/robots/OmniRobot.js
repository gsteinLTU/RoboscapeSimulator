const _ = require('lodash');
const Matter = require('matter-js');
const Body = Matter.Body,
    Bodies = Matter.Bodies,
    Vector = Matter.Vector,
    World = Matter.World;

const Robot = require('./Robot');

/**
 * Represents the WIP omni-wheel robot
 */
class OmniRobot extends Robot {
    constructor(mac = null, position = null, engine = null, settings = {}) {
        // Allow overriding sprite setting
        if (settings.image == undefined) {
            settings.image = 'omnibot';
        }

        settings = _.defaults(settings, Robot.defaultSettings);
        settings.width = settings.height;

        super(mac, position, engine, { ...settings });

        World.add(engine.world, this.body);
    }

    /**
     * Creates body for robot at given position.
     * @param {Object} position
     */
    createBody(position) {
        this.mainBody = Bodies.circle(position.x, position.y, this.settings.width / 2, {
            label: `${this.mac}_main`,
            friction: 0.6,
            frictionAir: 0.45,
            frictionStatic: 0
        });

        this.body = this.mainBody;

        this.mainBody.width = this.settings.width;
        this.mainBody.height = this.settings.height;
        this.mainBody.image = this.settings.image;
        this.body.width = this.mainBody.width;
        this.body.height = this.mainBody.height;
        this.body.image = this.mainBody.image;

        // Add to world
        World.add(this.engine.world, [this.body]);
    }

    /**
     * Handle an incoming "set speed" message
     * @param {Buffer} msg Message from server to this robot
     */
    updateSpeed(msg) {
        let v1 = msg.readInt16LE(1);
        let v2 = msg.readInt16LE(3);

        this.setSpeed = {
            left: (Math.sign(v1) * Math.pow(Math.abs(v1), 0.6)) / 15000,
            right: (Math.sign(v2) * Math.pow(Math.abs(v2), 0.6)) / 15000
        };
    }

    /**
     * Applies force of wheels to robot
     */
    drive() {
        let v1 = this.setSpeed.left;
        let v2 = this.setSpeed.right;
        let vleft = Vector.create(7, 0);
        vleft = Vector.rotate(vleft, this.body.angle);
        let vup = Vector.create(0, 1);
        vup = Vector.rotate(vup, this.body.angle);

        // Apply force
        Body.applyForce(this.body, Vector.add(this.mainBody.position, vleft), Vector.mult(vup, v1));
        Body.applyForce(this.body, Vector.sub(this.mainBody.position, vleft), Vector.mult(vup, v2));
    }
}

module.exports = OmniRobot;

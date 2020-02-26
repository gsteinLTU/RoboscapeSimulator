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
            settings.image = 'omni_robot';
        }

        settings = _.defaults(settings, Robot.defaultSettings);
        settings.width = settings.height;

        super(mac, position, engine, { ...settings });
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
        let angular = this.setSpeed.right;
        let forward = this.setSpeed.left;
        let perpendicular = 0;

        let f1 = (-Math.sqrt(3) / 2) * forward + perpendicular / 2 + angular;
        let f2 = -perpendicular / 2 + angular;
        let f3 = (Math.sqrt(3) / 2) * forward + perpendicular / 2 + angular;

        let vWheel1 = Vector.create(10, 0);
        let vWheel2 = Vector.create(10, 0);
        let vWheel3 = Vector.create(10, 0);
        const angle1 = this.body.angle + Math.PI / 3;
        const angle2 = this.body.angle + Math.PI;
        const angle3 = this.body.angle + (5 * Math.PI) / 3;

        vWheel1 = Vector.rotate(vWheel1, angle1);
        vWheel2 = Vector.rotate(vWheel2, angle2);
        vWheel3 = Vector.rotate(vWheel3, angle3);
        let vup = Vector.create(0, 1);

        // Apply force
        Body.applyForce(this.body, Vector.add(this.body.position, vWheel1), Vector.mult(Vector.rotate(vup, angle1), f1));
        Body.applyForce(this.body, Vector.add(this.body.position, vWheel2), Vector.mult(Vector.rotate(vup, angle2), f2));
        Body.applyForce(this.body, Vector.add(this.body.position, vWheel3), Vector.mult(Vector.rotate(vup, angle3), f3));
    }
}

module.exports = OmniRobot;

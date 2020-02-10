const _ = require('lodash');
const Matter = require('matter-js');
const Body = Matter.Body,
    Bodies = Matter.Bodies,
    Vector = Matter.Vector,
    World = Matter.World;

const Robot = require('./Robot');

/**
 * Represents a Parallax ActivityBot robot
 */
class ParallaxRobot extends Robot {
    constructor(mac = null, position = null, engine = null, settings = {}) {
        // Allow overriding sprite setting
        if (settings.image == undefined) {
            settings.image = 'parallax_robot';
        }

        settings = _.defaults(settings, Robot.defaultSettings);

        super(mac, position, engine, settings);

        // Add whiskers
        this.addWhiskers(engine);

        // Setup range sensor
        this.commandHandlers['R'] = this.sendRange.bind(this);

        // Update center of mass position
        this.body.positionPrev.x = this.mainBody.position.x - (this.body.position.x - this.body.positionPrev.x);
        this.body.positionPrev.y = this.mainBody.position.y - (this.body.position.y - this.body.positionPrev.y);
        this.body.position.x = this.mainBody.position.x;
        this.body.position.y = this.mainBody.position.y;

        this.body.width = this.mainBody.width;
        this.body.height = this.mainBody.height;
        this.body.image = this.mainBody.image;

        World.add(engine.world, this.body);
    }

    /**
     * Adds whiskers during robot creation
     * @param {Matter.Engine} engine
     */
    addWhiskers(engine) {
        let whiskerL = Bodies.rectangle(this.body.position.x - this.body.width / 2, this.body.position.y + this.body.height / 2, this.body.width * 0.99, this.body.height / 2, {
            label: `${this.mac}_whiskerL`,
            isSensor: true,
            friction: 0,
            frictionStatic: 0,
            frictionAir: 0
        });
        whiskerL.width = this.body.width * 0.99;
        whiskerL.height = this.body.height / 2;
        whiskerL.parent = this.body;
        whiskerL.currentState = false;
        let whiskerR = Bodies.rectangle(this.body.position.x + this.body.width / 2, this.body.position.y + this.body.height / 2, this.body.width * 0.99, this.body.height / 2, {
            label: `${this.mac}_whiskerR`,
            isSensor: true,
            friction: 0,
            frictionStatic: 0,
            frictionAir: 0
        });
        whiskerR.width = this.body.width * 0.99;
        whiskerR.height = this.body.height / 2;
        whiskerR.parent = this.body;
        whiskerR.currentState = false;
        let whiskerHit = function(thisWhisker, whiskerL, whiskerR, value) {
            thisWhisker.currentState = value;
            // Create Whiskers message
            let temp = new Buffer(2);
            temp.write('W');
            // These values were inverted in the original RoboScape code
            temp.writeUInt8((whiskerL.currentState ? 0 : 2) | (whiskerR.currentState ? 0 : 1), 1);
            this.sendToServer(temp);
        };
        // Create collision event functions
        whiskerL.onCollisionStart = whiskerHit.bind(this, whiskerL, whiskerL, whiskerR, true);
        whiskerR.onCollisionStart = whiskerHit.bind(this, whiskerR, whiskerL, whiskerR, true);
        whiskerL.onCollisionEnd = whiskerHit.bind(this, whiskerL, whiskerL, whiskerR, false);
        whiskerR.onCollisionEnd = whiskerHit.bind(this, whiskerR, whiskerL, whiskerR, false);
        World.remove(engine.world, this.body);
        this.body = Body.create({
            parts: [this.mainBody, whiskerL, whiskerR],
            label: this.mac,
            friction: 0.6,
            frictionAir: 0.45,
            frictionStatic: 0
        });
    }

    /**
     * Sends range sensor value to server
     */
    sendRange() {
        // Create Range message
        let temp = new Buffer(3);
        temp.write('R');

        const upVec = Vector.rotate(Vector.create(0, 1), this.body.angle);

        const rayStart = Vector.add(this.body.position, Vector.mult(upVec, 25));
        const rayEnd = Vector.add(this.body.position, Vector.mult(upVec, 325));

        // Find objects in sensor path
        let query = Matter.Query.ray(this.engine.world.bodies, rayStart, rayEnd);

        // Find closest object
        query = query.sort(r => Math.pow(r.bodyA.position.x - this.body.position.x, 2) + Math.pow(r.bodyA.position.y - this.body.position.y, 2));
        let distance = 300;
        if (query !== undefined && query.length > 0) {
            let target = query[0].bodyA;

            let targetVerts = [...target.vertices];
            targetVerts.push(targetVerts[0]);
            let targetEdges = [];

            // Get all edges
            for (let i in targetVerts) {
                targetEdges.push({
                    p1x: targetVerts[i].x,
                    p1y: targetVerts[i].y,
                    p2x: targetVerts[(i + 1) % targetVerts.length].x,
                    p2y: targetVerts[(i + 1) % targetVerts.length].y
                });
            }

            // Test for intersections
            let x1 = rayStart.x;
            let x2 = rayStart.x + upVec.x * 300;
            let y1 = rayStart.y;
            let y2 = rayStart.y + upVec.y * 300;
            for (let edge of targetEdges) {
                let x3 = edge.p1x;
                let y3 = edge.p1y;
                let x4 = edge.p2x;
                let y4 = edge.p2y;

                let t = (x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4);
                t /= (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

                let u = (x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3);
                u /= (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
                u *= -1;

                // No intersection between segments
                if (t > 1 || t < 0 || u > 1 || u < 0) {
                    continue;
                }

                distance = Math.min(distance, t * 300);
            }
        }

        // Return result with noise
        temp.writeInt16LE(distance * (Math.random() / 10 + 0.95), 1);
        this.sendToServer(temp);
    }
}

module.exports = ParallaxRobot;

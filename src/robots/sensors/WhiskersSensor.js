const Matter = require('matter-js');
const Body = Matter.Body,
    Bodies = Matter.Bodies,
    World = Matter.World;


/**
 * A pair of triggers in front of the robot, sending a message when the robot bumps something
 */
class WhiskersSensor {

    /**
     * Adds Whiskers sensors to robot
     * @param {Robot} robot 
     */
    static addTo(robot){
        WhiskersSensor._addWhiskers.bind(robot, robot.engine)();
    }


    /**
     * Adds whiskers during robot creation
     * @param {Matter.Engine} engine
     */
    static _addWhiskers(engine) {
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
        let whiskerHit = function (thisWhisker, whiskerL, whiskerR, value) {
            thisWhisker.currentState = value;
            // Create Whiskers message
            let temp = Buffer.alloc(2);
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
}

module.exports = WhiskersSensor;
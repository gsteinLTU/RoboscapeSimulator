const Matter = require('matter-js');
const Engine = Matter.Engine,
    World = Matter.World,
    Bodies = Matter.Bodies;
const Robot = require('./robots/Robot');

class Room {
    constructor() {
        this.engine = Engine.create();
        this.engine.world.gravity.y = 0;

        this.settings = { fps: 60 };
        const boxSize = 80;
        const groundWidth = 800;

        this.bodies = [];
        this.robots = [];

        // Create bounds
        var ground = Bodies.rectangle(groundWidth / 2 + boxSize, groundWidth, groundWidth, boxSize, { isStatic: true, label: 'ground' });
        ground.width = groundWidth;
        ground.height = boxSize;
        this.bodies.push(ground);
        var ground2 = Bodies.rectangle(groundWidth / 2 + boxSize, boxSize, groundWidth, boxSize, { isStatic: true, label: 'ground2' });
        ground2.width = groundWidth;
        ground2.height = boxSize;
        this.bodies.push(ground2);
        var ground3 = Bodies.rectangle(boxSize, groundWidth / 2 + boxSize / 2, boxSize, groundWidth, { isStatic: true, label: 'ground3' });
        ground3.width = boxSize;
        ground3.height = groundWidth;
        this.bodies.push(ground3);
        var ground4 = Bodies.rectangle(groundWidth + boxSize, groundWidth / 2 + boxSize / 2, boxSize, groundWidth, { isStatic: true, label: 'ground4' });
        ground4.width = boxSize;
        ground4.height = groundWidth;
        this.bodies.push(ground4);

        // Demo box
        var box = Bodies.rectangle(groundWidth / 2 - boxSize / 2, groundWidth / 2 - boxSize / 2, boxSize, boxSize, { label: 'box', frictionAir: 0.7 });
        box.width = boxSize;
        box.height = boxSize;
        this.bodies.push(box);

        World.add(this.engine.world, this.bodies);

        // Begin update loop
        this.updateInterval = setInterval(
            function() {
                Engine.update(this.engine, 1000 / this.settings.fps);
            }.bind(this),
            1000 / this.settings.fps
        );
    }

    /**
     * Returns an array of the objects in the scene
     */
    getBodies(onlySleeping = true) {
        return this.bodies
            .filter(body => !onlySleeping || !body.isSleeping)
            .map(body => {
                return { label: body.label, pos: body.position, vel: body.velocity, angle: body.angle, anglevel: body.angularVelocity, width: body.width, height: body.height };
            });
    }

    /**
     * Add a robot to the room
     * @param {String} mac
     * @param {Matter.Vector} position
     */
    addRobot(mac = null, position = null) {
        let bot = new Robot(mac, position);
        this.robots.push(bot);
        this.bodies.push(bot.body);
        World.add(this.engine.world, [bot.body]);
        return bot;
    }
}

module.exports = Room;

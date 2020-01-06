const Matter = require('matter-js');
const Engine = Matter.Engine,
    World = Matter.World,
    Bodies = Matter.Bodies;
const Robot = require('./robots/Robot');

class Room {
    constructor() {
        this.engine = Engine.create();
        this.settings = { fps: 60 };
        const boxSize = 80;
        const groundWidth = 800;

        this.bodies = [];
        this.robots = [];

        let bot = new Robot('e2:a3:90:f2:33:3e');
        this.robots.push(bot);
        this.bodies.push(bot.body);

        var ground = Bodies.rectangle(400, 610, groundWidth, boxSize, { isStatic: true, label: 'ground' });
        ground.width = groundWidth;
        ground.height = boxSize;
        this.bodies.push(ground);

        World.add(this.engine.world, this.bodies);

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
    getBodies() {
        return this.bodies.map(body => {
            return { label: body.label, pos: body.position, vel: body.velocity, angle: body.angle, anglevel: body.angularVelocity, width: body.width, height: body.height };
        });
    }
}

module.exports = Room;

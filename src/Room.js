const Matter = require('matter-js');
const Engine = Matter.Engine,
    World = Matter.World,
    Bodies = Matter.Bodies;
const _ = require('lodash');
const shortid = require('shortid');

const ParallaxRobot = require('./robots/ParallaxRobot');

const defaultSettings = {
    robotKeepAliveTime: 1000 * 60 * 10,
    fps: 60
};

class Room {
    constructor(settings = {}) {
        // Get unique ID for this Room
        if (settings.roomID == undefined) {
            this.roomID = shortid.generate();

            while (Room.existingIDs.indexOf(this.roomID) !== -1) {
                this.roomID = shortid.generate();
            }
        } else {
            this.roomID = settings.roomID;
        }

        this.robots = [];

        this.debug = require('debug')(`roboscape-sim:Room-${this.roomID}`);
        this.debug('Creating room');

        this.settings = _.defaults(settings, defaultSettings);

        this.engine = Engine.create();
        this.engine.world.gravity.y = 0;

        // Load environment objects
        this.setupEnvironment();

        // Begin update loop
        this.updateInterval = setInterval(
            function() {
                Engine.update(this.engine, 1000 / this.settings.fps);
            }.bind(this),
            1000 / this.settings.fps
        );
    }

    /**
     * Add initial objects to room
     */
    setupEnvironment() {
        const boxSize = 80;
        const groundWidth = 800;

        // Add walls
        var ground = Bodies.rectangle(groundWidth / 2 + boxSize, groundWidth, groundWidth, boxSize, { isStatic: true, label: 'ground' });
        ground.width = groundWidth;
        ground.height = boxSize;
        ground.image = 'wall';

        var ground2 = Bodies.rectangle(groundWidth / 2 + boxSize, boxSize, groundWidth, boxSize, { isStatic: true, label: 'ground2' });
        ground2.width = groundWidth;
        ground2.height = boxSize;
        ground2.image = 'wall';

        var ground3 = Bodies.rectangle(boxSize, groundWidth / 2 + boxSize / 2, boxSize, groundWidth, { isStatic: true, label: 'ground3' });
        ground3.width = boxSize;
        ground3.height = groundWidth;
        ground3.image = 'wall';

        var ground4 = Bodies.rectangle(groundWidth + boxSize, groundWidth / 2 + boxSize / 2, boxSize, groundWidth, { isStatic: true, label: 'ground4' });
        ground4.width = boxSize;
        ground4.height = groundWidth;
        ground4.image = 'wall';

        // Demo box
        var box = Bodies.rectangle(groundWidth / 2 - boxSize / 2, groundWidth / 2 - boxSize / 2, boxSize, boxSize, { label: 'box', frictionAir: 0.7 });
        box.width = boxSize;
        box.height = boxSize;
        box.image = 'box';

        World.add(this.engine.world, ground);
        World.add(this.engine.world, ground2);
        World.add(this.engine.world, ground3);
        World.add(this.engine.world, ground4);
        World.add(this.engine.world, box);
    }

    /**
     * Returns an array of the objects in the scene
     */
    getBodies(onlySleeping = true, allData = false) {
        let relevantBodies = this.engine.world.bodies.filter(body => !onlySleeping || (!body.isSleeping && !body.isStatic));

        if (allData) {
            return relevantBodies.map(body => {
                return {
                    label: body.label,
                    pos: body.position,
                    vel: body.velocity,
                    angle: body.angle,
                    anglevel: body.angularVelocity,
                    width: body.width,
                    height: body.height,
                    image: body.image
                };
            });
        } else {
            return relevantBodies.map(body => {
                // Only position/orientation for update
                return { label: body.label, pos: body.position, angle: body.angle };
            });
        }
    }

    /**
     * Add a robot to the room
     * @param {String} mac
     * @param {Matter.Vector} position
     * @returns {Robot} Robot created
     */
    addRobot(mac = null, position = null) {
        let bot = new ParallaxRobot(mac, position, this.engine);
        this.robots.push(bot);
        this.debug(`Robot ${bot.mac} added to room`);
        return bot;
    }

    /**
     * Removes robots that have not received a command recently
     * @returns {Boolean} Whether robots were removed
     */
    removeDeadRobots() {
        let deadRobots = this.robots.filter(robot => {
            return this.settings.robotKeepAliveTime > 0 && Date.now() - robot.lastCommandTime > this.settings.robotKeepAliveTime;
        });

        if (deadRobots.length > 0) {
            this.debug(
                'Dead robots: ',
                deadRobots.map(robot => robot.mac)
            );
            this.robots = _.without(this.robots, ...deadRobots);

            // Cleanup
            deadRobots.forEach(robot => {
                robot.close();
                World.remove(this.engine.world, robot.body);
            });

            return true;
        }

        return false;
    }

    /**
     * Destroy this room
     */
    close() {
        this.debug('Closing room...');

        this.robots.forEach(robot => {
            robot.close();
        });

        clearInterval(this.updateInterval);
    }
}

Room.existingIDs = [];

module.exports = Room;

const Matter = require('matter-js');
const Engine = Matter.Engine,
    World = Matter.World,
    Bodies = Matter.Bodies,
    Events = Matter.Events;
const _ = require('lodash');
const shortid = require('shortid');
const fs = require('fs');
const path = require('path');

const defaultSettings = {
    robotKeepAliveTime: 1000 * 60 * 10,
    fps: 60,
    environment: 'default'
};

Matter.Resolver._restingThresh = 0.1;

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
        this.setupEnvironment(this.settings.environment);

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
     * @param {String} environment Filename of environment to use
     */
    setupEnvironment(environment = 'default_box') {
        Room.listEnvironments().then(list => {
            // Validate that file actually exists on server as an environment
            if (list.map(env => env.file).indexOf(environment) === -1) {
                this.debug('Invalid environment requested, using default');
                environment = 'default';
            }

            // Load environment info from file
            fs.readFile(path.join(__dirname, '..', 'environments', environment + '.json'), (err, data) => {
                if (err) {
                    this.debug(`Error loading environment ${environment}`);
                    return;
                }

                let parsed = JSON.parse(data);
                this.debug(`Loading environment ${parsed.name}...`);

                for (let object of parsed.objects) {
                    var body = Bodies.rectangle(object.x, object.y, object.width, object.height, { label: object.label, isStatic: object.isStatic || false, frictionAir: object.frictionAir || 0.7 });
                    body.width = object.width;
                    body.height = object.height;
                    body.image = object.image;

                    World.add(this.engine.world, body);
                }

                // Get spawn settings
                if (parsed.robotSpawn.spawnType == 'RandomPosition') {
                    this.settings.robotSpawnType = 'RandomPosition';
                    this.settings.minX = parsed.robotSpawn.minX;
                    this.settings.maxX = parsed.robotSpawn.maxX;
                    this.settings.minY = parsed.robotSpawn.minY;
                    this.settings.maxY = parsed.robotSpawn.maxY;
                    this.settings.robotTypes = parsed.robotSpawn.robotTypes
                        .filter(type => {
                            return Object.keys(Room.robotTypes).indexOf(type) !== -1;
                        })
                        .map(type => Room.robotTypes[type]);
                }
            });

            // Setup collision events
            Events.on(this.engine, 'collisionStart', function(event) {
                var pairs = event.pairs;

                for (var i = 0; i < pairs.length; i++) {
                    var pair = pairs[i];

                    if (pair.bodyA.onCollisionStart !== undefined) {
                        pair.bodyA.onCollisionStart();
                    }

                    if (pair.bodyB.onCollisionStart !== undefined) {
                        pair.bodyB.onCollisionStart();
                    }
                }
            });

            Events.on(this.engine, 'collisionEnd', function(event) {
                var pairs = event.pairs;

                for (var i = 0; i < pairs.length; i++) {
                    var pair = pairs[i];

                    if (pair.bodyA.onCollisionEnd !== undefined) {
                        pair.bodyA.onCollisionEnd();
                    }

                    if (pair.bodyB.onCollisionEnd !== undefined) {
                        pair.bodyB.onCollisionEnd();
                    }
                }
            });
        });
    }

    /**
     * Returns an array of the objects in the scene
     */
    getBodies(onlySleeping = true, allData = false) {
        let relevantBodies = this.engine.world.bodies.filter(body => !onlySleeping || (!body.isSleeping && !body.isStatic));

        if (allData) {
            return relevantBodies.map(body => {
                let bodyInfo = {
                    label: body.label,
                    pos: body.position,
                    vel: body.velocity,
                    angle: body.angle,
                    anglevel: body.angularVelocity,
                    width: body.width,
                    height: body.height,
                    image: body.image
                };

                // Add LED status if it exists
                if(body.ledStatus !== undefined){
                    bodyInfo.ledStatus = body.ledStatus;
                }

                return bodyInfo;
            });
        } else {
            return relevantBodies.map(body => {
                // Only position/orientation for update
                let bodyInfo = { label: body.label, pos: body.position, angle: body.angle };

                // Add LED status if it exists
                if (body.ledStatus !== undefined) {
                    bodyInfo.ledStatus = body.ledStatus;
                }

                return bodyInfo;
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
        // Use loaded spawn type
        let settings = null;
        if (position === null && this.settings.robotSpawnType === 'RandomPosition') {
            settings = {
                minX: this.settings.minX,
                maxX: this.settings.maxX,
                minY: this.settings.minY,
                maxY: this.settings.maxY
            };
        }

        // Create a robot of a random allowable type
        let robotType = Math.floor(Math.random() * this.settings.robotTypes.length);
        let bot = new this.settings.robotTypes[robotType](mac, position, this.engine, settings);
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
                robot.body.parts.forEach(World.remove.bind(this, this.engine.world));
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

    /**
     * Returns an array of environments usable in Rooms
     */
    static listEnvironments() {
        return new Promise(resolve => {
            fs.readdir(path.join(__dirname, '..', 'environments'), (err, files) => {
                let environments = [];
                if (err) {
                    require('debug')('Room')('Error loading environments');
                    return;
                }

                for (let file of files) {
                    let fileData = fs.readFileSync(path.join(__dirname, '..', 'environments', file));
                    let parsed = JSON.parse(fileData);
                    environments.push({
                        file: path.basename(file, '.json'),
                        name: parsed.name
                    });
                }

                resolve(environments);
            });
        });
    }
}

Room.existingIDs = [];

/**
 * List of available robot types
 */
Room.robotTypes = {
    ParallaxRobot: require('./robots/ParallaxRobot'),
    OmniRobot: require('./robots/OmniRobot')
};

module.exports = Room;

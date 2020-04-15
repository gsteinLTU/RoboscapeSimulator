const OmniRobot = require('./OmniRobot');
const Matter = require('matter-js');
const Engine = Matter.Engine;

const { expectCommandHandlers } = require('../util');

describe('OmniRobot tests', () => {
    let engine;
    let testRobot;
    beforeAll(() => {
        engine = engine = Engine.create();
        testRobot = new OmniRobot(null, null, engine);
    });

    test('Has all expected command handlers', () => {
        expectCommandHandlers(['S'], testRobot);
    });
});

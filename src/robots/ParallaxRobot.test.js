const ParallaxRobot = require('./ParallaxRobot');
const Matter = require('matter-js');
const Engine = Matter.Engine;

const { expectCommandHandlers } = require('../util');

describe('ParallaxRobot tests', () => {
    let engine;
    let testRobot;
    beforeAll(() => {
        engine = engine = Engine.create();
        testRobot = new ParallaxRobot(null, null, engine);
    });

    test('Has all expected command handlers', () => {
        expectCommandHandlers(['S','L','R','T'], testRobot);
    });

    test('Can set LEDs', () => {
        testRobot.commandHandlers['L'](Buffer.of('L', 0, 1));
        testRobot.commandHandlers['L'](Buffer.of('L', 1, 1));
        expect(testRobot.body.ledStatus[0]).toEqual(1);
        expect(testRobot.body.ledStatus[1]).toEqual(1);
        testRobot.commandHandlers['L'](Buffer.of('L', 0, 0));
        testRobot.commandHandlers['L'](Buffer.of('L', 1, 0));
        expect(testRobot.body.ledStatus[0]).toEqual(0);
        expect(testRobot.body.ledStatus[1]).toEqual(0);
    });
});

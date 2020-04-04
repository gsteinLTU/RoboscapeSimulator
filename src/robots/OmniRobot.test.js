const OmniRobot = require('./OmniRobot');
const Matter = require('matter-js');
const Engine = Matter.Engine;

describe('OmniRobot tests', () => {
    let engine;
    let testRobot;
    beforeAll(() => {
        engine = engine = Engine.create();
        testRobot = new OmniRobot(null, null, engine);
    });

    test('Has all expected command handlers', () => {
        expect(testRobot.commandHandlers).toMatchObject(expect.objectContaining({
            'S': expect.any(Function)
        }));
    });
});

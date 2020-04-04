const Robot = require('./Robot');
const Matter = require('matter-js');
const Engine = Matter.Engine;

describe('Robot tests', () => {
    let engine;
    beforeAll(() => {
        engine = engine = Engine.create();
    });

    test('random MAC should be used if none provided', () => {
        let testRobot = new Robot(null, null, engine);
        expect(testRobot.mac).toEqual(expect.stringMatching(/^([0-9a-fA-F]{2}:){5}[0-9a-fA-F]{2}$/));
    });

    test('Correct MAC should be used if provided', () => {
        let testRobot = new Robot('aa:aa:aa:aa:aa:aa', null, engine);
        expect(testRobot.mac).toEqual('aa:aa:aa:aa:aa:aa');
    });

    test('Correct position should be used if provided', () => {
        let randX = Math.random() * 1000;
        let randY = Math.random() * 1000;

        let testRobot = new Robot(null, { x: randX, y: randY }, engine);
        expect(testRobot.body.position.x).toBeCloseTo(randX);
        expect(testRobot.body.position.y).toBeCloseTo(randY);
    });

    test('Has all expected command handlers', () => {
        let testRobot = new Robot(null, null, engine);
        expect(testRobot.commandHandlers).toMatchObject(expect.objectContaining({
            'S': expect.any(Function)
        }));
    });
});

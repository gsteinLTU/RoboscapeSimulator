import createRobotFactory from './createRobotFactory';
import createObstacleFactory from './createObstacleFactory';
import createGroundFactory from './createGroundFactory';

export default ({ simulator /* inject depenencies here */ }) => {
    return {
        'Robot': createRobotFactory({ simulator /* inject depenencies here */ }),
        'Obstacle': createObstacleFactory({ simulator /* inject depenencies here */}),
        'Ground': createGroundFactory({ simulator /* inject depenencies here */})
    };
};

import nengi from 'nengi';
import Robot from './entity/Robot';
import Identity from './message/Identity';
import Obstacle from './entity/Obstacle';
import Ground from './entity/Ground';

const config = {
    UPDATE_RATE: 15, 

    ID_BINARY_TYPE: nengi.UInt16,
    TYPE_BINARY_TYPE: nengi.UInt8, 

    ID_PROPERTY_NAME: 'nid',
    TYPE_PROPERTY_NAME: 'ntype', 

    USE_HISTORIAN: true,
    HISTORIAN_TICKS: 40,

    DIMENSIONALITY: 3,

    protocols: {
        entities: [
            ['Robot', Robot],
            ['Obstacle', Obstacle],
            ['Ground', Ground]
        ],
        localMessages: [],
        messages: [
            ['Identity', Identity],
        ],
        commands: [
        ],
        basics: []
    }
};

export default config;
import BABYLONRenderer from './graphics/BABYLONRenderer';

// ignoring certain data from the sever b/c we will be predicting these properties on the client
const ignoreProps = ['x', 'y', 'z'];
const shouldIgnore = (myId, update) => {
    if (update.nid === myId) {
        if (ignoreProps.indexOf(update.prop) !== -1) {
            return true;
        }
    }
    return false;
};

class Simulator {
    constructor(client) {
        this.client = client;
        this.renderer = new BABYLONRenderer();
        this.obstacles = new Map();

        this.myRawId = -1;
        this.mySmoothId = -1;

        this.myRawEntity = null;
        this.mySmoothEntity = null;

        client.entityUpdateFilter = (update) => {
            return shouldIgnore(this.myRawId, update);
        };

        client.on('message::Identity', message => {
            // these are the ids of our two entities.. we just store them here on simulator until
            // we receive these entities over the network (see: createPlayerFactory)
            this.myRawId = message.rawId;
            this.mySmoothId = message.smoothId;
            console.log('identified as', message);
        });
    }

    update(delta) {
        this.renderer.update();
    }
}

export default Simulator;

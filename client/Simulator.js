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

        client.entityUpdateFilter = (update) => {
            return shouldIgnore(this.myRawId, update);
        };

        // client.on('message::Identity', message => {

        // });
    }

    update(delta) {
        this.renderer.update();
    }
}

export default Simulator;

export default ({simulator}/* inject depenencies here */) => {
    return {
        create({ data, entity }) {
            entity.mesh.receiveShadows = true;
        },
        delete({ nid, entity }) {
            entity.mesh.dispose();
        }
    };
};

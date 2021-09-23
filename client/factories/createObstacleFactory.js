export default ({simulator}/* inject depenencies here */) => {
    return {
        create({ data, entity }) {
            simulator.renderer.shadowGenerator.addShadowCaster(entity.mesh, true);
        },
        delete({ nid, entity }) {
            entity.mesh.dispose();
        }
    };
};

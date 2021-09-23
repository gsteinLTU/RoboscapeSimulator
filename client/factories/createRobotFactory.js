export default ({simulator}) => {
    return {
        create({ data, entity }) {
            simulator.renderer.shadowGenerator.addShadowCaster(entity.mesh, true);
        },
        delete({ nid, entity }) {
            entity.mesh.dispose();
        }
    };
};

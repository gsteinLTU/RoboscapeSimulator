using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace RoboScapeSimulator.Entities
{
    /// <summary>
    /// A box-shaped object
    /// </summary>
    class Cube : DynamicEntity
    {
        private static uint ID = 0;

        /// <summary>
        /// Create a new Cube
        /// </summary>
        /// <param name="room">Room to put this Cube into</param>
        /// <param name="width">width (x-axis)</param>
        /// <param name="height">height (y-axis)</param>
        /// <param name="depth">depth (z-axis)</param>
        /// <param name="initialPosition">Initial location of the Cube, or null for a random location</param>
        /// <param name="initialOrientation">Initial orientation of the Cube, or null for a random yaw</param>
        /// <param name="isKinematic">Whether this object should be movable</param>
        /// <param name="visualInfo">Visual description string for the Cube</param>
        public Cube(Room room, float width = 1, float height = 1, float depth = 1, Vector3? initialPosition = null, Quaternion? initialOrientation = null, bool isKinematic = false, VisualInfo visualInfo = default, string? nameOverride = null)
        {
            if (nameOverride == null)
            {
                Name = $"cube_{ID++}";
            }
            else
            {
                Name = $"{nameOverride}_{ID++}";
            }

            VisualInfo = visualInfo;

            Width = width;
            Height = height;
            Depth = depth;

            var simulationInstance = room.SimInstance;
            var rng = new Random();
            var box = new Box(width, height, depth);
            var boxInertia = box.ComputeInertia(2);

            BodyHandle bodyHandle;
            Vector3 position = initialPosition ?? new Vector3(rng.Next(-5, 5), rng.Next(3, 5), rng.Next(-5, 5));
            if (isKinematic)
            {
                bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateKinematic(position, new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0.01f)));
            }
            else
            {
                bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateDynamic(position, boxInertia, new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0.01f)));
            }

            BodyReference = simulationInstance.Simulation.Bodies.GetBodyReference(bodyHandle);
            BodyReference.Pose.Orientation = initialOrientation ?? Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)rng.NextDouble() * MathF.PI);

            ref var bodyProperties = ref simulationInstance.Properties.Allocate(BodyReference.Handle);
            bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(BodyReference.Handle.Value, 0) };

            room.SimInstance.NamedBodies.Add(Name, BodyReference);
            room.SimInstance.Entities.Add(this);
        }
    }
}
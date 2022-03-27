using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace RoboScapeSimulator.Entities
{
    /// <summary>
    /// A box-shaped object
    /// </summary>
    class Cube : DynamicEntity, IResettable
    {
        private static uint ID = 0;

        /// <summary>
        /// When a scene reset is requested by the user, should this Cube reset
        /// </summary>
        public bool AllowReset = true;

        private Vector3 initialPosition = new();
        private Quaternion initialOrientation = Quaternion.Identity;

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
        public Cube(Room room, float width = 1, float height = 1, float depth = 1, Vector3? initialPosition = null, Quaternion? initialOrientation = null, bool isKinematic = false, VisualInfo visualInfo = default, string? nameOverride = null, float mass = 2, bool allowReset = true)
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

            AllowReset = allowReset;

            var simulationInstance = room.SimInstance;
            var rng = new Random();
            var box = new Box(width, height, depth);
            var boxInertia = box.ComputeInertia(mass);

            BodyHandle bodyHandle;
            Vector3 position = initialPosition ?? new Vector3(rng.Next(-5, 5), rng.Next(3, 5), rng.Next(-5, 5));
            RigidPose pose = new(position, initialOrientation ?? Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)rng.NextDouble() * MathF.PI));

            if (isKinematic)
            {
                bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0.01f)));
            }
            else
            {
                bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, boxInertia, new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(0)));
            }

            BodyReference = simulationInstance.Simulation.Bodies.GetBodyReference(bodyHandle);

            ref var bodyProperties = ref simulationInstance.Properties.Allocate(BodyReference.Handle);
            bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(BodyReference.Handle.Value, 0) };

            this.initialPosition = position;
            this.initialOrientation = BodyReference.Pose.Orientation;

            room.SimInstance.NamedBodies.Add(Name, BodyReference);
            room.SimInstance.Entities.Add(this);
        }

        public void Reset()
        {
            if (AllowReset)
            {
                Trace.WriteLine("Cube reset " + Name + " to " + initialPosition);
                BodyReference.Pose.Position = initialPosition;
                BodyReference.Pose.Orientation = initialOrientation;
                BodyReference.Velocity.Linear = new Vector3();
                BodyReference.Velocity.Angular = new Vector3();
            }
        }
    }
}
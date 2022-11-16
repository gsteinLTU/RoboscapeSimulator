using System.Diagnostics;
using System.Numerics;

namespace RoboScapeSimulator.Physics.Null
{
    /// <summary>
    /// SimulationInstance subclass for using no actual physics engine.
    /// Bodies have position and velocity, but no collisions outside of contraining them to optional boundaries.
    /// </summary>
    public class NullSimulationInstance : SimulationInstance
    {
        public Dictionary<string, SimBodyNull> Bodies = new();
        public Dictionary<string, SimStaticNull> StaticBodies = new();

        public Vector3 MaxBoundary = new(50,float.PositiveInfinity,50);

        public Vector3 MinBoundary = new(-50,0,-50);

        public Vector3 Gravity = Vector3.Zero;

        public override SimBody CreateBox(string name, Vector3 position, Quaternion? orientation = null, float width = 1, float height = 1, float depth = 1, float mass = 1, bool isKinematic = false)
        {
            Bodies.Add(name, new SimBodyNull(){
                position = position,
                orientation = orientation ?? Quaternion.Identity,
                angularVel = new(),
                linearVel = new(),
                mass = mass,
                size = new(width, height, depth),
                isKinematic = isKinematic
            });
            return Bodies[name];
        }

        public override SimStatic CreateStaticBox(string name, Vector3 position, Quaternion? orientation = null, float width = 100, float height = 100, float depth = 1)
        {
            StaticBodies.Add(name, new SimStaticNull(){
                position = position,
                orientation = orientation ?? Quaternion.Identity,
                size = new(width, height, depth)
            });
            return StaticBodies[name];
        } 

        /// <summary>
        /// Update the simulation
        /// </summary>
        /// <param name="dt">Delta time in s</param>
        public override void Update(float dt)
        {
            if (dt <= float.Epsilon)
                return;

            // Update positions
            foreach (var body in Bodies.Values)
            {
                if(body.isKinematic){
                    continue;
                }

                body.LinearVelocity += dt * Gravity;

                body.Position += dt * body.LinearVelocity;
                body.Orientation.ExtractYawPitchRoll(out var yaw, out var pitch, out var roll);
                body.Orientation = Quaternion.CreateFromYawPitchRoll(yaw + dt * body.AngularVelocity.Y,pitch + dt * body.AngularVelocity.X, roll + dt * body.AngularVelocity.Z);
                
                // Keep in bounds
                foreach (var corner in body.GetCorners())
                {
                    // Test each corner
                    if(!corner.Inside(MinBoundary, MaxBoundary)){
                        Vector3 delta = corner.Clamp(MinBoundary, MaxBoundary) - corner;
                        Debug.WriteLine($"Corner at {corner} is outside, moving position by {delta}");
                        
                        // Move corner back inside
                        body.Position += delta;
                        body.LinearVelocity += 1f / dt * delta;
                    }
                }
            }

            base.Update(dt);
        }
    }

    public class SimBodyNull : SimBody
    {
        internal Vector3 position;
        internal Quaternion orientation;
        internal Vector3 size;
        internal Vector3 linearVel;
        internal Vector3 angularVel;
        internal float mass;
        internal bool isKinematic = false;

        public override Vector3 Position { get => position; set => position = value; }
        public override Quaternion Orientation { get => orientation; set => orientation = value; }
        public override Vector3 LinearVelocity { get => linearVel; set => linearVel = value; }
        public override Vector3 AngularVelocity { get => angularVel; set => angularVel = value; }
        public override bool Awake { get => true; set {} }
        public override float Mass => mass;
        public override Vector3 Size => size;

        public override void ApplyForce(Vector3 force)
        {
            if(Mass > 0){
                LinearVelocity += force / Mass;
            }
        }

        public IEnumerable<Vector3> GetCorners(bool oriented = true){
            List<Vector3> corners = new(){
                new Vector3(Size.X / 2f, Size.Y / 2f, Size.Z / 2f),
                new Vector3(Size.X / 2f, Size.Y / 2f, -Size.Z / 2f),
                new Vector3(Size.X / 2f, -Size.Y / 2f, Size.Z / 2f),
                new Vector3(Size.X / 2f, -Size.Y / 2f, -Size.Z / 2f),
                new Vector3(-Size.X / 2f, Size.Y / 2f, Size.Z / 2f),
                new Vector3(-Size.X / 2f, Size.Y / 2f, -Size.Z / 2f),
                new Vector3(-Size.X / 2f, -Size.Y / 2f, Size.Z / 2f),
                new Vector3(-Size.X / 2f, -Size.Y / 2f, -Size.Z / 2f),
            };

            if(!oriented){
                return corners.Select(corner => corner + Position);
            }

            return corners.Select(corner => Vector3.Transform(corner, Orientation) + Position);
        }
    }

    public class SimStaticNull : SimStatic
    {
        internal Vector3 position;
        internal Quaternion orientation;
        internal Vector3 size;

        public override Vector3 Position { get => position; set => position = value; }
        public override Quaternion Orientation { get => orientation; set => orientation = value; }
        public override Vector3 Size => size;
    }
}
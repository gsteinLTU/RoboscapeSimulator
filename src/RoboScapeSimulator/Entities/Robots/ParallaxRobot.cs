using System;
using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

namespace RoboScapeSimulator.Entities.Robots
{
    /// <summary>
    /// Robot subtype based on the Parallax ActivityBot 360 used in physical classrooms
    /// </summary> 
    public class ParallaxRobot : Robot
    {
        public BodyHandle LWheel;
        public ConstraintHandle LMotor;
        public ConstraintHandle LHinge;
        public BodyHandle RWheel;
        public ConstraintHandle RMotor;
        public ConstraintHandle RHinge;
        public BodyHandle RearWheel;
        private RigidPose rWheelPose;
        private RigidPose lWheelPose;
        private RigidPose rearWheelPose;

        private float leftSpeed = 0;
        private float rightSpeed = 0;
        private float leftDistance = 0;
        private float rightDistance = 0;

        /// <summary>
        /// Encoder value for left wheel
        /// </summary>
        public double LeftTicks = 0;

        /// <summary>
        /// Encoder value for right wheel
        /// </summary>
        public double RightTicks = 0;

        /// <summary>
        /// Modes the robot can be in:
        /// SetSpeed from the "set speed" command (default),
        /// SetDistance from the "drive" command,
        /// OverrideSpeed for overriding any speed/drive commands from the NetsBlox server
        /// </summary>
        private enum DriveState
        {
            SetSpeed, SetDistance
        }

        /// <summary>
        /// The current DriveState of the robot
        /// </summary>
        private DriveState driveState = DriveState.SetSpeed;

        private bool whiskerL = false;

        private bool whiskerR = false;

        public ParallaxRobot(Room room, in Vector3? position = null, in Quaternion? rotation = null, bool debug = false, in VisualInfo? visualInfo = null, float spawnHeight = 0.4f) : base(room, position, rotation, visualInfo: visualInfo, spawnHeight: spawnHeight)
        {
            CreateHandlers();

            var wheelShape = new Cylinder(0.03f, .01f);
            var wheelInertia = wheelShape.ComputeInertia(0.25f);
            var wheelShapeIndex = simulation.Shapes.Add(wheelShape);

            var rearWheelShape = new Sphere(0.01f);
            _ = rearWheelShape.ComputeInertia(0.25f);
            var rearWheelShapeIndex = simulation.Shapes.Add(rearWheelShape);

            float wheelDistX = 0.05f;
            float wheelDistY = 0.003f;
            float wheelDistZ = 0.025f;

            var lWheelOffset = new RigidPose(new Vector3(-wheelDistX, wheelDistY, 0.05f), QuaternionEx.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f));
            RigidPose.MultiplyWithoutOverlap(lWheelOffset, BodyReference.Pose, out lWheelPose);

            LWheel = simulation.Bodies.Add(BodyDescription.CreateDynamic(
                lWheelPose,
                wheelInertia, new CollidableDescription(wheelShapeIndex, 0.1f), new BodyActivityDescription(0.01f)));

            LMotor = simulation.Solver.Add(LWheel, BodyReference.Handle, new AngularAxisMotor
            {
                LocalAxisA = new Vector3(0, 1, 0),
                Settings = default,
                TargetVelocity = default
            });

            LHinge = simulation.Solver.Add(BodyReference.Handle, LWheel, new AngularHinge
            {
                LocalHingeAxisA = new Vector3(1, 0, 0),
                LocalHingeAxisB = new Vector3(0, 1, 0),
                SpringSettings = new SpringSettings(30, 1)
            });

            simulation.Solver.Add(BodyReference.Handle, LWheel, new LinearAxisServo
            {
                LocalPlaneNormal = new Vector3(0, -1, 0),
                TargetOffset = 0.05f,
                LocalOffsetA = new Vector3(-wheelDistX, wheelDistY, wheelDistZ),
                LocalOffsetB = default,
                ServoSettings = ServoSettings.Default,
                SpringSettings = new SpringSettings(30, 1)
            });

            simulation.Solver.Add(BodyReference.Handle, LWheel, new PointOnLineServo
            {
                LocalDirection = new Vector3(0, -1, 0),
                LocalOffsetA = new Vector3(-wheelDistX, wheelDistY, wheelDistZ),
                LocalOffsetB = default,
                ServoSettings = ServoSettings.Default,
                SpringSettings = new SpringSettings(30, 1)
            });

            var rWheelOffset = new RigidPose(new Vector3(wheelDistX, wheelDistY, 0.05f), QuaternionEx.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f));

            RigidPose.MultiplyWithoutOverlap(rWheelOffset, BodyReference.Pose, out rWheelPose);

            RWheel = simulation.Bodies.Add(BodyDescription.CreateDynamic(
                rWheelPose,
                wheelInertia, new CollidableDescription(wheelShapeIndex, 0.1f), new BodyActivityDescription(0.01f)));

            RMotor = simulation.Solver.Add(RWheel, BodyReference.Handle, new AngularAxisMotor
            {
                LocalAxisA = new Vector3(0, -1, 0),
                Settings = default,
                TargetVelocity = default
            });

            RHinge = simulation.Solver.Add(BodyReference.Handle, RWheel, new AngularHinge
            {
                LocalHingeAxisA = new Vector3(-1, 0, 0),
                LocalHingeAxisB = new Vector3(0, 1, 0),
                SpringSettings = new SpringSettings(30, 1)
            });


            simulation.Solver.Add(BodyReference.Handle, RWheel, new LinearAxisServo
            {
                LocalPlaneNormal = new Vector3(0, -1, 0),
                TargetOffset = 0.05f,
                LocalOffsetA = new Vector3(wheelDistX, wheelDistY, wheelDistZ),
                LocalOffsetB = default,
                ServoSettings = ServoSettings.Default,
                SpringSettings = new SpringSettings(30, 1)
            });


            simulation.Solver.Add(BodyReference.Handle, RWheel, new PointOnLineServo
            {
                LocalDirection = new Vector3(0, -1, 0),
                LocalOffsetA = new Vector3(wheelDistX, wheelDistY, wheelDistZ),
                LocalOffsetB = default,
                ServoSettings = ServoSettings.Default,
                SpringSettings = new SpringSettings(30, 1)
            });

            var rearWheelOffset = new RigidPose(new Vector3(0, -0.03f, -0.03f));
            RigidPose.MultiplyWithoutOverlap(rearWheelOffset, BodyReference.Pose, out rearWheelPose);

            RearWheel = simulation.Bodies.Add(BodyDescription.CreateDynamic(
                rearWheelPose,
                wheelInertia, new CollidableDescription(rearWheelShapeIndex, 0.1f), new BodyActivityDescription(0.01f)));

            simulation.Solver.Add(BodyReference.Handle, RearWheel, new BallSocket
            {
                LocalOffsetA = new Vector3(0, -0.065f, -0.08f),
                LocalOffsetB = new Vector3(0, 0, 0),
                SpringSettings = new SpringSettings(60, 1)
            });

            // Setup collisions
            ref var bodyProperties = ref room.SimInstance.Properties.Allocate(BodyReference.Handle);
            bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(BodyReference.Handle.Value, 1) };

            ref var lwheelProperties = ref room.SimInstance.Properties.Allocate(LWheel);
            lwheelProperties = new BodyCollisionProperties { Filter = new SubgroupCollisionFilter(BodyReference.Handle.Value, 1), Friction = 1 };

            ref var rwheelProperties = ref room.SimInstance.Properties.Allocate(RWheel);
            rwheelProperties = new BodyCollisionProperties { Filter = new SubgroupCollisionFilter(BodyReference.Handle.Value, 1), Friction = 1 };

            ref var rearWheelProperties = ref room.SimInstance.Properties.Allocate(RearWheel);
            rearWheelProperties = new BodyCollisionProperties { Filter = new SubgroupCollisionFilter(BodyReference.Handle.Value, 1), Friction = 0.5f };

            SubgroupCollisionFilter.DisableCollision(ref lwheelProperties.Filter, ref bodyProperties.Filter);
            SubgroupCollisionFilter.DisableCollision(ref rwheelProperties.Filter, ref bodyProperties.Filter);
            SubgroupCollisionFilter.DisableCollision(ref rearWheelProperties.Filter, ref bodyProperties.Filter);
            SubgroupCollisionFilter.DisableCollision(ref rearWheelProperties.Filter, ref rwheelProperties.Filter);
            SubgroupCollisionFilter.DisableCollision(ref rearWheelProperties.Filter, ref lwheelProperties.Filter);
            SubgroupCollisionFilter.DisableCollision(ref rwheelProperties.Filter, ref lwheelProperties.Filter);

            if (debug)
            {
                room.SimInstance.NamedBodies.Add("wheelL", room.SimInstance.Simulation.Bodies.GetBodyReference(LWheel));
                room.SimInstance.NamedBodies.Add("wheelR", room.SimInstance.Simulation.Bodies.GetBodyReference(RWheel));
                room.SimInstance.NamedBodies.Add("wheelRear", room.SimInstance.Simulation.Bodies.GetBodyReference(RearWheel));
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            // Apply speed to wheels
            simulation.Solver.ApplyDescription(LMotor, new AngularAxisMotor
            {
                LocalAxisA = new Vector3(0, 1, 0),
                Settings = new MotorSettings(3, 1e-6f),
                TargetVelocity = MathF.PI * 2 * leftSpeed / 120
            });
            simulation.Solver.ApplyDescription(RMotor, new AngularAxisMotor
            {
                LocalAxisA = new Vector3(0, -1, 0),
                Settings = new MotorSettings(3, 1e-6f),
                TargetVelocity = MathF.PI * 2 * rightSpeed / 120
            });
            //rightWheelMotor.targetVelocity = 360 * rightSpeed / 60;

            LeftTicks += leftSpeed * dt;
            RightTicks += rightSpeed * dt;

            // Keep track of ticks for set distance
            if (driveState == DriveState.SetDistance)
            {
                leftDistance -= Math.Abs(leftSpeed * dt);
                rightDistance -= Math.Abs(rightSpeed * dt);

                if (leftDistance <= 0 + double.Epsilon)
                {
                    leftSpeed = 0;
                }

                if (rightDistance <= 0 + double.Epsilon)
                {
                    rightSpeed = 0;
                }
            }

            unsafe
            {
                // Do whisker tests
                const int whiskerRange = 11;
                var whiskerTestL = Utils.QuickRayCast(simulation, BodyReference.Pose.Position + Vector3.Transform(new Vector3(-0.05f, 0.05f, 0.15f), BodyReference.Pose.Orientation),
                               Vector3.Transform(new Vector3(0, 0, 1), BodyReference.Pose.Orientation), whiskerRange);
                var whiskerTestR = Utils.QuickRayCast(simulation, BodyReference.Pose.Position + Vector3.Transform(new Vector3(0.05f, 0.05f, 0.15f), BodyReference.Pose.Orientation),
                               Vector3.Transform(new Vector3(0, 0, 1), BodyReference.Pose.Orientation), whiskerRange);

                if (whiskerTestL != whiskerL || whiskerTestR != whiskerR)
                {
                    whiskerL = whiskerTestL;
                    whiskerR = whiskerTestR;

                    // Send update to server
                    byte[] messageBytes = new byte[2];
                    messageBytes[0] = (byte)'W';
                    messageBytes[1] = (byte)((whiskerR ? 0 : 1) | ((byte)(whiskerL ? 0 : 1) << 1));

                    SendRoboScapeMessage(messageBytes);
                }
            }
        }


        /// <summary>
        /// Setup command handlers and whisker event handlers
        /// </summary>
        private void CreateHandlers()
        {
            // Add command handlers
            AddHandler('D', OnDrive);
            AddHandler('S', OnSetSpeed);
            AddHandler('B', OnBeep);
            AddHandler('L', OnSetLED);
            AddHandler('R', OnGetRange);
            AddHandler('T', OnGetTicks);
        }

        #region Command Handlers

        public void OnDrive(byte[] msg)
        {
            if (msg.Length < 5)
            {
                Debug.WriteLine("Drive message too short!");
                return;
            }

            var left = BitConverter.ToInt16(msg, 1);
            var right = BitConverter.ToInt16(msg, 3);

            leftSpeed = 75 * Math.Sign(left);
            rightSpeed = 75 * Math.Sign(right);

            leftDistance = Math.Abs(left);
            rightDistance = Math.Abs(right);

            driveState = DriveState.SetDistance;

            Debug.WriteLine($"Drive {left} {right}");
        }

        public void OnSetSpeed(byte[] msg)
        {
            if (msg.Length < 5)
            {
                Debug.WriteLine("Set Speed message too short!");
                return;
            }

            leftSpeed = BitConverter.ToInt16(msg, 1);
            rightSpeed = BitConverter.ToInt16(msg, 3);

            driveState = DriveState.SetSpeed;

            Debug.WriteLine($"Set Speed {leftSpeed} {rightSpeed}");
        }

        public void OnBeep(byte[] msg)
        {
            if (msg.Length < 5)
            {
                Debug.WriteLine("Beep message too short!");
                return;
            }

            var duration = BitConverter.ToInt16(msg, 1);
            var tone = BitConverter.ToInt16(msg, 3);

            // TODO: Send beep to client
            room.SendToClients("beep", new BeepData() { Robot = BytesToHexstring(MacAddress, ""), Duration = duration, Frequency = tone });

            Debug.WriteLine($"Beep {duration} {tone}");
        }

        public void OnSetLED(byte[] msg)
        {
            if (msg.Length < 3)
            {
                Debug.WriteLine("Set LED message too short!");
                return;
            }

            // Determine which LED was requested
            var which = msg[1];
            var status = msg[2];

            Debug.WriteLine($"Set LED {which} {status}");
            room.SendToClients("led", Name, which, status);
        }

        unsafe public void OnGetRange(byte[] msg)
        {
            const short MAX_RANGE = 300;
            short distance = MAX_RANGE;

            int intersectionCount = 0;
            simulation.BufferPool.Take(1, out Buffer<RayHit> results);
            HitHandler hitHandler = new()
            {
                Hits = results,
                IntersectionCount = &intersectionCount
            };
            simulation.RayCast(BodyReference.Pose.Position + Vector3.Transform(new Vector3(0, 0.05f, 0.15f), BodyReference.Pose.Orientation),
                               Vector3.Transform(new Vector3(0, 0, 1), BodyReference.Pose.Orientation),
                               (float)MAX_RANGE / 100f, ref hitHandler);

            if (intersectionCount > 0)
            {
                distance = Math.Min((short)(hitHandler.Hits[0].T * 100), MAX_RANGE);
            }

            // Create response message
            byte[] messageBytes = new byte[3];
            messageBytes[0] = (byte)'R';
            BitConverter.GetBytes(distance).CopyTo(messageBytes, 1);

            SendRoboScapeMessage(messageBytes);

            Debug.WriteLine($"Get Range {distance}");
        }

        public void OnGetTicks(byte[] msg)
        {
            // Create response message
            byte[] messageBytes = new byte[9];
            messageBytes[0] = (byte)'T';
            BitConverter.GetBytes((int)LeftTicks).CopyTo(messageBytes, 1);
            BitConverter.GetBytes((int)RightTicks).CopyTo(messageBytes, 5);

            SendRoboScapeMessage(messageBytes);

            Debug.WriteLine($"Get Ticks {LeftTicks} {RightTicks}");
        }

        #endregion

        public void OnButtonPress(bool status)
        {
            // Create response message
            byte[] messageBytes = new byte[2];
            messageBytes[0] = (byte)'P';
            messageBytes[1] = (byte)(status ? 0 : 1);

            SendRoboScapeMessage(messageBytes);
            Debug.WriteLine($"Button Sent");
        }

        /// <summary>
        /// Set requested speed values to zero
        /// </summary>
        public void ResetSpeed()
        {
            leftSpeed = 0;
            rightSpeed = 0;
            driveState = DriveState.SetSpeed;
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Reset the Robot's position and movement
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            ResetSpeed();
            LeftTicks = 0;
            RightTicks = 0;
            leftDistance = 0;
            rightDistance = 0;

            // simulation.Solver.ApplyDescription(LMotor, new AngularAxisMotor
            // {
            //     LocalAxisA = new Vector3(0, 1, 0),
            //     Settings = new MotorSettings(3, 1e-6f),
            //     TargetVelocity = 0
            // });
            // simulation.Solver.ApplyDescription(RMotor, new AngularAxisMotor
            // {
            //     LocalAxisA = new Vector3(0, -1, 0),
            //     Settings = new MotorSettings(3, 1e-6f),
            //     TargetVelocity = 0
            // });

            // Reset wheels as well
            BodyReference rWheelBody = simulation.Bodies.GetBodyReference(RWheel);
            rWheelBody.Pose.Position = rWheelPose.Position;
            rWheelBody.Pose.Orientation = rWheelPose.Orientation;
            rWheelBody.Velocity.Linear = new Vector3();
            rWheelBody.Velocity.Angular = new Vector3();

            BodyReference lWheelBody = simulation.Bodies.GetBodyReference(LWheel);
            lWheelBody.Pose.Position = lWheelPose.Position;
            lWheelBody.Pose.Orientation = lWheelPose.Orientation;
            lWheelBody.Velocity.Linear = new Vector3();
            lWheelBody.Velocity.Angular = new Vector3();

            BodyReference rearWheelBody = simulation.Bodies.GetBodyReference(RearWheel);
            rearWheelBody.Pose.Position = rearWheelPose.Position;
            rearWheelBody.Pose.Orientation = rearWheelPose.Orientation;
            rearWheelBody.Velocity.Linear = new Vector3();
            rearWheelBody.Velocity.Angular = new Vector3();
        }

        /// <summary>
        /// Information for a beep message
        /// </summary>
        internal struct BeepData
        {
            public string Robot;
            public short Duration;
            public short Frequency;
        }
    }
}
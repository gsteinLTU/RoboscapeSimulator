using System;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;

/// <summary>
/// Robot subtype based on the Parallax ActivityBot 360 used in physical classrooms
/// </summary>
class ParallaxRobot : Robot
{
    public BodyHandle LWheel;
    public ConstraintHandle LMotor;
    public ConstraintHandle LHinge;
    public BodyHandle RWheel;
    public ConstraintHandle RMotor;
    public ConstraintHandle RHinge;
    public BodyHandle RearWheel;

    private float leftSpeed = 0;
    private float rightSpeed = 0;
    private float leftDistance = 0;
    private float rightDistance = 0;

    /// <summary>
    /// Encoder value for left wheel
    /// </summary>
    private double leftTicks = 0;

    /// <summary>
    /// Encoder value for right wheel
    /// </summary>
    private double rightTicks = 0;

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

    public ParallaxRobot(Room room) : base(room)
    {
        CreateHandlers();

        var wheelShape = new Cylinder(0.12f, .025f);
        wheelShape.ComputeInertia(0.25f, out var wheelInertia);
        var wheelShapeIndex = simulation.Shapes.Add(wheelShape);

        var rearWheelShape = new Sphere(0.05f);
        rearWheelShape.ComputeInertia(0.25f, out var rearWheelInertia);
        var rearWheelShapeIndex = simulation.Shapes.Add(rearWheelShape);

        var lWheelOffset = new RigidPose(new Vector3(-0.25f, -0.3f, 0.05f), QuaternionEx.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f));
        RigidPose.MultiplyWithoutOverlap(lWheelOffset, bodyReference.Pose, out var lWheelPose);

        LWheel = simulation.Bodies.Add(BodyDescription.CreateDynamic(
            lWheelPose,
            wheelInertia, new CollidableDescription(wheelShapeIndex, 0.1f), new BodyActivityDescription(0.01f)));

        LMotor = simulation.Solver.Add(LWheel, bodyReference.Handle, new AngularAxisMotor
        {
            LocalAxisA = new Vector3(0, 1, 0),
            Settings = default,
            TargetVelocity = default
        });

        LHinge = simulation.Solver.Add(bodyReference.Handle, LWheel, new AngularHinge
        {
            LocalHingeAxisA = new Vector3(1, 0, 0),
            LocalHingeAxisB = new Vector3(0, 1, 0),
            SpringSettings = new SpringSettings(30, 1)
        });

        simulation.Solver.Add(bodyReference.Handle, LWheel, new LinearAxisServo
        {
            LocalPlaneNormal = new Vector3(0, -1, 0),
            TargetOffset = 0.05f,
            LocalOffsetA = new Vector3(-0.1f, -0.0f, 0.05f),
            LocalOffsetB = default,
            ServoSettings = ServoSettings.Default,
            SpringSettings = new SpringSettings(5, 1)
        });

        simulation.Solver.Add(bodyReference.Handle, LWheel, new PointOnLineServo
        {
            LocalDirection = new Vector3(0, -1, 0),
            LocalOffsetA = new Vector3(-0.1f, -0.0f, 0.05f),
            LocalOffsetB = default,
            ServoSettings = ServoSettings.Default,
            SpringSettings = new SpringSettings(30, 1)
        });

        var rWheelOffset = new RigidPose(new Vector3(0.25f, -0.3f, 0.05f), QuaternionEx.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f));
        RigidPose.MultiplyWithoutOverlap(rWheelOffset, bodyReference.Pose, out var rWheelPose);

        RWheel = simulation.Bodies.Add(BodyDescription.CreateDynamic(
            rWheelPose,
            wheelInertia, new CollidableDescription(wheelShapeIndex, 0.1f), new BodyActivityDescription(0.01f)));

        RMotor = simulation.Solver.Add(RWheel, bodyReference.Handle, new AngularAxisMotor
        {
            LocalAxisA = new Vector3(0, -1, 0),
            Settings = default,
            TargetVelocity = default
        });

        RHinge = simulation.Solver.Add(bodyReference.Handle, RWheel, new AngularHinge
        {
            LocalHingeAxisA = new Vector3(-1, 0, 0),
            LocalHingeAxisB = new Vector3(0, 1, 0),
            SpringSettings = new SpringSettings(30, 1)
        });


        simulation.Solver.Add(bodyReference.Handle, RWheel, new LinearAxisServo
        {
            LocalPlaneNormal = new Vector3(0, -1, 0),
            TargetOffset = 0.05f,
            LocalOffsetA = new Vector3(0.1f, -0.0f, 0.05f),
            LocalOffsetB = default,
            ServoSettings = ServoSettings.Default,
            SpringSettings = new SpringSettings(5, 1)
        });


        simulation.Solver.Add(bodyReference.Handle, RWheel, new PointOnLineServo
        {
            LocalDirection = new Vector3(0, -1, 0),
            LocalOffsetA = new Vector3(0.1f, -0.0f, 0.05f),
            LocalOffsetB = default,
            ServoSettings = ServoSettings.Default,
            SpringSettings = new SpringSettings(30, 1)
        });

        var rearWheelOffset = new RigidPose(new Vector3(0, -0.3f, -0.15f));
        RigidPose.MultiplyWithoutOverlap(rearWheelOffset, bodyReference.Pose, out var rearWheelPose);

        RearWheel = simulation.Bodies.Add(BodyDescription.CreateDynamic(
            rearWheelPose,
            wheelInertia, new CollidableDescription(rearWheelShapeIndex, 0.1f), new BodyActivityDescription(0.01f)));

        simulation.Solver.Add(bodyReference.Handle, RearWheel, new BallSocket
        {
            LocalOffsetA = new Vector3(0, -0.1f, -0.15f),
            LocalOffsetB = new Vector3(0, 0, 0),
            SpringSettings = new SpringSettings(30, 1)
        });

        // Setup collisions
        ref var bodyProperties = ref room.SimInstance.Properties.Allocate(bodyReference.Handle);
        bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(bodyReference.Handle.Value, 1) };

        ref var lwheelProperties = ref room.SimInstance.Properties.Allocate(LWheel);
        lwheelProperties = new BodyCollisionProperties { Filter = new SubgroupCollisionFilter(bodyReference.Handle.Value, 1), Friction = 1 };

        ref var rwheelProperties = ref room.SimInstance.Properties.Allocate(RWheel);
        rwheelProperties = new BodyCollisionProperties { Filter = new SubgroupCollisionFilter(bodyReference.Handle.Value, 1), Friction = 1 };

        ref var rearWheelProperties = ref room.SimInstance.Properties.Allocate(RearWheel);
        rearWheelProperties = new BodyCollisionProperties { Filter = new SubgroupCollisionFilter(bodyReference.Handle.Value, 1), Friction = 0.5f };

        SubgroupCollisionFilter.DisableCollision(ref lwheelProperties.Filter, ref bodyProperties.Filter);
        SubgroupCollisionFilter.DisableCollision(ref rwheelProperties.Filter, ref bodyProperties.Filter);
        SubgroupCollisionFilter.DisableCollision(ref rearWheelProperties.Filter, ref bodyProperties.Filter);
        SubgroupCollisionFilter.DisableCollision(ref rearWheelProperties.Filter, ref rwheelProperties.Filter);
        SubgroupCollisionFilter.DisableCollision(ref rearWheelProperties.Filter, ref lwheelProperties.Filter);
        SubgroupCollisionFilter.DisableCollision(ref rwheelProperties.Filter, ref lwheelProperties.Filter);
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

        leftTicks += leftSpeed * dt;
        rightTicks += rightSpeed * dt;

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

        // Setup whisker event handlers
        // LeftWhisker.OnStatusChanged += OnWhisker;
        // RightWhisker.OnStatusChanged += OnWhisker;
    }

    #region Command Handlers

    public void OnDrive(byte[] msg)
    {
        if (msg.Length < 5)
        {
            Console.WriteLine("Drive message too short!");
            return;
        }

        var left = BitConverter.ToInt16(msg, 1);
        var right = BitConverter.ToInt16(msg, 3);

        leftSpeed = 75 * Math.Sign(left);
        rightSpeed = 75 * Math.Sign(right);

        leftDistance = Math.Abs(left);
        rightDistance = Math.Abs(right);

        driveState = DriveState.SetDistance;

        Console.WriteLine($"Drive {left} {right}");
    }

    public void OnSetSpeed(byte[] msg)
    {
        if (msg.Length < 5)
        {
            Console.WriteLine("Set Speed message too short!");
            return;
        }

        leftSpeed = BitConverter.ToInt16(msg, 1);
        rightSpeed = BitConverter.ToInt16(msg, 3);

        driveState = DriveState.SetSpeed;

        Console.WriteLine($"Set Speed {leftSpeed} {rightSpeed}");
    }

    public void OnBeep(byte[] msg)
    {
        if (msg.Length < 5)
        {
            Console.WriteLine("Beep message too short!");
            return;
        }

        var duration = BitConverter.ToInt16(msg, 1);
        var tone = BitConverter.ToInt16(msg, 3);

        // TODO: Send beep to client
        room.SendToClients("beep", new BeepData() { Robot = BytesToHexstring(MacAddress, ""), Duration = duration, Frequency = tone });

        Console.WriteLine($"Beep {duration} {tone}");
    }

    public void OnSetLED(byte[] msg)
    {
        if (msg.Length < 3)
        {
            Console.WriteLine("Set LED message too short!");
            return;
        }

        // TODO: send to client 
        // Determine which LED was requested
        var which = msg[1];
        var status = msg[2];

        Console.WriteLine($"Set LED {which} {status}");
    }

    public void OnGetRange(byte[] msg)
    {
        const short MAX_RANGE = 300;
        short distance = MAX_RANGE;

        // if (Physics.Raycast(new Ray(UltrasonicSensor.position, UltrasonicSensor.forward), out RaycastHit hitInfo, 5, ~LayerMask.GetMask("UI", "Ignore Raycast", "RobotGhost")))
        // {
        //     // Convert to cm
        //     distance = Math.Min(MAX_RANGE, (short)(hitInfo.distance * 100));
        // }

        // // Create response message
        // byte[] messageBytes = new byte[3];
        // messageBytes[0] = (byte)'R';
        // BitConverter.GetBytes(distance).CopyTo(messageBytes, 1);

        // SendRoboScapeMessage(messageBytes);

        Console.WriteLine($"Get Range {distance}");
    }

    public void OnGetTicks(byte[] msg)
    {
        // Create response message
        byte[] messageBytes = new byte[9];
        messageBytes[0] = (byte)'T';
        BitConverter.GetBytes((int)leftTicks).CopyTo(messageBytes, 1);
        BitConverter.GetBytes((int)rightTicks).CopyTo(messageBytes, 5);

        SendRoboScapeMessage(messageBytes);

        Console.WriteLine($"Get Ticks {leftTicks} {rightTicks}");
    }

    public void OnWhisker(Object sender, EventArgs e)
    {
        // Create response message
        // byte[] messageBytes = new byte[2];
        // messageBytes[0] = (byte)'W';
        // messageBytes[1] = 0;

        // if (!LeftWhisker.IsTriggered)
        // {
        //     messageBytes[1] |= 0x2;
        // }

        // if (!RightWhisker.IsTriggered)
        // {
        //     messageBytes[1] |= 0x1;
        // }

        // SendRoboScapeMessage(messageBytes);
        // Console.WriteLine($"Whiskers Sent {LeftWhisker.IsTriggered} {RightWhisker.IsTriggered}");
    }

    #endregion


    public void OnButtonPress(bool status)
    {
        // Create response message
        byte[] messageBytes = new byte[2];
        messageBytes[0] = (byte)'P';
        messageBytes[1] = (byte)(status ? 0 : 1);

        SendRoboScapeMessage(messageBytes);
        Console.WriteLine($"Button Sent");
    }

    public void ResetSpeed()
    {
        leftSpeed = 0;
        rightSpeed = 0;
    }

    public void Dispose()
    {
        base.Dispose();
    }

    internal struct BeepData
    {
        public string Robot;
        public short Duration;
        public short Frequency;
    }
}
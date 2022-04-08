using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using RoboScapeSimulator;
using RoboScapeSimulator.Entities;

class Trigger : DynamicEntity, IResettable
{
    private static uint ID = 0;

    public bool OneTime = false;

    private bool triggered = false;

    public event EventHandler<Entity>? OnTriggerEnter;

    public event EventHandler<Entity>? OnTriggerStay;

    public event EventHandler<Entity>? OnTriggerExit;

    public event EventHandler? OnTriggerEmpty;

    public List<Entity> InTrigger = new List<Entity>();

    private List<Entity> lastInTrigger = new List<Entity>();

    public Trigger(Room room, in Vector3 initialPosition, in Quaternion initialOrientation, float width = 1, float height = 1, float depth = 1, bool oneTime = false, bool debug = false)
    {
        if (debug)
        {
            VisualInfo = VisualInfo.None;
        }

        Name = $"trigger_{ID++}";

        Width = width;
        Height = height;
        Depth = depth;

        var simulationInstance = room.SimInstance;

        var box = new Box(width, height, depth);

        RigidPose pose = new(initialPosition, initialOrientation);

        // Body created which never sleeps, so trigger can repeatedly know bodies inside it
        BodyHandle bodyHandle = simulationInstance.Simulation.Bodies.Add(BodyDescription.CreateKinematic(pose, new CollidableDescription(simulationInstance.Simulation.Shapes.Add(box), 0.1f), new BodyActivityDescription(-1)));
        BodyReference = simulationInstance.Simulation.Bodies.GetBodyReference(bodyHandle);

        ref var bodyProperties = ref simulationInstance.Properties.Allocate(BodyReference.Handle);
        bodyProperties = new BodyCollisionProperties { Friction = 1f, Filter = new SubgroupCollisionFilter(BodyReference.Handle.Value, 0), IsTrigger = true };

        this.OneTime = oneTime;

        room.SimInstance.NamedBodies.Add(Name, BodyReference);
        room.SimInstance.Entities.Add(this);
    }

    public void EntityInside(Entity e)
    {
        lock (InTrigger)
        {
            if (!InTrigger.Contains(e))
            {
                InTrigger.Add(e);
            }
        }
    }

    public void Reset()
    {
        lock (InTrigger)
        {
            triggered = false;
            InTrigger.Clear();
        }
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        lock (InTrigger)
        {
            // Compare lists
            if (InTrigger.Any(o => !lastInTrigger.Contains(o)))
            {
                // OnEnter Event only occurs once if OneTime is set
                if (!OneTime || !triggered)
                {
                    foreach (var e in InTrigger.Where(o => !lastInTrigger.Contains(o)))
                    {
                        if (e != null)
                        {
                            OnTriggerEnter?.Invoke(this, e);
                        }
                    }

                    triggered = true;
                }
            }

            foreach (var e in lastInTrigger.Where(o => !InTrigger.Contains(o)))
            {
                if (e != null)
                {
                    OnTriggerExit?.Invoke(this, e);
                }
            }

            if (InTrigger.Count > 0)
            {
                InTrigger.ForEach(e =>
                {
                    if (e != null)
                    {
                        OnTriggerStay?.Invoke(this, e);
                    }
                });
            }

            if (InTrigger.Count == 0 && lastInTrigger.Count > 0)
            {
                OnTriggerEmpty?.Invoke(this, EventArgs.Empty);
            }

            // Cycle lists
            lastInTrigger.Clear();
            lastInTrigger.AddRange(InTrigger);
            InTrigger.Clear();
        }
    }
}
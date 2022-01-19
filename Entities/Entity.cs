
using BepuPhysics;

namespace RoboScapeSimulator.Entities
{
    /// <summary>
    /// Base class for objects within the simulation
    /// </summary>
    public abstract class Entity : IDisposable
    {
        /// <summary>
        /// Run the behavior of this Entity
        /// </summary>
        /// <param name="dt">Time delta in seconds</param>
        public virtual void Update(float dt) { }

        /// <summary>
        /// The name of this Entity
        /// </summary>
        public string Name = "entity";

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Entity()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Interface for Entities with the ability to be reset
    /// </summary>
    public interface IResettable
    {
        public void Reset();
    }

    /// <summary>
    /// Base class for Entity classes with static bodies
    /// </summary>
    public abstract class StaticEntity : Entity
    {
        /// <summary>
        /// The reference to this object in the simulation
        /// </summary>
        public StaticReference StaticReference;
    }


    /// <summary>
    /// Base class for Entity classes with non-static bodies
    /// </summary>
    public abstract class DynamicEntity : Entity
    {
        /// <summary>
        /// The reference to this object's body in the simulation
        /// </summary>
        public BodyReference BodyReference;
    }
}
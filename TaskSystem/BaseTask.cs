using System;
using UnityEngine;

namespace TaskSystem
{
    public abstract class BaseTask : ITask
    {
        public string Guid { get; }
        public virtual bool Interruptible => true;
        public bool IsRunned { get; private set; }
        public bool IsCompleted { get; private set; }
        public event Action<ITask> OnExecute;
        public event Action<ITask> OnComplete;
        public event Action<ITask> OnForceComplete;
        public event Action<ITask> OnInterrupt;

        protected TaskParams Requirements;
        protected TaskParams GivesReward;
        protected bool IsExecuted;
        protected bool Finalized;

        protected BaseTask()
        {
            Guid = System.Guid.NewGuid().ToString();
            Requirements = new TaskParams();
            GivesReward = new TaskParams();
        }

        public virtual void Execute(params object[] args)
        {
            IsRunned = true;
            IsExecuted = true;
            OnExecute?.Invoke(this);
        }

        public void ForceComplete()
        {
            if (Finalized) return;
            IsRunned = false;
            Finalized = true;
            IsCompleted = true;
            OnForceCompleted();
            OnDisposed();
            OnForceComplete?.Invoke(this);
            OnForceComplete = null;
            OnInterrupt = null;
            OnComplete = null;
        }

        public void Interrupt()
        {
            if (Finalized) return;
            IsRunned = false;
            Finalized = true;
            OnInterrupted();
            OnDisposed();
            OnInterrupt?.Invoke(this);
            OnInterrupt = null;
            OnComplete = null;
            Requirements = null;
            GivesReward = null;
        }

        protected void Completed()
        {
            if (Finalized) return;

            IsRunned = false;
            Finalized = true;
            IsCompleted = true;
            OnCompleted();
            OnDisposed();
            OnComplete?.Invoke(this);
            OnInterrupt = null;
            OnComplete = null;
            Requirements = null;
            GivesReward = null;
        }

        public virtual TaskParams GetRequirements()
        {
            return Requirements;
        }

        public TaskParams GetRewards()
        {
            return GivesReward;
        }

        public virtual Vector2[] GetPositions()
        {
            return new Vector2[0];
        }

        public virtual string GetName()
        {
            return GetType().Name;
        }

        protected virtual void OnInterrupted()
        {
        }

        protected virtual void OnForceCompleted()
        {
        }

        protected virtual void OnCompleted()
        {
        }
        /// <summary>
        /// called whenever the task ends 
        /// </summary>
        protected virtual void OnDisposed(){}
    }
}
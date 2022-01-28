using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace TaskSystem
{
    public class BootstrapTask : BaseTask, ITickable
    {
        private readonly TickableManager _tickableManager;
        protected readonly List<ITask> Tasks;
        protected ITask CurrentTask;
        private object[] _args;

        protected BootstrapTask(IEnumerable<ITask> tasks, TickableManager tickableManager)
        {
            Tasks = tasks.ToList();
            _tickableManager = tickableManager;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(_args = args);
            foreach (var task in Tasks)
            {
                task.OnInterrupt += _ => Interrupt();
                task.OnForceComplete += _ => ForceComplete();
            }

            _tickableManager.Add(this);
            SwitchTask();
        }

        public void Tick()
        {
            if (!IsRunned || IsCompleted)
            {
                return;
            }

            if (CurrentTask != null)
            {
                if (!CurrentTask.IsCompleted)
                {
                    return;
                }

                CurrentTask = null;
                SwitchTask();
            }
            else
            {
                SwitchTask();
            }
        }

        public override string GetName()
        {
            return CurrentTask?.GetName() ?? "";
        }

        public override Vector2[] GetPositions()
        {
            return Tasks.SelectMany(x => x.GetPositions()).ToArray();
        }

        protected virtual void SwitchTask()
        {
            // во избежания множественного переключения при симуляции или параллельных выполнений
            if(CurrentTask != null) return;
            
            while (true) // recursion loop
            {
                if (Tasks.Count > 0)
                {
                    var task = Tasks[0];
                    Tasks.RemoveAt(0);
                    if (task.IsNullOrDefault())
                    {
                        continue;
                    }
                    task.Execute(_args);
                    // задача может закончится моментально
                    if (task.IsCompleted)
                    {
                        continue;
                    }

                    CurrentTask = task;
                    return;
                }

                CurrentTask = null;
                Completed();
                break;
            }
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _tickableManager.Remove(this);
            }
            Tasks.Clear();
            _args = null;
            CurrentTask = null;
            base.OnDisposed();
        }

        protected override void OnForceCompleted()
        {
            CurrentTask?.ForceComplete();
            foreach (var task in Tasks)
            {
                task?.ForceComplete();
            }
        }

        protected override void OnInterrupted()
        {
            CurrentTask?.Interrupt();
            foreach (var task in Tasks)
            {
                task?.Interrupt();
            }
        }
    }
}
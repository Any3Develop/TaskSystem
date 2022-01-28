using System.Collections.Generic;
using System.Linq;
using UnitSystem;
using Zenject;

namespace TaskSystem
{
    public class GeneratedUnitTask : BootstrapTask
    {
        public override bool Interruptible { get; }
        private readonly UnitTaskDtoStorage _taskDtoStorage;
        private readonly UnitTaskDto _unitTaskDto;
        private readonly List<object> _args;
        private readonly string _mainTaskName;
        
        public GeneratedUnitTask(UnitTaskDto unitTaskDto,
                                 bool interruptible,
                                 IEnumerable<ITask> tasks,
                                 IEnumerable<object> args,
                                 TickableManager tickableManager,
                                 UnitTaskDtoStorage taskDtoStorage) : base(tasks, tickableManager)
        {
            _args = args.ToList();
            _mainTaskName = Tasks.Last().GetName();
            Interruptible = interruptible;
            _unitTaskDto = unitTaskDto;
            _taskDtoStorage = taskDtoStorage;
        }

        public override string GetName()
        {
            return _mainTaskName;
        }
        
        protected override void SwitchTask()
        {
            if (CurrentTask != null) return;
            while (true) // recursion loop
            {
                if (Tasks.Count > 0)
                {
                    var args = (object[]) _args[0];
                    var task = Tasks[0];
                    Tasks.RemoveAt(0);
                    _args.RemoveAt(0);
                    if (task.IsNullOrDefault())
                    {
                        continue;
                    }

                    task.Execute(args);

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
            if (!_taskDtoStorage.HasEntity(_unitTaskDto.UnitGuid))
            {
                return;
            }

            var targetDto = _taskDtoStorage.Get(_unitTaskDto.UnitGuid);

            if (targetDto == _unitTaskDto)
            {
                _taskDtoStorage.Remove(targetDto.UnitGuid);
            }

            _args.Clear();
            base.OnDisposed();
        }
    }
}
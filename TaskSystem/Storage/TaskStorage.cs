using System;
using System.Collections.Generic;
using System.Linq;
using UnitSystem;

namespace TaskSystem
{
    public class TaskInfo
    {
        public string TaskName { get; }
        public string TaskGuid { get; }
        public string SourceGuid { get; }
        public string SourceModelID { get; }
        public string TaskHash { get; }
        public TaskParams Requirements { get; }
        public TaskParams GivesReward { get; }
        
        public TaskInfo(string taskName, string sourceGuid, string sourceModelID, ITask task)
        {
            SourceGuid = sourceGuid;
            TaskName = taskName;
            TaskGuid = task.Guid;
            SourceModelID = sourceModelID;
            Requirements = task.GetRequirements();
            GivesReward = task.GetRewards();
            TaskHash = SourceGuid + Requirements.GetCustomHashCode() + GivesReward.GetCustomHashCode();
        }
    }
    
    public class TaskStorage
    {
        public event Action<string> TaskRemoved;
        public event Action<TaskInfo> TaskDeclare;
        
        private readonly List<TaskInfo> _definedTaskInfos;
        /// <summary>
        /// arg1 = SourceGuid , (arg1 = taskGuid, arg2 = task)
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, ITask>> _storage;
        public TaskStorage()
        {
            _definedTaskInfos = new List<TaskInfo>();
            _storage = new Dictionary<string, Dictionary<string, ITask>>();
        }
        
        public void DeclareTask(string sourceGuid, string modelID, string taskName, ITask task, bool notify = true)
        {
            if (!_storage.ContainsKey(sourceGuid))
            {
                _storage.Add(sourceGuid, new Dictionary<string, ITask>());
            }

            _storage[sourceGuid].Add(task.Guid, task);
            var taskInfo = new TaskInfo(taskName, sourceGuid, modelID, task);
            _definedTaskInfos.Add(taskInfo);
            if(notify)
            {
                TaskDeclare?.Invoke(taskInfo);
            }
        }
        
        public void RemoveAll(string sourceGuid)
        {
            var taskInfos = GetAllInfos(sourceGuid).ToArray();
            if (taskInfos.Length == 0)
            {
                return;
            }

            foreach (var taskInfo in taskInfos)
            {
                Remove(taskInfo);
            }
        }
        
        public void Remove(string taskGuid)
        {
            var taskInfo = GetTaskInfo(taskGuid);
            if (taskInfo == null)
            {
                return;
            }
            Remove(taskInfo);
        }
        
        public bool HasTask(TaskInfo taskInfo)
        {
            return _definedTaskInfos.Contains(taskInfo);
        }        
        
        public bool HasTask(string taskGuid)
        {
            return !GetTaskInfo(taskGuid).IsNullOrDefault();
        }
        
        public bool HasTasks()
        {
            return _definedTaskInfos.Any();
        }
        
        public bool HasTasks(string sourceGuid)
        {
            return _storage.ContainsKey(sourceGuid);
        }

        public IEnumerable<ITask> FindTasks(FindRequest request)
        {
            var listTasks = new List<ITask>();
            
            foreach (var taskInfo in GetAllInfo())
            {
                if(taskInfo.IsNullOrDefault()) continue;
                
                // находим задачу с идентичным хэш кодом
                if (request.WithHash == taskInfo.TaskHash)
                {
                    listTasks.Add(Get(taskInfo));
                    break;
                }
                // пропускаем все с требованиями
                if (request.WithoutRequirements && taskInfo.Requirements.Params.Length > 0) continue;
                
                // пропускаем все от ненужного гуида
                if(request.WithoutGuids.Length > 0 && request.WithoutGuids.Contains(taskInfo.SourceGuid)) continue;
                
                // пропускаем все от ненужных моделей ид
                if (request.WithoutModelIDs.Length > 0 && request.WithoutModelIDs.Contains(taskInfo.SourceModelID)) continue;
                
                // пропускаем все модели которые не нужны (нужно подумать чтобы понять что тут происходит)
                if (request.WithModelIDs.Length > 0 && !request.WithModelIDs.Contains(taskInfo.SourceModelID)) continue;
                
                if (request.WithGivesReward.Length > 0)
                {
                    var requirements = request.WithGivesReward;
                    var rewardsTargetTask = taskInfo.GivesReward.Params.Select(x=>x.Key).ToArray();

                    if (rewardsTargetTask.Length == 0)
                    {
                        continue;
                    }

                    if (rewardsTargetTask.Length < requirements.Length)
                    {
                        continue;
                    }
                    
                    if (requirements.Any(requirement => !rewardsTargetTask.Contains(requirement)))
                    {
                        continue;
                    }

                    // если достаточно совпадений то выбираем задачу
                    listTasks.Add(Get(taskInfo));
                }
            }
            
            return listTasks.Where(x=>x != null);
        }

        public IEnumerable<TaskInfo> GetAllInfo()
        {
            return _definedTaskInfos;
        }
        
        public IEnumerable<TaskInfo> GetAllInfos(string sourceGuid)
        {
            return _definedTaskInfos.Where(x => x.SourceGuid == sourceGuid);
        }
        
        public IEnumerable<ITask> GetAll(string sourceGuid)
        {
            return _storage.TryGetValue(sourceGuid,out var tasks) ? tasks.Values : default;
        }
        
        public ITask Get(string taskGuid)
        {
            var taskInfo = GetTaskInfo(taskGuid);
            return taskInfo.IsNullOrDefault() ? default : _storage[taskInfo.SourceGuid][taskGuid];
        }
        
        public ITask Get(TaskInfo taskInfo)
        {
            return taskInfo.IsNullOrDefault() ? default : _storage[taskInfo.SourceGuid][taskInfo.TaskGuid];
        }
        
        public IEnumerable<ITask> Get()
        {
            return _storage.SelectMany(x=>x.Value.Values);
        }
        
        public TaskInfo GetTaskInfo(string taskGuid)
        {
            var copy = _definedTaskInfos.ToArray();
            return copy.FirstOrDefault(x => x.TaskGuid == taskGuid);
        }
        
        private void Remove(TaskInfo taskInfo)
        {
            if (!HasTask(taskInfo))
            {
                return;
            }

            _definedTaskInfos.Remove(taskInfo);
            _storage[taskInfo.SourceGuid].Remove(taskInfo.TaskGuid);
            if (_storage[taskInfo.SourceGuid].Count == 0)
            {
                _storage.Remove(taskInfo.SourceGuid);
            }
            TaskRemoved?.Invoke(taskInfo.TaskGuid);
        }
    }
}
using System;
using System.Linq;
using UnityEngine;

namespace TaskSystem
{
    [Serializable]
    public enum TaskParamID {None = 0, WithoutModelID, WithoutGuid, WithModelID, ItemID }
    [Serializable]
    public class TaskParams
    {
        public TaskParamModel[] Params => _params;
        [SerializeField] private TaskParamModel[] _params;

        public TaskParams(params TaskParamModel[] args)
        {
            _params = args ?? new TaskParamModel[0];
        }

        public string GetCustomHashCode()
        {
            return _params.Aggregate("", (current, paramModel) => current + paramModel.GetCustomHashCode());
        }
    }
}
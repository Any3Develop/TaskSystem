using System;
using UnityEngine;

namespace TaskSystem
{
    [Serializable]
    public struct TaskParamModel
    {
        public TaskParamID ID => _id;
        public string Key => _key;
        public string Value => _value;        
        [SerializeField] private TaskParamID _id;
        [SerializeField] private string _key;
        [SerializeField] private string _value;
        public TaskParamModel(TaskParamID paramID, string key, string value)
        {
            _id = paramID;
            _key = key;
            _value = value;
        }

        public string GetCustomHashCode()
        {
            return ID + Key + Value;
        }
    }
}
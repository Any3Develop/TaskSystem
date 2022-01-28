using System;
using InfoCollectorSystem;
using Services.StatsService;
using SimulationSystem;
using UnityEngine;
using Zenject;

namespace TaskSystem
{
    public class TimerFromStatTask : BaseTask, ITickable
    {
        private readonly TickableManager _tickableManager;
        private readonly ISimulationSystem _simulationSystem;
        private readonly bool _hasSimulate;
        private float _chunkTime;
        private float _chunkTimer;
        private bool _hasChunkAction;
        private Action _onChunkComplete;
        private StatVital _timerStat;

        public TimerFromStatTask(TickableManager tickableManager,
                                 ISimulationSystem simulationSystem,
                                 bool hasSimulate = true)
        {
            _tickableManager = tickableManager;
            _simulationSystem = simulationSystem;
            _hasSimulate = hasSimulate;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            _timerStat = (StatVital) args[0];
            if (_timerStat.CurrentValue <= 0)
            {
                Completed();
                return;
            }

            base.Execute(args);
            _tickableManager.Add(this);
        }
        public void SetChunkAction(Action onChunkComplete, float chunkSeconds)
        {
            _hasChunkAction = true;
            _chunkTime = chunkSeconds;
            _chunkTimer = _chunkTime;
            _onChunkComplete = onChunkComplete;
        }
        public void Tick()
        {
            if (!IsRunned) return;
            
            var delta = _hasSimulate ? _simulationSystem.DeltaTime : Time.deltaTime;
            _timerStat.CurrentValue -= Format.SecondsToMinutes(delta);
            if (_hasChunkAction)
            {
                _chunkTimer -= delta;
                if (_chunkTimer <= 0)
                {
                    // дельта времени может быть больше чанки
                    // по этому нужно расчитать сколько выполненно чанков в этом кадре
                    // а остаток отправить в следующий кадр
                    // позволит корректно выполнить чанки в симуляциях
                    var tempChunkTime = Mathf.Abs(_chunkTimer) + _chunkTime;
                    while (tempChunkTime >= _chunkTime)
                    {
                        _onChunkComplete?.Invoke();
                        tempChunkTime -= _chunkTime;
                    }

                    _chunkTimer = tempChunkTime + _chunkTime;
                }
            }
            
            if (_timerStat.CurrentValue <= 0)
            {
                Completed();
            }
        }
        
        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _tickableManager.Remove(this);
            }

            _timerStat = null;
        }
    }
}
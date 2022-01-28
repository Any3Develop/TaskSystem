using System;
using SimulationSystem;
using UnityEngine;
using Zenject;

namespace TaskSystem
{
    public class SimpleTimerTask : BaseTask, ITickable
    {
        private readonly TickableManager _tickableManager;
        private float _chunkTime;
        private float _chunkTimer;
        private bool _hasChunkAction;
        private Action _onChunkComplete;
        private float _timer;

        public SimpleTimerTask(TickableManager tickableManager)
        {
            _tickableManager = tickableManager;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            _timer = Mathf.Abs((float) args[0]);
            if (_timer == 0)
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

            var delta = Time.deltaTime;
            _timer -= delta;

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

            if (_timer <= 0)
            {
                Completed();
            }
        }

        private void Dispose()
        {
            if (IsExecuted)
            {
                _tickableManager.Remove(this);
            }

            _onChunkComplete = null;
        }

        protected override void OnForceCompleted()
        {
            Dispose();
        }

        protected override void OnInterrupted()
        {
            Dispose();
        }

        protected override void OnCompleted()
        {
            Dispose();
        }
    }
}
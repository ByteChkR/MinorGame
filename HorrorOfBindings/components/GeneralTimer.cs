using System;
using Engine.Core;

namespace HorrorOfBindings.components
{
    public class GeneralTimer : AbstractComponent
    {
        private float _fireTime;
        private float _time;
        private Action _action;
        private bool _loop;

        public GeneralTimer(float fireTime, Action action, bool loop = false)
        {
            _action = action;
            _fireTime = fireTime;
            _loop = loop;
        }


        protected override void Update(float deltaTime)
        {
            _time += deltaTime;
            if (_time >= _fireTime)
            {
                _action?.Invoke();
                if (_loop)
                {
                    _time = 0;
                }
                else
                {
                    Destroy();
                }
            }
        }
    }
}
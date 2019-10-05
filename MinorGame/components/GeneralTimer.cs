using System;
using MinorEngine.engine.components;

namespace MinorGame.components
{
    public class GeneralTimer :AbstractComponent
    {
        private float _fireTime;
        private float _time;
        private Action _action;
        public GeneralTimer(float fireTime, Action action)
        {
            _action = action;
            _fireTime = fireTime;
        }


        protected override void Update(float deltaTime)
        {
            _time += deltaTime;
            if (_time >= _fireTime)
            {
                _action?.Invoke();
                Destroy();
            }
        }


    }
}
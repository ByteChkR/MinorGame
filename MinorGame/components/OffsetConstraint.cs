using System;
using MinorEngine.engine.components;
using MinorEngine.engine.core;
using OpenTK;

namespace MinorGame.components
{
    public class OffsetConstraint : AbstractComponent
    {
        private GameObject _attachedObject;
        public Vector3 Offset { get; set; }

        private Vector3 _velocity;
        private float _inverseDamp = 1;
        public float Damping
        {
            get => 1 - _inverseDamp;
            set => _inverseDamp = Math.Clamp(1 - value, 0f, 1f);
        }
        public float MoveSpeed { get; set; } = 1;

        protected override void Awake()
        {

            _velocity = Vector3.Zero;

        }


        public void Attach(GameObject obj, Vector3 offset)
        {
            _attachedObject = obj;
            Offset = offset;
        }

        public void Detach()
        {
            _attachedObject = null;
        }

        protected override void Update(float deltaTime)
        {
            if (_attachedObject != null)
            {
                if (_attachedObject.Destroyed)
                {
                    Detach();
                    Owner.Destroy();
                    return;
                }
                Vector3 moveAmount = ComputePositionChange() * deltaTime * MoveSpeed;
                Owner.Translate(moveAmount);
            }
        }

        private Vector3 ComputePositionChange()
        {
            Vector3 currentPos = Owner.GetLocalPosition();
            Vector3 targetPos = _attachedObject.GetLocalPosition() + Offset;
            Vector3 delta = targetPos - currentPos;

            if (delta == Vector3.Zero) return Vector3.Zero;

            float translateDistance = delta.Length * _inverseDamp;

            //if (translateDistance <= 0) return Vector3.Zero;

            return delta.Normalized() * translateDistance;

        }
    }
}
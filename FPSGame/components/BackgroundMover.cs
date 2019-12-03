using System;
using Engine.Core;
using Engine.UI;
using OpenTK;

namespace FPSGame.components
{
    public class BackgroundMover : AbstractComponent
    {
        private UiImageRendererComponent image;
        private float MoveSpeed = 0.15f;
        private float _timer;
        private float xYDelta;
        private float TimeScale = 0.15f;

        protected override void Awake()
        {
            base.Awake();

            image = Owner.GetComponent<UiImageRendererComponent>();
            Random rnd = new Random();
            xYDelta = (float) rnd.NextDouble();
        }

        protected override void Update(float deltaTime)
        {
            _timer += deltaTime;
            image.Offset += GetMoveDir(_timer) * MoveSpeed * deltaTime;
        }

        private Vector2 GetMoveDir(float time)
        {
            float d = MathF.Sin(xYDelta + time);
            return new Vector2(MathF.Sin(time * TimeScale + d), MathF.Cos(time * TimeScale - d));
        }
    }
}
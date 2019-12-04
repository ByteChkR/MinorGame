using System;
using Engine.Core;
using Engine.Physics;
using Engine.Physics.BEPUphysics.CollisionTests;
using Engine.Physics.BEPUphysics.NarrowPhaseSystems.Pairs;
using HorrorOfBindings.scenes;
using OpenTK;

namespace HorrorOfBindings.components
{
    public class MapEndTrigger : AbstractComponent
    {
        private float time = 0;
        private float y;

        protected override void Awake()
        {
            y = Owner.LocalPosition.Y;
        }

        protected override void Update(float deltaTime)
        {
            Owner.Rotate(new Vector3(1, 0.5f, 1), deltaTime * 4);
            Owner.Scale = Vector3.One * (1.5f + MathF.Sin((time += deltaTime) * 4) * 0.01f);
            Owner.SetLocalPosition(new Vector3(Owner.LocalPosition.X, y + MathF.Abs(MathF.Sin(deltaTime * 4)),
                Owner.LocalPosition.Z));
        }

        protected override void OnContactCreated(Collider other, CollidablePairHandler handler, ContactData contact)
        {
            if (other.Owner.Name == "Player")
            {
                GameTestScene.ComesFromMenu = false;
                GameEngine.Instance.InitializeScene<GameTestScene>();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using MinorEngine.components;
using MinorEngine.debug;
using MinorEngine.engine.components;
using OpenTK;
using OpenTK.Input;

namespace MinorGame.components
{
    public class PlayerController : AbstractComponent
    {

        private float MoveSpeed = 1;
        private Key Forward = Key.W;
        private Key Left = Key.A;
        private Key Back = Key.S;
        private Key Right = Key.D;
        private bool UseGlobalForward = true;
        private bool ApplyVelocity = false;
        private Vector3 velocity;
        private RigidBodyComponent Rigidbody;

        public PlayerController(float speed, bool useGlobalForward)
        {
            MoveSpeed = speed;
            UseGlobalForward = useGlobalForward;
        }


        protected override void Awake()
        {
            Rigidbody = Owner.GetComponent<RigidBodyComponent>();
            if (Rigidbody == null)
            {
                this.Log("No Rigid body attached", DebugChannel.Warning);

            }

        }

        protected override void Update(float deltaTime)
        {
            if (ApplyVelocity)
            {
                ApplyVelocity = false;
                Vector3 vel = velocity;
                if (UseGlobalForward)
                {
                    vel = new Vector3(new Vector4(vel, 0) * Owner.GetWorldTransform());
                }
                Rigidbody.SetVelocityLinear(vel * deltaTime);

            }

        }

        protected override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Forward)
            {
                velocity += Vector3.UnitZ;
                ApplyVelocity = true;
            }
            else if (e.Key == Back)
            {
                velocity -= Vector3.UnitZ;
                ApplyVelocity = true;
            }
            else if (e.Key == Left)
            {
                velocity -= Vector3.UnitX;
                ApplyVelocity = true;
            }
            else if (e.Key == Right)
            {
                velocity += Vector3.UnitX;
                ApplyVelocity = true;
            }

        }

        protected override void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Forward)
            {
                velocity -= Vector3.UnitZ;
                ApplyVelocity = true;
            }
            else if (e.Key == Back)
            {
                velocity += Vector3.UnitZ;
                ApplyVelocity = true;
            }
            else if (e.Key == Left)
            {
                velocity += Vector3.UnitX;
                ApplyVelocity = true;
            }
            else if (e.Key == Right)
            {
                velocity -= Vector3.UnitX;
                ApplyVelocity = true;
            }
        }
    }
}

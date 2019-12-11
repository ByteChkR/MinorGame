using System;
using Engine.Core;
using Engine.Debug;
using Engine.Physics;
using Engine.Physics.BEPUphysics.CollisionTests;
using Engine.Physics.BEPUphysics.NarrowPhaseSystems.Pairs;
using OpenTK;
using OpenTK.Graphics.Vulkan.Xcb;
using OpenTK.Input;
using MathHelper = OpenTK.MathHelper;
using Vector3 = Engine.Physics.BEPUutilities.Vector3;

namespace FPSGame.components
{
    public class FPSController : AbstractComponent
    {
        private Collider c;
        private float pitch;
        private float sensitivity = 0.1f;
        private float yaw;
        private bool firstUpdate = true;
        protected override void Awake()
        {
            c = Owner.GetComponent<Collider>();
        }

        protected override void Update(float deltaTime)
        {
            if (firstUpdate)
            {
                firstUpdate = false;
            }
            else
            {
                if (GameEngine.Instance.HasFocus)
                {
                    //Note: This is not the exact center of the screen, but its good enough
                    OpenTK.Input.Mouse.SetPosition(
                        GameEngine.Instance.WindowPosition.X + GameEngine.Instance.Width / 2f,
                        GameEngine.Instance.WindowPosition.Y + GameEngine.Instance.Height / 2f);
                }
                Vector2 delta = GameEngine.Instance.MouseDelta;

                yaw += delta.X * sensitivity;
                if (pitch > 89.0f)
                {
                    pitch = 89.0f;
                }
                else if (pitch < -89.0f)
                {
                    pitch = -89.0f;
                }
                else
                {
                    pitch -= delta.Y * sensitivity;
                }
            }

            Vector3 front;
            front.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) *
                      (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) *
                      (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            front = Vector3.Normalize(front);
            front += Owner.LocalPosition;
            Owner.LookAt(front);

            Vector3 v = Vector3.Zero;
            if (Input.GetKey(Key.W))
            {
                v += -Vector3.UnitZ;
            }
            if (Input.GetKey(Key.A))
            {
                v += -Vector3.UnitX;
            }
            if (Input.GetKey(Key.S))
            {
                v += Vector3.UnitZ;
            }
            if (Input.GetKey(Key.D))
            {
                v += Vector3.UnitX;
            }

            bool jump = Input.GetKey(Key.Space);

            if (v == Vector3.Zero || !Grounded) return;
            v = (new Vector4(v) * Owner.GetWorldTransform()).Xyz;
            v.Y = 0;
            v.Normalize();
            v *= 1f;
            v.Y = jump ? 5 : 0;

            c.PhysicsCollider.ApplyLinearImpulse(ref v);
        }

        private bool Grounded = false;

        protected override void OnContactCreated(Collider other, CollidablePairHandler handler, ContactData contact)
        {
            if (handler.Contacts.Count == 1 && other.Owner.Name == "Ground" && !Grounded &&
                contact.Position.Y < c.PhysicsCollider.Position.Y)
            {
                Grounded = true;
            }
            base.OnContactCreated(other, handler, contact);
        }


        protected override void OnContactRemoved(Collider other, CollidablePairHandler handler, ContactData contact)
        {
            if (other.Owner.Name == "Ground")
            {
                bool isGrounded = false;
                for (int i = 0; i < handler.Contacts.Count; i++)
                {
                    if (handler.Contacts[i].Contact.Position.Y < c.PhysicsCollider.Position.Y)
                    {
                        isGrounded = true;
                    }
                }

                Grounded = isGrounded;
            }
            base.OnContactRemoved(other, handler, contact);
        }
    }
}
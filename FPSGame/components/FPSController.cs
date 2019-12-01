using System;
using Engine.Core;
using Engine.Debug;
using Engine.Physics;
using OpenTK;
using MathHelper = OpenTK.MathHelper;
using Vector3 = Engine.Physics.BEPUutilities.Vector3;

namespace FPSGame.components
{
    public class FPSController : AbstractComponent
    {
        private Collider c;
        private OpenTK.Vector2 wpos = GameEngine.Instance.WindowPosition;
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
                OpenTK.Vector2 windowCenter = wpos + GameEngine.Instance.WindowSize / 2f;
                OpenTK.Vector2 screenMousePos = GameEngine.Instance.MousePosition + GameEngine.Instance.WindowPosition;

                if (GameEngine.Instance.HasFocus)
                {
                    OpenTK.Input.Mouse.SetPosition(windowCenter.X + 8, windowCenter.Y + 31); //Magic Numbers because windows thinks that window borders and frames are not counting towards the window size :^)
                }
                OpenTK.Vector2 delta = screenMousePos - windowCenter;
                Logger.Log("Delta: " + delta, DebugChannel.Log, 10);

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
            front.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            front = Vector3.Normalize(front);
            front += Owner.LocalPosition;
            Owner.LookAt(front);
        }

        protected override void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            Vector3 v = new Vector3();
            if (e.KeyChar == 'w')
            {
                v = (-Vector4.UnitZ * Owner.GetWorldTransform()).Xyz;
            }
            else if(e.KeyChar == 'a')
            {
                v = (-Vector4.UnitX * Owner.GetWorldTransform()).Xyz;
            }
            else if (e.KeyChar == 's')
            {
                v = (Vector4.UnitZ * Owner.GetWorldTransform()).Xyz;
            }
            else if(e.KeyChar == 'd')
            {
                v = (Vector4.UnitX * Owner.GetWorldTransform()).Xyz;
            }

            v *= 10;
            c.SetVelocityLinear(v);
        }
    }
}

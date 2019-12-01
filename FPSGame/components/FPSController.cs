using System;
using Engine.Core;
using Engine.Debug;
using Engine.Physics.BEPUutilities;
using MathHelper = OpenTK.MathHelper;

namespace FPSGame.components
{
    public class FPSController : AbstractComponent
    {
        private OpenTK.Vector2 wpos = GameEngine.Instance.WindowPosition;
        private float pitch;
        private float sensitivity = 0.1f;
        private float yaw;
        private bool firstUpdate = true;
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
                    OpenTK.Input.Mouse.SetPosition(windowCenter.X+8, windowCenter.Y+31); //Magic Numbers because windows thinks that window borders and frames are not counting towards the window size :^)
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


            Owner.LookAt(front);
        }
    }
}

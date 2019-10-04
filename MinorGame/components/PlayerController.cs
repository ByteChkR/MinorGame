using System;
using System.Collections.Generic;
using System.Text;
using MinorEngine.BEPUphysics.Materials;
using MinorEngine.components;
using MinorEngine.debug;
using MinorEngine.engine.components;
using MinorEngine.engine.core;
using MinorEngine.engine.ui.utils;
using OpenTK;
using OpenTK.Input;

namespace MinorGame.components
{
    public class PlayerController : AbstractComponent
    {

        private float MoveSpeed = 100;
        private Key Forward = Key.W;
        private Key Left = Key.A;
        private Key Back = Key.S;
        private Key Right = Key.D;
        private bool UseGlobalForward = true;
        private bool ApplyVelocity = false;
        private Vector3 velocity;
        private Collider Collider;

        private bool left, right, fwd, back;

        public PlayerController(float speed, bool useGlobalForward)
        {
            MoveSpeed = speed;
            UseGlobalForward = useGlobalForward;
        }

        private string cmdChangeForce(string[] args)
        {
            if (args.Length != 0 && float.TryParse(args[0], out float res))
            {
                MoveSpeed = res;
                return "MoveSpeed Changed to: " + res;
            }

            return "Argument 1 was not a number";
        }

        private string cmdChangeDamp(string[] args)
        {
            if (args.Length != 0 && float.TryParse(args[0], out float res))
            {
                Collider.PhysicsCollider.LinearDamping = res;
                return "Damp Changed to: " + res;
            }

            return "Argument 1 was not a number";
        }

        private string cmdResetPlayer(string[] args)
        {
            Owner.SetLocalPosition(Vector3.Zero);
            return "Player Reset";
        }

        protected override void Awake()
        {
            Collider = Owner.GetComponent<Collider>();
            if (Collider == null)
            {
                this.Log("No Rigid body attached", DebugChannel.Warning);

            }

            GameObject dbg = Owner.World.GetChildWithName("Console");
            if (dbg != null)
            {
                DebugConsoleComponent console = dbg.GetComponent<DebugConsoleComponent>();
                if (console != null)
                {
                    console.AddCommand("preset", cmdResetPlayer);
                    console.AddCommand("pcdamp", cmdChangeDamp);
                    console.AddCommand("pcmove", cmdChangeForce);
                }
            }

        }

        private Vector3 inputDir()
        {
            Vector3 ret = Vector3.Zero;
            if (left) ret -= Vector3.UnitX;
            if (right) ret += Vector3.UnitX;
            if (fwd) ret -= Vector3.UnitZ;
            if (back) ret += Vector3.UnitZ;
            return ret;
        }

        protected override void Update(float deltaTime)
        {
            this.Log("Velocity: " + Collider.PhysicsCollider.LinearVelocity, DebugChannel.Log);
            Vector3 vel = inputDir();
            if (vel != Vector3.Zero)
            {
                vel.Normalize();
                if (UseGlobalForward)
                {
                    vel = new Vector3(new Vector4(vel, 0) * Owner.GetWorldTransform());
                }

                Vector3 vec = vel * deltaTime * MoveSpeed;
                MinorEngine.BEPUutilities.Vector3 v = new MinorEngine.BEPUutilities.Vector3(vec.X, vec.Y, vec.Z);
                Collider.PhysicsCollider.ApplyLinearImpulse(ref v);
            }


        }


        protected override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Forward)
            {
                fwd = true;
            }
            else if (e.Key == Back)
            {
                back = true;
            }
            else if (e.Key == Left)
            {
                left = true;
            }
            else if (e.Key == Right)
            {
                right = true;
            }

        }

        protected override void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Forward)
            {
                fwd = false;
            }
            else if (e.Key == Back)
            {
                back = false;
            }
            else if (e.Key == Left)
            {
                left = false;
            }
            else if (e.Key == Right)
            {
                right = false;
            }
        }
    }
}

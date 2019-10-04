using System;
using System.Collections.Generic;
using System.Text;
using MinorEngine.BEPUphysics.Entities.Prefabs;
using MinorEngine.BEPUphysics.Materials;
using MinorEngine.BEPUutilities;
using MinorEngine.components;
using MinorEngine.debug;
using MinorEngine.engine.components;
using MinorEngine.engine.core;
using MinorEngine.engine.physics;
using MinorEngine.engine.rendering;
using MinorEngine.engine.ui.utils;
using OpenTK.Input;
using Vector3 = OpenTK.Vector3;
using Vector4 = OpenTK.Vector4;

namespace MinorGame.components
{
    public class PlayerController : AbstractComponent
    {
        private Layer raycastLayer;
        private float MoveSpeed = 100;
        private Key Forward = Key.W;
        private Key Left = Key.A;
        private Key Back = Key.S;
        private Key Right = Key.D;
        private bool UseGlobalForward = true;
        private Collider Collider;
        

        private bool left, right, fwd, back;


        public static GameObject[] CreatePlayer(GameModel model, GameModel headModel, Camera cam, GameObject mouseTarget, Layer gamePhysicsLayer, Layer raycastLayer, ShaderProgram shader)
        {



            GameObject player = new GameObject(new Vector3(0, 10, 0), "Player");
            GameObject playerH = new GameObject(new Vector3(0, 10, 0), "PlayerHead");

            //Movement for camera
            OffsetConstraint cameraController = new OffsetConstraint();
            cameraController.Attach(player, new Vector3(0, 50, 10));
            cam.AddComponent(cameraController);

            //Rotation for Player Head depending on mouse position
            cam.AddComponent(new CameraRaycaster(mouseTarget, playerH, raycastLayer));

            //Movement for Player Head
            OffsetConstraint connection = new OffsetConstraint()
            {
                Damping = 0, //Directly over the moving collider, no inertia
                MoveSpeed = 20, //Even less inertia by moving faster in general
            };
            connection.Attach(player, Vector3.UnitY * 30);
            playerH.AddComponent(connection);
            playerH.AddComponent(new MeshRendererComponent(shader, headModel, 1));


            //Player Setup
            Collider collider = new Collider(new Sphere(Vector3.Zero, 1, 1), gamePhysicsLayer);
            collider.PhysicsCollider.Material = new Material(10, 10, 0);
            collider.PhysicsCollider.LinearDamping = 0.99f;
            RigidBodyConstraints constraints = collider.ColliderConstraints;
            //constraints.RotationConstraints = FreezeConstraints.X | FreezeConstraints.Z;
            collider.ColliderConstraints = constraints;

            player.AddComponent(collider);

            player.AddComponent(new MeshRendererComponent(shader, model, 1));
            player.AddComponent(new PlayerController(100, false, raycastLayer));
            return new[] { player, playerH };

        }


        public PlayerController(float speed, bool useGlobalForward, Layer rayCastLayer)
        {
            MoveSpeed = speed;
            UseGlobalForward = useGlobalForward;
            raycastLayer = rayCastLayer;
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

﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using MinorEngine.BEPUphysics.CollisionTests;
using MinorEngine.BEPUphysics.Entities.Prefabs;
using MinorEngine.BEPUphysics.Materials;
using MinorEngine.BEPUphysics.NarrowPhaseSystems.Pairs;
using MinorEngine.components;
using MinorEngine.debug;
using MinorEngine.engine.components;
using MinorEngine.engine.core;
using MinorEngine.engine.physics;
using MinorEngine.engine.rendering;
using MinorGame.scenes;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace MinorGame.components
{
    public class PlayerController : AbstractComponent
    {
        private int raycastLayer;
        private int bulletLayer;
        private float MoveSpeed = 10;
        private Key Forward = Key.W;
        private Key Left = Key.A;
        private Key Back = Key.S;
        private Key Right = Key.D;
        private Key Shoot = Key.Space;
        private bool UseGlobalForward = true;
        private Collider Collider;
        private GameObject nozzle;
        private GameMesh bulletModel;
        private ShaderProgram bulletShader;
        private float BulletLaunchForce = 100;
        private float BulletsPerSecond = 5;
        private static float BulletMass = 1;
        private static bool physicalBullets = true;
        private int hp = 15;
        private float BulletThreshold => 1f / BulletsPerSecond;
        private bool left, right, fwd, back, shoot;


        public static GameObject[] CreatePlayer(Vector3 position, Camera cam)
        {
            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out ShaderProgram shader);


            GameMesh mouseTargetModel = ResourceManager.MeshIO.FileToMesh("models/sphere_smooth.obj");
            mouseTargetModel.SetTextureBuffer( new[] { ResourceManager.TextureIO.FileToTexture("textures/TEST.png") });

            GameObject mouseTarget = new GameObject(Vector3.UnitY * -3, "BG");
            mouseTarget.Scale = new Vector3(1, 1, 1);
            mouseTarget.AddComponent(new MeshRendererComponent(shader, mouseTargetModel, 1));

            GameMesh playerModel = ResourceManager.MeshIO.FileToMesh("models/sphere_smooth.obj");
            playerModel.SetTextureBuffer( new[] { ResourceManager.TextureIO.FileToTexture("textures/TEST.png") });
            GameMesh headModel = ResourceManager.MeshIO.FileToMesh("models/cube_flat.obj");
            headModel.SetTextureBuffer( new[] { ResourceManager.TextureIO.FileToTexture("textures/TEST.png") });
            GameMesh bullet = ResourceManager.MeshIO.FileToMesh("models/cube_flat.obj");
            bullet.SetTextureBuffer(new[] { ResourceManager.TextureIO.FileToTexture("textures/TEST.png") });


            GameObject player = new GameObject(new Vector3(0, 10, 0), "Player");
            GameObject playerH = new GameObject(new Vector3(0, 10, 0), "PlayerHead");

            //Movement for camera
            OffsetConstraint cameraController = new OffsetConstraint();
            cameraController.Attach(player, new Vector3(0, 20, 7));
            cam.AddComponent(cameraController);

            //Rotation for Player Head depending on mouse position
            cam.AddComponent(new CameraRaycaster(mouseTarget, playerH));

            //Movement for Player Head
            OffsetConstraint connection = new OffsetConstraint()
            {
                Damping = 0, //Directly over the moving collider, no inertia
                MoveSpeed = 20, //Even less inertia by moving faster in general
            };
            connection.Attach(player, Vector3.UnitY * 1);
            playerH.AddComponent(connection);
            playerH.Scale = new Vector3(0.5f);
            playerH.AddComponent(new MeshRendererComponent(shader, headModel, 1));




            //Player Setup
            Collider collider = new Collider(new Sphere(Vector3.Zero, 1, 1), LayerManager.NameToLayer("physics"));
            collider.PhysicsCollider.Material = new Material(10, 10, 0);
            collider.PhysicsCollider.LinearDamping = 0.99f;
            RigidBodyConstraints constraints = collider.ColliderConstraints;
            collider.ColliderConstraints = constraints;

            player.AddComponent(collider);

            player.AddComponent(new MeshRendererComponent(shader, playerModel, 1));
            player.AddComponent(new PlayerController(playerH, bullet, shader, 100, false));
            player.LocalPosition = position;



            return new[] { player, playerH };

        }

        protected override void OnInitialCollisionDetected(Collider other, CollidablePairHandler handler)
        {

            RigidBodyConstraints constraints = Collider.ColliderConstraints;
            constraints.PositionConstraints = FreezeConstraints.Y;
            Collider.ColliderConstraints = constraints;
        }

        private void SpawnProjectile()
        {
            MinorEngine.BEPUutilities.Vector3 vel = new Vector3(-Vector4.UnitZ * nozzle.GetWorldTransform()) * BulletLaunchForce;
            Vector3 v = vel;

            GameObject obj = new GameObject(nozzle.LocalPosition + v.Normalized(), "BulletPlayer");
            obj.Rotation = nozzle.Rotation;
            obj.AddComponent(new MeshRendererComponent(bulletShader, bulletModel, 1, false));
            obj.AddComponent(new DestroyTimer(5));
            obj.Scale = new Vector3(0.3f, 0.3f, 1);

            Collider coll = new Collider(new Box(Vector3.Zero, 0.3f, 0.3f, 1, BulletMass), bulletLayer);
            if (!physicalBullets)
            {
                coll.isTrigger = true;
            }
            obj.AddComponent(coll);
            coll.PhysicsCollider.ApplyLinearImpulse(ref vel);
            Owner.World.Add(obj);
        }


        void GameLogic()
        {
            if (hp <= 0)
            {
                GameEngine.Instance.InitializeScene<GameTestScene>();
            }
        }

        public PlayerController(GameObject nozzle, GameMesh bulletModel, ShaderProgram bulletShader, float speed, bool useGlobalForward)
        {
            this.bulletLayer = LayerManager.NameToLayer("physics");
            this.nozzle = nozzle;
            this.bulletModel = bulletModel;
            this.bulletShader = bulletShader;
            MoveSpeed = speed;
            UseGlobalForward = useGlobalForward;
            raycastLayer = LayerManager.NameToLayer("raycast"); ;
        }

        private string cmdBulletMass(string[] args)
        {
            if (args.Length != 0 && float.TryParse(args[0], out float res))
            {
                BulletMass = res;
                return "BulletMass Changed to: " + res;
            }

            return "Argument 1 was not a number";
        }

        private string cmdToggleBulletPhysics(string[] args)
        {
            physicalBullets = !physicalBullets;
            return "Physical: " + physicalBullets;
        }

        private string cmdBulletForce(string[] args)
        {
            if (args.Length != 0 && float.TryParse(args[0], out float res))
            {
                BulletLaunchForce = res;
                return "BulletLaunchForce Changed to: " + res;
            }

            return "Argument 1 was not a number";
        }

        private string cmdBulletPerSecond(string[] args)
        {
            if (args.Length != 0 && float.TryParse(args[0], out float res))
            {
                BulletsPerSecond = res;
                return "BulletsPerSecond Changed to: " + res;
            }

            return "Argument 1 was not a number";
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

        private void ActivateEnemies()
        {
            EnemyComponent.active = true;
        }

        protected override void Awake()
        {


            GameEngine.Instance.World.AddComponent(new GeneralTimer(5, ActivateEnemies));

            Collider = Owner.GetComponent<Collider>();
            if (Collider == null)
            {
                Logger.Log("No Rigid body attached", DebugChannel.Warning);

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
                    console.AddCommand("pcforce", cmdBulletForce);
                    console.AddCommand("pcbrate", cmdBulletPerSecond);
                    console.AddCommand("pcbmass", cmdBulletMass);
                    console.AddCommand("pcbphys", cmdToggleBulletPhysics);

                }
            }

        }
        

        protected override void OnContactCreated(Collider other, CollidablePairHandler handler, ContactData contact)
        {

            if (other.Owner.Name == "BulletEnemy")
            {
                hp--;
                Logger.Log("Current Player HP: " + hp, DebugChannel.Log);
                other.Owner.Destroy();
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

        private float time;
        protected override void Update(float deltaTime)
        {
            GameLogic();
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

            if (shoot)
            {
                time += deltaTime;
                if (time >= BulletThreshold)
                {
                    time = 0;
                    SpawnProjectile();
                }
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
            else if (e.Key == Shoot)
            {
                shoot = true;
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
            else if (e.Key == Shoot)
            {
                shoot = false;
            }
        }
    }
}

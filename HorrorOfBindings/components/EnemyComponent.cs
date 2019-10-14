using System;
using System.Collections.Generic;
using System.Resources;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics;
using Engine.Physics.BEPUphysics.CollisionTests;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Physics.BEPUphysics.Materials;
using Engine.Physics.BEPUphysics.NarrowPhaseSystems.Pairs;
using Engine.Rendering;
using MinorGame.exceptions;
using MinorGame.scenes;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace MinorGame.components
{
    public class EnemyComponent : AbstractComponent
    {
        private GameObject target;
        private int bulletLayer;
        private static float MoveSpeed = 5;
        private bool UseGlobalForward = true;
        private Collider Collider;
        private GameObject nozzle;
        private Mesh bulletModel;
        private ShaderProgram bulletShader;
        private static float BulletLaunchForce = 50;
        private static float BulletsPerSecond = 1;
        private static float BulletMass = 1;
        private static bool physicalBullets = true;
        public static bool active = false;
        private int hp = 3;
        private float BulletThreshold => 1f / BulletsPerSecond;
        public static int enemyCount = 5;


        private static Random rnd = new Random();

        public static void CreateEnemies(Vector2 bounds, int count)
        {
            Vector2 hbounds = bounds / 2;
            for (int j = 0; j < count; j++)
            {
                Vector2 pos = new Vector2((float)rnd.NextDouble() * bounds.X, (float)rnd.NextDouble() * bounds.Y);
                pos -= hbounds;
                Vector3 tilepos = new Vector3(pos.X, 5, pos.Y);
                GameObject[] objs = CreateEnemy(tilepos);
                for (int i = 0; i < objs.Length; i++)
                {
                    GameEngine.Instance.CurrentScene.Add(objs[i]);
                }
            }
        }

        private static Mesh enemyhead_prefab = MeshLoader.FileToMesh("models/cube_flat.obj");
        private static Mesh enemy_prefab = MeshLoader.FileToMesh("models/sphere_smooth.obj");
        private static Mesh bullet_prefab = MeshLoader.FileToMesh("models/cube_flat.obj");
        private static Texture enemyTex, headTex, bulletTex;
        private static bool init;

        public static GameObject[] CreateEnemy(Vector3 position)
        {
            if (!init)
            {
                init = true;
                enemyTex = TextureLoader.FileToTexture("textures/sphereTexture.png");
                headTex = TextureLoader.FileToTexture("textures/enemyHead.jpg");
                bulletTex = TextureLoader.FileToTexture("textures/bulletTexture.png");
            }

            Mesh enemyHeadModel = enemyhead_prefab.Copy();
            Mesh enemyModel = enemy_prefab.Copy();
            Mesh bullet = bullet_prefab.Copy();

            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out ShaderProgram shader);


            GameObject enemyHead = new GameObject(new Vector3(0, 0.5f, 0), "Nozzle");
            GameObject enemy = new GameObject(position, "Enemy");
            Collider collider = new Collider(new Sphere(Vector3.Zero, 1, 1), "physics");
            collider.PhysicsCollider.Material = new Material(10, 10, 0);
            collider.PhysicsCollider.LinearDamping = 0.99f;
            //collider.PhysicsCollider.Gravity = MinorEngine.BEPUutilities.Vector3.Down * 50;
            enemy.AddComponent(collider);


            //Movement for Player Head
            OffsetConstraint connection = new OffsetConstraint()
            {
                Damping = 0, //Directly over the moving collider, no inertia
                MoveSpeed = 20, //Even less inertia by moving faster in general
            };
            connection.Attach(enemy, Vector3.UnitY * 1);

            enemyHead.AddComponent(connection);
            enemyHead.Scale = new Vector3(0.6f);


            enemyHead.AddComponent(new MeshRendererComponent(shader, enemyHeadModel, headTex, 1));
            enemy.AddComponent(new MeshRendererComponent(shader, enemyModel, enemyTex, 1));

            enemy.AddComponent(new EnemyComponent(enemyHead, bullet, shader, 50, false));


            return new[] { enemy, enemyHead };
        }

        protected override void OnInitialCollisionDetected(Collider other, CollidablePairHandler handler)
        {
            //if (other.Owner.Name == "Ground")
            //{

            //    Collider.PhysicsCollider.Gravity = null;
            //    RigidBodyConstraints constraints = Collider.ColliderConstraints;
            //    constraints.PositionConstraints = FreezeConstraints.Y;
            //    Collider.ColliderConstraints = constraints;
            //}
        }

        private void SpawnProjectile()
        {
            Engine.Physics.BEPUutilities.Vector3 vel =
                new Vector3(-Vector4.UnitZ * nozzle.GetWorldTransform()) * BulletLaunchForce;
            Vector3 v = vel;

            GameObject obj = new GameObject(nozzle.LocalPosition + (Engine.Physics.BEPUutilities.Vector3)v.Normalized(), "BulletEnemy");
            obj.Rotation = nozzle.Rotation;

            obj.AddComponent(new MeshRendererComponent(bulletShader, bulletModel, bulletTex, 1,
                false)); //<- Passing false enables using the same mesh for multiple classes
            //Otherwise it would dispose the data when one object is destroyed
            //Downside is that we need to store a reference somewhere and dispose them manually
            obj.AddComponent(new DestroyTimer(5));
            obj.Scale = new Vector3(0.3f, 0.3f, 1);

            Collider coll = new Collider(new Box(Vector3.Zero, 0.3f, 0.3f, 1, BulletMass), bulletLayer);
            if (!physicalBullets)
            {
                coll.IsTrigger = true;
            }

            obj.AddComponent(coll);
            coll.PhysicsCollider.ApplyLinearImpulse(ref vel);
            Owner.Scene.Add(obj);
        }

        private void GameLogic()
        {
            if (hp <= 0)
            {
                Owner.Destroy();
            }
        }

        private static int enemiesAlive;

        public EnemyComponent(GameObject nozzle, Mesh bulletModel, ShaderProgram bulletShader, float speed,
            bool useGlobalForward)
        {
            bulletLayer = LayerManager.NameToLayer("physics");
            this.nozzle = nozzle;
            this.bulletModel = bulletModel;
            this.bulletShader = bulletShader;
            MoveSpeed = speed;
            UseGlobalForward = useGlobalForward;
        }

        protected override void OnContactCreated(Collider other, CollidablePairHandler handler, ContactData contact)
        {
            if (other.Owner.Name == "BulletPlayer")
            {
                hp--;
                Logger.Log("Current Enemy HP: " + hp, DebugChannel.Log);
                other.Owner.Destroy();
            }
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

        private string cmdActivate(string[] args)
        {
            active = !active;
            return "Enemy Active: " + active;
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

        protected override void Awake()
        {
            enemiesAlive++;
            target = Owner.Scene.GetChildWithName("Player");
            if (target == null)
            {
                Logger.Crash(new GameException("Target is Null"), false);
            }

            Collider = Owner.GetComponent<Collider>();
            if (Collider == null)
            {
                Logger.Log("No Rigid body attached", DebugChannel.Warning);
            }

            GameObject dbg = Owner.Scene.GetChildWithName("Console");
            if (dbg != null)
            {
                DebugConsoleComponent console = dbg.GetComponent<DebugConsoleComponent>();
                if (console != null)
                {
                    console.AddCommand("ereset", cmdResetPlayer);
                    console.AddCommand("ecdamp", cmdChangeDamp);
                    console.AddCommand("ecmove", cmdChangeForce);
                    console.AddCommand("ecforce", cmdBulletForce);
                    console.AddCommand("ecbrate", cmdBulletPerSecond);
                    console.AddCommand("eactive", cmdActivate);
                    console.AddCommand("pcbmass", cmdBulletMass);
                    console.AddCommand("pcbphys", cmdToggleBulletPhysics);
                }
            }
        }


        private float time;

        protected override void Update(float deltaTime)
        {
            GameLogic();
            if (!active)
            {
                return;
            }

            Vector3 vel = GetWalkDirection();
            if (vel != Vector3.Zero)
            {
                vel.Normalize();
                if (UseGlobalForward)
                {
                    vel = new Vector3(new Vector4(vel, 0) * Owner.GetWorldTransform());
                }

                Vector3 vec = vel * deltaTime * MoveSpeed;
                Engine.Physics.BEPUutilities.Vector3 v = new Vector3(vec.X, vec.Y, vec.Z);
                Collider.PhysicsCollider.ApplyLinearImpulse(ref v);
            }

            ApplyRotation();

            if (WantsToShoot())
            {
                time += deltaTime;
                if (time >= BulletThreshold)
                {
                    time = 0;
                    SpawnProjectile();
                }
            }
        }

        private void ApplyRotation()
        {
            nozzle.LookAt((OpenTK.Vector3)nozzle.LocalPosition + GetWalkDirection());
        }

        private bool WantsToShoot()
        {
            return Vector3.Distance(target.LocalPosition, Owner.LocalPosition) < 10f;
        }


        private Vector3 GetWalkDirection()
        {
            Vector3 ret = (target.LocalPosition - Owner.LocalPosition);
            return ret.Normalized();
        }

        private void activateEnemies()
        {
            active = true;
        }

        protected override void OnDestroy()
        {
            enemiesAlive--;

            if (enemiesAlive == 0)
            {
                active = false;
                GameEngine.Instance.CurrentScene.AddComponent(new GeneralTimer(5, activateEnemies));
                PlayerController.wavesSurvived++;
                enemyCount *= 2;
                CreateEnemies(new Vector2(50, 50), enemyCount);
            }
        }
    }
}
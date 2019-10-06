using System;
using System.Collections.Generic;
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
        private GameMesh bulletModel;
        private ShaderProgram bulletShader;
        private static float BulletLaunchForce = 50;
        private static float BulletsPerSecond = 1;
        private static float BulletMass = 1;
        private static bool physicalBullets = true;
        public static bool active = false;
        private int hp = 10;
        private float BulletThreshold => 1f / BulletsPerSecond;


        public static GameObject[] CreateEnemy(Vector3 position)
        {
            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out ShaderProgram shader);

            GameMesh enemyHeadModel = ResourceManager.MeshIO.FileToMesh("models/cube_flat.obj");
            GameMesh enemyModel = ResourceManager.MeshIO.FileToMesh("models/sphere_smooth.obj");

            enemyModel.SetTextureBuffer(new[] { ResourceManager.TextureIO.FileToTexture("textures/ground4k.png") });
            enemyHeadModel.SetTextureBuffer(new[] { ResourceManager.TextureIO.FileToTexture("textures/ground4k.png") });

            GameObject enemyHead = new GameObject(new OpenTK.Vector3(0, 0.5f, 0), "Nozzle");
            GameObject enemy = new GameObject(position, "Enemy");
            Collider collider = new Collider(new Sphere(OpenTK.Vector3.Zero, 1, 1), "physics");
            collider.PhysicsCollider.Material = new Material(10, 10, 0);
            collider.PhysicsCollider.LinearDamping = 0.99f;
            enemy.AddComponent(collider);


            //Movement for Player Head
            OffsetConstraint connection = new OffsetConstraint()
            {
                Damping = 0, //Directly over the moving collider, no inertia
                MoveSpeed = 20, //Even less inertia by moving faster in general
            };
            connection.Attach(enemy, Vector3.UnitY * 1);

            enemyHead.AddComponent(connection);
            enemyHead.Scale = new Vector3(0.5f);

            GameMesh bullet = ResourceManager.MeshIO.FileToMesh("models/cube_flat.obj");
            bullet.SetTextureBuffer(new []{ ResourceManager.TextureIO.FileToTexture("textures/TEST.png") });


            enemyHead.AddComponent(new MeshRendererComponent(shader, enemyHeadModel, 1));
            enemy.AddComponent(new MeshRendererComponent(shader, enemyModel, 1));

            enemy.AddComponent(new EnemyComponent(enemyHead, bullet, shader, 50, false));


            return new[] { enemy, enemyHead };

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

            GameObject obj = new GameObject(nozzle.LocalPosition + v.Normalized(), "BulletEnemy");
            obj.Rotation = nozzle.Rotation;

            obj.AddComponent(new MeshRendererComponent(bulletShader, bulletModel, 1, false));    //<- Passing false enables using the same mesh for multiple classes
                                                                                                 //Otherwise it would dispose the data when one object is destroyed
                                                                                                 //Downside is that we need to store a reference somewhere and dispose them manually
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
                Owner.Destroy();
            }
        }

        public EnemyComponent(GameObject nozzle, GameMesh bulletModel, ShaderProgram bulletShader, float speed, bool useGlobalForward)
        {
            this.bulletLayer = LayerManager.NameToLayer("physics");
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
            target = Owner.World.GetChildWithName("Player");
            if (target == null)
            {
                Logger.Crash(new GameException("Target is Null"), false);
            }
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
            if (!active) return;
            Vector3 vel = GetWalkDirection();
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

        void ApplyRotation()
        {
            nozzle.LookAt(nozzle.LocalPosition + GetWalkDirection());
        }

        bool WantsToShoot()
        {
            return Vector3.Distance(target.LocalPosition, Owner.LocalPosition) < 10f;
        }


        Vector3 GetWalkDirection()
        {
            return (target.LocalPosition - Owner.LocalPosition).Normalized();
        }
    }
}
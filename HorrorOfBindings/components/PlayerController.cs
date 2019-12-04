using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Assimp;
using Engine.Audio;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics.CollisionTests;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Physics.BEPUphysics.NarrowPhaseSystems.Pairs;
using Engine.Physics.BEPUphysics.PositionUpdating;
using Engine.Physics.BEPUutilities;
using Engine.Rendering;
using HorrorOfBindings.mapgenerator;
using HorrorOfBindings.scenes;
using HorrorOfBindings.ui;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ArbTextureNonPowerOfTwo = OpenTK.Graphics.OpenGL.ArbTextureNonPowerOfTwo;
using Material = Engine.Physics.BEPUphysics.Materials.Material;
using Mesh = Engine.DataTypes.Mesh;
using Vector2 = OpenTK.Vector2;
using Vector3 = OpenTK.Vector3;
using Vector4 = OpenTK.Vector4;

namespace HorrorOfBindings.components
{
    public class PlayerController : AbstractComponent
    {
        private float GravityIncUngrounded = 5;
        private float CurrentGravity = 0;
        private int raycastLayer;
        private int bulletLayer;
        private float MoveSpeed = 10;
        private float JumpForce = 200;
        private Key Forward = Key.W;
        private Key Left = Key.A;
        private Key Back = Key.S;
        private Key Right = Key.D;
        private Key Jump = Key.Space;
        private bool UseGlobalForward = true;
        private Collider Collider;
        private GameObject nozzle;
        private Mesh bulletModel;
        private Texture bulletTexture;
        private ShaderProgram bulletShader;
        private float BulletLaunchForce = 100;
        private float BulletsPerSecond => wavesSurvived * baseBulletsPerSecond;
        private static float BulletMass = 1;
        private static bool physicalBullets = true;
        private int hp = 15;
        private int maxHP = 15;
        private float BulletThreshold => 1f / BulletsPerSecond;
        private bool left, right, fwd, back, jump;
        private static float baseBulletsPerSecond = 5;
        public static int wavesSurvived = 1;
        private bool Grounded = false;
        private AudioSourceComponent AudioSource;
        private static AudioFile JumpSound, SpawnSound, ShootSound, ShootSound2;

        public delegate void onHpChange(float ratio);

        public static onHpChange OnHPChange;


        public static GameObject[] CreatePlayer(Vector3 position, BasicCamera cam)
        {
            Mesh mouseTargetModel = MeshLoader.FileToMesh("assets/models/sphere_smooth.obj");


            GameObject mouseTarget = new GameObject(Vector3.UnitY * -3, "BG");
            mouseTarget.Scale = new Vector3(1, 1, 1);
            mouseTarget.AddComponent(new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, mouseTargetModel,
                Prefabs.White, 1));

            Mesh playerModel = MeshLoader.FileToMesh("assets/models/sphere_smooth.obj");
            Mesh headModel = MeshLoader.FileToMesh("assets/models/cube_flat.obj");
            Mesh bullet = MeshLoader.FileToMesh("assets/models/cube_flat.obj");


            GameObject player = new GameObject(new Vector3(0, 10, 0), "Player");
            GameObject playerH = new GameObject(new Vector3(0, 10, 0), "PlayerHead");
            GameObject lightS = new GameObject(Vector3.UnitY * 2f, "Light");
            playerH.Add(lightS);
            lightS.AddComponent(new LightComponent() {AmbientContribution = 0f});
            lightS.LocalPosition = Vector3.UnitY * 4f;


            //Movement for camera
            OffsetConstraint cameraController = new OffsetConstraint {Damping = 0, MoveSpeed = 2};
            cameraController.Attach(player, new Vector3(0, 15, 5));
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
            playerH.Scale = new Vector3(0.6f);
            playerH.AddComponent(new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, headModel,
                TextureLoader.FileToTexture("assets/textures/playerHead.png"), 1));


            //Player Setup
            Collider collider = new Collider(new Sphere(Vector3.Zero, 1, 10), LayerManager.NameToLayer("physics"));
            collider.PhysicsCollider.PositionUpdateMode = PositionUpdateMode.Continuous;
            collider.PhysicsCollider.Material = new Material(1.5f, 1.5f, 0);
            collider.PhysicsCollider.LinearDamping = 0.99f;

            player.AddComponent(collider);


            player.AddComponent(new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, playerModel,
                TextureGenerator.GetPlayerTexture(), 1));

            AudioSourceComponent source = new AudioSourceComponent();
            AudioLoader.TryLoad("assets/audio/ShootSound.wav", out ShootSound);
            AudioLoader.TryLoad("assets/audio/ShootSound2.wav", out ShootSound2);
            AudioLoader.TryLoad("assets/audio/SpawnSound.wav", out SpawnSound);
            AudioLoader.TryLoad("assets/audio/JumpSound.wav", out JumpSound);
            source.Clip = SpawnSound;
            source.Play();
            source.UpdatePosition = false;
            source.Gain = 0.5f;
            player.AddComponent(source);

            player.AddComponent(new PlayerController(playerH, bullet,
                TextureLoader.ColorToTexture(Color.Red), DefaultFilepaths.DefaultLitShader, 650, false, source));
            player.LocalPosition = position;


            GameObject playerUI = new GameObject("PlayerHUD");
            playerUI.AddComponent(new PlayerHUD());


            return new[] {player, playerH, playerUI};
        }

        protected override void OnInitialCollisionDetected(Collider other, CollidablePairHandler handler)
        {
            if (other.Owner.Name == "Ground")
            {
                Grounded = true;
            }
        }

        protected override void OnCollisionEnded(Collider other, CollidablePairHandler handler)
        {
            if (other.Owner.Name == "Ground")
            {
                Grounded = false;
            }
        }

        private void SpawnProjectile()
        {
            Engine.Physics.BEPUutilities.Vector3 v = Vector3.Zero;
            if (Grounded || !CameraRaycaster.ObjectUnderMouse(GameEngine.Instance.CurrentScene.Camera.LocalPosition,
                    out KeyValuePair<Collider, RayHit> hit))
            {
                Engine.Physics.BEPUutilities.Vector3 vel =
                    new Vector3(-Vector4.UnitZ * nozzle.GetWorldTransform()) * BulletLaunchForce;
                v = vel;
            }
            else
            {
                Vector3 pos = hit.Value.Location;
                //pos.Y = looker.LocalPosition.Y;
                nozzle.LookAt(pos);
                Engine.Physics.BEPUutilities.Vector3 vel =
                    new Vector3(-Vector4.UnitZ * nozzle.GetWorldTransform()) * BulletLaunchForce;
                v = vel;
            }

            Vector3 v1 = v;
            GameObject obj =
                new GameObject(nozzle.LocalPosition + (Engine.Physics.BEPUutilities.Vector3) v1.Normalized(),
                    "BulletPlayer");
            obj.Rotation = nozzle.Rotation;
            obj.AddComponent(new LitMeshRendererComponent(bulletShader, bulletModel, bulletTexture, 1, false));
            obj.AddComponent(new DestroyTimer(5));
            obj.Scale = new Vector3(0.3f, 0.3f, 1);

            Collider coll = new Collider(new Box(Vector3.Zero, 0.3f, 0.3f, 1, BulletMass), bulletLayer);
            coll.PhysicsCollider.PositionUpdateMode = PositionUpdateMode.Continuous;
            if (!physicalBullets)
            {
                coll.IsTrigger = true;
            }

            obj.AddComponent(coll);
            coll.PhysicsCollider.ApplyLinearImpulse(ref v);
            Owner.Scene.Add(obj);
            //AudioSource.Clip = ShootSound;
            //AudioSource.Play();
            AudioSource.Clip = BulletsPerSecond < 20 ? ShootSound : ShootSound2;
            AudioSource.Play();
        }

        private int lastEnemySpawn = 0;
        private Random rnd = new Random();

        private void GameLogic()
        {
            if (hp <= 0)
            {
                wavesSurvived = 1;
                EnemyComponent.enemyCount = 5;
                GameEngine.Instance.InitializeScene<GameTestScene>();
            }

            int pos = (int) Owner.LocalPosition.Z;
            if (pos < lastEnemySpawn - 10)
            {
                lastEnemySpawn = pos;
                if (rnd.Next(0, 2) == 0)
                {
                    Logger.Log("Spawning Enemies..", DebugChannel.Log, 10);
                    for (int i = 0; i < 6; i++)
                    {
                        GameObject[] objs = EnemyComponent.CreateEnemy(new Vector3(i * 4, 5, pos - 50));
                        for (int j = 0; j < objs.Length; j++)
                        {
                            GameEngine.Instance.CurrentScene.Add(objs[j]);
                        }
                    }
                }
            }
        }

        public PlayerController(GameObject nozzle, Mesh bulletModel, Texture bulletTexture, ShaderProgram bulletShader,
            float speed, bool useGlobalForward, AudioSourceComponent audioSource)
        {
            AudioSource = audioSource;
            this.bulletTexture = bulletTexture;
            bulletLayer = LayerManager.NameToLayer("physics");
            this.nozzle = nozzle;
            this.bulletModel = bulletModel;
            this.bulletShader = bulletShader;
            MoveSpeed = speed;
            UseGlobalForward = useGlobalForward;
            raycastLayer = LayerManager.NameToLayer("raycast");
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
                baseBulletsPerSecond = res;
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
            Collider.PhysicsCollider.Position = Engine.Physics.BEPUutilities.Vector3.UnitY * 4;
            ColliderConstraints constraints = Collider.ColliderConstraints;
            constraints.PositionConstraints = FreezeConstraints.NONE;
            Collider.ColliderConstraints = constraints;
            return "Player Reset";
        }


        protected override void Awake()
        {
            Collider = Owner.GetComponent<Collider>();
            if (Collider == null)
            {
                Logger.Log("No Rigid body attached", DebugChannel.Warning, 10);
            }

            GameObject dbg = Owner.Scene.GetChildWithName("Console");
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

            lastEnemySpawn = (int) Owner.LocalPosition.Z;
        }


        protected override void OnContactCreated(Collider other, CollidablePairHandler handler, ContactData contact)
        {
            if (other.Owner.Name == "BulletEnemy")
            {
                hp--;
                OnHPChange?.Invoke(hp / (float) maxHP);
                Logger.Log("Current Player HP: " + hp, DebugChannel.Log, 5);
                other.Owner.Destroy();
            }
        }

        private Vector3 inputDir()
        {
            Vector3 ret = Vector3.Zero;
            if (left)
            {
                ret -= Vector3.UnitX;
            }

            if (right)
            {
                ret += Vector3.UnitX;
            }

            if (fwd)
            {
                ret -= Vector3.UnitZ;
            }

            if (back)
            {
                ret += Vector3.UnitZ;
            }


            return ret;
        }

        private float time;

        private Vector3 computeJumpAcc()
        {
            if (Grounded && jump)
            {
                jump = false;
                return Vector3.UnitY * JumpForce;
            }

            return Vector3.Zero;
        }

        protected override void Update(float deltaTime)
        {
            GameLogic();
            Vector3 vel = inputDir();
            if (vel != Vector3.Zero)
            {
                if (vel != Vector3.Zero)
                {
                    vel.Normalize();
                }

                if (UseGlobalForward)
                {
                    vel = new Vector3(new Vector4(vel, 0) * Owner.GetWorldTransform());
                }

                Vector3 vec = new Vector3(vel.X * deltaTime * MoveSpeed, vel.Y * deltaTime * JumpForce,
                    vel.Z * deltaTime * MoveSpeed);


                //vec.Y -= CurrentGravity;

                Engine.Physics.BEPUutilities.Vector3 v = new Engine.Physics.BEPUutilities.Vector3(vec.X, vec.Y, vec.Z);
                Collider.PhysicsCollider.ApplyLinearImpulse(ref v);
            }

            if (Grounded)
            {
                CurrentGravity = 0;
            }
            else
            {
                CurrentGravity += GravityIncUngrounded * deltaTime;
            }


            Engine.Physics.BEPUutilities.Vector3 grav = new Vector3(0, -CurrentGravity, 0);
            Collider.PhysicsCollider.ApplyLinearImpulse(ref grav);

            if (jump && Grounded)
            {
                Engine.Physics.BEPUutilities.Vector3 jumpAcc = computeJumpAcc();
                Collider.PhysicsCollider.ApplyLinearImpulse(ref jumpAcc);

                AudioSource.Clip = JumpSound;
                AudioSource.Play();
            }

            if (Mouse.GetCursorState().LeftButton == ButtonState.Pressed)
            {
                time += deltaTime;
                if (time >= BulletThreshold)
                {
                    time = 0;
                    SpawnProjectile();
                }
            }
            else
            {
                {
                    time = 0;
                }
            }
        }

        protected override void OnKeyPress(object sender, KeyPressEventArgs e)
        {
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
            else if (e.Key == Jump)
            {
                jump = true;
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
using System.Collections.Generic;
using MinorEngine.BEPUphysics.Entities.Prefabs;
using MinorEngine.BEPUphysics.Materials;
using MinorEngine.components;
using MinorEngine.engine.core;
using MinorEngine.engine.physics;
using MinorEngine.engine.rendering;
using MinorEngine.engine.ui.utils;
using MinorGame.components;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MinorGame.scenes
{
    public class GameTestScene : AbstractScene
    {

        protected override void InitializeScene()
        {
            
            GameModel bgBox = new GameModel("models/cube_flat.obj");

            bgBox.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/ground4k.png") });


            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out ShaderProgram shader);

            DebugConsoleComponent dbg = DebugConsoleComponent.CreateConsole().GetComponent<DebugConsoleComponent>();

            GameEngine.Instance.World.Add(dbg.Owner);

            //Ground
            GameModel groundModel = new GameModel("models/cube_flat.obj");

            GameObject ground = new GameObject("Ground");
            groundModel.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/ground4k.png") });

            ground.AddComponent(new MeshRendererComponent(shader, groundModel, 1));
            Collider groundColl = new Collider(new Box(MinorEngine.BEPUutilities.Vector3.Zero, 100, 2, 100));
            groundColl.PhysicsCollider.Material = new Material(0, 0, 0);
            ground.AddComponent(groundColl);

            ground.Scale(new Vector3(50, 1, 50));
            GameEngine.Instance.World.Add(ground);

            //Player

            GameModel playerModel = new GameModel("models/sphere_smooth.obj");
            playerModel.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/TEST.png") });

            GameObject player = new GameObject(new Vector3(0, 10, 0), "Player");


            RigidBodyConstraints playerConstraints = new RigidBodyConstraints
            {
                //RotationConstraints = FreezeConstraints.X | FreezeConstraints.Z,
            };

            Collider collider = new Collider(new Sphere(Vector3.Zero, 1, 1));
            //collider.PhysicsCollider.Gravity = MinorEngine.BEPUutilities.Vector3.Down;
            
            collider.PhysicsCollider.Material = new Material(0, 0, 0);
            collider.ColliderConstraints = playerConstraints;
            collider.PhysicsCollider.LinearDamping = 0.99f;
            
            player.AddComponent(collider);

            player.AddComponent(new MeshRendererComponent(shader, playerModel, 1));
            player.AddComponent(new PlayerController(100, true));

            GameEngine.Instance.World.Add(player);

            Camera c = new Camera(
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                    GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);
            c.Rotate(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-75));
            c.Translate(new Vector3(0, 75, 15));


            GameEngine.Instance.World.Add(c);
            GameEngine.Instance.World.SetCamera(c);
        }

        public override void OnDestroy()
        {

        }

        public override void Update(float deltaTime)
        {

        }
    }
}
using System.Collections.Generic;
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
            Physics.AddBoxStatic(System.Numerics.Vector3.UnitY * -4, new System.Numerics.Vector3(50, 10, 50), 1, 1);
            GameObject ground = new GameObject("Ground");
            groundModel.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/ground4k.png") });

            ground.AddComponent(new MeshRendererComponent(shader, groundModel, 1));

            ground.Scale(new Vector3(50, 1, 50));
            GameEngine.Instance.World.Add(ground);

            //Player

            GameModel playerModel = new GameModel("models/sphere_smooth.obj");
            playerModel.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/TEST.png") });

            GameObject player = new GameObject(new Vector3(0, 100, 0), "Player");
            PhysicsMaterial pm = new PhysicsMaterial(1);
            AbstractDynamicCollider collider = new SphereCollider(pm, 1, 1, 1);
            player.AddComponent(collider);
            RigidBodyConstraints playerConstraints = new RigidBodyConstraints
            {
                FixRotation = true
            };
            RigidBodyComponent rb = new RigidBodyComponent(collider, playerConstraints);

            player.AddComponent(rb);
            player.AddComponent(new MeshRendererComponent(shader, playerModel, 1));
            player.AddComponent(new PlayerController(1, false));

            GameEngine.Instance.World.Add(player);

            Camera c = new Camera(
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                    GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);
            c.Rotate(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-25));
            c.Translate(new Vector3(0, 15, 15));


            player.Add(c);
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
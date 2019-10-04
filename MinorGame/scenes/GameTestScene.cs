using System.Collections.Generic;
using MinorEngine.BEPUphysics.Entities.Prefabs;
using MinorEngine.BEPUphysics.Materials;
using MinorEngine.BEPUphysics.PositionUpdating;
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

            Layer raycastLayer = new Layer(1, 2);
            Layer hybridLayer = new Layer(1, 1 | 2);
            Layer gamePhysicsLayer = new Layer(1, 1);

            GameModel bgBox = new GameModel("models/cube_flat.obj");
            GameModel sphere = new GameModel("models/sphere_smooth.obj");

            bgBox.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/ground4k.png") });
            sphere.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/ground4k.png") });


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
            Collider groundColl = new Collider(new Box(MinorEngine.BEPUutilities.Vector3.Zero, 100, 2, 100), hybridLayer);
            groundColl.PhysicsCollider.Material = new Material(10, 10, 0);
            ground.AddComponent(groundColl);

            ground.Scale(new Vector3(50, 1, 50));
            GameEngine.Instance.World.Add(ground);

            //Player
            GameObject mouseTarget = new GameObject(Vector3.UnitY * -3, "BG");
            mouseTarget.Scale(new Vector3(1, 1, 1));
            mouseTarget.AddComponent(new MeshRendererComponent(shader, sphere, 1));

            GameEngine.Instance.World.Add(mouseTarget);


            GameModel playerModel = new GameModel("models/sphere_smooth.obj");
            GameModel headModel = new GameModel("models/cube_flat.obj");
            playerModel.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/TEST.png") });
            headModel.SetTextureBuffer(0, new[] { TextureProvider.Load("textures/TEST.png") });

            Camera c = new Camera(
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                    GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);
            c.Rotate(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-75));
            c.Translate(new Vector3(0, 75, 15));

            GameObject[] objs = PlayerController.CreatePlayer(sphere, headModel, c, mouseTarget, gamePhysicsLayer, raycastLayer, shader);

            for (int i = 0; i < objs.Length; i++)
            {

                GameEngine.Instance.World.Add(objs[i]);
            }

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
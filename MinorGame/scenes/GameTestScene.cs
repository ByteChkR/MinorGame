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
            LayerManager.RegisterLayer("raycast", new Layer(1, 2));
            int hybridLayer = LayerManager.RegisterLayer("hybrid", new Layer(1, 1 | 2));
            LayerManager.RegisterLayer("physics", new Layer(1, 1));

            GameMesh bgBox = ResourceManager.MeshIO.FileToMesh("models/cube_flat.obj");

            bgBox.SetTextureBuffer(new[] { ResourceManager.TextureIO.FileToTexture("textures/ground4k.png") });



            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out ShaderProgram shader);

            DebugConsoleComponent dbg = DebugConsoleComponent.CreateConsole().GetComponent<DebugConsoleComponent>();

            GameEngine.Instance.World.Add(dbg.Owner);

            //Ground
            GameMesh groundModel = ResourceManager.MeshIO.FileToMesh("models/cube_flat.obj");

            GameObject ground = new GameObject("Ground");
            groundModel.SetTextureBuffer(new[] { ResourceManager.TextureIO.FileToTexture("textures/ground4k.png") });

            ground.AddComponent(new MeshRendererComponent(shader, groundModel, 1));
            Collider groundColl = new Collider(new Box(MinorEngine.BEPUutilities.Vector3.Zero, 100, 2, 100), hybridLayer);
            groundColl.PhysicsCollider.Material = new Material(10, 10, 0);
            ground.AddComponent(groundColl);

            ground.Scale = new Vector3(50, 1, 50);
            GameEngine.Instance.World.Add(ground);


            Camera c = new Camera(
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                    GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);
            c.Rotate(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-75));
            c.Translate(new Vector3(0, 75, 15));

            GameObject[] objs = PlayerController.CreatePlayer(Vector3.UnitY * 2, c);

            for (int i = 0; i < objs.Length; i++)
            {

                GameEngine.Instance.World.Add(objs[i]);
            }


            for (int j = 0; j < 5; j++)
            {
                objs = EnemyComponent.CreateEnemy(Vector3.UnitY * 2 + Vector3.UnitZ * -5 * (j + 1));
                for (int i = 0; i < objs.Length; i++)
                {
                    GameEngine.Instance.World.Add(objs[i]);
                }
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
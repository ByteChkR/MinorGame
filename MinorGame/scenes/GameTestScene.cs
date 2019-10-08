using System;
using System.Collections.Generic;
using MinorEngine.BEPUphysics.Materials;
using MinorEngine.components;
using MinorEngine.engine.components;
using MinorEngine.engine.core;
using MinorEngine.engine.physics;
using MinorEngine.engine.rendering;
using MinorGame.components;
using MinorGame.mapgenerator;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Quaternion = MinorEngine.BEPUutilities.Quaternion;

namespace MinorGame.scenes
{
    public class GameTestScene : AbstractScene
    {
        private Camera camera;
        private GameObject groundObj;
        private ShaderProgram shader;
        private void LoadGameScene(Camera c)
        {
            c.Rotate(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-75));
            c.Translate(new Vector3(0, 75, 15));





            GameObject[] objs = PlayerController.CreatePlayer(Vector3.UnitY * 2, c);

            for (int i = 0; i < objs.Length; i++)
            {

                GameEngine.Instance.World.Add(objs[i]);
            }

            EnemyComponent.CreateEnemies(new Vector2(50, 50), 5);


        }



        private static GameObject[] objects = new GameObject[0];

        private void CreateMap(System.Drawing.Bitmap input, ShaderProgram prog)
        {
            byte[] data = new byte[input.Height * input.Width];
            for (int w = 0; w < input.Width; w++)
            {
                for (int h = 0; h < input.Height; h++)
                {
                    data[w * input.Height + h] = input.GetPixel(w, h).R;
                }
            }

            foreach (var gameObject in objects)
            {
                gameObject.Destroy();
            }

            objects = TileCreator.CreateTileMap(TileCreator.CreateObject_Box, data, input.Width, input.Height, 3, 2, new Vector2(input.Width, input.Height), prog);
            foreach (var gameObject in objects)
            {
                GameEngine.Instance.World.Add(gameObject);
            }

            groundObj.Destroy();
            groundObj = CreateGround(new Vector3(input.Width, 2, input.Height) / 2);
            GameEngine.Instance.World.Add(groundObj);

        }

        private GameObject CreateGround(Vector3 scale)
        {
            GameObject ret = TileCreator.CreateCube(Vector3.Zero, scale, Quaternion.Identity,
                ResourceManager.TextureIO.FileToTexture("textures/ground4k.png"), shader);
            ret.Name = "Ground";
            Collider groundColl = ret.GetComponent<Collider>();
            groundColl.PhysicsCollider.Material = new Material(10, 10, 0);
            return ret;
        }

        private string cmd_Move(string[] args)
        {
            if (args.Length < 3) return "Not Enough arguments";
            if (!int.TryParse(args[0], out int X))
            {
                return "Could not parse X";
            }
            if (!int.TryParse(args[1], out int Y))
            {
                return "Could not parse Y";
            }
            if (!int.TryParse(args[2], out int Z))
            {
                return "Could not parse Z";
            }

            camera.Translate(new Vector3(X, Y, Z));

            return "Done";
        }

        private string cmd_Rotate(string[] args)
        {
            if (args.Length < 4) return "Not Enough arguments";
            if (!int.TryParse(args[0], out int X))
            {
                return "Could not parse X";
            }
            if (!int.TryParse(args[1], out int Y))
            {
                return "Could not parse Y";
            }
            if (!int.TryParse(args[2], out int Z))
            {
                return "Could not parse Z";
            }
            if (!int.TryParse(args[3], out int angle))
            {
                return "Could not parse Z";
            }

            camera.Rotate(new Vector3(X, Y, Z), angle);

            return "Done";
        }

        private void LoadTestScene(Camera c)
        {
            this.camera = c;

            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out ShaderProgram shader);
            DebugConsoleComponent dbg = DebugConsoleComponent.CreateConsole().GetComponent<DebugConsoleComponent>();
            dbg.AddCommand("mov", cmd_Move);
            dbg.AddCommand("rot", cmd_Rotate);
            GameEngine.Instance.World.Add(dbg.Owner);

            WFCMapGenerator preview = WFCMapGenerator.CreateWFCPreview(Vector3.Zero, "WFCTiles", false, (input) => CreateMap(input, shader)).GetComponent<WFCMapGenerator>();



            GameEngine.Instance.World.Add(preview.Owner);

        }

        protected override void InitializeScene()
        {
            LayerManager.RegisterLayer("raycast", new Layer(1, 2));
            int hybridLayer = LayerManager.RegisterLayer("hybrid", new Layer(1, 1 | 2));
            LayerManager.RegisterLayer("physics", new Layer(1, 1));

            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out shader);

            groundObj = CreateGround(new Vector3(50, 1, 50));

            GameEngine.Instance.World.Add(groundObj);

            Camera c = new Camera(
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                    GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);

            LoadTestScene(c);
            LoadGameScene(camera);

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
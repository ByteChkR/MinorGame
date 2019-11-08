using System.Collections.Generic;
using System.Drawing;
using Engine.Audio;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.OpenCL.DotNetCore.Memory;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Materials;
using Engine.Rendering;
using Engine.WFC;
using MinorGame.components;
using MinorGame.mapgenerator;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Bitmap = System.Drawing.Bitmap;

namespace MinorGame.scenes
{
    public class GameTestScene : AbstractScene
    {
        private BasicCamera camera;
        private GameObject groundObj;
        public static ShaderProgram TextureShader;
        public static ShaderProgram TextShader;
        public static ShaderProgram UIImageShader;

        private void LoadGameScene(BasicCamera c)
        {
            c.Rotate(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-75));
            c.Translate(new Vector3(0, 75, 15));


            GameObject[] objs = PlayerController.CreatePlayer(Vector3.UnitY * 2, c);

            for (int i = 0; i < objs.Length; i++)
            {
                GameEngine.Instance.CurrentScene.Add(objs[i]);
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

            foreach (GameObject gameObject in objects)
            {
                gameObject.Destroy();
            }

            objects = TileCreator.CreateTileMap(TileCreator.CreateObject_Box, data, input.Width, input.Height, 3, 4,
                new Vector2(input.Width * 4, input.Height * 4), prog);
            foreach (GameObject gameObject in objects)
            {
                Add(gameObject);
            }

            groundObj?.Destroy();
            groundObj = CreateGround(input, new Vector3(input.Width * 4, 2, input.Height * 4) / 2);
            Add(groundObj);
        }

        private GameObject CreateGround(Bitmap mapLayout, Vector3 scale)
        {
            Texture tex = TextureLoader.BitmapToTexture(new Bitmap(mapLayout, 512, 512));
            Texture texS = TextureLoader.BitmapToTexture(new Bitmap(mapLayout, 512, 512));




            TextureGenerator.CreateGroundTexture(tex, texS);
            GameObject ret = TileCreator.CreateCube(Vector3.Zero, scale, Quaternion.Identity,
                tex, TextureShader, new Vector2(scale.X/4, scale.Z/4), Vector2.Zero, texS);
            ret.Name = "Ground";
            Collider groundColl = ret.GetComponent<Collider>();
            groundColl.PhysicsCollider.Material = new Material(10, 10, 0);
            return ret;
        }
        

        private string cmd_Move(string[] args)
        {
            if (args.Length < 3)
            {
                return "Not Enough arguments";
            }

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
            if (args.Length < 4)
            {
                return "Not Enough arguments";
            }

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
        private string cmd_ReLoadScene(string[] args)
        {
            GameEngine.Instance.InitializeScene<GameTestScene>();
            TextureGenerator.Reset();
            return "Reloaded";
        }
        private void LoadTestScene(BasicCamera c)
        {
            camera = c;
            
            DebugConsoleComponent dbg = DebugConsoleComponent.CreateConsole().GetComponent<DebugConsoleComponent>();
            dbg.AddCommand("reload", cmd_ReLoadScene);
            dbg.AddCommand("mov", cmd_Move);
            dbg.AddCommand("rot", cmd_Rotate);
            GameEngine.Instance.CurrentScene.Add(dbg.Owner);

            WFCMapGenerator preview = WFCMapGenerator
                .CreateWFCPreview(Vector3.Zero, "assets/WFCTiles", false, (input) => CreateMap(input, TextureShader))
                .GetComponent<WFCMapGenerator>();

            int tries = 1;
            Add(preview.Owner);
            preview.Generate(1);

            while (!preview.Success)
            {
                Logger.Log("Generating map Try " + tries, DebugChannel.Log|DebugChannel.Game, 8);
                preview.Generate(1);
                tries++;
            }

        }

        protected override void InitializeScene()
        {
            LayerManager.RegisterLayer("raycast", new Layer(1, 2));
            int hybridLayer = LayerManager.RegisterLayer("hybrid", new Layer(1, 1 | 2));
            LayerManager.RegisterLayer("physics", new Layer(1, 1));

            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "assets/shader/lit/point.fs"},
                {ShaderType.VertexShader, "assets/shader/lit/point.vs"}
            }, out TextureShader);
            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "assets/shader/UITextRender.fs"},
                {ShaderType.VertexShader, "assets/shader/UITextRender.vs"}
            }, out TextShader);
            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "assets/shader/UIRender.fs"},
                {ShaderType.VertexShader, "assets/shader/UIRender.vs"}
            }, out UIImageShader);

            BasicCamera c = new BasicCamera(
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                    GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);

            LoadTestScene(c);
            LoadGameScene(camera);
            TextureGenerator.Process();

            GameEngine.Instance.CurrentScene.Add(c);
            GameEngine.Instance.CurrentScene.SetCamera(c);
        }

        public override void OnDestroy()
        {
        }
    }
}
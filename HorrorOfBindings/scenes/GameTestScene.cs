using System;
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
using Engine.UI;
using Engine.UI.Animations;
using Engine.UI.Animations.AnimationTypes;
using Engine.UI.Animations.Interpolators;
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
        public static bool[,] map;

        private void LoadGameScene(BasicCamera c)
        {
            c.Rotate(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-75));
            c.Translate(new Vector3(0, 75, 15));



            Add(CreateGoal(FindGoalSpawn(-(map.GetLength(1)+6))));

            GameObject[] objs = PlayerController.CreatePlayer(FindPlayerSpawn(map), c);

            for (int i = 0; i < objs.Length; i++)
            {
                GameEngine.Instance.CurrentScene.Add(objs[i]);
            }
            //EnemyComponent.CreateEnemies(new Vector2(30, 500), map, 5,0);
        }


        private static GameObject CreateGoal(Vector3 position)
        {
            GameObject ret = TileCreator.CreateCube(position, Vector3.One, Quaternion.Identity, TextureLoader.ColorToTexture(Color.Red), DefaultFilepaths.DefaultLitShader, TextureLoader.ColorToTexture(Color.White));

            ret.AddComponent(new MapEndTrigger());
            return ret;

        }

        private static Vector3 FindGoalSpawn(int mapHeight)
        {
            Vector3 offset = new Vector3(0, 0, mapHeight / 2f);
            Vector3 scale = new Vector3(4, 1, 4);
            return (new Vector3(0, 5, mapHeight) - offset) * scale;
        }

        private static Vector3 FindPlayerSpawn(bool[,] map)
        {
            Vector3 offset = new Vector3(map.GetLength(0) / 2f, 0, map.GetLength(1) / 2f);
            Vector3 scale = new Vector3(4, 1, 4);

            for (int i = map.GetLength(1) - 1; i >= 0; i--)
            {
                if (map[map.GetLength(0) / 2, i]) return (new Vector3(map.GetLength(0) / 2f, 5, i) - offset) * scale;
            }

            return (new Vector3(map.GetLength(0) / 2f, 5, map.GetLength(1)) - offset) * scale;
        }

        private static GameObject[] objects = new GameObject[0];

        private void CreateMap(System.Drawing.Bitmap input, ShaderProgram prog)
        {
            Color[] data = new Color[input.Height * input.Width];
            map = new bool[input.Width, input.Height];
            for (int w = 0; w < input.Width; w++)
            {
                for (int h = 0; h < input.Height; h++)
                {
                    map[w, h] = input.GetPixel(w, h).R >= 128;
                    data[w * input.Height + h] = input.GetPixel(w, h);
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

            Vector3 scale = new Vector3(input.Width * 4, 2, input.Height * 4 + 50) / 2;
            Size size = new Size(input.Width, input.Height);
            groundObj = CreateGround(input, scale, size);
            Add(groundObj);
        }

        private GameObject CreateGround(Bitmap mapLayout, Vector3 scale, Size size)
        {
            Texture tex = TextureLoader.BitmapToTexture(new Bitmap(mapLayout, 512, 512));
            Texture texS = TextureLoader.BitmapToTexture(new Bitmap(mapLayout, 512, 512));




            TextureGenerator.CreateGroundTexture(tex, texS);
            GameObject ret = TileCreator.CreateCube(Vector3.Zero, scale, Quaternion.Identity,
                tex, DefaultFilepaths.DefaultLitShader, new Vector2(size.Height, size.Width), Vector2.Zero, texS);
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
                .CreateWFCPreview(Vector3.Zero, "assets/WFCTiles", false, (input) => CreateMap(input, DefaultFilepaths.DefaultLitShader))
                .GetComponent<WFCMapGenerator>();
            preview.Height = 256;
            preview.Width = 32;

            int tries = 1;
            Add(preview.Owner);
            preview.Generate(1);

            while (!preview.Success)
            {
                Logger.Log("Generating map Try " + tries, DebugChannel.Log | DebugChannel.Game, 8);
                preview.Generate(1);
                tries++;
            }

        }

        protected override void InitializeScene()
        {
            LayerManager.RegisterLayer("raycast", new Layer(1, 2));
            int hybridLayer = LayerManager.RegisterLayer("hybrid", new Layer(1, 1 | 2));
            LayerManager.RegisterLayer("physics", new Layer(1, 1));


            BasicCamera c = new BasicCamera(
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                    GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);

            LoadTestScene(c);
            LoadGameScene(camera);
            Texture bg = null;
            if (!ComesFromMenu)
            {
                bg = TextureLoader.ColorToTexture(Color.Black);
            }
            LoadLoadingScreen(bg);
            TextureGenerator.Process(LoadingFinished);

            GameEngine.Instance.CurrentScene.Add(c);
            GameEngine.Instance.CurrentScene.SetCamera(c);
        }

        private static Texture BlackBG = TextureLoader.ColorToTexture(Color.Black);
        private static Texture LoadingSymbol = TextureLoader.FileToTexture("assets/textures/LoadingSymbol.jpg");

        internal static bool ComesFromMenu = true;
        private static GameObject bg;
        private static GameObject loading;
        public void LoadLoadingScreen(Texture background = null)
        {
            LoadingSymbol = TextureLoader.FileToTexture("assets/textures/LoadingSymbol.jpg");
            bg = new GameObject("Background");
            if (background == null)
                BlackBG = MenuScene.menubg;
            else BlackBG = background;
            UIImageRendererComponent bgImage = new UIImageRendererComponent(BlackBG, false, 1, DefaultFilepaths.DefaultUIImageShader);
            bgImage.RenderQueue = -1;
            bg.AddComponent(bgImage);
            bg.AddComponent(new BackgroundMover());
            Add(bg);

            GameObject text = new GameObject("text");
            GameFont f = FontLibrary.LoadFontDirect("assets/fonts/default_font.ttf", 64);
            UITextRendererComponent tr = new UITextRendererComponent(f, false, 1, DefaultFilepaths.DefaultUITextShader);
            text.AddComponent(tr);
            bg.Add(text);
            tr.Text = "Loading...";
            tr.SystemColor = Color.Black;
            tr.Scale = Vector2.One * 3;
            tr.RenderQueue = -1;
            tr.Position = new Vector2(-0.7f, -0.7f);
            loading = new GameObject("Loading");
            UIImageRendererComponent loadingImage = new UIImageRendererComponent(LoadingSymbol, false, 1, DefaultFilepaths.DefaultUIImageShader);
            loadingImage.RenderQueue = -1;
            loading.AddComponent(loadingImage);
            Add(loading);
            float size = 0.05f;
            loadingImage.Position = new Vector2(0.7f, -0.7f);
            loadingImage.Scale = new Vector2(size, GameEngine.Instance.AspectRatio * size);
            LinearAnimation loadAnim = new LinearAnimation();
            Interpolator intP = new Arc2Interpolator();

            loadAnim.Interpolator = intP;
            loadAnim.StartPos = loadingImage.Position;
            loadAnim.EndPos = loadingImage.Position + Vector2.UnitY * 0.1f;
            loadAnim.MaxAnimationTime = 0.5f;
            loadAnim.Trigger = AnimationTrigger.None;
            loadAnim.AnimationDelay = 0f;
            Animator anim = new Animator(new List<Animation> { loadAnim }, loadingImage);
            loading.AddComponent(anim);
            LoopTimer(anim, loadAnim);
            GameObject obj = new GameObject("Timer");
            GeneralTimer timer = new GeneralTimer(0.5f, () => LoopTimer(anim, loadAnim), true);
            obj.AddComponent(timer);
            Add(obj);
        }

        private void LoopTimer(Animator anim, LinearAnimation animation)
        {
            Vector2 v = animation.EndPos - animation.StartPos;
            v = new Vector2(-v.Y, v.X);
            animation.EndPos = animation.StartPos + v;
            anim.TriggerEvent(AnimationTrigger.None);
        }

        private void ActivateEnemies()
        {
            EnemyComponent.active = true;
        }

        private void LoadingFinished()
        {
            bg.Destroy();
            loading.Destroy();
            GameEngine.Instance.CurrentScene.AddComponent(new GeneralTimer(5, ActivateEnemies));
        }

        public override void OnDestroy()
        {
            TextureGenerator.Reset();
        }
    }
}
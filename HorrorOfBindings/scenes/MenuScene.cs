using System;
using System.Collections.Generic;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.OpenCL;
using Engine.OpenCL.DotNetCore.Memory;
using Engine.OpenCL.TypeEnums;
using Engine.OpenFL;
using Engine.Physics.BEPUphysics.BroadPhaseEntries;
using Engine.Rendering;
using Engine.UI;
using Engine.UI.EventSystems;
using MinorGame.components;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Color = System.Drawing.Color;

namespace MinorGame.scenes
{
    public class MenuScene : AbstractScene
    {
        private static ShaderProgram UIShader;
        private static ShaderProgram TextShader;
        private static Texture buttonITex;
        private static Texture buttonHTex;
        private static Texture buttonCTex;
        protected override void InitializeScene()
        {
            buttonITex = TextureLoader.ColorToTexture(Color.Blue);
            buttonHTex = TextureLoader.ColorToTexture(Color.FromArgb(0, 0, 128));
            buttonCTex = TextureLoader.ColorToTexture(Color.Black);


            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "assets/shader/UIRender.fs"},
                {ShaderType.VertexShader, "assets/shader/UIRender.vs"}
            }, out UIShader);
            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "assets/shader/UITextRender.fs"},
                {ShaderType.VertexShader, "assets/shader/UITextRender.vs"}
            }, out TextShader);



            BasicCamera mainCamera =
                new BasicCamera(
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                        GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);

            object mc = mainCamera;

            //EngineConfig.LoadConfig("assets/configs/camera_menu.xml", ref mc);
            Add(mainCamera);
            SetCamera(mainCamera);

            Texture menubg = GenerateMenuBackground();
            UIImageRendererComponent bg = new UIImageRendererComponent(menubg, false, 1, UIShader);
            
            GameObject bgobj = new GameObject("BG");
            bgobj.AddComponent(new BackgroundMover());
            bgobj.AddComponent(bg);
            Add(bgobj);

            CreateButton("Start Game", new Vector2(-0.5f, 0.5f), new Vector2(0.2f, 0.1f), btnStartGame);
            CreateButton("Credits", new Vector2(-0.5f, 0.25f), new Vector2(0.2f, 0.1f));
            CreateButton("Exit", new Vector2(-0.5f, 0.0f), new Vector2(0.2f, 0.1f), btnExit);
            DebugConsoleComponent c = DebugConsoleComponent.CreateConsole().GetComponent<DebugConsoleComponent>();
            Add(c.Owner);

        }


        private void btnStartGame()
        {
            GameEngine.Instance.InitializeScene<GameTestScene>();
        }

        private void btnExit()
        {
            GameEngine.Instance.Exit();
        }


        private Texture GenerateMenuBackground()
        {
            Interpreter i = new Interpreter("assets/filter/game/menubg.fl", DataTypes.UCHAR1, CLAPI.CreateEmpty<byte>(64 * 64 * 4, MemoryFlag.ReadWrite), 64, 64, 1, 4, "assets/kernel/", true);

            do
            {
                i.Step();
            } while (!i.Terminated);

            Texture tex = TextureLoader.ParameterToTexture(64, 64);
            TextureLoader.Update(tex, i.GetResult<byte>(), (int)tex.Width, (int)tex.Height);
            return tex;
        }


        private GameObject CreateButton(string Text, Vector2 Position, Vector2 Scale, Action onClick = null)
        {
            GameObject container = new GameObject("BtnContainer");
            GameObject obj = new GameObject("Button");
            GameObject tObj = new GameObject("Text");
            Texture btnIdle = TextureLoader.ColorToTexture(Color.Green);
            Texture btnHover = TextureLoader.ColorToTexture(Color.Red);
            Texture btnClick = TextureLoader.ColorToTexture(Color.Blue);
            Button btn = new Button(btnIdle, UIShader, 1, btnClick, btnHover, onClick);

            UITextRendererComponent tr = new UITextRendererComponent("Arial", false, 1, TextShader);
            obj.AddComponent(btn);
            tObj.AddComponent(tr);
            container.Add(obj);
            container.Add(tObj);
            Add(container);
            btn.Position = Position;
            btn.Scale = Scale;
            Vector2 textpos = Position;
            tr.Center = true;
            tr.Position = textpos;
            tr.Text = Text;
            return obj;
        }
    }
}
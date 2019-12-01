using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.OpenCL;
using Engine.OpenCL.DotNetCore.Memory;
using Engine.OpenCL.TypeEnums;
using Engine.OpenFL;
using Engine.Rendering;
using Engine.UI;
using Engine.UI.Animations;
using Engine.UI.Animations.AnimationTypes;
using Engine.UI.Animations.Interpolators;
using Engine.UI.EventSystems;
using FPSGame.components;
using OpenTK;

namespace FPSGame.scenes
{
    public class MenuScene : AbstractScene
    {
        internal static Texture menubg;
        protected override void InitializeScene()
        {



            BasicCamera mainCamera =
                new BasicCamera(
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                        GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);

            object mc = mainCamera;

            //EngineConfig.LoadConfig("assets/configs/camera_menu.xml", ref mc);
            Add(mainCamera);
            SetCamera(mainCamera);
            menubg = GenerateMenuBackground();
            UIImageRendererComponent bg = new UIImageRendererComponent(menubg.Copy(), false, 1, DefaultFilepaths.DefaultUIImageShader);

            GameObject bgobj = new GameObject("BG");
            bgobj.AddComponent(new BackgroundMover());
            bgobj.AddComponent(bg);
            Add(bgobj);
            //Positions are wrong(0.5 => 5) out of the screen because the correct positions are defined in CreateButtonAnimation.
            CreateButton("assets/textures/btn/btn", "Start Game", new Vector2(-5f, 0.5f), new Vector2(0.2f, 0.1f), CreateButtonAnimation(new Vector2(-0.5f, 0.5f), 0), btnStartGame);
            CreateButton("assets/textures/btn/btn", "Credits", new Vector2(-5f, 0.25f), new Vector2(0.2f, 0.1f), CreateButtonAnimation(new Vector2(-0.5f, 0.25f), 0.2f));
            CreateButton("assets/textures/btn/btn", "Exit", new Vector2(-5f, 0.0f), new Vector2(0.2f, 0.1f), CreateButtonAnimation(new Vector2(-0.5f, 0.0f), 0.4f), btnExit);
            DebugConsoleComponent c = DebugConsoleComponent.CreateConsole().GetComponent<DebugConsoleComponent>();
            Add(c.Owner);

        }

        private List<Animation> CreateButtonAnimation(Vector2 endPos, float delay)
        {
            LinearAnimation loadAnim = new LinearAnimation();
            loadAnim.Interpolator = new SmoothInterpolator();
            loadAnim.StartPos = new Vector2(endPos.X - 1, endPos.Y);
            loadAnim.EndPos = endPos;
            loadAnim.MaxAnimationTime = 1;
            loadAnim.Trigger = AnimationTrigger.OnLoad;
            loadAnim.AnimationDelay = delay;

            LinearAnimation clickAnim = new LinearAnimation();
            clickAnim.Interpolator = new Arc2Interpolator();
            clickAnim.StartPos = endPos;
            clickAnim.EndPos = endPos + Vector2.UnitY * 0.1f;
            clickAnim.MaxAnimationTime = 0.5f;
            clickAnim.Trigger = AnimationTrigger.OnEnter;
            return new List<Animation> { loadAnim, clickAnim };
        }


        private void btnStartGame(Button target)
        {
            GameEngine.Instance.InitializeScene<GameTestScene>();
        }

        private void btnExit(Button target)
        {
            GameEngine.Instance.Exit();
        }


        private Texture GenerateMenuBackground()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int texWidth = 128;
            int texHeight = 128;
            
            Texture tex = TextureLoader.ColorToTexture(Color.Red);
            Logger.Log("Time for Menu Background(ms): " + sw.ElapsedMilliseconds, DebugChannel.Log, 10);
            sw.Stop();
            return tex;
        }


        private GameObject CreateButton(string buttonString, string Text, Vector2 Position, Vector2 Scale, List<Animation> animations, Action<Button> onClick = null, Action<Button> onEnter = null, Action<Button> onLeave = null, Action<Button> onHover = null)
        {
            GameObject container = new GameObject("BtnContainer");
            GameObject obj = new GameObject("Button");
            GameObject tObj = new GameObject("Text");
            Texture btnIdle = TextureLoader.FileToTexture(buttonString + ".png");
            Texture btnHover = TextureLoader.FileToTexture(buttonString + "H.png");
            Texture btnClick = TextureLoader.FileToTexture(buttonString + "C.png");
            Button btn = new Button(btnIdle, DefaultFilepaths.DefaultUIImageShader, 1, btnClick, btnHover, onClick, onEnter, onHover, onLeave);



            UITextRendererComponent tr = new UITextRendererComponent("Arial", false, 1, DefaultFilepaths.DefaultUITextShader);
            obj.AddComponent(btn);
            tObj.AddComponent(tr);
            container.Add(obj);
            container.Add(tObj);
            Add(container);
            btn.Position = Position;
            btn.Scale = Scale;
            Vector2 textpos = Position;
            tr.Scale = Vector2.One * 2;
            tr.Center = true;
            tr.Position = textpos;
            tr.Text = Text;

            Animator anim = new Animator(animations, btn, tr);
            obj.AddComponent(anim);

            return obj;
        }
    }
}
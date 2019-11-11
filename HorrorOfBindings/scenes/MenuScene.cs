﻿using System;
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
using Engine.UI.Animations;
using Engine.UI.Animations.AnimationTypes;
using Engine.UI.Animations.Interpolators;
using Engine.UI.EventSystems;
using MinorGame.components;
using MinorGame.mapgenerator;
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
            TextureGenerator.Initialize(true);

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
            
            CreateButton("assets/textures/btn/btnStart" ,"", new Vector2(-0.5f, 0.5f), new Vector2(0.2f, 0.1f), CreateButtonAnimation(new Vector2(-0.5f, 0.5f),0), btnStartGame);
            CreateButton("assets/textures/btn/btnCredits","", new Vector2(-0.5f, 0.25f), new Vector2(0.2f, 0.1f), CreateButtonAnimation(new Vector2(-0.5f, 0.25f), 0.2f));
            CreateButton("assets/textures/btn/btnExit", "", new Vector2(-0.5f, 0.0f), new Vector2(0.2f, 0.1f), CreateButtonAnimation(new Vector2(-0.5f, 0.0f), 0.4f), btnExit);
            DebugConsoleComponent c = DebugConsoleComponent.CreateConsole().GetComponent<DebugConsoleComponent>();
            Add(c.Owner);

        }

        private List<Animation> CreateButtonAnimation(Vector2 endPos, float delay)
        {
            LinearAnimation loadAnim = new LinearAnimation();
            loadAnim.Interpolator = new SmoothInterpolator();
            loadAnim.StartPos = new Vector2(endPos.X-1, endPos.Y);
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
            Interpreter i = new Interpreter(CLAPI.MainThread, "assets/filter/game/menubg.fl", DataTypes.UCHAR1, CLAPI.CreateEmpty<byte>(CLAPI.MainThread, 64 * 64 * 4, MemoryFlag.ReadWrite), 64, 64, 1, 4, "assets/kernel/", true);

            do
            {
                i.Step();
            } while (!i.Terminated);

            Texture tex = TextureLoader.ParameterToTexture(64, 64);
            TextureLoader.Update(tex, i.GetResult<byte>(), (int)tex.Width, (int)tex.Height);
            return tex;
        }


        private GameObject CreateButton(string buttonString, string Text, Vector2 Position, Vector2 Scale, List<Animation> animations, Action<Button> onClick = null, Action<Button> onEnter = null, Action<Button> onLeave = null, Action<Button> onHover = null)
        {
            GameObject container = new GameObject("BtnContainer");
            GameObject obj = new GameObject("Button");
            GameObject tObj = new GameObject("Text");
            Texture btnIdle = TextureLoader.FileToTexture(buttonString +".png");
            Texture btnHover = TextureLoader.FileToTexture(buttonString + "H.png");
            Texture btnClick = TextureLoader.FileToTexture(buttonString + "C.png");
            Button btn = new Button(btnIdle, UIShader, 1, btnClick, btnHover, onClick, onEnter, onHover, onLeave);

            Animator anim = new Animator(btn, animations);
            obj.AddComponent(anim);
            
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
﻿using System;
using System.Reflection;
using MinorEngine.engine;
using MinorEngine.engine.core;
using MinorGame.scenes;
using OpenTK;
using OpenTK.Graphics;

namespace MinorGame
{
    class Program
    {
        static void Main(string[] args)
        {
            GraphicsMode gm = new GraphicsMode(ColorFormat.Empty, 8, 0, 16);

            EngineSettings es = new EngineSettings
            {
                WindowFlags = GameWindowFlags.Default,
                Mode = gm,
                InitWidth = 1280,
                InitHeight = 720,
                Title = "Test",
                PhysicsThreadCount = 4,
                VSync = VSyncMode.Off,

#if LOG_NETWORK
                DebugNetwork = true,
                NetworkMask = -1,
                ProgramID = 2,
                ProgramVersion = Assembly.GetExecutingAssembly().GetName().Version, //We Want the debug system to take the game version
#endif
            };

            GameEngine engine = new GameEngine(es);
            engine.Initialize();
            engine.InitializeScene<GameTestScene>();
            engine.Run();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using MinorGame.scenes;
using OpenTK;
using OpenTK.Graphics;

namespace MinorGame
{
    internal class Program
    {
        private static bool AskForDebugLogSending()
        {
            Console.WriteLine("Allow Sending Debug Logs? [y/N]:");
            if (Console.ReadLine().ToLower() == "y")
            {
                return true;
            }

            return false;
        }

        private static void Main(string[] args)
        {
            GraphicsMode gm = new GraphicsMode(ColorFormat.Empty, 8, 0, 16);

            GameEngine engine = new GameEngine(EngineSettings.DefaultSettings);
            
            ManifestReader.RegisterAssembly(Assembly.GetExecutingAssembly());
            ManifestReader.PrepareManifestFiles(true);
            //EngineConfig.CreateConfig(Assembly.GetAssembly(typeof(GameEngine)), "Engine.Core" , "configs/engine.settings.xml");
            EngineConfig.LoadConfig("assets/configs/engine_settings.xml", Assembly.GetAssembly(typeof(GameEngine)),
                "Engine.Core");
            DebugSettings dbgSettings = EngineSettings.Settings.DebugSettings;
            engine.SetSettings(EngineSettings.Settings);
            engine.Initialize();
            engine.InitializeScene<MenuScene>();
            engine.Run();
        }
    }
}
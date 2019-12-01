using System;
using System.Reflection;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using FPSGame.scenes;
using OpenTK.Graphics;

namespace FPSGame
{
    internal class Program
    {
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
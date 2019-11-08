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


#if COLLECT_LOGS
            if (AskForDebugLogSending())
            {
                List<ILogStreamSettings> streams = new List<ILogStreamSettings>(EngineSettings.Settings.DebugSettings.Streams);
                LogStreamSettings network = new LogStreamSettings()
                {
                    StreamType = 2,
                    Mask = -1,
                    Destination = "213.109.162.193",
                    NetworkAppID = 2,
                    NetworkAuthVersion =
                        Assembly.GetExecutingAssembly().GetName()
                            .Version, //We Want the debug system to take the game version
                    NetworkPort = 420,
                    MatchMode = 1,
                    Timestamp = true
                };
                streams.Add(network);
                EngineSettings.Settings.DebugSettings.Streams = streams.ToArray();
            }
#endif


            GameEngine engine = new GameEngine(EngineSettings.DefaultSettings);
            
            ManifestReader.RegisterAssembly(Assembly.GetExecutingAssembly());
            ManifestReader.PrepareManifestFiles(true);
            //EngineConfig.CreateConfig(Assembly.GetAssembly(typeof(GameEngine)), "Engine.Core" , "configs/engine.settings.xml");
            EngineConfig.LoadConfig("assets/configs/engine.settings.xml", Assembly.GetAssembly(typeof(GameEngine)),
                "Engine.Core");
            DebugSettings dbgSettings = EngineSettings.Settings.DebugSettings;
            engine.SetSettings(EngineSettings.Settings);
            engine.Initialize();
            engine.InitializeScene<MenuScene>();
            engine.Run();
        }
    }
}
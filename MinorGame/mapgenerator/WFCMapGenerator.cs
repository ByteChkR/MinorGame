using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using MinorEngine.components;
using MinorEngine.engine.components;
using MinorEngine.engine.core;
using MinorEngine.engine.rendering;
using MinorEngine.engine.ui.utils;
using MinorEngine.FilterLanguage.Generators;

namespace MinorGame.mapgenerator
{
    public class WFCMapGenerator : AbstractComponent
    {
        private List<string> sampleTextures = new List<string>();
        private WaveFunctionCollapse wfc;
        private int current = 0;
        private MeshRendererComponent renderer;

        private int _n = 3;
        private int _width = 128;
        private int _height = 128;
        private bool _periodicInput = false;
        private bool _periodicOutput = true;
        private int _symmetry = 8;
        private int _ground = 0;
        private int _seed = 1337;
        private bool _useSeed;
        private int _limit = 0;

        #region ConsoleCommands

        private string cmd_Run(string[] args)
        {
            if (args.Length == 0)
            {
                return "Command Finished: " + Generate();
            }
            else if (int.TryParse(args[0], out int id))
            {
                return "Command Finished: " + Generate(id);
            }

            return "Invalid Input";
        }

        private string cmd_N(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }
            _n = temp;
            return "Command Finished";
        }

        private string cmd_Width(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }
            _width = temp;
            return "Command Finished";
        }

        private string cmd_Height(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }
            _height = temp;
            return "Command Finished";
        }

        private string cmd_PeriodicInput(string[] args)
        {
            if (args.Length == 0 || !bool.TryParse(args[0], out bool temp))
            {
                return "Argument is not a boolean";
            }
            _periodicInput = temp;
            return "Command Finished";
        }

        private string cmd_PeriodicOutput(string[] args)
        {
            if (args.Length == 0 || !bool.TryParse(args[0], out bool temp))
            {
                return "Argument is not a boolean";
            }
            _periodicOutput = temp;
            return "Command Finished";
        }

        private string cmd_UseSeed(string[] args)
        {
            if (args.Length == 0 || !bool.TryParse(args[0], out bool temp))
            {
                return "Argument is not a boolean";
            }
            _useSeed = temp;
            return "Command Finished";
        }

        private string cmd_Symmetry(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }
            _symmetry = temp;
            return "Command Finished";
        }

        private string cmd_Ground(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }
            _ground = temp;
            return "Command Finished";
        }

        private string cmd_Seed(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }
            _seed = temp;
            return "Command Finished";
        }

        private string cmd_Limit(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }
            _limit = temp;
            return "Command Finished";
        }

        #endregion

        public WFCMapGenerator(string folderName)
        {


            sampleTextures = Directory.GetFiles(folderName, "*.png").ToList();

        }

        protected override void Awake()
        {
            DebugConsoleComponent console = Owner.World.GetChildWithName("DebugConsole").GetComponent<DebugConsoleComponent>();
            console.AddCommand("n", cmd_N);
            console.AddCommand("ground", cmd_Ground);
            console.AddCommand("height", cmd_Height);
            console.AddCommand("limit", cmd_Limit);
            console.AddCommand("seed", cmd_Seed);
            console.AddCommand("width", cmd_Width);
            console.AddCommand("symmetry", cmd_Symmetry);
            console.AddCommand("pin", cmd_PeriodicInput);
            console.AddCommand("pout", cmd_PeriodicOutput);
            console.AddCommand("run", cmd_Run);

            renderer = Owner.GetComponent<MeshRendererComponent>();
        }


        private void ChangeTexture(GameTexture texture)
        {
            GameTexture[] textures = renderer.Model.GetTextureBuffer();
            foreach (var gameTexture in textures)
            {
                gameTexture.Dispose();
            }
            renderer.Model.SetTextureBuffer(new[] { texture });
        }

        public bool Generate(int sampleID)
        {
            wfc = new WFCOverlayMode(sampleTextures[sampleID], _n, _width, _height, _periodicInput, _periodicOutput, _symmetry, _ground);
            bool ret = false;
            if (_useSeed)
            {
                ret = wfc.Run(_seed, _limit);
            }
            else
            {
                ret = wfc.Run(_limit);
            }

            GameTexture tex = ResourceManager.TextureIO.BitmapToTexture(wfc.Graphics());
            ChangeTexture(tex);
            return ret;
        }

        public bool Generate()
        {
            if (sampleTextures.Count == 0) return false;
            if (current >= sampleTextures.Count) current = 0;
            return Generate(current++);
        }
    }
}
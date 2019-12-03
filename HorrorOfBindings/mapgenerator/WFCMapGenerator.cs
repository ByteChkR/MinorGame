using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.Exceptions;
using Engine.IO;
using Engine.Rendering;
using Engine.WFC;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Bitmap = System.Drawing.Bitmap;

namespace MinorGame.mapgenerator
{
    public class WFCMapGenerator : AbstractComponent
    {
        private List<string> sampleTextures = new List<string>();
        private WfcOverlayMode wfc;
        private int current = 0;
        private MeshRendererComponent renderer;

        public int N { get; set; } = 3;
        public int Width { get; set; } = 64;
        public int Height { get; set; } = 64;
        public bool PeriodicInput { get; set; }
        public bool PeriodicOutput { get; set; } = true;
        public int Symmetry { get; set; } = 8;
        public int Ground { get; set; } = 0;
        public int Seed { get; set; } = 1337;
        public bool UseSeed { get; set; }
        public int Limit { get; set; } = 0;
        public bool Success => wfc.Success;
        private string _folderName;

        #region ConsoleCommands

        private string cmd_Reload(string[] args)
        {
            sampleTextures.Clear();

            sampleTextures = Directory.GetFiles(_folderName, "*.png").ToList();
            return "Sample Count: " + sampleTextures.Count;
        }

        private string cmd_List(string[] args)
        {
            string s = "Loaded Files:\n";
            for (int i = 0; i < sampleTextures.Count; i++)
            {
                s += "\t" + sampleTextures[i];
            }

            return s;
        }

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

            N = temp;
            return "Command Finished";
        }

        private string cmd_Width(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }

            Width = temp;
            return "Command Finished";
        }

        private string cmd_Height(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }

            Height = temp;
            return "Command Finished";
        }

        private string cmd_PeriodicInput(string[] args)
        {
            if (args.Length == 0 || !bool.TryParse(args[0], out bool temp))
            {
                return "Argument is not a boolean";
            }

            PeriodicInput = temp;
            return "Command Finished";
        }

        private string cmd_PeriodicOutput(string[] args)
        {
            if (args.Length == 0 || !bool.TryParse(args[0], out bool temp))
            {
                return "Argument is not a boolean";
            }

            PeriodicOutput = temp;
            return "Command Finished";
        }

        private string cmd_UseSeed(string[] args)
        {
            if (args.Length == 0 || !bool.TryParse(args[0], out bool temp))
            {
                return "Argument is not a boolean";
            }

            UseSeed = temp;
            return "Command Finished";
        }

        private string cmd_Symmetry(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }

            Symmetry = temp;
            return "Command Finished";
        }

        private string cmd_Ground(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }

            Ground = temp;
            return "Command Finished";
        }

        private string cmd_Seed(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }

            Seed = temp;
            return "Command Finished";
        }

        private string cmd_Limit(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int temp))
            {
                return "Argument is not a number";
            }

            Limit = temp;
            return "Command Finished";
        }

        #endregion

        public static GameObject CreateWFCPreview(Vector3 position, string folderName, bool attachDebugRenderer = true,
            OutputCallback outputCallback = null)
        {
            //Ground
            Mesh mesh = MeshLoader.FileToMesh("assets/models/cube_flat.obj");
            GameObject obj = new GameObject(position, "WFCPreview");

            obj.AddComponent(new WFCMapGenerator(folderName, outputCallback));
            if (attachDebugRenderer)
            {
                obj.AddComponent(new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, mesh,
                    TextureLoader.FileToTexture("assets/textures/TEST.png"), 1));
            }

            obj.Scale = new Vector3(5, 5, 5);
            return obj;
        }

        private OutputCallback _callback;

        public delegate void OutputCallback(Bitmap result);

        public WFCMapGenerator(string folderName, OutputCallback callback = null)
        {
            if (!IOManager.FolderExists(folderName))
            {
                Logger.Crash(new InvalidFolderPathException(folderName), true);
                Logger.Log("Creating Directory: " + folderName, DebugChannel.Warning, 10);
            }

            _folderName = folderName;
            sampleTextures = IOManager.GetFiles(folderName, "*.png").ToList();
            _callback = callback;
        }

        protected override void Awake()
        {
            DebugConsoleComponent console =
                Owner.Scene.GetChildWithName("Console").GetComponent<DebugConsoleComponent>();
            console.AddCommand("n", cmd_N);
            console.AddCommand("ground", cmd_Ground);
            console.AddCommand("height", cmd_Height);
            console.AddCommand("limit", cmd_Limit);
            console.AddCommand("seed", cmd_Seed);
            console.AddCommand("useseed", cmd_UseSeed);
            console.AddCommand("width", cmd_Width);
            console.AddCommand("symmetry", cmd_Symmetry);
            console.AddCommand("pin", cmd_PeriodicInput);
            console.AddCommand("pout", cmd_PeriodicOutput);
            console.AddCommand("run", cmd_Run);
            console.AddCommand("reload", cmd_Reload);
            console.AddCommand("list", cmd_List);

            renderer = Owner.GetComponent<MeshRendererComponent>();
        }


        private void ChangeTexture(Texture texture)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.Textures[0]?.Dispose();


            renderer.Textures[0] = texture;
        }

        public bool Generate(int sampleID)
        {
            wfc = new WfcOverlayMode(sampleTextures[sampleID], N, Width, Height, PeriodicInput, PeriodicOutput,
                Symmetry, Ground);
            bool ret = false;
            if (UseSeed)
            {
                ret = wfc.Run(Seed, Limit);
            }
            else
            {
                ret = wfc.Run(Limit);
            }

            Bitmap bmp = wfc.Graphics();

            if (wfc.Success)
            {
                _callback?.Invoke(bmp);
            }


            if (renderer != null)
            {
                Texture tex = TextureLoader.BitmapToTexture(bmp);
                ChangeTexture(tex);
            }

            return ret;
        }

        public bool Generate()
        {
            if (sampleTextures.Count == 0)
            {
                return false;
            }

            if (current >= sampleTextures.Count)
            {
                current = 0;
            }

            return Generate(current++);
        }
    }
}
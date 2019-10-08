using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using MinorEngine.BEPUphysics.Entities.Prefabs;
using MinorEngine.BEPUphysics.Materials;
using MinorEngine.BEPUutilities;
using MinorEngine.components;
using MinorEngine.debug;
using MinorEngine.engine.components;
using MinorEngine.engine.components.ui;
using MinorEngine.engine.core;
using MinorEngine.engine.rendering;
using MinorEngine.engine.rendering.contexts;
using MinorEngine.exceptions;
using MinorEngine.FilterLanguage.Generators;
using OpenTK.Graphics.OpenGL;

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

        public static GameObject CreateWFCPreview(Vector3 position, string folderName, bool attachDebugRenderer = true, OutputCallback outputCallback = null)
        {
            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out ShaderProgram shader);

            //Ground
            GameMesh mesh = ResourceManager.MeshIO.FileToMesh("models/cube_flat.obj");
            GameObject obj = new GameObject("WFCPreview");
            mesh.SetTextureBuffer(new[] { ResourceManager.TextureIO.FileToTexture("textures/TEST.png") });

            obj.AddComponent(new WFCMapGenerator(folderName, outputCallback));
            if (attachDebugRenderer)
            {
                obj.AddComponent(new MeshRendererComponent(shader, mesh, 1));
            }

            obj.Scale = new OpenTK.Vector3(5, 5, 5);
            return obj;
        }

        private OutputCallback _callback;
        public delegate void OutputCallback(Bitmap result);
        public WFCMapGenerator(string folderName, OutputCallback callback = null)
        {
            if (!Directory.Exists(folderName))
            {
                Logger.Crash(new InvalidFolderPathException(folderName), true);
                Logger.Log("Creating Directory: " + folderName, DebugChannel.Warning, 10);
            }

            _folderName = folderName;
            sampleTextures = Directory.GetFiles(folderName, "*.png").ToList();
            _callback = callback;
        }

        protected override void Awake()
        {
            DebugConsoleComponent console = Owner.World.GetChildWithName("Console").GetComponent<DebugConsoleComponent>();
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


        private void ChangeTexture(GameTexture texture)
        {
            GameTexture[] oldTextures = renderer.Model.GetTextureBuffer();

            for (int i = 0; i < oldTextures.Length; i++)
            {
                oldTextures[i].Dispose();
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

            Bitmap bmp = wfc.Graphics();

            _callback?.Invoke(bmp);


            if (renderer != null)
            {
                GameTexture tex = ResourceManager.TextureIO.BitmapToTexture(bmp);
                ChangeTexture(tex);
            }
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
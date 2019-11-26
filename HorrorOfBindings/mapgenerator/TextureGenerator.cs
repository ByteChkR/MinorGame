using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using Assimp;
using Engine.DataTypes;
using Engine.IO;
using Engine.OpenCL;
using Engine.OpenCL.DotNetCore.Memory;
using Engine.OpenCL.Runner;
using Engine.OpenCL.TypeEnums;
using Engine.OpenFL;
using OpenTK.Graphics.OpenGL;

namespace MinorGame.mapgenerator
{
    public static class TextureGenerator
    {
        private static Interpreter intP;

        private static bool _initPerlin = false;
        private static Texture[] wallTextures;
        private static Texture[] wallSpecTextures;
        private static Texture playerSphereTexture;
        private static Texture playerSphereSpecTexture;
        private static FLRunner runner;
        private static bool runnerInit;

        public static void Initialize(bool multiThread)
        {
            if (runnerInit) return;
            runnerInit = true;
            if (multiThread)
            {
                runner = new FLMultiThreadRunner(null);
            }
            else
            {
                runner = new FLRunner(CLAPI.MainThread);
            }
            InitPerlin();
        }

        public static Texture GetTexture(int type)
        {
            if (!_initPerlin) InitPerlin();
            return wallTextures[type];

        }
        public static Texture GetSTexture(int type)
        {
            if (!_initPerlin) InitPerlin();
            return wallSpecTextures[type];

        }

        public static void Process(Action onFinish = null)
        {
            runner.Process(onFinish);
        }

        public static void Reset()
        {
            _initPerlin = false;
            playerSphereTexture.Dispose();
            playerSphereSpecTexture.Dispose();
            for (int i = 0; i < wallTextures.Length; i++)
            {
                wallTextures[i].Dispose();
            }
            for (int i = 0; i < wallSpecTextures.Length; i++)
            {
                wallSpecTextures[i].Dispose();
            }
        }

        public static Texture GetPlayerTexture()
        {
            if (!_initPerlin) InitPerlin();
            return playerSphereTexture;

        }

        private static void InitPerlin()
        {
            _initPerlin = true;

            //Texture tex = TextureLoader.ParameterToTexture(512, 512);

            //FLExecutionContext exec =new FLExecutionContext("assets/filter/game/perlin.fl", tex, new Dictionary<string, Texture>(), null);
            //runner.Enqueue(exec);

            playerSphereTexture = TextureLoader.ParameterToTexture(512, 512);
            playerSphereSpecTexture = TextureLoader.ParameterToTexture(512, 512);
            CreatePlayerTexture(playerSphereTexture, playerSphereSpecTexture);

            wallTextures = new Texture[2];
            wallSpecTextures = new Texture[2];
            for (int i = 0; i < 2; i++)
            {
                wallTextures[i] = TextureLoader.ParameterToTexture(512, 512);
                wallSpecTextures[i] = TextureLoader.ParameterToTexture(512, 512);
                CreateWallTexture(wallTextures[i], wallSpecTextures[i], i);
            }
        }

        public static void CreateGroundTexture(Texture destTexture, Texture specTexture)
        {
            runner.Enqueue(GetExecutionContext("assets/filter/game/cobble_grass.fl", destTexture, specTexture, null));
        }

        public static void CreateWallTexture(Texture destTexture, Texture specTexture, int i)
        {
            runner.Enqueue(GetExecutionContext($"assets/filter/game/wall{i}.fl", destTexture, specTexture, null));
        }
        public static void CreatePlayerTexture(Texture destTexture, Texture specTexture)
        {
            runner.Enqueue(GetExecutionContext($"assets/filter/game/tennisball.fl", destTexture, specTexture, null));
        }

        private static FLExecutionContext GetExecutionContext(string file, Texture dest, Texture specular, Action<Dictionary<Texture, byte[]>> onFinishCallback)
        {

            Dictionary<string, Texture> otherTex = new Dictionary<string, Texture>() { { "result", dest }, { "specularOut", specular } };
            return new FLExecutionContext(file, dest, otherTex, onFinishCallback);
        }




    }
}
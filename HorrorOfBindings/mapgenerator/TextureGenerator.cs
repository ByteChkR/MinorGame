﻿using System;
using System.Collections.Generic;
using Engine.DataTypes;
using Engine.IO;
using Engine.OpenCL;
using Engine.OpenFL;
using Engine.OpenFL.Runner;

namespace HorrorOfBindings.mapgenerator
{
    public static class TextureGenerator
    {
        private static Interpreter intP;

        private static bool _initPerlin = false;
        private static Texture[] wallTextures;
        private static Texture[] wallSpecTextures;
        private static Texture playerSphereTexture;
        private static Texture playerSphereSpecTexture;
        private static FlRunner runner;
        private static bool runnerInit;

        public static void Initialize(bool multiThread)
        {
            if (runnerInit)
            {
                return;
            }

            runnerInit = true;
            if (multiThread)
            {
                runner = new FlMultiThreadRunner(null);
            }
            else
            {
                runner = new FlRunner(Clapi.MainThread);
            }

            InitPerlin();
        }

        public static Texture GetTexture(int type)
        {
            if (!_initPerlin)
            {
                InitPerlin();
            }

            return wallTextures[type];
        }

        public static Texture GetSTexture(int type)
        {
            if (!_initPerlin)
            {
                InitPerlin();
            }

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
            if (!_initPerlin)
            {
                InitPerlin();
            }

            return playerSphereTexture;
        }

        private static void InitPerlin()
        {
            _initPerlin = true;

            //Texture tex = TextureLoader.ParameterToTexture(512, 512);

            //FlExecutionContext exec =new FlExecutionContext("assets/filter/game/perlin.fl", tex, new Dictionary<string, Texture>(), null);
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

        public static void CreateBoundsTexture(Texture destTexture, Texture specTexture)
        {
            runner.Enqueue(GetExecutionContext($"assets/filter/game/concrete.fl", destTexture, specTexture, null));
        }

        private static FlExecutionContext GetExecutionContext(string file, Texture dest, Texture specular,
            Action<Dictionary<Texture, byte[]>> onFinishCallback)
        {
            Dictionary<string, Texture> otherTex = new Dictionary<string, Texture>()
                {{"result", dest}, {"specularOut", specular}};
            return new FlExecutionContext(file, dest, otherTex, onFinishCallback);
        }
    }
}
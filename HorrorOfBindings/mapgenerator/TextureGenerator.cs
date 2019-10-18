using System;
using System.Drawing;
using Engine.DataTypes;
using Engine.IO;
using Engine.OpenCL;
using Engine.OpenCL.DotNetCore.Memory;
using Engine.OpenCL.TypeEnums;
using Engine.OpenFL;

namespace MinorGame.mapgenerator
{
    public static class TextureGenerator
    {
        private static Interpreter intP;

        private static bool _initPerlin = false;
        private static Texture[] wallTextures;
        private static Texture playerSphereTexture;

        public static Texture GetTexture(int type)
        {
            if (!_initPerlin) InitPerlin();
            return wallTextures[type];

        }

        public static void Reset()
        {
            _initPerlin = false;
            playerSphereTexture.Dispose();
            for (int i = 0; i < wallTextures.Length; i++)
            {
                wallTextures[i].Dispose();
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


            MemoryBuffer buf = CLAPI.CreateEmpty<byte>(512 * 512 * 4, MemoryFlag.ReadWrite);

            intP?.ReleaseResources();
            intP = new Interpreter("filter/game/perlin.fl", DataTypes.UCHAR1, buf, 512, 512, 1, 4, "kernel");


            do
            {
                intP.Step();
            } while (!intP.Terminated);

            byte[] b = intP.GetResult<byte>();

            playerSphereTexture = TextureLoader.BytesToTexture(new byte[512 * 512 * 4], 512, 512);
            CreatePlayerTexture(playerSphereTexture, 512, 512);

            wallTextures = new Texture[2];
            for (int i = 0; i < 2; i++)
            {
                wallTextures[i] = TextureLoader.BytesToTexture(new byte[512 * 512 * 4], 512, 512);
                CreateWallTexture(new Bitmap(512, 512), wallTextures[i], 512, 512, i);
            }
        }

        public static void CreateGroundTexture(Bitmap input, Texture destTexture)
        {
            if (!_initPerlin) InitPerlin();
            intP?.ReleaseResources();
            intP = new Interpreter("filter/game/cobble_grass.fl", DataTypes.UCHAR1, TextureLoader.TextureToMemoryBuffer(destTexture), (int)destTexture.Width, (int)destTexture.Height, 1, 4, "kernel");


            do
            {
                intP.Step();
            } while (!intP.Terminated);

            TextureLoader.Update(destTexture, intP.GetResult<byte>(), (int)destTexture.Width, (int)destTexture.Height);

        }

        public static void CreateWallTexture(Bitmap input, Texture destTexture, int width, int height, int i)
        {
            if (!_initPerlin) InitPerlin();
            intP?.ReleaseResources();
            intP = new Interpreter($"filter/game/wall{i}.fl", DataTypes.UCHAR1, TextureLoader.TextureToMemoryBuffer(destTexture), width, height, 1, 4, "kernel");

            do
            {
                intP.Step();
            } while (!intP.Terminated);

            TextureLoader.Update(destTexture, intP.GetResult<byte>(), width, height);

        }

        public static void CreatePlayerTexture(Texture destTexture, int width, int height)
        {
            if (!_initPerlin) InitPerlin();
            intP?.ReleaseResources();
            intP = new Interpreter($"filter/game/tennisball.fl", DataTypes.UCHAR1, TextureLoader.TextureToMemoryBuffer(destTexture), 512, 512, 1, 4, "kernel");

            do
            {
                intP.Step();
            } while (!intP.Terminated);

            byte[] b = intP.GetResult<byte>();
            TextureLoader.Update(destTexture, intP.GetResult<byte>(), 512, 512);

        }


    }
}
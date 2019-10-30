using System;
using System.Drawing;
using System.Net.NetworkInformation;
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
        private static Texture[] wallSpecTextures;
        private static Texture playerSphereTexture;
        private static Texture playerSphereSpecTexture;

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


            MemoryBuffer buf = CLAPI.CreateEmpty<byte>(512 * 512 * 4, MemoryFlag.ReadWrite);

            intP?.ReleaseResources();
            intP = new Interpreter("assets/filter/game/perlin.fl", DataTypes.UCHAR1, buf, 512, 512, 1, 4, "assets/kernel");


            do
            {
                intP.Step();
            } while (!intP.Terminated);

            byte[] b = intP.GetResult<byte>();

            playerSphereTexture = TextureLoader.BytesToTexture(new byte[512 * 512 * 4], 512, 512);
            playerSphereSpecTexture = TextureLoader.BytesToTexture(new byte[512 * 512 * 4], 512, 512);
            CreatePlayerTexture(playerSphereTexture, playerSphereSpecTexture, 512, 512);

            wallTextures = new Texture[2];
            wallSpecTextures = new Texture[2];
            for (int i = 0; i < 2; i++)
            {
                wallTextures[i] = TextureLoader.BytesToTexture(new byte[512 * 512 * 4], 512, 512);
                wallSpecTextures[i] = TextureLoader.BytesToTexture(new byte[512 * 512 * 4], 512, 512);
                CreateWallTexture(new Bitmap(512, 512), wallTextures[i], wallSpecTextures[i], 512, 512, i);
            }
        }

        public static void CreateGroundTexture(Bitmap input, Texture destTexture, Texture specTexture)
        {
            if (!_initPerlin) InitPerlin();
            intP?.ReleaseResources();
            intP = new Interpreter("assets/filter/game/cobble_grass.fl", DataTypes.UCHAR1, TextureLoader.TextureToMemoryBuffer(destTexture), (int)destTexture.Width, (int)destTexture.Height, 1, 4, "assets/kernel");


            do
            {
                intP.Step();
            } while (!intP.Terminated);


            TextureLoader.Update(destTexture, intP.GetResult<byte>(), (int)destTexture.Width, (int)destTexture.Height);
            CLBufferInfo spe = intP.GetBuffer("specularOut");
            if (spe != null)
            {
                byte[] spec = CLAPI.ReadBuffer<byte>(spe.Buffer, (int)spe.Buffer.Size);

                TextureLoader.Update(specTexture, spec, (int)specTexture.Width, (int)specTexture.Height);
            }
        }

        public static void CreateWallTexture(Bitmap input, Texture destTexture, Texture specTexture, int width, int height, int i)
        {
            if (!_initPerlin) InitPerlin();
            intP?.ReleaseResources();
            intP = new Interpreter($"assets/filter/game/wall{i}.fl", DataTypes.UCHAR1, TextureLoader.TextureToMemoryBuffer(destTexture), width, height, 1, 4, "assets/kernel");

            do
            {
                intP.Step();
            } while (!intP.Terminated);

            TextureLoader.Update(destTexture, intP.GetResult<byte>(), width, height);
            CLBufferInfo spe = intP.GetBuffer("specularOut");
            if (spe != null)
            {
                byte[] spec = CLAPI.ReadBuffer<byte>(spe.Buffer, (int)spe.Buffer.Size);

                TextureLoader.Update(specTexture, spec, (int)specTexture.Width, (int)specTexture.Height);
            }

        }

        public static void CreatePlayerTexture(Texture destTexture, Texture specTexture, int width, int height)
        {
            if (!_initPerlin) InitPerlin();
            intP?.ReleaseResources();
            intP = new Interpreter($"assets/filter/game/tennisball.fl", DataTypes.UCHAR1, TextureLoader.TextureToMemoryBuffer(destTexture), 512, 512, 1, 4, "assets/kernel");

            do
            {
                intP.Step();
            } while (!intP.Terminated);

            byte[] b = intP.GetResult<byte>();
            TextureLoader.Update(destTexture, intP.GetResult<byte>(), 512, 512);
            CLBufferInfo spe = intP.GetBuffer("specularOut");
            if (spe != null)
            {
                byte[] spec = CLAPI.ReadBuffer<byte>(spe.Buffer, (int)spe.Buffer.Size);

                TextureLoader.Update(specTexture, spec, (int)specTexture.Width, (int)specTexture.Height);
            }
        }


    }
}
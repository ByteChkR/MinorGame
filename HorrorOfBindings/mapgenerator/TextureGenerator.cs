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
        
        private static MemoryBuffer perlin512;
        private static bool _initPerlin = false;
        private static Texture[] wallTextures;

        public static Texture GetTexture(int type)
        {
            if(!_initPerlin)InitPerlin();
            return wallTextures[type];

        }

        private static void InitPerlin()
        {
            _initPerlin = true;
            

            MemoryBuffer buf = CLAPI.CreateEmpty<byte>(512 * 512 * 4, MemoryFlag.ReadWrite);

            intP?.ReleaseResources();
            intP = new Interpreter("filter/game/perlin.fl", DataTypes.CHAR1, buf, 512, 512, 1, 4, "kernel_");


            do
            {
                intP.Step();
            } while (!intP.Terminated);

            byte[] b = intP.GetResult<byte>();
            perlin512 = intP.GetActiveBuffer();
            


            wallTextures = new Texture[3];
            for (int i = 0; i < 3; i++)
            {
                wallTextures[i] = TextureLoader.BytesToTexture(new byte[512 * 512 * 4], 512, 512);
                CreateWallTexture(new Bitmap(512, 512), wallTextures[i], 512, 512, i);
            }
        }

        public static void CreateGroundTexture(Bitmap input, Texture destTexture)
        {
            if (!_initPerlin) InitPerlin();
            intP?.ReleaseResources();
            intP = new Interpreter("filter/game/ground.fl", DataTypes.CHAR1, perlin512, (int)destTexture.Width, (int)destTexture.Height, 1, 4, "kernel_");


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
            intP = new Interpreter($"filter/game/wall{i}.fl", DataTypes.CHAR1, perlin512, width, height, 1, 4, "kernel_");
            
            do
            {
                intP.Step();
            } while (!intP.Terminated);

            TextureLoader.Update(destTexture, intP.GetResult<byte>(), width, height);

        }
    }
}
using System.Drawing;
using MinorEngine.CLHelperLibrary;
using MinorEngine.CLHelperLibrary.cltypes;
using MinorEngine.engine.core;
using MinorEngine.engine.rendering;
using MinorEngine.FilterLanguage;

namespace MinorGame.mapgenerator
{
    public static class TextureGenerator
    {
        public static void CreateGroundTexture(Bitmap input, GameTexture destTexture, int width, int height)
        {
            Bitmap bmp = new Bitmap(input, (int)destTexture.Width, (int)destTexture.Height);
            

        }
    }
}
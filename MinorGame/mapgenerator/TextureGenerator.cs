using System.Drawing;
using Engine.DataTypes;

namespace MinorGame.mapgenerator
{
    public static class TextureGenerator
    {
        public static void CreateGroundTexture(Bitmap input, Texture destTexture, int width, int height)
        {
            Bitmap bmp = new Bitmap(input, (int)destTexture.Width, (int)destTexture.Height);
            

        }
    }
}
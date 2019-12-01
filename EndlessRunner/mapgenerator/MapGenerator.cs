using System.Collections.Generic;
using System.Linq;
using EndlessRunner.scenes;
using Engine.Core;
using Engine.DataTypes;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Rendering;
using Engine.WFC;
using MinorGame.mapgenerator;
using OpenTK;
using Vector3 = Engine.Physics.BEPUutilities.Vector3;

namespace EndlessRunner.mapgenerator
{
    public class MapGenerator
    {
        private static WFCOverlayMode _wfc;

        public static List<GameObject> Generate(string filename, int width, int length)
        {
            _wfc = new WFCOverlayMode(filename, 3, width, length, false, true, 8, 0);


            do
            {
                _wfc.Run(0);
            } while (!_wfc.Success);

            byte[] map = TextureLoader.BitmapToBytes(_wfc.Graphics());


            return TileCreator.CreateTileMap(TileCreator.CreateObject_Box, map, width, length, 3, 1,
                new Vector2(width, length)).ToList();
        }
    }
}
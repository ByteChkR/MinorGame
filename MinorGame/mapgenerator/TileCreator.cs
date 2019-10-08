using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using MinorEngine.BEPUphysics.Entities.Prefabs;
using MinorEngine.BEPUutilities;
using MinorEngine.components;
using MinorEngine.debug;
using MinorEngine.engine.core;
using MinorEngine.engine.rendering;
using MinorGame.exceptions;
using Vector2 = OpenTK.Vector2;

namespace MinorGame.mapgenerator
{
    public class TileCreator
    {
        private static GameTexture tex = ResourceManager.TextureIO.FileToTexture("textures/TEST.png");
        public delegate GameObject CreateObject(byte input, Vector3 pos, Vector3 scale, ShaderProgram program);

        public static GameObject CreateObject_Box(byte input, Vector3 pos, Vector3 scale, ShaderProgram program)
        {
            if (input < 128)
            {
                return CreateCube(pos, scale, Quaternion.Identity, tex, program);
            }

            return null;
        }

        public static GameObject[] CreateTileMap(CreateObject creator, byte[] data, int width, int height, float tileYOffset, float tileHeight, Vector2 fieldSize, ShaderProgram program)
        {
            List<GameObject> ret = new List<GameObject>();
            if (width * height != data.Length)
            {
                Logger.Crash(new GameException("Tilemap has the wrong format"), false);
                return ret.ToArray();
            }

            if (creator == null)
            {
                Logger.Crash(new GameException("Object Creator was null"), false);
                return ret.ToArray();
            }

            Vector2 fieldHalfSize = fieldSize / 2f;

            float xDelta = fieldSize.X / width;
            float yDelta = fieldSize.Y / height;
            float xScale = fieldSize.X / width / 2;
            float zScale = fieldSize.Y / height / 2;
            float yScale = tileHeight / 2;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int index = i * height + j;
                    Vector3 tilePos = new Vector3(i * xDelta, tileYOffset, j * yDelta) - new Vector3(fieldHalfSize.X, 1, fieldHalfSize.Y);
                    GameObject obj = creator(data[index], tilePos, new Vector3(xScale, yScale, zScale), program);
                    if (obj != null)
                    {
                        ret.Add(obj);
                    }
                }
            }

            return ret.ToArray();
        }

        public static GameObject CreateCube(Vector3 position, Vector3 scale, Quaternion rotation, GameTexture texture,
            ShaderProgram program, int mass = -1)
        {
            GameObject box = new GameObject(position, "Box");
            box.Scale = scale;
            box.Rotation = rotation;
            GameMesh cube = ResourceManager.Prefabs.Cube;
            cube.SetTextureBuffer(new[] { texture });
            box.AddComponent(new MeshRendererComponent(program, cube, 1, false));
            Vector3 bounds = scale * 2;
            Collider coll;
            if (mass == -1)
            {
                coll = new Collider(new Box(Vector3.Zero, bounds.X, bounds.Y, bounds.Z), "physics");
            }
            else
            {
                coll = new Collider(new Box(Vector3.Zero, bounds.X, bounds.Y, bounds.Z, mass), "physics");
            }
            box.AddComponent(coll);
            return box;
        }
    }
}
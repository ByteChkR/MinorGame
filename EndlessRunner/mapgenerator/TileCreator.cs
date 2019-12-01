using System.Collections.Generic;
using System.Resources;
using System.Runtime.ExceptionServices;
using EndlessRunner.scenes;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Rendering;
using OpenTK;
using Vector2 = OpenTK.Vector2;

namespace MinorGame.mapgenerator
{
    public class TileCreator
    {
        private static Texture tex1 = TextureLoader.FileToTexture("textures/sphereTexture.png");
        private static Texture tex2 = TextureLoader.FileToTexture("textures/boxTexture.png");

        public delegate GameObject CreateObject(byte r, byte g, byte b, byte a, Vector3 pos, Vector3 scale);

        public static GameObject CreateObject_Box(byte r, byte g, byte b, byte a, Vector3 pos, Vector3 scale)
        {
            if (r > g)
            {
                return CreateCube(pos, scale, Quaternion.Identity, tex2);
            }

            if (r <= g)
            {
                return CreateSphere(pos, scale, Quaternion.Identity, tex1);
            }

            if (b <= g)
            {
                return CreateCube(pos, scale, Quaternion.Identity, tex2);
            }

            if (b > r)
            {
                return CreateSphere(pos, scale, Quaternion.Identity, tex1);
            }

            return null;
        }

        public static GameObject[] CreateTileMap(CreateObject creator, byte[] data, int width, int height,
            float tileYOffset, float tileHeight, Vector2 fieldSize)
        {
            List<GameObject> ret = new List<GameObject>();
            if (width * height != data.Length / 4)
            {
                return ret.ToArray();
            }

            if (creator == null)
            {
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
                for (int j = 0; j < height / 4; j++)
                {
                    int index = i * height + j;
                    Vector3 tilePos = new Vector3(i * xDelta, tileYOffset, j * yDelta) -
                                      new Vector3(fieldHalfSize.X, 1, fieldHalfSize.Y);
                    GameObject obj = creator(data[index], data[index + 1], data[index + 2], data[index + 3], tilePos,
                        new Vector3(xScale, yScale, zScale));
                    if (obj != null)
                    {
                        ret.Add(obj);
                    }
                }
            }

            return ret.ToArray();
        }

        public static GameObject CreateCube(Vector3 position, Vector3 scale, Quaternion rotation, Texture texture,
            int mass = -1)
        {
            GameObject box = new GameObject(position, "Box");
            box.Scale = scale;
            box.Rotation = rotation;
            Mesh cube = Prefabs.Cube;
            box.AddComponent(new LitMeshRendererComponent(GameScene.TextureShader, cube, texture, 1, false));
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

        public static GameObject CreateSphere(Vector3 position, Vector3 scale, Quaternion rotation, Texture texture,
            int mass = -1)
        {
            GameObject box = new GameObject(position, "Sphere");
            box.Scale = scale;
            box.Rotation = rotation;
            Mesh sphere = Prefabs.Sphere;
            box.AddComponent(new LitMeshRendererComponent(GameScene.TextureShader, sphere, texture, 1, false));
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Resources;
using System.Runtime.ExceptionServices;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Physics.BEPUphysics.Materials;
using Engine.Rendering;
using MinorGame.exceptions;
using OpenTK;
using Vector2 = OpenTK.Vector2;

namespace MinorGame.mapgenerator
{
    public class TileCreator
    {
        private static Texture boundsTex = TextureLoader.FileToTexture("assets/textures/boundsTexture.png");
        private static Random rnd = new Random();

        public delegate GameObject CreateObject(byte input, Vector3 pos, Vector3 scale, ShaderProgram program);

        public static GameObject CreateObject_Box(byte input, Vector3 pos, Vector3 scale, ShaderProgram program)
        {
            if (input < 128)
            {
                int r = rnd.Next(0, 2);
                return CreateCube(pos, scale, Quaternion.Identity, TextureGenerator.GetTexture(r), program, TextureGenerator.GetSTexture(r));
            }

            return null;
        }

        private static GameObject[] CreateBounds(int width, int height, ShaderProgram program)
        {

            GameObject[] ret = new GameObject[4];
            GameObject obj;
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    obj = new GameObject("BoundsLeft");
                    obj.LocalPosition = new Vector3(-width / 2f, 1, 0);
                    Collider c = new Collider(new Box(Vector3.Zero, 1, 16, height), "physics");
                    c.PhysicsCollider.Material = new Material(0.1f, 0.1f, 0.1f);
                    obj.AddComponent(c);
                    obj.Scale = new Vector3(1, 8, height / 2f);
                }
                else if (i == 1)
                {
                    obj = new GameObject("BoundsRight");
                    obj.LocalPosition = new Vector3(width / 2f, 1, 0);
                    Collider c = new Collider(new Box(Vector3.Zero, 1, 16, height), "physics");
                    c.PhysicsCollider.Material = new Material(0.1f, 0.1f, 0.1f);
                    obj.AddComponent(c);
                    obj.Scale = new Vector3(1, 8, height / 2f);
                }
                else if (i == 2)
                {
                    obj = new GameObject("BoundsTop");
                    obj.LocalPosition = new Vector3(0, 1, -height / 2f);
                    Collider c = new Collider(new Box(Vector3.Zero, width, 16, 1), "physics");
                    c.PhysicsCollider.Material = new Material(0.1f, 0.1f, 0.1f);
                    obj.AddComponent(c);
                    obj.Scale = new Vector3(width / 2f, 8, 1);
                }
                else
                {
                    obj = new GameObject("BoundsBottom");
                    obj.LocalPosition = new Vector3(0, 1, height / 2f);
                    Collider c = new Collider(new Box(Vector3.Zero, width, 16, 1), "physics");
                    c.PhysicsCollider.Material = new Material(0.1f, 0.1f, 0.1f);
                    obj.AddComponent(c);
                    obj.Scale = new Vector3(width / 2f, 8, 1);
                }
                obj.AddComponent(new LitMeshRendererComponent(program, Prefabs.Cube, boundsTex, 1));

                ret[i] = obj;
            }

            return ret;
        }

        public static GameObject[] CreateTileMap(CreateObject creator, byte[] data, int width, int height,
            float tileYOffset, float tileHeight, Vector2 fieldSize, ShaderProgram program)
        {
            List<GameObject> ret = CreateBounds((int)fieldSize.X, (int)fieldSize.Y, program).ToList();
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
                    Vector3 tilePos = new Vector3(i * xDelta, tileYOffset, j * yDelta) -
                                      new Vector3(fieldHalfSize.X, 1, fieldHalfSize.Y);
                    GameObject obj = creator(data[index], tilePos, new Vector3(xScale, yScale, zScale), program);
                    if (obj != null)
                    {
                        ret.Add(obj);
                    }
                }
            }

            return ret.ToArray();
        }

        public static GameObject CreateCube(Vector3 position, Vector3 scale, Quaternion rotation, Texture texture,
            ShaderProgram program, Texture tesS, int mass = -1)
        {
            return CreateCube(position, scale, rotation, texture, program, Vector2.One, Vector2.Zero, tesS, mass);
        }

        public static GameObject CreateCube(Vector3 position, Vector3 scale, Quaternion rotation, Texture texture,
            ShaderProgram program, Vector2 tiling, Vector2 offset, Texture tesS = null, int mass = -1)
        {
            GameObject box = new GameObject(position, "Box");
            box.Scale = scale;
            box.Rotation = rotation;
            Mesh cube = Prefabs.Cube;
            LitMeshRendererComponent mr = new LitMeshRendererComponent(program, cube, texture, 1, false);
            if (tesS != null)
            {
                tesS.TexType = TextureType.Specular;
                mr.Textures = new[] {mr.Textures[0], tesS};
            }
            mr.Tiling = tiling;
            mr.Offset = offset;
            box.AddComponent(mr);
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

            coll.PhysicsCollider.Material = new Material(0.1f, 0.1f, 0.1f);
            box.AddComponent(coll);
            return box;
        }
    }
}
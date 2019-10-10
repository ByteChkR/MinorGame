using System.Collections.Generic;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Physics.BEPUphysics.Materials;
using Engine.Rendering;
using EndlessRunner.components;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace EndlessRunner.scenes
{
    public class GameScene : AbstractScene
    {

        public static ShaderProgram TextureShader;
        protected override void InitializeScene()
        {
            ShaderProgram.TryCreate(new Dictionary<ShaderType, string>
            {
                {ShaderType.FragmentShader, "shader/texture.fs"},
                {ShaderType.VertexShader, "shader/texture.vs"}
            }, out TextureShader);


            Add(DebugConsoleComponent.CreateConsole());
            BasicCamera camera =
                new BasicCamera(
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f), 16f / 9f, 0.01f, 1000),
                    new Vector3(0, 6, 6));
            SetCamera(camera);
            Add(camera);
            
            Add(PlayerController.CreatePlayer(Vector3.UnitY * 10, camera));
            Add(CreateGround(new Vector3(50, 1, 50), TextureShader));

        }

        public static GameObject CreateCube(Vector3 position, Vector3 scale, Quaternion rotation, Texture texture,
            ShaderProgram program, int mass = -1)
        {
            GameObject box = new GameObject(position, "Box");
            box.Scale = scale;
            box.Rotation = rotation;
            Mesh cube = MeshLoader.Prefabs.Cube;
            box.AddComponent(new MeshRendererComponent(program, cube, texture, 1, false));
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


        private static GameObject CreateGround(Vector3 scale, ShaderProgram shader)
        {
            GameObject ret = CreateCube(Vector3.Zero, scale, Quaternion.Identity,
                TextureLoader.FileToTexture("textures/groundTexture.png"), shader);
            ret.Name = "Ground";
            Collider groundColl = ret.GetComponent<Collider>();
            groundColl.PhysicsCollider.Material = new Material(10, 10, 0);
            return ret;
        }
    }
}
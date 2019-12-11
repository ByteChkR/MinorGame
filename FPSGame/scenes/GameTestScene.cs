using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.OpenCL;
using Engine.OpenCL.DotNetCore.Memory;
using Engine.OpenCL.TypeEnums;
using Engine.OpenFL;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Physics.BEPUphysics.Materials;
using Engine.Rendering;
using FPSGame.components;
using OpenTK;

namespace FPSGame.scenes
{
    public class GameTestScene : AbstractScene
    {
        private Texture GenerateMenuBackground()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int texWidth = 128;
            int texHeight = 128;
            Interpreter i = new Interpreter(Clapi.MainThread, "assets/filter/game/grass.fl", DataTypes.Uchar1,
                Clapi.CreateEmpty<byte>(Clapi.MainThread, texWidth * texHeight * 4, MemoryFlag.ReadWrite), texWidth,
                texHeight, 1, 4, "assets/kernel/", true);

            do
            {
                i.Step();
            } while (!i.Terminated);

            Texture tex = TextureLoader.ParameterToTexture(texWidth, texHeight);
            TextureLoader.Update(tex, i.GetResult<byte>(), (int) tex.Width, (int) tex.Height);
            Logger.Log("Time for Menu Background(ms): " + sw.ElapsedMilliseconds, DebugChannel.Log, 10);
            sw.Stop();
            return tex;
        }

        protected override void InitializeScene()
        {
            LayerManager.RegisterLayer("physics", new Layer(0, 1));

            BasicCamera mainCamera =
                new BasicCamera(
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                        GameEngine.Instance.Width / (float) GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);

            object mc = mainCamera;

            SetCamera(mainCamera);
            Add(mainCamera);
            mainCamera.AddComponent(new FPSController());
            mainCamera.LocalPosition = new Engine.Physics.BEPUutilities.Vector3(0, 3, 0);
            Collider playerColl = new Collider(new Sphere(new Engine.Physics.BEPUutilities.Vector3(0, 3, 0), 1f, 5f),
                "physics");
            mainCamera.AddComponent(playerColl);
            playerColl.UpdateRotation = false;
            playerColl.ColliderConstraints = new ColliderConstraints {RotationConstraints = FreezeConstraints.ALL};
            playerColl.PhysicsCollider.Material = new Material(1.5f, 1.5f, 0);
            playerColl.PhysicsCollider.LinearDamping = 0.99f;
            for (int i = 0; i < 50; i++)
            {
                GameObject obj = new GameObject(new Vector3(i * 2 - 50, 3, -10), "Cube" + i);
                LitMeshRendererComponent lmr = new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader,
                    Prefabs.Cube, TextureLoader.ColorToTexture(Color.Red), 1);
                Collider bcoll = new Collider(new Box(Engine.Physics.BEPUutilities.Vector3.Zero, 1, 1, 1, 1f),
                    "physics");
                bcoll.PhysicsCollider.Material = new Material(10, 10, 0);

                obj.AddComponent(lmr);
                obj.AddComponent(bcoll);
                Add(obj);
            }

            GameObject ground = new GameObject("Ground");
            LitMeshRendererComponent groundRenderer = new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader,
                Prefabs.Cube, GenerateMenuBackground(), 1);
            ground.AddComponent(groundRenderer);
            Collider coll = new Collider(new Box(Engine.Physics.BEPUutilities.Vector3.Zero, 100, 100, 100), "physics");
            coll.PhysicsCollider.Material = new Material(10, 10, 0);
            ground.AddComponent(coll);
            Add(ground);
            ground.Scale = new Engine.Physics.BEPUutilities.Vector3(50, 50, 50);
            ground.LocalPosition = new Engine.Physics.BEPUutilities.Vector3(0, -50, 0);
        }
    }
}
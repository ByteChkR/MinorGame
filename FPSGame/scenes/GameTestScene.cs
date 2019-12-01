using System.Drawing;
using Engine.Core;
using Engine.DataTypes;
using Engine.IO;
using Engine.Rendering;
using FPSGame.components;
using OpenTK;

namespace FPSGame.scenes
{
    public class GameTestScene : AbstractScene
    {
        protected override void InitializeScene()
        {
            BasicCamera mainCamera =
                new BasicCamera(
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                        GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);

            object mc = mainCamera;
            SetCamera(mainCamera);
            Add(mainCamera);
            mainCamera.AddComponent(new FPSController());

            for (int i = 0; i < 50; i++)
            {
                GameObject obj = new GameObject(new Vector3(i * 2 - 50, 0, -10), "Cube" + i);
                LitMeshRendererComponent lmr = new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, Prefabs.Cube, TextureLoader.ColorToTexture(Color.Red), 1);
                obj.AddComponent(lmr);
                Add(obj);
            }

        }
    }
}
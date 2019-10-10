using Engine.Core;
using Engine.Debug;

namespace MinorGame.scenes
{
    public class EmptyTestScene : AbstractScene
    {
        protected override void InitializeScene()
        {
            GameEngine.Instance.CurrentScene.Add(DebugConsoleComponent.CreateConsole());
        }
    }
}
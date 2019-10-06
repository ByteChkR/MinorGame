using MinorEngine.engine.components;
using MinorEngine.engine.core;

namespace MinorGame.scenes
{
    public class EmptyTestScene : AbstractScene
    {
        protected override void InitializeScene()
        {
            GameEngine.Instance.World.Add(DebugConsoleComponent.CreateConsole());
        }
    }
}
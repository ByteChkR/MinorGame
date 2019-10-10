using Engine.Core;
using Engine.Debug;

namespace FPSGame.scenes
{
    public class GameScene : AbstractScene
    {
        protected override void InitializeScene()
        {
            Add(DebugConsoleComponent.CreateConsole());   
        }
    }
}
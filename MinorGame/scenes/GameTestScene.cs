
using GameEngine.engine.core;
using GameEngine.engine.ui.utils;
using OpenTK;

namespace MinorGame.scenes
{
    public class GameTestScene : AbstractScene
    {

        protected override void InitializeScene()
        {
            GameObject dbgConsole = DebugConsoleComponent.CreateConsole();
            SceneRunner.Instance.World.Add(dbgConsole);
        }

        public override void OnDestroy()
        {

        }

        public override void Update(float deltaTime)
        {

        }
    }
}
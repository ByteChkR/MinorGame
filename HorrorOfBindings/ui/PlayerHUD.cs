using System.Linq.Expressions;
using Engine.Core;
using Engine.IO;
using Engine.UI;
using MinorGame.components;
using MinorGame.scenes;
using OpenTK;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;

namespace MinorGame.ui
{
    public class PlayerHUD : AbstractComponent
    {
        public static PlayerHUD Instance;
        private UITextRendererComponent EnemiesLeftText;
        private UIImageRendererComponent PlayerHPBarBGObj;
        private UIImageRendererComponent PlayerHPBarObj;
        private PlayerController Controller;

        public PlayerHUD()
        {
            PlayerController.OnHPChange += PlayerHPUpdate;
            EnemyComponent.OnEnemyKilled += EnemyKilled;
        }

        protected override void Awake()
        {
            Instance = this;


            GameObject enemiesLeftObj = new GameObject("EnemiesLeft");
            EnemiesLeftText = new UITextRendererComponent("Arial", false, 1, GameTestScene.TextShader);
            enemiesLeftObj.AddComponent(EnemiesLeftText);
            EnemiesLeftText.Position = new Vector2(0.2f, -0.45f);
            EnemiesLeftText.Scale = new Vector2(2f);
            EnemiesLeftText.Text = "Enemies Left: 0/0";
            Owner.Add(enemiesLeftObj);


            Bitmap bmp = new Bitmap(1, 1);


            GameObject playerHPBarBGObj = new GameObject("PlayerHPBar");
            bmp.SetPixel(0, 0, Color.Black);

            PlayerHPBarBGObj = new UIImageRendererComponent(TextureLoader.BitmapToTexture(bmp), false, 1, GameTestScene.UIImageShader);
            playerHPBarBGObj.AddComponent(PlayerHPBarBGObj);
            PlayerHPBarBGObj.Position = new Vector2(0.0f, 0.9f);
            PlayerHPBarBGObj.Scale = new Vector2(0.31f, 0.05f * GameEngine.Instance.AspectRatio * 1.2f);
            Owner.Add(playerHPBarBGObj);

            GameObject playerHPBarObj = new GameObject("PlayerHPBar");

            bmp.SetPixel(0, 0, Color.Red);

            PlayerHPBarObj = new UIImageRendererComponent(TextureLoader.BitmapToTexture(bmp), false, 1, GameTestScene.UIImageShader);
            playerHPBarObj.AddComponent(PlayerHPBarObj);
            PlayerHPBarObj.Position = new Vector2(0.0f, 0.9f);
            PlayerHPBarObj.Scale = new Vector2(0.3f, 0.05f * GameEngine.Instance.AspectRatio);
            Owner.Add(playerHPBarObj);


        }

        protected override void OnDestroy()
        {

            PlayerController.OnHPChange -= PlayerHPUpdate;
            EnemyComponent.OnEnemyKilled -= EnemyKilled;
        }

        private void PlayerHPUpdate(float hpRatio)
        {
            Vector2 v = PlayerHPBarObj.Scale;
            v.X = 0.3f * hpRatio;
            Vector2 v1 = PlayerHPBarObj.Position;
            v1.X = -(1 - hpRatio) * 0.3f;

            PlayerHPBarObj.Position = v1;
            PlayerHPBarObj.Scale = v;
        }

        private void EnemyKilled(int newEnemyCount, int maxCount)
        {
            EnemiesLeftText.Text = $"Enemies Remaining: {newEnemyCount}/{maxCount}";
        }

        protected override void Update(float deltaTime)
        {

        }
    }
}
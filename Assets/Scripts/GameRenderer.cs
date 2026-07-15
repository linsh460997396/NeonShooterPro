using UnityEngine;

namespace NeonShooter
{
    public class GameRenderer : MonoBehaviour
    {
        public static GameRenderer Instance { get; private set; }

        public Font gameFont;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnGUI()
        {
            if (GameManager.Grid != null)
                GameManager.Grid.Draw();
            EntityManager.Draw();
            if (GameManager.ParticleManager != null)
                GameManager.ParticleManager.Draw();

            if (gameFont == null)
            {
                gameFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (gameFont == null)
                    gameFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }

            GUI.skin.font = gameFont;
            GUI.skin.label.normal.textColor = Color.white;

            GUI.Label(new Rect(5, 5, 200, 30), "Lives: " + PlayerStatus.Lives);

            string scoreText = "Score: " + PlayerStatus.Score;
            float scoreWidth = gameFont.fontSize * scoreText.Length * 0.6f;
            GUI.Label(new Rect(Screen.width - scoreWidth - 5, 5, scoreWidth, 30), scoreText);

            string multiplierText = "Multiplier: " + PlayerStatus.Multiplier;
            float multiplierWidth = gameFont.fontSize * multiplierText.Length * 0.6f;
            GUI.Label(new Rect(Screen.width - multiplierWidth - 5, 35, multiplierWidth, 30), multiplierText);

            if (Art.Pointer != null)
            {
                Vector2 mousePos = Input.MousePosition;
                GUI.DrawTexture(new Rect(mousePos.x - 16, mousePos.y - 16, 32, 32), Art.Pointer);
            }

            if (PlayerStatus.IsGameOver)
            {
                string text = "Game Over\nYour Score: " + PlayerStatus.Score + "\nHigh Score: " + PlayerStatus.HighScore;
                string[] lines = text.Split('\n');
                float textHeight = lines.Length * gameFont.fontSize * 1.5f;
                float maxWidth = 0;
                foreach (string line in lines)
                {
                    float lineWidth = gameFont.fontSize * line.Length * 0.6f;
                    if (lineWidth > maxWidth)
                        maxWidth = lineWidth;
                }

                Rect textRect = new Rect(Screen.width / 2 - maxWidth / 2, Screen.height / 2 - textHeight / 2, maxWidth, textHeight);
                GUI.Label(textRect, text);
            }
        }

        void Start()
        {
            Instance = this;
        }
    }
}
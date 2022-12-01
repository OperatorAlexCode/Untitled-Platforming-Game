using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Enums;
using Platforming_Game.GameObjects;
using SharpDX.Direct2D1;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;
using Color = Microsoft.Xna.Framework.Color;
using spriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Platforming_Game.Managers
{
    /// <summary>
    /// Manages what UI is drawn on the screen
    /// </summary>
    public static class UIManager
    {
        // Int
        static int ScreenHeight;
        static int ScreenWidth;
        static int HealthIconSpacing = 10;
        static int HealthIconYpos = 10;

        // Vector2
        static Vector2 HealthIconDimensions = new(20, 20);
        static Vector2 CameraPos;

        // Other
        static SpriteBatch SpriteBatch;
        static SpriteFont GameFont;
        static float DrawLayer = 1.0f;
        static Color HealthIconColor = Color.HotPink;
        static Player PlayerChar;
        static Texture2D PixelTex;

        public static void InitializeFields(spriteBatch spriteBatch,SpriteFont gameFont, int screenHeight, int screenWidth, float? drawLayer)
        {
            SpriteBatch = spriteBatch;
            GameFont = gameFont;
            ScreenHeight = screenHeight;
            ScreenWidth = screenWidth;
            if (drawLayer.HasValue)
                DrawLayer = drawLayer.Value;

            PixelTex = new(spriteBatch.GraphicsDevice, 1, 1);
            PixelTex.SetData(new[] { Color.White });
        }

        public static void Update(Player player, Vector2 cameraPos)
        {
            PlayerChar = player;
            CameraPos = cameraPos;
        }

        public static void Draw(spriteBatch spriteBatch,GameState currentState)
        {
            SpriteBatch = spriteBatch;
            switch (currentState)
            {
                case GameState.InMenu:
                    DrawMainMenu();
                    break;
                case GameState.InGame:
                    DrawHud();
                    break;
                case GameState.InLevelEditor:
                    DrawLevelEditorHud();
                    break;
                case GameState.GameEnd:
                    DrawEndScreen();
                    break;
                default:
                    DrawErrorScreen();
                    break;
            }
        }

        static void DrawMainMenu()
        {
            float titleScale = 2.5f;
            float optionsScale = 1.5f;

            string titleStr = "Untitled Platforming Game";
            string beginningStr = "Press Enter To Start From Beginning";
            string randomStr = "Press R To Start From Random Level";

            Vector2 titleSize = MeasureString(titleStr) * titleScale;
            Vector2 beginningSize = MeasureString(beginningStr) * optionsScale;
            Vector2 randomSize = MeasureString(randomStr) * optionsScale;

            Vector2 winPos = new(ScreenWidth / 2 - titleSize.X / 2, ScreenHeight / 3);
            Vector2 beginninPos = new(ScreenWidth / 2 - beginningSize.X / 2, ScreenHeight / 2);
            Vector2 randomPos = new(ScreenWidth / 2 - randomSize.X/2, beginninPos.Y+randomSize.Y);

            DrawStringOnScreen(titleStr, winPos, titleScale);
            DrawStringOnScreen(beginningStr, beginninPos, optionsScale);
            DrawStringOnScreen(randomStr, randomPos, optionsScale);
        }

        static void DrawEndScreen()
        {
            float scale = 2.0f;

            string winStr = "You Won";
            string playAgainStr = "Play Again? (Y/N)";

            Vector2 winSize = MeasureString(winStr) * scale;
            Vector2 playAgainSize = MeasureString(playAgainStr) * scale;

            Vector2 winPos = new(ScreenWidth/2- winSize.X/2,ScreenHeight/3);
            Vector2 playAgainPos = new(ScreenWidth / 2 - playAgainSize.X / 2, ScreenHeight / 2);

            DrawStringOnScreen(winStr, winPos,scale);
            DrawStringOnScreen(playAgainStr, playAgainPos,scale);
        }

        static void DrawHud()
        {
            int startSpacing = 10;

            for (int x = 0; x < PlayerChar.GetCurrentHealth(); x++)
            {
                Rectangle destRec = new(startSpacing + ((int)HealthIconDimensions.X  + HealthIconSpacing) * x, HealthIconYpos, (int)HealthIconDimensions.X, (int)HealthIconDimensions.Y);
                DrawTexOnScreen(destRec, HealthIconColor);
            }
        }

        static void DrawLevelEditorHud()
        {
            int centerPointSize = 2;

            Rectangle destRec = new((ScreenWidth - centerPointSize) /2, (ScreenHeight - centerPointSize) /2, centerPointSize, centerPointSize);

            DrawTexOnScreen(destRec, Color.White);

            float cordScale = 1.2f;

            string xPosText = $"X:{CameraPos.X}";
            string yPosText = $"Y:{CameraPos.Y}";

            Vector2 xPosTextSize = MeasureString(xPosText);
            Vector2 yPosTextSize = MeasureString(yPosText);

            Vector2 xPosTextPos = new(0, ScreenHeight - (xPosTextSize.Y + yPosTextSize.Y) * cordScale);
            Vector2 yPosTextPos = new(0, ScreenHeight - yPosTextSize.Y * cordScale);

            DrawStringOnScreen(xPosText, xPosTextPos, cordScale);
            DrawStringOnScreen(yPosText, yPosTextPos, cordScale);

            if (LevelEditorManager.GetAddChangeState() != 0)
            {
                float scale = 1.3f;

                string topLeftText = "";
                string topRightText = "";

                switch (LevelEditorManager.GetState())
                {
                    case LevelManagerState.AddPlatform:
                        topRightText = "Platform";
                        break;
                    case LevelManagerState.ChangePlayerPos:
                        topRightText = "Player Start";
                        break;
                    case LevelManagerState.ChangeGoal:
                        topRightText = "Goal";
                        break;
                    case LevelManagerState.AddGoomba:
                        topRightText = "Goomba";
                        break;
                    case LevelManagerState.AddJumpPad:
                        topRightText = "Jump Pad";
                        break;
                    case LevelManagerState.AddLavaPool:
                        topRightText = "Lava Pool";
                        break;
                }

                Vector2 topRightTextSize = MeasureString(topRightText); 
                Vector2 topRightTextPos = new(ScreenWidth-topRightTextSize.X * scale, 0);

                switch (LevelEditorManager.GetAddChangeState())
                {
                    case 1:
                        topLeftText = $"X : {LevelEditorManager.GetValueCache()}";
                        break;
                    case 2:
                        topLeftText = $"Y : {LevelEditorManager.GetValueCache()}";
                        break;
                    case 3:
                        topLeftText = $"Width : {LevelEditorManager.GetValueCache()}";
                        break;
                    case 4:
                        topLeftText = $"Height : {LevelEditorManager.GetValueCache()}";
                        break;
                }

                DrawStringOnScreen(topLeftText,Vector2.Zero, scale);
                DrawStringOnScreen(topRightText, topRightTextPos, scale);
            }
        }

        static void DrawErrorScreen()
        {
            float scale = 5.0f;

            string errorStr = "ERROR!";

            Vector2 errorSize = MeasureString(errorStr) * scale;

            Vector2 errorPos = new(ScreenWidth / 2 - errorSize.X / 2, ScreenHeight / 2 - errorSize.Y/2);

            DrawStringOnScreen(errorStr, errorPos, scale);

        }

        /// <summary>
        /// Takes a string and returns it's height and width
        /// </summary>
        /// <param name="stringToMeasure">The string that is to be measured</param>
        /// <returns>Vector2 containing the length and width of the string</returns>
        static Vector2 MeasureString(string stringToMeasure)
        {
            return GameFont.MeasureString(stringToMeasure);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringToDraw">String that is to be drawn</param>
        /// <param name="drawPos">Position to draw</param>
        /// <param name="scale">Scale of text</param>
        static void DrawStringOnScreen(string stringToDraw, Vector2 drawPos, float scale = 1.0f)
        {
            SpriteBatch.DrawString(GameFont, stringToDraw, drawPos, Color.White, 0f, new Vector2(), scale, new SpriteEffects(), DrawLayer);
        }

        static void DrawTexOnScreen(Rectangle destRec, Color color)
        {
            SpriteBatch.Draw(PixelTex, destRec, null, color, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }

        static void DrawTexOnScreen(Texture2D tex, Rectangle destRec, Color color)
        {
            SpriteBatch.Draw(tex, destRec, null, color, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platforming_Game.Enums;
using Platforming_Game.GameObjects;
using Platforming_Game.Managers;
using SharpDX.Direct2D1;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Platforming_Game
{
    public class Game1 : Game
    {
        // Int
        int Height = 600;
        int Width = 600;
        int LastPlayerHp;

        // Keys
        Keys LevelEditorKey = Keys.Home;
        List<Keys> ValidMenuInputKeys = new() { Keys.Enter, Keys.R, Keys.C };
        List<Keys> NumberKeys = new() { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };

        List<Keys[]> AllowedOtherInputs = new() {
            new Keys[] { Keys.OemMinus, Keys.Subtract }
        };

        List<Keys[]> ValidInputKeys = new() {
            new Keys[] { Keys.W, Keys.S, Keys.A, Keys.D, Keys.Space },
            new Keys[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Space }
        };

        List<Keys[]> ValidEditorInputKeys = new() {
            //new Keys[] { Keys.W, Keys.S, Keys.A, Keys.D, Keys.P, Keys.S, Keys.Delete,Keys.Back, Keys.Enter },
            new Keys[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Enter, Keys.Back, Keys.Tab,Keys.Delete, Keys.R, Keys.P, Keys.S,Keys.E,Keys.G, Keys.J, Keys.L, Keys.O }
        };

        // Vector 2
        Vector2 CameraDimensions = new(400, 600);
        Vector2 LevelEditorCameraPos;
        Vector2 LevelEditorCamSpeed = new(2, 2);

        // Other
        private GraphicsDeviceManager Graphics;
        private SpriteBatch SpriteBatch;
        GameState CurrentState = GameState.InMenu;
        float DeltaTime;
        Player PlayerChar;
        Camera Camera;
        Color BackGroundColor = Color.CornflowerBlue;
        bool KeyIsPressed;

        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Graphics.PreferredBackBufferWidth = Width;
            Graphics.PreferredBackBufferHeight = Height;
            Graphics.ApplyChanges();

            UIManager.InitializeFields(SpriteBatch, Content.Load<SpriteFont>("GameFont"), Height, Width, 1.0f);
            EnemyManager.InstantializeFields();
            LevelManager.InitalizeFields(new int[] { Height, Width }, SpriteBatch, new() { { "Goal", LoadNextLevel }, { "Kill", RestartLevel } });
            LevelEditorManager.InstantializeFields(new int[] { Height, Width },NumberKeys.ToArray(), AllowedOtherInputs);
            ItemManager.InitalizeFields();

            Camera = new Camera();
            Camera.ChangeCameraSpecs((int)CameraDimensions.Y, (int)CameraDimensions.X);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            GameState? newState = null;
            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] pressedKeys = keyboardState.GetPressedKeys();

            if (keyboardState.GetPressedKeyCount() == 0)
                KeyIsPressed = false;

            switch (CurrentState)
            {
                case GameState.InMenu:
                    if (keyboardState.GetPressedKeyCount() == 1 && pressedKeys.Any(k => ValidMenuInputKeys.Contains(k)) && !KeyIsPressed)
                    {
                        StartGame(ValidMenuInputKeys.IndexOf(pressedKeys[0]));
                        KeyIsPressed = true;
                    }
                    break;
                case GameState.InGame:
                    LevelManager.Update(DeltaTime, PlayerChar, out newState);
                    UpdateEntities(DeltaTime);
                    ItemManager.Update(PlayerChar);
                    Camera.FollowHorizontaly(PlayerChar);
                    if (newState.HasValue)
                        CurrentState = newState.Value;

                    if (keyboardState.IsKeyDown(LevelEditorKey) && !KeyIsPressed)
                    {
                        StartGame(2);
                        CurrentState = GameState.InLevelEditor;
                        KeyIsPressed = true;
                        Camera.ChangeCameraSpecs(Height, Width);
                        LevelEditorCameraPos = new(PlayerChar.DestRec.X, PlayerChar.DestRec.Y);
                        LevelEditorManager.StartUp();
                        Camera.FollowExact(LevelEditorCameraPos);
                    }
                    break;
                case GameState.InLevelEditor:
                    if (keyboardState.IsKeyDown(LevelEditorKey) && !KeyIsPressed)
                    {
                        CurrentState = GameState.InGame;
                        LevelManager.RestartLevel(out PlayerChar);
                        KeyIsPressed = true;
                        Camera.ChangeCameraSpecs((int)CameraDimensions.Y, (int)CameraDimensions.X);
                        Camera.FollowExact(PlayerChar);
                    }
                    else
                    {
                        List<int> inputs = new();
                        foreach (Keys[] inputScheme in ValidEditorInputKeys)
                            inputs.AddRange(Array.FindAll(inputScheme, k => pressedKeys.Contains(k)).Select(k => Array.IndexOf(inputScheme, k)).Where(x => x != -1));

                        inputs = inputs.Distinct().ToList();

                        if (inputs.Count > 0)
                        {
                            foreach (int index in inputs.Where(i => i >= 0 && i <= 3))
                            {
                                switch (index)
                                {
                                    case 0:
                                        LevelEditorCameraPos.Y -= LevelEditorCamSpeed.Y;
                                        break;
                                    case 1:
                                        LevelEditorCameraPos.Y += LevelEditorCamSpeed.Y;
                                        break;
                                    case 2:
                                        LevelEditorCameraPos.X -= LevelEditorCamSpeed.X;
                                        break;
                                    case 3:
                                        LevelEditorCameraPos.X += LevelEditorCamSpeed.X;
                                        break;
                                }
                            }

                            Camera.FollowExact(LevelEditorCameraPos);

                            if ((inputs.Where(i => i > 3).ToList().Count() > 0) && !KeyIsPressed)
                            {
                                int? nonMovementInput = inputs.First(i => i > 3);

                                switch (nonMovementInput)
                                {
                                    case 8:
                                        LevelEditorCameraPos = new(Width / 2, Height / 2);
                                        LevelEditorManager.Update(LevelEditorCameraPos, null);
                                        break;
                                    default:
                                        LevelEditorManager.Update(LevelEditorCameraPos, nonMovementInput);
                                        break;
                                }

                                KeyIsPressed = true;
                            }
                        }

                        if ((pressedKeys.Any(k => NumberKeys.Contains(k)) || AllowedOtherInputs.Any(k1 => k1.Any(k2 => pressedKeys.Contains(k2)))) && !KeyIsPressed)
                        {
                            LevelEditorManager.Update(LevelEditorCameraPos,null);
                            KeyIsPressed = true;
                        }
                    }
                    break;
                case GameState.GameEnd:
                    RetryCheck();
                    break;
            }

            UIManager.Update(PlayerChar, LevelEditorCameraPos);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackGroundColor);

            //if (CurrentState == GameState.InGame)
            //    SpriteBatch.Begin(SpriteSortMode.FrontToBack, transformMatrix: Camera.Transform/*, null, SamplerState.PointWrap*/);
            //else
            //    SpriteBatch.Begin(SpriteSortMode.FrontToBack/*, null, SamplerState.PointWrap*/);

            SpriteBatch.Begin(SpriteSortMode.FrontToBack, transformMatrix: Camera.Transform);

            if (IsCurrentState(GameState.InGame) || IsCurrentState(GameState.InLevelEditor))
            {
                PlayerChar.Draw(SpriteBatch);
                LevelManager.Draw();
                if (!EnemyManager.IsEmpty())
                    EnemyManager.Draw(SpriteBatch);
                if (!ItemManager.IsEmpty())
                    ItemManager.Draw(SpriteBatch);
            }

            SpriteBatch.End();


            SpriteBatch.Begin(SpriteSortMode.FrontToBack);

            UIManager.Draw(SpriteBatch, CurrentState);

            SpriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        void StartGame(int type)
        {
            switch (type)
            {
                case 0:
                    LevelManager.StartFrombeginning(out PlayerChar);
                    break;
                case 1:
                    LevelManager.LoadRandomLevel(out PlayerChar);
                    break;
                case 2:
                    LevelManager.LoadCurrentLevel(out PlayerChar);
                    break;
            }
            LastPlayerHp = PlayerChar.GetCurrentHealth();
            CurrentState = GameState.InGame;
        }

        void UpdateEntities(float deltaTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            Keys[] input = keyboard.GetPressedKeys();
            List<int> inputs = new();

            if (CurrentState == GameState.InGame && input.Length > 0)
                foreach (Keys[] inputScheme in ValidInputKeys)
                    inputs.AddRange(Array.FindAll(inputScheme, k => input.Contains(k)).Select(k => Array.IndexOf(inputScheme, k)).Where(x => x != -1));

            PlayerChar.Update(deltaTime, inputs.Distinct().ToArray());

            EnemyManager.Update(deltaTime, PlayerChar);
        }

        void RestartLevel()
        {
            LevelManager.RestartLevel(out PlayerChar);
            PlayerChar.SetHealth(LastPlayerHp);
        }

        void LoadNextLevel()
        {
            LastPlayerHp = PlayerChar.GetCurrentHealth();
            LevelManager.LoadNextLevel(out PlayerChar);
            PlayerChar.SetHealth(LastPlayerHp);
        }

        void RetryCheck()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Y))
                StartGame(0);
            else if (Keyboard.GetState().IsKeyDown(Keys.N))
                CurrentState = GameState.InMenu;
        }

        public bool IsCurrentState(GameState state)
        {
            return CurrentState == state;
        }
    }
}
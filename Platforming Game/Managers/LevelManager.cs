using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Platforming_Game.Enums;
using Platforming_Game.GameObjects;
using Platforming_Game.Other;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Platforming_Game.Managers
{
    public static class LevelManager
    {
        // Color
        static Color PlatformColor = Color.Green;
        static Color BackGroundColor = Color.CornflowerBlue;

        // Int
        static int ScreenHeight;
        static int ScreenWidth;
        static int CurrentLevelNum = 4;
        static int LevelAmount = 6;

        // Float
        static float BackGroundLayer = 0.0f;
        static float EntityLayer = 0.6f;
        static float PlatformLayer = 0.5f;
        static float PoolLayer = 0.7f;
        static float ItemLayer = 0.8f;
        static float CheckLevelFileDelay = 1.0f;

        // Other : Lists
        static List<Platform> Platforms = new();
        static List<JumpPad> JumpPads = new();
        static List<LavaPool> LavaPools = new();
        static Dictionary<string, Action> TriggerColliderFuncs;

        // Other : Level
        static Goal LevelGoal;
        static Level? CurrentLevel;
        static KillBarrier OutOfBoundsKillBarrier;
        static Timer CheckLevelFile;
        static bool DoEndGame;

        // Other : Misc
        static string LevelsFolderPath = @"Levels\";
        static Vector2 PlayerDimensions = new(10,20);
        static Player PlayerChar;
        static SpriteBatch SpriteBatch;

        public static void InitalizeFields(int[] windowSpecs, SpriteBatch spriteBatch, Dictionary<string, Action> triggerColliderFuncs)
        {
            ScreenHeight = windowSpecs[0];
            ScreenWidth = windowSpecs[1];

            SpriteBatch = spriteBatch;

            TriggerColliderFuncs = triggerColliderFuncs;

            OutOfBoundsKillBarrier = new(new(-ScreenWidth * 2, ScreenHeight + 60, ScreenWidth * 100, 100), TriggerColliderFuncs["Kill"]);
            CheckLevelFile = new Timer();
        }

        public static void Update(float? deltaTime, Player player, out GameState? newState)
        {
            if (deltaTime.HasValue)
                CheckLevelFile.Update(deltaTime.Value);

            if (CheckLevelFile.IsDone())
            {
                if (HasLevelFileBeenEdited())
                    UpdateLevel();

                CheckLevelFile.StartTimer(CheckLevelFileDelay);
            }

            foreach (Platform platform in Platforms)
                platform.Update(player);

            foreach (JumpPad jumpPad in JumpPads)
                jumpPad.Update(deltaTime, player);

            if (LevelGoal == null)
                throw new Exception("There Is No goal for the level. Add Goal To level");
            else
            {
                LevelGoal.UpdatePlayer(player);
                LevelGoal.Update(null);
            }

            // Goes throught and updates all lava pools. try catch for exceptions when level restarts so game doesn't stop
            try
            {
                foreach (LavaPool pool in LavaPools)
                {
                    pool.UpdatePlayer(player);
                    pool.Update(null);
                }
            }
            catch
            {

            }

            OutOfBoundsKillBarrier.UpdatePlayer(player);
            OutOfBoundsKillBarrier.Update(null);

            if (DoEndGame)
                newState = GameState.GameEnd;
            else
                newState = null;
        }

        public static void Draw()
        {
            foreach (Platform platform in Platforms)
                platform.Draw(SpriteBatch);

            foreach (JumpPad pad in JumpPads)
                pad.Draw(SpriteBatch);

            foreach (LavaPool pool in LavaPools)
                pool.Draw(SpriteBatch);

            if (LevelGoal == null)
                throw new Exception("There Is No goal for the level. Add Goal To level");
            else
                LevelGoal.Draw(SpriteBatch);
        }

        public static void StartFrombeginning(out Player playerOut)
        {
            CurrentLevelNum = 1;

            DoEndGame = false;

            LoadLevelData(CurrentLevelNum, out playerOut);
        }

        public static void LoadCurrentLevel(out Player playerOut)
        {
            LoadLevelData(CurrentLevelNum, out playerOut);
        }

        static void LoadLevelData(int levelIndex, out Player playerOut)
        {
            if (CurrentLevel != null)
            {
                CurrentLevel = null;
                ClearLevelData();
            }

            try
            {
                CurrentLevel = JsonConvert.DeserializeObject<Level>(File.ReadAllText(GetLevelFilePath(levelIndex)));
            }
            catch
            {
                EndGame();
                //throw new Exception($"No level by name "+ @"Levels\Level" + $"{levelIndex}.json exists");
            }

            if (CurrentLevel != null)
            {
                LoadLevel();
            }

            playerOut = PlayerChar;
            CheckLevelFile.StartTimer(CheckLevelFileDelay);
        }

        static void LoadLevel()
        {
            foreach (Rectangle rec in CurrentLevel.Platforms)
                Platforms.Add(CreatePlatform(rec, PlatformColor, PlatformLayer));

            PlayerChar = new(SpriteBatch, new Rectangle((int)CurrentLevel.PlayerSpawn.X, (int)CurrentLevel.PlayerSpawn.Y, (int)PlayerDimensions.X, (int)PlayerDimensions.Y), EntityLayer);

            PlayerChar.SetDeathFunction(TriggerColliderFuncs["Kill"]);

            if (CurrentLevel.Goombas != null)
                foreach (Rectangle rec in CurrentLevel.Goombas)
                    EnemyManager.AddGoomba(new(SpriteBatch, rec, EntityLayer));

            if (CurrentLevel.JumpPads != null)
                foreach (Rectangle rec in CurrentLevel.JumpPads)
                    JumpPads.Add(new(SpriteBatch, rec, PlatformLayer));

            if (CurrentLevel.LavaPools != null)
                foreach (Rectangle rec in CurrentLevel.LavaPools)
                    LavaPools.Add(new(SpriteBatch, rec, PoolLayer, TriggerColliderFuncs["Kill"]));

            if (CurrentLevel.OneUps != null)
                foreach (Rectangle rec in CurrentLevel.OneUps)
                    ItemManager.AddItem(new OneUp(SpriteBatch, rec, ItemLayer), 0);

            LevelGoal = new(SpriteBatch, CurrentLevel.Goal, TriggerColliderFuncs["Goal"]);
        }

        static void UpdateLevel()
        {
            Level level = JsonConvert.DeserializeObject<Level>(File.ReadAllText(LevelsFolderPath + $"level{CurrentLevelNum}.json"));

            if (level.Platforms.Any(p => !CurrentLevel.Platforms.Contains(p)) || level.Platforms.Length != CurrentLevel.Platforms.Length)
            {
                Platforms.Clear();
                foreach (Rectangle rec in level.Platforms)
                    Platforms.Add(CreatePlatform(rec, PlatformColor, PlatformLayer));
            }

            if (!CurrentLevel.Goal.Equals(level.Goal) && !level.Goal.IsEmpty)
                LevelGoal = new Goal(SpriteBatch, level.Goal, TriggerColliderFuncs["Goal"]);

            if (level.JumpPads.Any(j => !CurrentLevel.JumpPads.Contains(j)) || level.JumpPads.Length != CurrentLevel.JumpPads.Length)
            {
                JumpPads.Clear();
                if (level.JumpPads != null)
                    foreach (Rectangle rec in level.JumpPads)
                        JumpPads.Add(new(SpriteBatch, rec, PlatformLayer));
            }

            if (level.LavaPools.Any(l => !CurrentLevel.LavaPools.Contains(l)) || level.LavaPools.Length != CurrentLevel.LavaPools.Length)
            {
                LavaPools.Clear();
                if (level.LavaPools != null)
                    foreach (Rectangle rec in level.LavaPools)
                        LavaPools.Add(new(SpriteBatch, rec, PoolLayer, TriggerColliderFuncs["Kill"]));
            }

            if (level.OneUps.Any(o => !CurrentLevel.OneUps.Contains(o)) || level.OneUps.Length != CurrentLevel.OneUps.Length)
            {
                ItemManager.ClearItems(0);
                if (CurrentLevel.OneUps != null)
                    foreach (Rectangle rec in CurrentLevel.OneUps)
                        ItemManager.AddItem(new OneUp(SpriteBatch, rec, ItemLayer), 0);
            }
        }

        public static void LoadNextLevel(out Player playerOut)
        {
            CurrentLevelNum++;

            LoadLevelData(CurrentLevelNum, out playerOut);
        }

        public static void LoadRandomLevel(out Player playerOut)
        {
            CurrentLevelNum = new Random().Next(LevelAmount + 1);
            LoadLevelData(CurrentLevelNum, out playerOut);
        }

        public static void RestartLevel(out Player playerOut)
        {
            ClearLevelData();
            LoadLevel();
            playerOut = PlayerChar;
        }

        static void ClearLevelData()
        {
            Platforms.Clear();
            JumpPads.Clear();
            LavaPools.Clear();
            EnemyManager.ClearEnemies();
            ItemManager.ClearItems();
        }

        static void EndGame()
        {
            DoEndGame = true;
        }

        static void CreateBorderWalls(bool makeFloor)
        {
            int borderThickness = 10;
            int floorThickness = 10;

            Platforms.Add(CreatePlatform(-borderThickness, 0, ScreenHeight + floorThickness, borderThickness, null, BackGroundLayer));
            Platforms.Add(CreatePlatform(ScreenWidth, 0, ScreenHeight + floorThickness, borderThickness, null, BackGroundLayer));

            if (makeFloor)
                Platforms.Add(CreatePlatform(-floorThickness, ScreenHeight - floorThickness, floorThickness, ScreenWidth + 2 * floorThickness, PlatformColor, PlatformLayer));
        }

        static void CreateBox(Vector2 pos, int height, int width, int thickness)
        {
            Platforms.Add(CreatePlatform((int)pos.X, (int)pos.Y, thickness, width - thickness, PlatformColor, PlatformLayer));
            Platforms.Add(CreatePlatform((int)pos.X, (int)pos.Y + thickness, height - thickness, thickness, PlatformColor, PlatformLayer));
            Platforms.Add(CreatePlatform((int)pos.X + thickness, (int)pos.Y + height - thickness, thickness, width - thickness, PlatformColor, PlatformLayer));
            Platforms.Add(CreatePlatform((int)pos.X + width - thickness, (int)pos.Y, height - thickness, thickness, PlatformColor, PlatformLayer));
        }

        static Platform CreatePlatform(int x, int y, int height, int width, Color? color, float drawLayer)
        {
            return new Platform(SpriteBatch, new(x, y, width, height), color, drawLayer);
        }

        static Platform CreatePlatform(Rectangle destRec, Color? color, float drawLayer)
        {
            return new Platform(SpriteBatch, destRec, color, drawLayer);
        }

        static bool HasLevelFileBeenEdited()
        {
            Level level = JsonConvert.DeserializeObject<Level>(File.ReadAllText(LevelsFolderPath + $"Level{CurrentLevelNum}.json"));

            bool test1 = level.Platforms.Any(p => !CurrentLevel.Platforms.Contains(p)) || level.Platforms.Length != CurrentLevel.Platforms.Length;
            bool test2 = level.JumpPads.Any(j => !CurrentLevel.JumpPads.Contains(j)) || level.JumpPads.Length != CurrentLevel.JumpPads.Length;
            bool test3 = level.LavaPools.Any(l => !CurrentLevel.LavaPools.Contains(l)) || level.LavaPools.Length != CurrentLevel.LavaPools.Length;
            bool test4 = !CurrentLevel.Goal.Equals(level.Goal);
            bool test5 = level.OneUps.Any(o => !CurrentLevel.OneUps.Contains(o)) || level.OneUps.Length != CurrentLevel.OneUps.Length;

            return test1 || test2 || test3 || test4 || test5;
        }

        static public void EditLevel(Rectangle newValue, int whatToEdit, int? index = null)
        {
            switch (whatToEdit)
            {
                case 0:
                    if (index.HasValue && index.Value >= 0 && index.Value < CurrentLevel.Platforms.Length)
                        CurrentLevel.Platforms[index.Value] = newValue;
                    else if (!CurrentLevel.Platforms.Any(p => p.X == newValue.X && p.Y == newValue.Y))
                        CurrentLevel.Platforms = AddToArray(CurrentLevel.Platforms, newValue);
                    break;
                case 1:
                    CurrentLevel.Goal = newValue;
                    break;
                case 2:
                    if (index.HasValue && index.Value >= 0 && index.Value < CurrentLevel.Goombas.Length)
                        CurrentLevel.Goombas[index.Value] = newValue;
                    else if (!CurrentLevel.Goombas.Any(p => p.X == newValue.X && p.Y == newValue.Y))
                        CurrentLevel.Goombas = AddToArray(CurrentLevel.Goombas, newValue);
                    break;
                case 3:
                    if (index.HasValue && index.Value >= 0 && index.Value < CurrentLevel.JumpPads.Length)
                        CurrentLevel.JumpPads[index.Value] = newValue;
                    else if (!CurrentLevel.JumpPads.Any(p => p.X == newValue.X && p.Y == newValue.Y))
                        CurrentLevel.JumpPads = AddToArray(CurrentLevel.JumpPads, newValue);
                    break;
                case 4:
                    if (index.HasValue && index.Value >= 0 && index.Value < CurrentLevel.LavaPools.Length)
                        CurrentLevel.LavaPools[index.Value] = newValue;
                    else if (!CurrentLevel.LavaPools.Any(p => p.X == newValue.X && p.Y == newValue.Y))
                        CurrentLevel.LavaPools = AddToArray(CurrentLevel.LavaPools, newValue);
                    break;
                case 5:
                    if (index.HasValue && index.Value >= 0 && index.Value < CurrentLevel.LavaPools.Length)
                        CurrentLevel.OneUps[index.Value] = newValue;
                    else if (!CurrentLevel.LavaPools.Any(o => o.X == newValue.X && o.Y == newValue.Y))
                        CurrentLevel.OneUps = AddToArray(CurrentLevel.OneUps, newValue);
                    break;
            }

            ClearLevelData();
            LoadLevel();
        }

        static public void EditLevel(Rectangle[] newValue, int whatToEdit)
        {
            switch (whatToEdit)
            {
                case 0:
                    CurrentLevel.Platforms = newValue;
                    break;
                case 1:
                    CurrentLevel.Goombas = newValue;
                    break;
                case 2:
                    CurrentLevel.JumpPads = newValue;
                    break;
                case 3:
                    CurrentLevel.LavaPools = newValue;
                    break;
                case 4:
                    CurrentLevel.OneUps = newValue;
                    break;
            }

            ClearLevelData();
            LoadLevel();
        }

        static public void EditLevel(Vector2 newPlayerSpawn)
        {
            CurrentLevel.PlayerSpawn = newPlayerSpawn;
            LoadLevel();
        }

        static public void SaveLevel()
        {
            File.WriteAllText(GetLevelFilePath(), JsonConvert.SerializeObject(CurrentLevel));
            UpdateLevel();
        }

        /// <summary>
        /// Gets filepath for the current level
        /// </summary>
        /// <returns>string corresponding to the filepath</returns>
        static string GetLevelFilePath()
        {
            return LevelsFolderPath + $"Level{CurrentLevelNum}.json";
        }

        /// <summary>
        /// Gets filepath for a specified level
        /// </summary>
        /// <returns>string corresponding to the filepath</returns>
        static string GetLevelFilePath(int levelIndex)
        {
            return LevelsFolderPath + $"Level{levelIndex}.json";
        }

        static Rectangle[] AddToArray(Rectangle[] array, Rectangle valueToAdd)
        {
            List<Rectangle> newArray = array.ToList();
            newArray.Add(valueToAdd);
            return newArray.ToArray();
        }

        static public List<Rectangle> GetDestRecs(int type)
        {
            switch (type)
            {
                case 0:
                    return CurrentLevel.Platforms.ToList();
                    break;
                case 1:
                    return CurrentLevel.Goombas.ToList();
                    break;
                case 2:
                    return CurrentLevel.JumpPads.ToList();
                    break;
                case 3:
                    return CurrentLevel.LavaPools.ToList();
                    break;
                case 4:
                    return CurrentLevel.OneUps.ToList();
                    break;
                default:
                    return new List<Rectangle>();
                    break;
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Platforming_Game.Enums;
using Platforming_Game.GameObjects;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Serialization;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Platforming_Game.Managers
{
    static public class LevelEditorManager
    {
        // Int
        static int ScreenHeight;
        static int ScreenWidth;
        static int AddChangeStage;

        // LevelManagerState
        static LevelManagerState CurrentState;
        static LevelManagerState[] VectorOutputStates = {
            LevelManagerState.ChangePlayerPos
        };
        static LevelManagerState[] RectangleOutputStates = {
            LevelManagerState.AddPlatform,
            LevelManagerState.ChangeGoal,
            LevelManagerState.AddGoomba,
            LevelManagerState.AddJumpPad,
            LevelManagerState.AddLavaPool,
            LevelManagerState.AddOneUp
        };

        // Other
        static Keys[] NumberKeys;
        static List<Keys[]> AllowedInputs;
        /// <summary>
        /// Current Stage of adding or changing level elements
        /// </summary>
        static Vector2 VectorCache = new();
        static Rectangle RectangleCache = new();
        static string ValueCache;

        static public void Update(Vector2 cameraPos, int? input)
        {
            if (IsCurrentState(LevelManagerState.Normal) && input.HasValue)
            {
                switch (input.Value)
                {
                    // Save
                    case 6:
                        LevelManager.SaveLevel();
                        break;
                    // Delete Element
                    case 7:
                        if (LevelManager.GetDestRecs(4).Any(o => o.Contains(cameraPos)))
                        {
                            List<Rectangle> oneUps = LevelManager.GetDestRecs(4).FindAll(o => !o.Contains(cameraPos));
                            LevelManager.EditLevel(oneUps.ToArray(), 4);
                        }

                        else if (LevelManager.GetDestRecs(3).Any(l => l.Contains(cameraPos)))
                        {
                            List<Rectangle> lavapools = LevelManager.GetDestRecs(3).FindAll(l => !l.Contains(cameraPos));
                            LevelManager.EditLevel(lavapools.ToArray(), 3);
                        }

                        else if (LevelManager.GetDestRecs(2).Any(j => j.Contains(cameraPos)))
                        {
                            List<Rectangle> jumpPads = LevelManager.GetDestRecs(2).FindAll(j => !j.Contains(cameraPos));
                            LevelManager.EditLevel(jumpPads.ToArray(), 2);
                        }

                        else if (LevelManager.GetDestRecs(1).Any(g => g.Contains(cameraPos)))
                        {
                            List<Rectangle> goombas = LevelManager.GetDestRecs(1).FindAll(g => !g.Contains(cameraPos));
                            LevelManager.EditLevel(goombas.ToArray(), 1);
                        }

                        else if (LevelManager.GetDestRecs(0).Any(p => p.Contains(cameraPos)))
                        {
                            List<Rectangle> platforms = LevelManager.GetDestRecs(0).FindAll(p => !p.Contains(cameraPos));
                            LevelManager.EditLevel(platforms.ToArray(), 0);
                        }

                        break;
                    // Add Platform
                    case 9:
                        SetState(LevelManagerState.AddPlatform);
                        AddChangeStage = 1;
                        break;
                    // Change Start PlayerPos
                    case 10:
                        SetState(LevelManagerState.ChangePlayerPos);
                        AddChangeStage = 1;
                        break;
                    // Change Goal
                    case 11:
                        SetState(LevelManagerState.ChangeGoal);
                        AddChangeStage = 1;
                        break;
                    // Add Goomba
                    case 12:
                        SetState(LevelManagerState.AddGoomba);
                        AddChangeStage = 1;
                        break;
                    // Add JumpPad
                    case 13:
                        SetState(LevelManagerState.AddJumpPad);
                        AddChangeStage = 1;
                        break;
                    // Add Lavapool
                    case 14:
                        SetState(LevelManagerState.AddLavaPool);
                        AddChangeStage = 1;
                        break;
                    // Add OneUp
                    case 15:
                        SetState(LevelManagerState.AddOneUp);
                        AddChangeStage = 1;
                        break;
                }
            }
            else
            {
                Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
                bool isInputNumber = pressedKeys.Any(k => NumberKeys.Contains(k));

                if (VectorOutputStates.Contains(CurrentState))
                {
                    if (isInputNumber)
                        ValueCache += $"{Array.IndexOf(NumberKeys, pressedKeys.First(k => NumberKeys.Contains(k)))}";
                    else if (pressedKeys.Any(k1 => AllowedInputs.Any(k2 => k2.Contains(k1))))
                    {
                        switch (Array.IndexOf(AllowedInputs.ToArray(), AllowedInputs.Find(k1 => k1.Any(k2 => pressedKeys.Contains(k2)))))
                        {
                            case 0:
                                if (ValueCache.Contains("-"))
                                    ValueCache.Replace("-", "");
                                else
                                    ValueCache = $"-{ValueCache}";
                                break;
                        }
                    }

                    else if (input.HasValue)
                        switch (input.Value)
                        {
                            case 5:
                                if (ValueCache == "")
                                {
                                    if (AddChangeStage == 1)
                                        Reset();
                                    else
                                        AddChangeStage--;
                                }
                                else
                                    ValueCache = "";
                                break;
                            case 4:
                                switch (AddChangeStage)
                                {
                                    case 1:
                                        VectorCache.X = StringToInt(ValueCache);
                                        Proceed();
                                        break;
                                    case 2:
                                        VectorCache.Y = StringToInt(ValueCache);
                                        AddChangesToLevel();
                                        Reset();
                                        break;
                                }
                                break;
                        }
                }

                else if (RectangleOutputStates.Contains(CurrentState))
                {
                    if (isInputNumber)
                        ValueCache += $"{Array.IndexOf(NumberKeys, pressedKeys.First(k => NumberKeys.Contains(k)))}";
                    else if (pressedKeys.Any(k1 => AllowedInputs.Any(k2 => k2.Contains(k1))))
                    {
                        switch (Array.IndexOf(AllowedInputs.ToArray(), AllowedInputs.Find(k1 => k1.Any(k2 => pressedKeys.Contains(k2)))))
                        {
                            case 0:
                                if (ValueCache.Contains('-'))
                                    ValueCache.Replace("-", "");
                                else
                                    ValueCache = $"-{ValueCache}";
                                break;
                        }
                    }

                    else if (input.HasValue)
                        switch (input.Value)
                        {
                            case 5:
                                if (ValueCache == "")
                                {
                                    if (AddChangeStage == 1)
                                        Reset();
                                    else
                                        AddChangeStage--;
                                }
                                else
                                    ValueCache = "";
                                break;
                            case 4:
                                switch (AddChangeStage)
                                {
                                    case 1:
                                        RectangleCache.X = StringToInt(ValueCache);
                                        Proceed();
                                        break;
                                    case 2:
                                        RectangleCache.Y = StringToInt(ValueCache);
                                        Proceed();
                                        break;
                                    case 3:
                                        RectangleCache.Width = StringToInt(ValueCache);
                                        Proceed();
                                        break;
                                    case 4:
                                        RectangleCache.Height = StringToInt(ValueCache);
                                        AddChangesToLevel();
                                        Reset();
                                        break;
                                }
                                break;
                        }
                }
            }
        }

        static public void InstantializeFields(int[] screenDimensions, Keys[] numberKeys, List<Keys[]> allowedInputs)
        {
            ScreenHeight = screenDimensions[0];
            ScreenWidth = screenDimensions[1];
            NumberKeys = numberKeys;
            AllowedInputs = allowedInputs;
            CurrentState = LevelManagerState.Normal;
            ValueCache = "";
        }

        static public void StartUp()
        {
            CurrentState = LevelManagerState.Normal;
        }

        static public LevelManagerState GetState()
        {
            return CurrentState;
        }

        static bool IsCurrentState(LevelManagerState state)
        {
            return CurrentState == state;
        }

        static void SetState(LevelManagerState state)
        {
            CurrentState = state;
        }

        static void AddChangesToLevel()
        {
            switch (CurrentState)
            {
                case LevelManagerState.AddPlatform:
                    LevelManager.EditLevel(RectangleCache, 0);
                    break;
                case LevelManagerState.ChangePlayerPos:
                    LevelManager.EditLevel(VectorCache);
                    break;
                case LevelManagerState.ChangeGoal:
                    LevelManager.EditLevel(RectangleCache, 1);
                    break;
                case LevelManagerState.AddGoomba:
                    LevelManager.EditLevel(RectangleCache, 2);
                    break;
                case LevelManagerState.AddJumpPad:
                    LevelManager.EditLevel(RectangleCache, 3);
                    break;
                case LevelManagerState.AddLavaPool:
                    LevelManager.EditLevel(RectangleCache, 4);
                    break;
                case LevelManagerState.AddOneUp:
                    LevelManager.EditLevel(RectangleCache, 5);
                    break;
            }
        }

        static int StringToInt(string valueToParse)
        {
            return int.Parse(valueToParse);
        }

        static public int GetAddChangeState()
        {
            return AddChangeStage;
        }

        static public string GetValueCache()
        {
            return ValueCache;
        }

        static void Proceed()
        {
            ValueCache = "";
            AddChangeStage++;
        }

        static void Reset()
        {
            ValueCache = "";
            AddChangeStage = 0;
            SetState(LevelManagerState.Normal);
        }
    }
}

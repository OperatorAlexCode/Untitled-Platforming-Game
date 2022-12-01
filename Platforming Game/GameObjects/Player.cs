using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Enums;
using Platforming_Game.Other;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = Platforming_Game.Other.Timer;

namespace Platforming_Game.GameObjects
{
    public class Player
    {
        // Int
        int[] LastInputs;
        int MaxDashAmount = 2;
        int DashesUsed = 0;
        int HealthPoints = 3;
        int GroundCheckRecMargin = 2;
        int GroundCheckRecHeight = 2;

        // Float : Timea & Durations
        float JumpTime = 0.5f;
        float CyoteTime = 0.2f;
        float DashDuration = 0.175f;
        float DashDelayDuration = 0.3f;
        float GraceDelay = 1.0f;
        float GroundPoundDelay = 0.1f;

        // Float : Forces
        float Gravity = 400.0f;
        float WalkForce = 400.0f;
        float InitialJumpforce = 7000.0f;
        float ConstantJumpForce = 400.0f;
        float GroundPoundForce = 8000.0f;

        // Float : Misc
        float DrawLayer;
        float DragCoeficient = 2.1f;
        float Mass = 0.8f;

        // bool
        bool OnGround;
        bool IsJumping;
        bool GroundPound;

        // Rectangle
        public Rectangle DestRec;
        public Rectangle GroundRec;

        // Vector2 : Velocities
        Vector2 Vel;
        Vector2 MaxVel = new(450, 500);
        Vector2 MaxDashVel = new(400, 500);
        Vector2 DashMinVel = new(10, 0);
        Vector2 CyoteTimeVel = new(10, -2);

        // Vector2 : Misc
        Vector2 ObjectForce;
        Vector2 Pos;

        // Timer
        Timer JumpTimer;
        Timer CyoteTimer;
        Timer DashTimer;
        Timer GraceTimer;
        Timer GroundPoundTimer;
        Timer DashDelay;

        // Other
        Texture2D Tex;
        Color CurrentColor = Color.Pink;
        Action DeathFunction;
        PlayerState CurrentState;

        public Player(Texture2D tex, Rectangle destRec, float drawLayer)
        {
            Tex = tex;
            DestRec = destRec;
            DrawLayer = drawLayer;
            SetConstants();
        }

        public Player(SpriteBatch spriteBatch, Rectangle destRec, float drawLayer)
        {
            Tex = new(spriteBatch.GraphicsDevice, 1, 1);
            Tex.SetData(new[] { Color.White });
            DestRec = destRec;
            DrawLayer = drawLayer;
            SetConstants();
        }

        /// <summary>
        /// Sets variables that aren't different between constructors
        /// </summary>
        void SetConstants()
        {
            OnGround = false;
            IsJumping = false;
            JumpTimer = new();
            CyoteTimer = new();
            DashTimer = new();
            DashDelay = new();
            GraceTimer = new();
            GroundPoundTimer = new();
            GroundRec = CalculateGroundRec();
            Pos = new(DestRec.X, DestRec.Y);
            CurrentState = PlayerState.Normal;
            LastInputs = new int[0];
        }

        public void Update(float deltaTime, int[] moveIndexes)
        {
            UpdateTimers(deltaTime);

            if (DashTimer.IsDone() && IsCurrentState(PlayerState.Dashing))
                CurrentState = PlayerState.Normal;

            if (GroundPoundTimer.IsDone() && GroundPound)
            {
                AddForce(new Vector2(0, GroundPoundForce));
                GroundPoundTimer.StartTimer(GroundPoundDelay);
            }

            if (OnGround && GroundPound)
                GroundPound = false;

            else if (!OnGround && !GroundPound && IsCurrentState(PlayerState.Normal))
                AddForce(new(0, Gravity));

            if (moveIndexes.Length > 0)
                foreach (int index in moveIndexes)
                    switch (index)
                    {
                        case 0:
                            if (!IsJumping && (OnGround || !CyoteTimer.IsDone()) && !LastInputs.Contains(0) && IsCurrentState(PlayerState.Normal))
                            {
                                if (OnGround)
                                    AddForce(new Vector2(0, -InitialJumpforce));
                                else if (!CyoteTimer.IsDone())
                                {
                                    SetVel(Vel.X, 0);
                                    AddForce(new Vector2(0, -InitialJumpforce));
                                }
                                IsJumping = true;
                                JumpTimer.StartTimer(JumpTime);
                            }
                            else if (!JumpTimer.IsDone())
                                AddForce(new Vector2(0, -ConstantJumpForce));
                            else
                                IsJumping = false;
                            break;
                        case 1:
                            if (!OnGround && !GroundPound && !LastInputs.Contains(1))
                            {
                                GroundPound = true;
                                if (Vel.Y < 0)
                                    SetVel(Vel.X, 0);
                                AddForce(new Vector2(0, GroundPoundForce));
                                GroundPoundTimer.StartTimer(GroundPoundDelay);
                                goto StopJump;
                            }
                            break;
                        case 2:
                            AddForce(new Vector2(-WalkForce, 0));
                            break;
                        case 3:
                            AddForce(new Vector2(WalkForce, 0));
                            break;
                        case 4:
                            if (DashesUsed < MaxDashAmount && Vel.X != 0 && (Vel.X >= DashMinVel.X || Vel.X <= -DashMinVel.X) && DashDelay.IsDone())
                            {
                                CurrentState = PlayerState.Dashing;
                                SetVel(MaxDashVel.X * Vel.X, 0);
                                DashesUsed++;
                                DashTimer.StartTimer(DashDuration);
                                DashDelay.StartTimer(DashDelayDuration);
                            }
                            break;
                    }

                StopJump:
            if (IsJumping && !moveIndexes.Contains(0))
            {
                JumpTimer.StartTimer(0);
                IsJumping = false;
            }

            AddForce(new(-Vel.X * DragCoeficient, 0));

            Vel += ObjectForce / Mass * deltaTime;
            RoundVel();
            Pos += Vel * deltaTime;
            ObjectForce = Vector2.Zero;

            SetRecsPoses();

            LastInputs = moveIndexes;
        }

        void UpdateTimers(float deltaTime)
        {
            JumpTimer.Update(deltaTime);
            CyoteTimer.Update(deltaTime);
            DashTimer.Update(deltaTime);
            DashDelay.Update(deltaTime);
            GraceTimer.Update(deltaTime);
            GroundPoundTimer.Update(deltaTime); 
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, DestRec, null, CurrentColor, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }

        public void SetOnGround(bool isOnground)
        {
            if (OnGround && !isOnground && (Vel.X >= CyoteTimeVel.X || Vel.X <= -CyoteTimeVel.X) && Vel.Y >= CyoteTimeVel.Y)
                CyoteTimer.StartTimer(CyoteTime);

            OnGround = isOnground;
        }

        public bool GetOnGround()
        {
            return OnGround;
        }

        public void GetHurt()
        {
            if (GraceTimer.IsDone())
            {
                GraceTimer.StartTimer(GraceDelay);
                if (HealthPoints-1 <= 0)
                    DeathFunction.Invoke();
                else
                    HealthPoints--;
            }
        }

        public void GetHurt(int damageDealt)
        {
            if (GraceTimer.IsDone())
            {
                GraceTimer.StartTimer(GraceDelay);
                if (HealthPoints - damageDealt <= 0)
                    DeathFunction.Invoke();
                else
                    HealthPoints -= damageDealt;
            }
        }

        /// <summary>
        /// Sets the function that is to execute when
        /// </summary>
        /// <param name="deathFunc"></param>
        public void SetDeathFunction(Action deathFunc)
        {
            DeathFunction = deathFunc;
        }

        /// <summary>
        /// Adds force to player
        /// </summary>
        /// <param name="x">x value of vector</param>
        /// <param name="y">y value of vector</param>
        public void AddForce(float x, float y)
        {
            ObjectForce += new Vector2(x, y);
        }

        /// <summary>
        /// Adds force to player
        /// </summary>
        /// <param name="f">vector force</param>
        public void AddForce(Vector2 f)
        {
            ObjectForce += f;
        }

        /// <summary>
        /// Set players velocity to a new value
        /// </summary>
        /// <param name="x">x value of new velocity</param>
        /// <param name="y">y value of new velocity</param>
        public void SetVel(float x, float y)
        {
            Vel = new Vector2(x, y);
            RoundVel();
        }

        /// <summary>
        /// Set players velocity  to a new value
        /// </summary>
        /// <param name="newVel">Vector of new velocity</param>
        public void SetVel(Vector2 newVel)
        {
            Vel = newVel;
            RoundVel();
        }

        /// <returns>Vector of Current velocity</returns>
        public Vector2 GetVel()
        {
            return Vel;
        }

        void RoundVel()
        {
            switch (CurrentState)
            {
                case PlayerState.Dashing:
                    if (Vel.X >= MaxDashVel.X)
                        Vel.X = MaxDashVel.X;
                    else if (Vel.X <= -MaxDashVel.X)
                        Vel.X = -MaxDashVel.X;

                    if (Vel.Y >= MaxDashVel.Y * 2)
                        Vel.Y = MaxVel.Y;
                    else if (Vel.Y <= -MaxDashVel.Y)
                        Vel.Y = -MaxDashVel.Y;
                    break;
                default:
                    if (Vel.X >= MaxVel.X)
                        Vel.X = MaxVel.X;
                    else if (Vel.X <= -MaxVel.X)
                        Vel.X = -MaxVel.X;

                    if (Vel.Y >= MaxVel.Y * 2)
                        Vel.Y = MaxVel.Y * 2;
                    else if (Vel.Y <= -MaxVel.Y)
                        Vel.Y = -MaxVel.Y;
                    break;
            }
        }

        /// <summary>
        /// Sets the Destination rectangle and GrounnCheck rectangle
        /// </summary>
        void SetRecsPoses()
        {
            DestRec.X = (int)Pos.X;
            DestRec.Y = (int)Pos.Y;

            GroundRec = CalculateGroundRec();
        }

        /// <summary>
        /// Set players position to a new value
        /// </summary>
        /// <param name="x">x value of new position</param>
        /// <param name="y">y value of new position</param>
        public void SetPos(float x, float y)
        {
            Pos = new Vector2(x, y);

            SetRecsPoses();
        }

        /// <summary>
        /// Set players position to a new value
        /// </summary>
        /// <param name="newPos">Vector of new position</param>
        public void SetPos(Vector2 newPos)
        {
            Pos = newPos;
            SetRecsPoses();
        }

        /// <returns>Playerstate corresponding to players current state</returns>
        public PlayerState GetState()
        {
            return CurrentState;
        }

        /// <summary>
        /// Checks if the player's is a certain state
        /// </summary>
        /// <param name="state">Player state thet's to be checked</param>
        /// <returns>True if CurrentState Matches state, otherwise false</returns>
        public bool IsCurrentState(PlayerState state)
        {
            return CurrentState == state;
        }

        /// <returns>True if groundpounding, else false</returns>
        public bool IsGroundPounding()
        {
            return GroundPound;
        }

        public void SetGroundPound(bool newValue)
        {
            GroundPound = newValue;
        }

        /// <param name="health">New value</param>
        public void SetHealth(int health)
        {
            HealthPoints = health;
        }

        /// <returns>Current player health</returns>
        public int GetCurrentHealth()
        {
            return HealthPoints;
        }

        /// <summary>
        /// Creates a ground check rectangle
        /// </summary>
        /// <returns>New ground check rec</returns>
        Rectangle CalculateGroundRec()
        {
            return new(DestRec.X + GroundCheckRecMargin, DestRec.Bottom, DestRec.Width - GroundCheckRecMargin*2, GroundCheckRecHeight);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Enums;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Platforming_Game.GameObjects
{
    public abstract class Enemy
    {
        // Rectangle
        public Rectangle DestRec;
        protected Rectangle GroundCheckRec;

        // Vector2
        protected Vector2 Vel;
        protected Vector2 MaxVel;
        protected Vector2 Pos;
        protected Vector2 ObjectForce;

        // Float
        protected float DrawLayer;
        protected float Gravity;
        protected float Mass;
        protected float DragCoeficient;

        // Bool
        protected bool OnGround;
        protected bool Patrolling;

        // Other
        protected Texture2D Tex;
        protected Color CurrentColor;
        protected EnemyState CurrentState;

        public void Update(float? deltaTime, Player player)
        {
            GroundCheckRec.X = DestRec.X;
            GroundCheckRec.Y = DestRec.Bottom;

            if (CurrentState == EnemyState.Alive)
                Enemylogic(deltaTime, player);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentState == EnemyState.Alive)
            spriteBatch.Draw(Tex, DestRec, null, CurrentColor, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }

        public virtual void Enemylogic(float? deltaTime, Player player)
        {

        }

        public void SetOnGround(bool isOnground)
        {
            OnGround = isOnground;
        }

        public bool GetOnGround()
        {
            return OnGround;
        }

        public Rectangle GetGroundcheckRec()
        {
            return GroundCheckRec;
        }

        public EnemyState GetCurrentState()
        {
            return CurrentState;
        }

        public void Kill()
        {
            CurrentState = EnemyState.Dead;
        }

        protected void CalculateNewPos()
        {
            if (Vel.X >= MaxVel.X)
                Vel.X = MaxVel.X;
            else if (Vel.X <= -MaxVel.X)
                Vel.X = -MaxVel.X;

            if (Vel.Y >= MaxVel.Y * 1.5f)
                Vel.Y = MaxVel.Y * 1.5f;
            else if (Vel.Y <= -MaxVel.Y)
                Vel.Y = -MaxVel.Y;

            SetRecsPoses();
        }

        protected void SetRecsPoses()
        {
            DestRec.X = (int)Pos.X;
            DestRec.Y = (int)Pos.Y;

            GroundCheckRec.X = DestRec.X;
            GroundCheckRec.Y = DestRec.Bottom;
        }

        public void AddForce(Vector2 f)
        {
            ObjectForce += f;
        }

        public void SetVel(float x, float y)
        {
            Vel = new(x,y);
            CalculateNewPos();
        }

        public void SetVel(Vector2 newVel)
        {
            Vel = newVel;
            CalculateNewPos();
        }

        public Vector2 GetVel()
        {
            return Vel;
        }

        public void SetPos(float x, float y)
        {
            Pos = new Vector2(x, y);

            SetRecsPoses();
        }

        public void SetPos(Vector2 newPos)
        {
            Pos = newPos;

            SetRecsPoses();
        }
    }
}

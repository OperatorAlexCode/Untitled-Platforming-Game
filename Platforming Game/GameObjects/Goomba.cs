using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Enums;
using Platforming_Game.Other;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforming_Game.GameObjects
{
    public class Goomba : Enemy
    {
        Vector2 PlayerPushBackForce;
        float WalkForce;
        public Goomba(SpriteBatch spriteBatch, Rectangle destRec,float drawLayer)
        {
            Tex = new(spriteBatch.GraphicsDevice, 1, 1);
            Tex.SetData(new[] { Color.White });
            DestRec = destRec;
            DrawLayer = drawLayer;
            SetConstants();
        }

        public Goomba(Texture2D tex, Rectangle destRec, float drawLayer)
        {
            Tex = tex;
            DestRec = destRec;
            DrawLayer = drawLayer;
            SetConstants();
        }

        void SetConstants()
        {
            CurrentColor = Color.Red;
            CurrentState = EnemyState.Alive;

            GroundCheckRec = new(DestRec.X,DestRec.Bottom,DestRec.Width,5);
            PlayerPushBackForce = new(0, -6000);
            Pos = new(DestRec.X, DestRec.Y);
            MaxVel = new(60, 400);

            Gravity = 350.0f;
            Mass = 1.5f;
            WalkForce = 450.0f;
            DragCoeficient = 0.8f;
        }

        public override void Enemylogic(float? deltaTime, Player player)
        {
            if (!OnGround)
                AddForce(new(0,Gravity));

            else
            {
                if (DestRec.Center.X > player.DestRec.Center.X)
                    AddForce(new(-WalkForce, 0));
                else if (DestRec.Center.X < player.DestRec.Center.X)
                    AddForce(new(WalkForce, 0));
            }

            if (DestRec.Intersects(player.DestRec))
            {
                if (player.IsCurrentState(PlayerState.Dashing))
                    CurrentState = EnemyState.Dead;

                else if (player.DestRec.Bottom >= DestRec.Top && player.DestRec.Bottom < DestRec.Center.Y)
                {
                    player.SetVel(new(player.GetVel().X, 0));
                    player.AddForce(PlayerPushBackForce);
                    CurrentState = EnemyState.Dead;
                }
                    
                else
                    player.GetHurt();
            }     

            OnGround = false;

            AddForce(new(-Vel.X * DragCoeficient, 0));

            Vel += ObjectForce / Mass * deltaTime.Value;
            Pos += Vel * deltaTime.Value;
            ObjectForce = Vector2.Zero;

            CalculateNewPos();
        }
    }
}

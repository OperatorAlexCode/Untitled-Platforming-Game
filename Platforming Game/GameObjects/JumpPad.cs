using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Base_Classes;
using Platforming_Game.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Platforming_Game.GameObjects
{
    public class JumpPad
    {
        Texture2D Tex;
        Rectangle DestRec;
        JumpPadTrigger Trigger;
        Color PadColor = Color.Purple;
        float DrawLayer;
        int JumpPadheight = 5;
        bool IsPlayerOnPad;

        public JumpPad(SpriteBatch spriteBatch, Rectangle destRec, float drawLayer)
        {
            Tex = new(spriteBatch.GraphicsDevice, 1, 1);
            Tex.SetData(new[] { Color.White });
            DrawLayer = drawLayer;
            DestRec = new(destRec.X, destRec.Bottom - JumpPadheight, destRec.Width, JumpPadheight);
            Trigger = new(new(destRec.X, destRec.Y, destRec.Width, destRec.Height - JumpPadheight));
        }

        public void Update(float? deltaTime, Player? player)
        {
            

            if (DestRec.Intersects(player.DestRec))
            {
                IsPlayerOnPad = true;
                player.SetOnGround(true);

                if (player.GetVel().Y != 0)
                    player.SetVel(new(player.GetVel().X, 0));

                if (player.DestRec.Bottom > DestRec.Top)
                {
                    player.SetPos(new(player.DestRec.X, DestRec.Top - player.DestRec.Height));
                }   
            }

            if (IsPlayerOnPad && !DestRec.Intersects(player.GroundRec))
            {
                IsPlayerOnPad = false;
                player.SetOnGround(false);
            }

            if (player != null)
                Trigger.UpdatePlayer(player);
            Trigger.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, DestRec, null, PadColor, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }

        public Rectangle GetDestRec()
        {
            return DestRec;
        }
    }

    internal class JumpPadTrigger : TriggerCollider
    {
        // Vector2
        Vector2 JumpForce = new(0, -5500);
        Vector2 VelThresholds = new(0, -5);
        Vector2 LastPlayerVel;

        // Other
        float CoolDownTime = 0.5f;
        Player PlayerChar;
        Timer CoolDown;

        public JumpPadTrigger(Rectangle colliderArea)
        {
            ColliderArea = colliderArea;
            CoolDown = new();
        }

        public override void Update(float? deltaTime)
        {
            if (deltaTime.HasValue)
                CoolDown.Update(deltaTime.Value);

            if (PlayerChar != null)
            {
                if (ColliderArea.Contains(PlayerChar.DestRec) && CoolDown.IsDone())
                {
                    if (!PlayerChar.GetOnGround() && PlayerChar.GetVel().Y <= VelThresholds.Y && LastPlayerVel.Y == 0)
                    {
                        PlayerChar.AddForce(JumpForce);
                        CoolDown.StartTimer(CoolDownTime);
                    }

                    else if (PlayerChar.GetOnGround() && PlayerChar.GetVel().Y == 0 && LastPlayerVel.Y >= -VelThresholds.Y && PlayerChar.DestRec.Bottom >= ColliderArea.Bottom)
                    {
                        PlayerChar.SetVel(0, -LastPlayerVel.Y);
                        PlayerChar.AddForce(JumpForce);

                        if (PlayerChar.GetOnGround())
                            PlayerChar.SetOnGround(false);
                        if (PlayerChar.IsGroundPounding())
                            PlayerChar.SetGroundPound(false);

                        CoolDown.StartTimer(CoolDownTime);
                    }
                }

                LastPlayerVel = PlayerChar.GetVel();
            }
        }

        public void UpdatePlayer(Player player)
        {
            PlayerChar = player;
        }
    }
}

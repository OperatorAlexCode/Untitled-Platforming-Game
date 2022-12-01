using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Enums;
using Platforming_Game.Managers;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Platforming_Game.GameObjects
{
    public class Platform
    {
        Texture2D Tex;
        Rectangle DestRec;
        float DrawLayer;
        Color CurrentColor = Color.Green;
        bool PlayerIsOnPlatform;
        PlatformType Type = PlatformType.Normal;

        public Platform(Texture2D tex, Rectangle destRec, Color? color, float drawLayer, PlatformType? type = null)
        {
            Tex = tex;
            DestRec = destRec;
            if (color.HasValue)
                CurrentColor = color.Value;
            DrawLayer = drawLayer;
            if (type.HasValue)
                Type = type.Value;
        }

        public Platform(SpriteBatch spriteBatch, Rectangle destRec, Color? color, float drawLayer, PlatformType? type = null)
        {
            Tex = new(spriteBatch.GraphicsDevice, 1, 1);
            Tex.SetData(new[] { Color.White });
            DestRec = destRec;
            if (color.HasValue)
                CurrentColor = color.Value;
            DrawLayer = drawLayer;
            if (type.HasValue)
                Type = type.Value;
        }

        public void Update(Player? player)
        {
            if (player != null)
            {
                if ((DestRec.Intersects(player.GroundRec) || (!DestRec.Intersects(player.GroundRec) && DestRec.Intersects(player.DestRec))) && RecCheck(player, 0))
                {
                    player.SetOnGround(true);
                    PlayerIsOnPlatform = true;
                    if (player.GetVel().Y != 0)
                        player.SetVel(player.GetVel().X, 0);

                    if (DestRec.Contains(player.GroundRec) || player.GroundRec.Top > DestRec.Bottom)
                        player.SetPos(player.DestRec.X, DestRec.Top - player.DestRec.Height);
                }
                else if (DestRec.Intersects(player.DestRec) && Type != PlatformType.PassThrought)
                {
                    if (RecCheck(player, 1))
                    {
                        player.SetVel(player.GetVel().X, -player.GetVel().Y);
                        player.SetPos(player.DestRec.X, DestRec.Bottom);
                    }
                    else if (RecCheck(player, 2))
                    {
                        player.SetVel(0, player.GetVel().Y);
                        player.SetPos(DestRec.Left - player.DestRec.Width, player.DestRec.Y);
                    }
                    else if (RecCheck(player, 3))
                    {
                        player.SetVel(0, player.GetVel().Y);
                        player.SetPos(DestRec.Right, player.DestRec.Y);
                    }
                }

                if (PlayerIsOnPlatform && !DestRec.Intersects(player.GroundRec))
                {
                    player.SetOnGround(false);
                    PlayerIsOnPlatform = false;
                }
            }

            if (!EnemyManager.IsEmpty())
            {
                List<Goomba> goombas = EnemyManager.GetGoombas().Where(g => DestRec.Intersects(g.GetGroundcheckRec()) || DestRec.Intersects(g.DestRec)).ToList();

                if (goombas.Count > 0)
                {
                    foreach (Goomba goomba in goombas)
                    {
                        if (DestRec.Intersects(goomba.GetGroundcheckRec()) && RecCheck(goomba, 0))
                        {
                            goomba.SetOnGround(true);
                            if (goomba.GetVel().Y != 0)
                            goomba.SetVel(new(goomba.GetVel().X, 0));
                            if (DestRec.Intersects(goomba.GetGroundcheckRec()))
                                goomba.SetPos(goomba.DestRec.X, DestRec.Top - goomba.DestRec.Height);
                        }

                        else if (DestRec.Intersects(goomba.DestRec))
                        {
                            if (RecCheck(goomba, 2))
                            {
                                goomba.SetVel(0, goomba.GetVel().Y);
                                goomba.SetPos(DestRec.Left - goomba.DestRec.Width, goomba.DestRec.Y);
                            }
                            else if (RecCheck(goomba, 3))
                            {
                                goomba.SetVel(0, goomba.GetVel().Y);
                                goomba.SetPos(DestRec.Right, goomba.DestRec.Y);
                            }
                        }
                    }

                    EnemyManager.UpdateGoombas(goombas);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, DestRec, null, CurrentColor, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }

        /// <summary>
        /// Checks to see if any sides of a player intersects with the destination recatngle
        /// </summary>
        /// <param name="player">player to check</param>
        /// <param name="side">side of the player</param>
        /// <returns>true if it intersects, else false</returns>
        bool RecCheck(Player player, int side)
        {
            bool result = false;

            switch (side)
            {
                case 0:
                    result = player.DestRec.Bottom > DestRec.Top && player.DestRec.Top < DestRec.Top && player.DestRec.Bottom < DestRec.Bottom;
                    break;
                case 1:
                    result = player.DestRec.Top < DestRec.Bottom && player.DestRec.Top > DestRec.Top && player.DestRec.Bottom > DestRec.Bottom;
                    break;
                case 2:
                    result = player.DestRec.Right > DestRec.Left && player.DestRec.Right < DestRec.Right && player.DestRec.Left < DestRec.Left;
                    break;
                case 3:
                    result = player.DestRec.Left < DestRec.Right && player.DestRec.Right > DestRec.Right && player.DestRec.Left > DestRec.Left;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Checks to see if any sides of a goomba intersects with the destination recatngle
        /// </summary>
        /// <param name="player">goomba to check</param>
        /// <param name="side">side of the player</param>
        /// <returns>true if it intersects, else false</returns>
        bool RecCheck(Goomba goomba, int side)
        {
            bool result = false;

            switch (side)
            {
                case 0:
                    result = goomba.DestRec.Bottom > DestRec.Top && goomba.DestRec.Top < DestRec.Top && goomba.DestRec.Bottom < DestRec.Bottom;
                    break;
                case 1:
                    result = goomba.DestRec.Top < DestRec.Bottom && goomba.DestRec.Top > DestRec.Top && goomba.DestRec.Bottom > DestRec.Bottom;
                    break;
                case 2:
                    result = goomba.DestRec.Right > DestRec.Left && goomba.DestRec.Right < DestRec.Right;
                    break;
                case 3:
                    result = goomba.DestRec.Left < DestRec.Right && goomba.DestRec.Left > DestRec.Left;
                    break;
            }

            return result;
        }
    }
}

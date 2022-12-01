using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Base_Classes;
using SharpDX.Direct2D1;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Platforming_Game.GameObjects
{
    public class OneUp : Item
    {
        public OneUp(SpriteBatch spriteBatch, Rectangle destRec, float drawLayer) : base(spriteBatch, destRec, drawLayer)
        {
            CurrentColor = Color.LightPink;
        }

        public override void Effect(Player? player = null)
        {
            player.SetHealth(player.GetCurrentHealth()+1);
            IsUsed = true;
        }
    }
}

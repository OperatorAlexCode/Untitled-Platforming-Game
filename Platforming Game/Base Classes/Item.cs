using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.GameObjects;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Platforming_Game.Base_Classes
{
    public class Item
    {
        protected Texture2D Tex;
        protected Rectangle DestRec;
        protected float DrawLayer;
        protected Color CurrentColor;
        protected bool IsUsed;

        public Item(SpriteBatch spriteBatch, Rectangle destRec, float drawLayer)
        {
            Tex = new(spriteBatch.GraphicsDevice, 1, 1);
            Tex.SetData(new[] { Color.White });
            DestRec = destRec;
            DrawLayer = drawLayer;
        }

        public void Update(Player player)
        {
            if (DestRec.Intersects(player.DestRec) && !IsUsed)
                Effect(player);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsUsed)
            spriteBatch.Draw(Tex, DestRec, null, CurrentColor, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }

        public virtual void Effect(Player? player = null)
        {

        }

        public bool IsItemUsed()
        {
            return IsUsed;
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Base_Classes;
using Platforming_Game.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforming_Game.GameObjects
{
    public class LavaPool : KillBarrier
    {
        Texture2D Tex;
        Player Playerchar;
        Color CurrentColor = Color.Orange;
        float DrawLayer;

        public LavaPool(Texture2D tex, Rectangle colliderArea, float drawLayer,Action triggerFunction) : base(colliderArea, triggerFunction)
        {
            Tex = tex;
            ColliderArea = colliderArea;
            TriggerFunction = triggerFunction;
            DrawLayer = drawLayer;
        }

        public LavaPool(SpriteBatch spriteBatch, Rectangle colliderArea, float drawLayer, Action triggerFunction) : base(colliderArea, triggerFunction)
        {
            Tex = new(spriteBatch.GraphicsDevice, 1, 1);
            Tex.SetData(new[] { Color.White });
            ColliderArea = colliderArea;
            TriggerFunction = triggerFunction;
            DrawLayer = drawLayer;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, ColliderArea, null, CurrentColor, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }
    }
}

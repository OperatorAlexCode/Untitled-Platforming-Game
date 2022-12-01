using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Base_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforming_Game.GameObjects
{
    public class Goal : TriggerCollider
    {
        Color CurrentColor = new Color(255, 255, 255, 0.1f);
        float DrawLayer = 0.0f;
        Texture2D Tex;
        Player PlayerChar;
        public Goal(SpriteBatch spriteBatch,Rectangle colliderArea, Action? triggerFunction)
        {
            Tex = new(spriteBatch.GraphicsDevice, 1, 1);
            Tex.SetData(new[] { Color.White });
            ColliderArea = colliderArea;
            TriggerFunction = triggerFunction;
        }

        public override void Update(float? deltaTime)
        {
            if (PlayerChar != null && TriggerFunction != null)
                if (ColliderArea.Intersects(PlayerChar.DestRec))
                    TriggerFunction.Invoke();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, ColliderArea, null, CurrentColor, 0.0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        }

        public void UpdatePlayer(Player player)
        {
            PlayerChar = player;
        }
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforming_Game.Base_Classes
{
    public abstract class TriggerCollider
    {
        protected Rectangle ColliderArea;
        protected Action? TriggerFunction;

        public abstract void Update(float? deltaTime);

        public Rectangle GetColliderAre()
        {
            return ColliderArea;
        }
    }
}

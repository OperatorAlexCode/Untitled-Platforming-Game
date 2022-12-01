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
    public class KillBarrier : TriggerCollider
    {
        Player PlayerChar;
        public KillBarrier(Rectangle colliderArea, Action? triggerFunction)
        {
            ColliderArea = colliderArea;
            TriggerFunction = triggerFunction;
        }

        public override void Update(float? deltaTime)
        {
            if (PlayerChar != null && TriggerFunction != null)
                if (ColliderArea.Intersects(PlayerChar.DestRec))
                    TriggerFunction.Invoke();

            if (EnemyManager.GetGoombas().Any(g => ColliderArea.Contains(g.DestRec) && g.GetCurrentState() == Enums.EnemyState.Alive))
            {
                List<Goomba> goombas = EnemyManager.GetGoombas().Where(g => ColliderArea.Contains(g.DestRec) && g.GetCurrentState() == Enums.EnemyState.Alive).ToList();

                foreach (Goomba goomba in goombas)
                    goomba.Kill();
            }
        }

        public void UpdatePlayer(Player player)
        {
            PlayerChar = player;
        }
    }
}

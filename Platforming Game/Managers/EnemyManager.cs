using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Enums;
using Platforming_Game.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforming_Game.Managers
{
    public static class EnemyManager
    {
        static List<Goomba> Goombas { get; set; }

        public static void InstantializeFields()
        {
            Goombas = new();
        }

        public static void Update(float? deltaTime,Player player)
        {
            List<Goomba> goombasToRemove = Goombas.Where(g => g.GetCurrentState() == EnemyState.Dead).ToList();

            if (goombasToRemove.Count > 0)
                foreach (Goomba goomba in goombasToRemove)
                    Goombas.Remove(goomba);

            for (int x = 0; x < Goombas.Count; x++)
                Goombas[x].Update(deltaTime, player);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < Goombas.Count; x++)
                Goombas[x].Draw(spriteBatch);
        }

        public static void AddGoomba(Goomba newGoomba)
        {
            Goombas.Add(newGoomba);
        }

        public static void AddGoombas(List<Goomba> newGoombas)
        {
            Goombas.AddRange(newGoombas);
        }

        public static void ClearEnemies()
        {
            Goombas.Clear();
        }

        /// <summary>
        /// Check wether EnememyManager has any enemies in it
        /// </summary>
        /// <returns>true if there are any enemies inside, otherwise false</returns>
        public static bool IsEmpty()
        {
            return Goombas.Count == 0;
        }

        public static List<Goomba> GetGoombas()
        {
            return Goombas;
        }

        public static void UpdateGoomba(Goomba goombaToUpdate)
        {
            int updateIndex = Goombas.IndexOf(goombaToUpdate);

            if (updateIndex != -1)
                Goombas[updateIndex] = goombaToUpdate;
        }

        public static void UpdateGoombas(List<Goomba> goombasToUpdate)
        {
            foreach(Goomba goomba in goombasToUpdate)
            {
                int updateIndex = Goombas.IndexOf(goomba);

                if (updateIndex != -1)
                    Goombas[updateIndex] = goomba;
            } 
        }

        public static void KillGoomba(Goomba goombaTokill)
        {
            Goombas.Remove(goombaTokill);
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using Platforming_Game.Base_Classes;
using Platforming_Game.Enums;
using Platforming_Game.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforming_Game.Managers
{
    public static class ItemManager
    {
        static List<OneUp> OneUps;
        public static void Update(Player player)
        {
            for (int x = 0; x < OneUps.Count; x++)
                OneUps[x].Update(player);

            if (OneUps.Any(o => o.IsItemUsed()))
                OneUps = OneUps.Where(o => !o.IsItemUsed()).ToList();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach(OneUp oneUp in OneUps)
                oneUp.Draw(spriteBatch);
        }

        public static void InitalizeFields()
        {
            OneUps = new();
        }

        public static void ClearItems()
        {
            OneUps.Clear();
        }

        public static void ClearItems(int itemType)
        {
            switch (itemType)
            {
                case 0:
                    OneUps.Clear();
                    break;
            }
        }

        public static void AddItem(Item item, int itemType)
        {
            switch(itemType)
            {
                case 0:
                    OneUps.Add((OneUp)item);
                    break;
            }
        }

        public static bool IsEmpty()
        {
            return OneUps.Count == 0;
        }
    }
}

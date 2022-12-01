using Microsoft.Xna.Framework;
using Platforming_Game.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforming_Game.Other
{
    public class Level
    {
        public Rectangle[] Platforms { get; set; }
        public Vector2 PlayerSpawn { get; set; }
        public Rectangle Goal { get; set; }
        public Rectangle[] Goombas { get; set; }
        public Rectangle[] JumpPads { get; set; }
        public Rectangle[] LavaPools { get; set; }
        public Rectangle[] OneUps { get; set; }
    }
}

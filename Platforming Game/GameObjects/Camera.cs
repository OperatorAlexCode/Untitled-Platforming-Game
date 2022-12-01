using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platforming_Game.GameObjects
{
    public class Camera
    {
        public Matrix Transform { get; private set; }
        public int CameraWidth { get; set; }
        public int CameraHeight { get; set; }

        public void ChangeCameraSpecs(int newHeight, int newWidth)
        {
            CameraHeight = newHeight;
            CameraWidth = newWidth;
        }

        public void FollowExact(Player player)
        {
            Matrix position = Matrix.CreateTranslation(
                -player.DestRec.X - (player.DestRec.Width/2),
                -player.DestRec.Y - (player.DestRec.Height/2),
                0);

            Matrix offset = Matrix.CreateTranslation(
                CameraWidth/2,
                CameraHeight/2,
                0);

            Transform = position * offset;
        }

        public void FollowExact(Vector2 toFollow)
        {
            Matrix position = Matrix.CreateTranslation(
                -toFollow.X,
                -toFollow.Y,
                0);

            Matrix offset = Matrix.CreateTranslation(
                CameraWidth / 2,
                CameraHeight / 2,
                0);

            Transform = position * offset;
        }

        public void FollowHorizontaly(Player player)
        {
            Matrix position = Matrix.CreateTranslation(
                -player.DestRec.X - (player.DestRec.Width / 2),
                -(CameraHeight / 2),
                0);

            Matrix offset = Matrix.CreateTranslation(
                CameraWidth / 2,
                CameraHeight / 2,
                0);

            Transform = position * offset;
        }

        public void FollowVerticaly(Player player)
        {
            Matrix position = Matrix.CreateTranslation(
                -(CameraWidth / 2),
                -player.DestRec.Y - (player.DestRec.Height / 2),
                0);

            Matrix offset = Matrix.CreateTranslation(
                CameraWidth / 2,
                CameraHeight / 2,
                0);

            Transform = position * offset;
        }
    }
}

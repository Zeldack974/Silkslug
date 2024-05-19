using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Silkslug.ColosseumRubicon
{
    public class HKLevel : CosmeticSprite
    {
        public HKLevel(Room room)
        { 
            this.room = room;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("illustrations/hkback", true);
            sLeaser.sprites[1] = new FSprite("illustrations/hkfront", true);
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Midground");
            }

            rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[0]);
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[1]);

            if (sLeaser.containers != null)
            {
                foreach (FContainer node in sLeaser.containers)
                {
                    newContatiner.AddChild(node);
                }   
            }
            //base.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            float ratio = 1358 / 722;
            Debug.Log($"camPos: {camPos}, screenSize: {rCam.sSize}");
            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.height = rCam.sSize.y;
                sprite.width = rCam.sSize.x;
                sprite.scale = 1.1f;
                sprite.SetPosition(camPos + (rCam.sSize * sprite.scale) / 2 + new Vector2(60, 60 - 20));
                //sprite.scale = 2;
            }

        }

    }
}

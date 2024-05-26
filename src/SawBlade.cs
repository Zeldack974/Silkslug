using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevConsole;
using RWCustom;
using UnityEngine;

namespace Silkslug
{
    internal class SawBlade : CosmeticSprite
    {
        public PositionedSoundEmitter soundEmitter;
        public float rad;
        public SawBlade(Room room, Vector2 pos, float rad = 70)
        {
            this.room = room;
            this.pos = pos;
            this.rad = rad;
            soundEmitter = new PositionedSoundEmitter(this.pos, 0.5f, 1f);
            room.PlaySound(Sounds.SAW_LOOP, soundEmitter, true, 1f, 1f, false);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            //this.pos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + room.game.cameras[0].pos;
            soundEmitter.pos = pos;

            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    if (room.physicalObjects[i][j] != null && !room.physicalObjects[i][j].slatedForDeletetion && Custom.DistLess(pos, room.physicalObjects[i][j].firstChunk.pos, rad))
                    {
                        room.physicalObjects[i][j].firstChunk.vel = Custom.DirVec(pos, room.physicalObjects[i][j].firstChunk.pos).normalized * 50f;
                        if (room.physicalObjects[i][j] is Creature && !(room.physicalObjects[i][j] as Creature).dead)
                        {
                            (room.physicalObjects[i][j] as Creature).Violence(null, Vector2.zero, this.room.physicalObjects[i][j].firstChunk, null, Creature.DamageType.Blunt, 10f, 10f);
                            room.PlaySound(SoundID.Spear_Stick_In_Creature, room.physicalObjects[i][j].firstChunk);
                        }
                    }
                }
            }
        }

        public override void Destroy()
        {
            soundEmitter.Destroy();
            base.Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);

            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("assets/sawblade", true);
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].SetPosition(this.pos - camPos);
            sLeaser.sprites[0].scale = (rad / 555f) * 2f;
            UnityEngine.Debug.Log(Time.time * 40);

            sLeaser.sprites[0].rotation = (sLeaser.sprites[0].rotation + 360f * (2f / 3f)) % 361f;

        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Background");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }
    }
}

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
        private readonly PlacedObject _pObj;
        private DevtoolObjects.SawBladeData data => (_pObj.data as DevtoolObjects.SawBladeData);
        public float t = 1f;
        public bool backward;
        public float continueTIme;

        public float rad => data.radVec.magnitude;
        public float speed => data.speed / (100f * 4f);
        public float waitingTime => data.wait;
        public Vector2 startPos => _pObj.pos;
        public Vector2 endPos => startPos + data.Vec;
        public SawBlade(Room room, PlacedObject _pObj)
        {
            this.room = room;
            this._pObj = _pObj;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            //this.pos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + room.game.cameras[0].pos;

            if (room.ReadyForPlayer && (soundEmitter == null || !soundEmitter.soundStillPlaying))
            {
                soundEmitter = new PositionedSoundEmitter(this.pos, 1.0f, 1f);
                room.PlaySound(Sounds.SAW_LOOP, soundEmitter, true, 1f, 1f, false);
            }

            if (Time.time > continueTIme)
            {
                int sn = backward ? -1 : 1;
                t += speed * sn;

                if (t >= 1 || t <= 0)
                {
                    t = Mathf.Clamp01(t);
                    backward = !backward;
                    continueTIme = Time.time + waitingTime;
                }
            }



            this.pos = Vector2.Lerp(startPos, endPos, t);
            if (soundEmitter != null) soundEmitter.pos = pos;


            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    if (room.physicalObjects[i][j] != null && !room.physicalObjects[i][j].slatedForDeletetion)
                    {
                        for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
                        {
                            if (Custom.DistLess(pos, room.physicalObjects[i][j].bodyChunks[k].pos, rad))
                            {
                                if (room.physicalObjects[i][j] is Creature && !(room.physicalObjects[i][j] as Creature).dead)
                                {
                                    (room.physicalObjects[i][j] as Creature).Violence(null, Vector2.zero, this.room.physicalObjects[i][j].bodyChunks[k], null, Creature.DamageType.Blunt, 10f, 10f);
                                    room.PlaySound(SoundID.Spear_Stick_In_Creature, room.physicalObjects[i][j].firstChunk);
                                }
                                room.physicalObjects[i][j].firstChunk.vel = Custom.DirVec(pos, room.physicalObjects[i][j].bodyChunks[k].pos).normalized * 25f;
                            }
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
            Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
            sLeaser.sprites[0].SetPosition(vector - camPos);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using RegionKit.Extras;

namespace Silkslug
{
    public class MovingPlatform : CosmeticSprite
    {
        private readonly PlacedObject _pObj;
        private DevtoolObjects.PlatformData data => (_pObj.data as DevtoolObjects.PlatformData);
        public float t = 0f;
        public bool backward;
        public float continueTime;

        public float speed => data.speed / (100f * 10f);
        public float waitingTime => data.wait;
        public Vector2 startPos { get { return room.GetTilePosition(_pObj.pos).ToVector2() * 20; } }
        public Vector2 endPos => startPos + data.Vec.ToVector2() * 20;
        public Vector2 rect => (data.scaleVec.ToVector2() + Vector2.one) * 20;
        public MovingPlatform(Room room, PlacedObject _pObj)
        {
            this.room = room;
            this._pObj = _pObj;
            continueTime = Time.time + waitingTime;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            //this.pos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + room.game.cameras[0].pos;


            if (Time.time > continueTime)
            {
                int sn = backward ? -1 : 1;
                t += speed * sn;

                if (t >= 1 || t <= 0)
                {
                    t = Mathf.Clamp01(t);
                    backward = !backward;
                    continueTime = Time.time + waitingTime;
                }
            }

            this.pos = Vector2.Lerp(startPos, endPos, t);

            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    if (room.physicalObjects[i][j] != null && !room.physicalObjects[i][j].slatedForDeletetion)
                    {
                        for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
                        {
                            BodyChunk chuck = room.physicalObjects[i][j].bodyChunks[k];
                            Utils.BoxCollisionResult result = Utils.PointInBox(chuck.pos, this.pos, this.pos + this.rect, chuck.rad);

                            if (result.collide)
                            {
                                //Plugin.Log($"colide with chucks");
                            }

                            Utils.BoxCollisionResult r1 = Utils.PointInBox(chuck.pos, this.pos, this.pos + this.rect + Vector2.up * (Math.Abs((this.pos - this.lastPos).y) + 10f), chuck.rad);
                            Utils.BoxCollisionResult r2 = Utils.PointInBox(chuck.pos, this.lastPos, this.lastPos + this.rect + Vector2.up * (Math.Abs((this.pos - this.lastPos).y) + 10f), chuck.rad);

                            int l = 0;
                            while (result.collide)
                            {
                                l++;
                                //Plugin.Log("correction: " + l);
                                //Plugin.Log("correction vec" + result.correctionVec);


                                if (Math.Abs(result.correctionVec.x) <= Math.Abs(result.correctionVec.y))
                                {
                                    //Plugin.Log("correct x");
                                    chuck.pos.x += result.correctionVec.x;
                                    chuck.vel.x += result.correctionVec.x;

                                    if (chuck.owner is Creature && Math.Abs(result.correctionVec.x) > 0.01 && Time.time > continueTime) // !(chuck.owner as Creature).dead
                                    {
                                        //Plugin.Log("crush vec check: " + Vector2.up * (result.correctionVec.x / Math.Abs(result.correctionVec.x) * (chuck.rad / 20)));
                                        IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, chuck.pos, chuck.pos + Vector2.up * (result.correctionVec.x / Math.Abs(result.correctionVec.x) * (chuck.rad / 20)));
                                        if (intVector.HasValue)
                                        {
                                            if (!(chuck.owner as Creature).dead) Plugin.Log($"{(chuck.owner as Creature).Template.name} Crushed!");
                                            room.PlaySound(SoundID.Spear_Stick_In_Creature, chuck);
                                            (chuck.owner as Creature).Violence(null, null, chuck, null, Creature.DamageType.Blunt, 999999f, 0f);
                                        }
                                    }
                                }
                                else
                                {
                                    //Plugin.Log("correct y");
                                    chuck.pos.y += result.correctionVec.y;
                                    chuck.vel.y += result.correctionVec.y;
                                    chuck.vel.x *= 0.75f;

                                    if (chuck.owner is Player)
                                    {
                                        (chuck.owner as Player).canJump = 2;
                                    }

                                    if (chuck.owner is Creature && Math.Abs(result.correctionVec.y) > 0.01 && Time.time > continueTime) // !(chuck.owner as Creature).dead
                                    {
                                        //Plugin.Log("crush vec check: " + Vector2.up * (result.correctionVec.y / Math.Abs(result.correctionVec.y) * (chuck.rad / 20)));
                                        IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, chuck.pos, chuck.pos + Vector2.up * (result.correctionVec.y / Math.Abs(result.correctionVec.y) * (chuck.rad / 20)));
                                        if (intVector.HasValue)
                                        {
                                            if (!(chuck.owner as Creature).dead) Plugin.Log($"{(chuck.owner as Creature).Template.name} Crushed!");
                                            room.PlaySound(SoundID.Spear_Stick_In_Creature, chuck);
                                            (chuck.owner as Creature).Violence(null, null, chuck, null, Creature.DamageType.Blunt, 999999f, 0f);
                                        }
                                    }
                                }

                                if (l == 2)
                                {
                                    //Plugin.Log("failed to correct");
                                    break;
                                }
                                result = Utils.PointInBox(chuck.pos, this.pos, this.pos + this.rect, chuck.rad);
                            }

                            if ((r1.collide || r2.collide) && Time.time > continueTime)
                            {
                                chuck.pos += (this.pos - this.lastPos) * 1f;
                            }
                        }
                    }
                }
            }
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);

            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].SetAnchor(Vector2.zero);
            sLeaser.sprites[0].color = new Color(0.01f, 0.01f, 0.01f); ;
            sLeaser.sprites[1] = new FSprite("FaceE0", true);
            sLeaser.sprites[1].color = Color.red;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            data.scaleVec.x = Math.Max(data.scaleVec.x, 0);
            data.scaleVec.y = Math.Max(data.scaleVec.y, 0);

            Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
            sLeaser.sprites[0].SetPosition(vector - camPos);
            sLeaser.sprites[0].scaleX = rect.x * 0.0625f;
            sLeaser.sprites[0].scaleY = rect.y * 0.0625f;

            sLeaser.sprites[1].SetPosition((vector + rect / 2f) - camPos);
            float scale = rect.x < rect.y ? rect.x : rect.y;
            sLeaser.sprites[1].scale = scale * 0.0625f;
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }
    }
}

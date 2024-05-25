using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;
using Noise;
using UnityEngine;

namespace Silkslug.ColosseumRubicon
{
    public class HKLevel : CosmeticSprite
    {
        public bool cinematicStarted;
        public int explosionTicks = 30;
        public SingularityBomb bomb;
        public bool playerSpawned;
        public HKLevel(Room room)
        {
            this.room = room;

        }
        // 540, 730

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (!playerSpawned && room.ReadyForPlayer)
            {
                foreach (var abstractPlayer in room.game.Players)
                {
                    if (abstractPlayer.realizedCreature != null)
                    {
                        ConsoleWrite("teleport player");
                        (abstractPlayer.realizedCreature as Player).SuperHardSetPosition(new Vector2((float)(room.LocalCoordinateOfNode(0).x + 2) * 20f, (float)room.LocalCoordinateOfNode(0).y * 20f));
                    }
                }
                playerSpawned = true;
            }

            if (room.ReadyForPlayer)
            {
                if (!cinematicStarted && room.game.Players[0].realizedCreature.mainBodyChunk.pos.x >= 540f)
                {
                    room.game.Players[0].realizedCreature.mainBodyChunk.pos.x = 540f;
                    room.game.Players[0].realizedCreature.mainBodyChunk.vel.x = 0f;
                    Vector2 pos = new Vector2(540f, 730f);
                    AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, room.GetWorldCoordinate(pos), room.game.GetNewID());
                    abstractCreature.RealizeInRoom();

                    AbstractPhysicalObject Abstbomb = new AbstractPhysicalObject(room.world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, room.GetWorldCoordinate(pos), room.game.GetNewID());
                    Abstbomb.RealizeInRoom();
                    bomb = Abstbomb.realizedObject as SingularityBomb;
                    abstractCreature.realizedCreature.Grab(Abstbomb.realizedObject, UnityEngine.Random.Range(0, abstractCreature.realizedCreature.grasps.Length), 0, Creature.Grasp.Shareability.CanNotShare, 0.5f, true, true);


                    abstractCreature.realizedCreature.mainBodyChunk.pos = pos;
                    //abstractCreature.realizedCreature.mainBodyChunk.vel.y = - 100f;
                    abstractCreature.realizedCreature.Die();

                    cinematicStarted = true;
                }

                if (cinematicStarted)
                {
                    if (explosionTicks == 0)
                    {
                        bomb.ignited = true;
                    }
                    if (room.game.Players[0].realizedCreature.dead)
                    {
                        room.game.rainWorld.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.Credits);
                    }
                    explosionTicks--;
                }
            }
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
                sprite.SetAnchor(new Vector2(0.5f, 0.5f));
                //sprite.height = rCam.sSize.y;
                //sprite.width = rCam.sSize.x;
                sprite.scale = 1.1f * (rCam.sSize.y / 722);
                sprite.SetPosition(rCam.sSize / 2f + new Vector2(60, 60 - 34));
                //sprite.scale = 2;
            }
            //foreach (FSprite sprite in sLeaser.sprites)
            //{
            //    sprite.height = rCam.sSize.y;
            //    sprite.width = rCam.sSize.x;
            //    sprite.scale = 1.1f * (rCam.sSize.y / 722);
            //    sprite.SetPosition(camPos + (rCam.sSize * sprite.scale) / 2 + new Vector2(60, 60 - 20));
            //    //sprite.scale = 2;
            //}

        }

    }
}

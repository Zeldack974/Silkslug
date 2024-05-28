using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Silkslug.ColosseumRubicon.Boss
{


    public class DemonEye : CosmeticSprite
    {
        public static int maxCloudIndex = 50;

        public Vector2 eyeCenter = Vector2.zero;
        public int openFrames = -1;
        public bool open;
        public int introFrames = -1;
        public int preIntroFrames = -1;
        public int totalFrames = -1;

        public DemonEye(Room room)
        {
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            this.pos = eyeCenter;
            AbstractCreature abstractPlayer = this.room.game.Players[0];
            if (abstractPlayer != null && abstractPlayer.realizedCreature != null)
            {
                Vector2 addPos = (abstractPlayer.realizedCreature.mainBodyChunk.pos - (this.room.game.cameras[0].pos + this.room.game.cameras[0].sSize / 2)).normalized * 14;
                addPos.y = Math.Min(addPos.y, 5);
                addPos.y = Math.Max(addPos.y, -5);

                this.pos += addPos;
            }

            if (totalFrames > -1)
            {
                totalFrames++;
            }
            //UnityEngine.Debug.Log("totalFrames: " + totalFrames);
            //UnityEngine.Debug.Log("preIntroFrames: " + preIntroFrames);
            //UnityEngine.Debug.Log("openFrames: " + openFrames);
            //UnityEngine.Debug.Log("introFrames: " + introFrames);



            if (preIntroFrames > -1)
            {
                preIntroFrames--;
                if (preIntroFrames == 0)
                {
                    preIntroFrames = -1;
                    openFrames = 60 * 3;
                }
            }

            if (openFrames > -1)
            {

                openFrames--;
                if (openFrames == 0)
                {
                    open = true;
                    openFrames = -1;
                    introFrames = 60 * 3;
                    //this.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, eyeCenter, 1f, 0.5f + UnityEngine.Random.value * 0.5f);
                    //room.AddObject(new ShockWave(eyeCenter, 350f, 0.485f, 300, true));
                }
            }

            //UnityEngine.Debug.Log(introFrames);

            if (introFrames > 0)
            {
                introFrames--;
                if (introFrames == 0)
                {
                    introFrames = -1;
                }

                if(introFrames == 6)
                {
                    BossManager.Instance.SpawnBoss();
                }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[4];
            sLeaser.sprites[0] = new FSprite("assets/eye2", true);
            sLeaser.sprites[0].scale = 0.25f;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];

            sLeaser.sprites[1] = new FSprite("guardEye", true);
            sLeaser.sprites[1].color = new Color(1, 1, 0);
            sLeaser.sprites[1].scale = 2.5f;
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Background"];

            sLeaser.sprites[2] = new FSprite("Futile_White", true);
            sLeaser.sprites[2].scaleX = (rCam.room.RoomRect.right - rCam.room.RoomRect.left) / 4f;
            sLeaser.sprites[2].scaleY = (rCam.room.RoomRect.top - rCam.room.RoomRect.bottom) / 8f;
            sLeaser.sprites[2].x = rCam.room.RoomRect.Center.x;
            sLeaser.sprites[2].y = rCam.room.RoomRect.Center.y / 2;
            sLeaser.sprites[2].color = Color.black;
            sLeaser.sprites[2].isVisible = false;

            sLeaser.sprites[3] = new FSprite("illustrations/bossintro", true);
            sLeaser.sprites[3].x = rCam.room.RoomRect.Center.x;
            sLeaser.sprites[3].y = rCam.room.RoomRect.Center.y / 2;
            sLeaser.sprites[3].SetAnchor(new Vector2(0.5f, 0.5f));
            sLeaser.sprites[3].isVisible = false;

            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            //ConsoleWrite($"camPos: {camPos}");
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i].SetPosition(rCam.sSize / 2);
                //if (open)
                    sLeaser.sprites[i]._container.AddChildAtIndex(sLeaser.sprites[i], maxCloudIndex - 2);
            }
            sLeaser.sprites[3].SetPosition(rCam.sSize / 2f);


            eyeCenter = camPos + rCam.sSize / 2 + new Vector2(0, 10);
            sLeaser.sprites[1].isVisible = open;

            sLeaser.sprites[2].isVisible = introFrames > 0;
            sLeaser.sprites[3].isVisible = introFrames > 0;
            sLeaser.sprites[2].alpha = Math.Min(introFrames, 6f) / 6f;
            sLeaser.sprites[3].alpha = Math.Max(Math.Min(introFrames, 30f) - (30f - 6f), 0f) / 6f;




            if (openFrames == -1)
            {
                sLeaser.sprites[0].SetElementByName(open ? "assets/eye2" : "assets/eye0");
            }
            else if (openFrames > 60 * 1.5f)
            {
                sLeaser.sprites[0].SetElementByName("assets/eye1");
            }
            else
            {
                sLeaser.sprites[0].SetElementByName("assets/eye2");
            }

            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Water");
            }
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

        public void Open()
        {
            preIntroFrames = 570 - (60 * 3);
            totalFrames = 0;
            //open = true;
            //BossManager.Instance.SpawnBoss();
        }

    }
}

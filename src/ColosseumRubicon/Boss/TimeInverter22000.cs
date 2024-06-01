using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;
using UnityEngine;

namespace Silkslug.ColosseumRubicon.Boss
{
    public class TimeInverter22000 : CosmeticSprite
    {
        public FadeOut fadeOut;
        public bool active;
        public int growTicks;
        public int totalTicks = 1240 - (60 * 3);
        public TimeInverter22000(Room room)
        {
            this.room = room;
            fadeOut = new FadeOut(room, Color.black, 60 * 10, false); // 60 * 10
            room.AddObject(fadeOut);
            Manager.DisablePauseMenu = true;
            this.room.game.manager.musicPlayer?.FadeOutAllSongs(0.5f);

        }

        public override void Update(bool eu)
        {
            base.Update(eu);


            if (fadeOut != null && fadeOut.IsDoneFading() && !this.slatedForDeletetion && !active)
            {
                active = true;
                this.room.game.manager.musicPlayer?.GameRequestsSong(new MusicEvent()
                {
                    prio = 100,
                    songName = "22 Minutes",
                    cyclesRest = 0,
                    loop = false,
                });
            }

            if (active)
            {
                growTicks++;
                //UnityEngine.Debug.Log("gowTicks: " + growTicks);
                foreach (var abstractPlayer in room.game.Players)
                {
                    (abstractPlayer.realizedCreature as Player).SuperHardSetPosition(new Vector2(700, 1200));
                }

                if (growTicks == totalTicks - 60)
                {
                    RainWorldGame.ForceSaveNewDenLocation(this.room.game, "CR_THEEND", true);
                }

                if (growTicks >= totalTicks)
                {
                    Manager.DisablePauseMenu = false;
                    this.Destroy();
                    this.room.game.rainWorld.processManager.RequestMainProcessSwitch(HKMainMenu.HKMainMenuID);
                }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            //ConsoleWrite(string.Join(", ", Futile.atlasManager._allElementsByName.ToList().ConvertAll(e => e.Key)));
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("silksong_memory01", true);
            sLeaser.sprites[0].alpha = 0f;
            sLeaser.sprites[0].SetAnchor(new Vector2(0.5f, 0.5f));
            sLeaser.sprites[1] = new FSprite("illustrations/hornethead", true);
            sLeaser.sprites[1].alpha = 0f;
            sLeaser.sprites[1].SetAnchor(new Vector2(0.5f, 0.12f));
            sLeaser.sprites[1].scale = rCam.sSize.x / 440f * (1f / 24f);
            AddToContainer(sLeaser, rCam, null);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            if (active)
            {
                Vector2 center = rCam.sSize / 2f;
                if (growTicks < 300)
                {
                    float num = growTicks / 300f;
                    sLeaser.sprites[1].alpha = growTicks/60f * 3f;
                    sLeaser.sprites[1].x = center.x;
                    sLeaser.sprites[1].y = Mathf.Lerp(rCam.sSize.y * (0.5f - 0.12f), center.y, num);
                    sLeaser.sprites[1].anchorX = Mathf.Lerp(0.5f, 0.70f, num);
                    //sLeaser.sprites[1].scale += (1f / 225f) * 40f;
                    sLeaser.sprites[1].scale += (rCam.sSize.x / 440f) * (1f / 60f) * 0.025f * (1f + (num * num * num * 195f));
                }

                if (growTicks > 160)
                {
                    sLeaser.sprites[0].alpha = Mathf.InverseLerp(160, 160 + 30, growTicks);
                    float num = Mathf.InverseLerp(200f, totalTicks - 60, growTicks);

                    string nb = ((int)Math.Min(Mathf.Lerp(1, 52, num * num), 51)).ToString();
                    if (nb.Length < 2) nb = "0" + nb;
                    Plugin.Log("image name: " + "silksong_memory" + nb);
                    sLeaser.sprites[0].SetElementByName("silksong_memory" + nb);

                    sLeaser.sprites[0].SetPosition(center.x, center.y);
                    sLeaser.sprites[0].scale = Mathf.Lerp(1.5f, (rCam.sSize.y / 144f) * 1.15f, num * num * num);
                    if (growTicks >= totalTicks - 60)
                    {
                        sLeaser.sprites[0].alpha = 1f - Mathf.InverseLerp(totalTicks - 60 + 30, totalTicks, growTicks);
                    }
                }

                //if (gowTicks < 225)
                //{
                //    sLeaser.sprites[0].scale = Mathf.Lerp(0.01f, 0.05f, (float)gowTicks / 225f);
                //}
                //else
                //{
                //    sLeaser.sprites[0].scale = Mathf.Lerp(0.05f, 4f, Mathf.InverseLerp(225f, (float)totalTicks, (float)gowTicks));

                //}
                //UnityEngine.Debug.Log("scale: " + sLeaser.sprites[0].scale);
                //sLeaser.sprites[0].alpha = Math.Min(sLeaser.sprites[0].alpha + timeStacker / 600f * 3f, 1f);
            }
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("HUD");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

    }
}

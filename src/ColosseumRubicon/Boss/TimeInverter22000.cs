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
        public int gowTicks;
        public int totalTicks = 1240;
        public TimeInverter22000(Room room)
        {
            this.room = room;
            fadeOut = new FadeOut(room, Color.black, 60 * 10, false);
            room.AddObject(fadeOut);
            Manager.DisablePauseMenu = true;
            this.room.game.manager.musicPlayer.RainRequestStopSong();
        }

        public override void Update(bool eu)
        {
            base.Update(eu);


            if (fadeOut != null && fadeOut.IsDoneFading() && !this.slatedForDeletetion && !active)
            {
                active = true;
                this.room.game.manager.musicPlayer.GameRequestsSong(new MusicEvent()
                {
                    prio = 100,
                    songName = "22 Minutes",
                    cyclesRest = 0,
                    loop = false,
                });
            }

            if (active)
            {
                gowTicks++;
                UnityEngine.Debug.Log("gowTicks: " + gowTicks);
                foreach (var abstractPlayer in room.game.Players)
                {
                    (abstractPlayer.realizedCreature as Player).SuperHardSetPosition(new Vector2(700, 1200));
                }

                if (gowTicks >= totalTicks)
                {
                    Manager.DisablePauseMenu = false;
                    RainWorldGame.ForceSaveNewDenLocation(this.room.game, "CR_THEEND", true);
                    this.Destroy();
                    this.room.game.rainWorld.processManager.RequestMainProcessSwitch(HKMainMenu.HKMainMenuID);
                }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("illustrations/memories/memory01", true);
            sLeaser.sprites[0].alpha = 0f;
            AddToContainer(sLeaser, rCam, null);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].SetPosition(rCam.sSize / 2);
            sLeaser.sprites[0].isVisible = !this.slatedForDeletetion;

            if (active)
            {
                if (gowTicks < 225)
                {
                    sLeaser.sprites[0].scale = Mathf.Lerp(0.01f, 0.05f, (float)gowTicks / 225f);
                }
                else
                {
                    sLeaser.sprites[0].scale = Mathf.Lerp(0.05f, 4f, Mathf.InverseLerp(225f, (float)totalTicks, (float)gowTicks));

                }
                UnityEngine.Debug.Log("scale: " + sLeaser.sprites[0].scale);
                sLeaser.sprites[0].alpha = Math.Min(sLeaser.sprites[0].alpha + timeStacker / 600f * 3f, 1f);
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

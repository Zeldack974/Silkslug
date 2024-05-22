using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;
using UnityEngine;
using Kittehface.Framework20;

namespace Silkslug.ColosseumRubicon.Boss
{
    public static class BossHooks
    {
        public static FSprite cloudSprite;

        public static void Register()
        {
            On.Player.Update += Player_Update;
            On.Player.Die += Player_Die;
            On.AboveCloudsView.CloseCloud.DrawSprites += CloseCloud_DrawSprites;
        }

        private static void CloseCloud_DrawSprites(On.AboveCloudsView.CloseCloud.orig_DrawSprites orig, AboveCloudsView.CloseCloud self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            DemonEye.maxCloudIndex = Math.Max(DemonEye.maxCloudIndex, sLeaser.sprites.Last()._container.GetChildIndex(sLeaser.sprites.Last()));
            orig(self, sLeaser, rCam, timeStacker, camPos);
        }


        public static int iFrame = 0;
        public static bool TakeDamage(this Player player, int damage = 1)
        {
            if (player.Karma > 0 || player.KarmaIsReinforced || iFrame > 0)
            {
                if (iFrame > 0)
                {
                    return true;
                }
                if (player.KarmaIsReinforced)
                {
                    (player.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = false;
                }
                else
                {
                    (player.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = Math.Max(player.Karma - damage, 0);
                }
                player.room.game.cameras[0].hud.karmaMeter.reinforceAnimation = 130;
                player.room.AddObject(new ShockWave(player.mainBodyChunk.pos, 50f, 0.35f, 15, true));
                if (damage > 1)
                {
                    player.room.PlaySound(Sounds.DOUBLE_DAMAGE, player.mainBodyChunk, false, 1f, 1f);
                }
                else
                {
                    player.room.PlaySound(Sounds.DAMAGE, player.mainBodyChunk, false, 1f, 1f);
                    player.mainBodyChunk.vel = new Vector2(0, 15f);
                }
                iFrame = 78;

                return player.Karma > 0;
            }
            return false;
        }

        private static void Player_Die(On.Player.orig_Die orig, Player self)
        {
            BossManager bossManager = BossManager.Instance;
            //ConsoleWrite($"{!self.slatedForDeletetion}, {self.room.abstractRoom.name}, {bossManager}, {((bossManager != null) ? bossManager.initialize : false)}");

            if (!self.slatedForDeletetion && self.room.abstractRoom.name == "CR_BOSS" && bossManager != null && bossManager.initialize)
            {
                if (self.Karma > 0 || self.KarmaIsReinforced)
                {
                    if (self.TakeDamage(1))
                    {
                        return;
                    }
                }
            }
            orig(self);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            iFrame = Math.Max(0, iFrame - 1);
            BossManager bossManager = BossManager.Instance;
            UnityEngine.Debug.Log($"pos: {self.mainBodyChunk.pos}");

            if (!self.slatedForDeletetion && self.room.abstractRoom.name == "CR_BOSS" && bossManager != null && bossManager.initialize && !self.dead)
            {
                self.stun = 0;

                if (bossManager.playerSpear != null && self.spearOnBack.spear == null)
                {
                    self.SlugcatGrab(bossManager.playerSpear, 0);
                }

                float minY = self.room.RoomRect.bottom;
                float maxY = self.room.RoomRect.bottom + ((self.room.RoomRect.top - self.room.RoomRect.bottom) / 2) - 10;

                if (bossManager.upperArena)
                {
                    minY = maxY + 10;
                    maxY = self.room.RoomRect.top;
                }

                if (self.mainBodyChunk.pos.y < minY)
                {
                    self.SuperHardSetPosition(new Vector2(650, 145));
                    if (!self.TakeDamage(1))
                    {
                        self.Die();
                    }
                }
                //else if (self.mainBodyChunk.pos.y > maxY)
                //{
                //    self.mainBodyChunk.vel.y = 0;
                //}
                //self.mainBodyChunk.pos.y = Math.Min(self.mainBodyChunk.pos.y, maxY);


            }
            orig(self, eu);
        }
    }
}

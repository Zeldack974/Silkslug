using BepInEx;
using UnityEngine;
using Random = UnityEngine.Random;
using SlugBase.Features;
using MoreSlugcats;
using RWCustom;
using static SlugBase.Features.FeatureTypes;
using static Silkslug.MyDevConsole;
using static Silkslug.Shaw;
using Silkslug;
using System.Collections.Generic;

namespace Slikslug
{
    [BepInPlugin(MOD_ID, "Silkslug", "1.0.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "zeldak974.slikslug";

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("slikslug/super_jump");
        public static readonly PlayerFeature<bool> SpearAbilites = PlayerBool("slikslug/spear_abilities");

        public static readonly SlugcatStats.Name ShawName = new SlugcatStats.Name("Shaw", false);

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            try { RegisterCommands(); } catch { }

            // Put your custom hooks here!
            On.Player.Jump += Player_Jump;
            On.Player.ThrowObject += Player_ThrowObject;
            On.Player.ThrowToGetFree += Player_ThrowToGetFree;
            On.Player.ThrownSpear += Player_ThrownSpear;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.ReleaseObject += Player_ReleaseObject;
            On.Player.Update += Player_Update;
            On.Player.Die += Player_Die;
            On.Player.SlugcatGrab += Player_SlugcatGrab;
            On.Player.SpearOnBack.Update += SpearOnBack_Update;

            On.Spear.DrawSprites += Spear_DrawSprites;
            On.Spear.Update += Spear_Update;

        }

        private void SpearOnBack_Update(On.Player.SpearOnBack.orig_Update orig, Player.SpearOnBack self, bool eu)
        {
            if (SpearAbilites.TryGet(self.owner, out bool customAbilities) && customAbilities)
            {
                return;
            }
            orig(self, eu);
        }

        private void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
        {
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && obj is Spear)
            {
                bool shouldPutToBack = obj != self.spearOnBack.spear;
                if (!self.CanPutSpearToBack)
                {
                    self.spearOnBack.DropSpear();
                }
                if (shouldPutToBack)
                {
                    self.spearOnBack.SpearToBack(obj as Spear);
                }
            }
            else
            {
                orig(self, obj, graspUsed);
            }
        }

        private void Player_Die(On.Player.orig_Die orig, Player self)
        {
            bool wasDead = self.dead;
            orig(self);
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && !wasDead && self.dead)
            {
                if (Random.value <= 0.2)
                {
                    self.room.PlaySound(Sounds.HORNET_GIT_GUD, self.firstChunk);
                }
                else
                {
                    self.room.PlaySound(Sounds.HORNET_DEATH, self.firstChunk);
                }
            }
        }

        private void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
        {
            orig(self, eu);
            if (invisibleSpear.ContainsKey(self))
            {
                if (invisibleSpear[self] > 0)
                {
                    invisibleSpear[self]--;
                }
                else
                {
                    invisibleSpear.Remove(self);
                }
            }
        }

        private void Spear_DrawSprites(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            bool isHidden = invisibleSpear.ContainsKey(self) && invisibleSpear[self] > 0;
            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.isVisible = !isHidden;
            }
        }

        private void Player_ReleaseObject(On.Player.orig_ReleaseObject orig, Player self, int grasp, bool eu)
        {
            if (!self.TryGetShaw(out ShawData shawData) || (shawData.dashFrame <= 0 && shawData.dashCooldown <= 0 && shawData.dashDropLock <= 0))
            {
                orig(self, grasp, eu);
            }
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            //if (self.slugcatStats.name.ToString() == "White" && Random.value >= 0.75)
            //{
            //    self.room.AddObject(new Slash(self.room, self, null, new Vector2(1, 0), 100f, 1f, 0f));
            //}
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && self.TryGetShaw(out ShawData shawData))
            {
                shawData.lastThrowDir = self.ThrowDirection;
                if (shawData.dashFrame > 0)
                {
                    self.firstChunk.vel = shawData.throwDir * 20f;
                    self.animation = Player.AnimationIndex.ZeroGSwim;
                    self.bodyMode = Player.BodyModeIndex.ZeroG;
                    self.airFriction = 0.7f;
                    self.wantToJump = 0;
                    self.customPlayerGravity = 0;
                    self.input[0].x = 0;
                    self.input[0].y = 0;

                    shawData.dashFrame--;
                    if (shawData.dashFrame <= 0 || self.canJump > 0 || self.canWallJump > 0)
                    {
                        shawData.dashFrame = 0;
                        shawData.attackCooldown = DashSlash.lifeTime + 1;
                        self.firstChunk.vel = new Vector2(0, 0);
                        self.animation = Player.AnimationIndex.Flip;
                    }

                }

                if (shawData.dashDropLock > 0)
                {
                    shawData.dashDropLock--;
                }
            }
        }

        private void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && self.TryGetShaw( out ShawData shawData ) && self.spearOnBack.HasASpear)
            {
                bool haveItem = false;
                foreach (Creature.Grasp grasp in self.grasps)
                {
                    if (grasp != null)
                    {
                        haveItem = true;
                    }
                }

                float damageFac = 1f;
                Spear spear = self.spearOnBack.spear;

                if (spear.abstractSpear.hue != 0f)
                {
                    damageFac = 3f;
                }

                if (!haveItem)
                {
                    IntVector2 intVector = new IntVector2(self.ThrowDirection, 0);
                    bool flag = self.input[0].y < 0;
                    if (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value)
                    {
                        flag = (self.input[0].y != 0);
                    }
                    if (self.animation == Player.AnimationIndex.Flip && flag && self.input[0].x == 0)
                    {
                        intVector = new IntVector2(0, (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value) ? self.input[0].y : -1);
                    }
                    if (ModManager.MMF && self.bodyMode == Player.BodyModeIndex.ZeroG && MMF.cfgUpwardsSpearThrow.Value)
                    {
                        int y = self.input[0].y;
                        if (y != 0)
                        {
                            intVector = new IntVector2(0, y);
                        }
                        else
                        {
                            intVector = new IntVector2(self.ThrowDirection, 0);
                        }
                    }

                    if (shawData.attackCooldown > 0)
                    {
                        shawData.attackCooldown--;
                    }
                    else if (self.input[0].thrw && !self.input[1].thrw)
                    {
                        shawData.chargeSlashCounter = 0;
                        self.room.PlaySound(Sounds.NAIL, self.firstChunk);
                        if (Random.value < 0.5f)
                        {
                            self.room.PlaySound(Sounds.HORNET_ATTACK, self.firstChunk);
                        }
                        self.room.AddObject(new Slash(self.room, self, spear, intVector.ToVector2(), 100f, 1f, 0.25f * damageFac));
                        spear.SetInvisible(10);

                        self.firstChunk.vel += intVector.ToVector2() * 4f;
                        shawData.attackCooldown = 10;
                    }

                    if (self.input[0].thrw && self.input[1].thrw)
                    {
                        shawData.chargeSlashCounter++;
                        if (shawData.chargeSlashCounter > chargeSlashTime)
                        {
                            if (shawData.chargeLoopSound == null)
                            {
                                shawData.chargeLoopSound = self.room.PlaySound(Sounds.HERO_NAIL_ART_CHARGE_LOOP, self.firstChunk, true, 1f, 1f);
                            }
                            self.room.AddObject(new ExplosionSpikes(self.room, self.firstChunk.pos, 16, 25f, 1f, 7f, 40f, Color.white));
                        }
                        else if (shawData.chargeSlashCounter > 12)
                        {
                            if (shawData.chargeInitSound == null)
                            {
                                shawData.chargeInitSound = self.room.PlaySound(Sounds.HERO_NAIL_ART_CHARGE_INITIATE, self.firstChunk);
                            }
                        }
                    }
                    else if (!self.input[0].thrw && self.input[1].thrw)
                    {
                        if (shawData.chargeSlashCounter > chargeSlashTime)
                        {
                            self.room.PlaySound(Sounds.HERO_NAIL_ART_GREAT_SLASH, self.firstChunk);
                            self.room.PlaySound(Sounds.HORNET_GREAT_SLASH, self.firstChunk);
                            self.room.AddObject(new Slash(self.room, self, spear, intVector.ToVector2(), 150f, 1f, 0.75f * damageFac));
                            spear.SetInvisible(10);
                            shawData.chargeSlashCounter = 0;
                        }
                        else
                        {
                            shawData.chargeSlashCounter = 0;
                        }
                    }
                    else
                    {
                        shawData.chargeSlashCounter = 0;
                    }

                    if (shawData.chargeSlashCounter == 0)
                    {
                        if (shawData.chargeLoopSound != null)
                        {
                            shawData.chargeLoopSound.Destroy();
                            shawData.chargeLoopSound = null;
                        }
                        if (shawData.chargeInitSound != null)
                        {
                            shawData.chargeInitSound.Destroy();
                            shawData.chargeInitSound = null;
                        }
                    }
                }

                if (shawData.dashCooldown > 0)
                {
                    shawData.dashCooldown--;
                }
                else if (self.wantToJump > 0 && self.input[0].pckp)
                {
                    Vector2 dir = new Vector2((float)self.input[0].x, (float)self.input[0].y).normalized;
                    if (dir.magnitude < 1)
                    {
                        dir = new Vector2(shawData.lastThrowDir, 0);
                    }
                    shawData.throwDir = dir;

                    shawData.dashFrame = dashTotalFrame;
                    shawData.chargeSlashCounter = 0;
                    shawData.dashCooldown = 40;

                    List<SoundID> sounds = new()
                    {
                        Sounds.HORNET_FIGHT_YELL_06,
                        Sounds.HORNET_FIGHT_YELL_08,
                        Sounds.HORNET_FIGHT_YELL_09
                    };

                    self.room.PlaySound(sounds[Random.Range(0, sounds.Count)], self.firstChunk);
                    self.room.PlaySound(Sounds.HORNET_DASH, self.firstChunk);
                    self.room.AddObject(new DashSlash(self.room, self, spear, shawData.throwDir, 100f, 1f, 0.25f * damageFac));
                    spear.SetInvisible(dashTotalFrame + DashSlash.lifeTime + 6);
                }
                if (self.pickUpCandidate != null && self.input[0].pckp && !self.input[1].pckp && shawData.dashDropLock <= 0 && self.pickUpCandidate is Spear && ((self.grasps[0] != null && self.Grabability(self.grasps[0].grabbed) >= Player.ObjectGrabability.BigOneHand) || (self.grasps[1] != null && self.Grabability(self.grasps[1].grabbed) >= Player.ObjectGrabability.BigOneHand) || (self.grasps[0] != null && self.grasps[1] != null)))
                {
                    if (!self.CanPutSpearToBack)
                    {
                        self.spearOnBack.DropSpear();
                    }
                    self.spearOnBack.SpearToBack(self.pickUpCandidate as Spear);
                }
            }


            orig(self, eu);
        }

        private void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && self.TryGetShaw(out ShawData shawData) && (self.grasps[grasp] != null && self.grasps[grasp].grabbed is Spear))
            {
                if (shawData.chargeSlashCounter < 50)
                {
                    return;
                }
                else
                {
                    shawData.chargeSlashCounter = 0;
                }
            }
            orig(self, grasp, eu);
        }

        private void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
        {
            orig(self, spear);
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities)
            {
                spear.spearDamageBonus = 10f;
                if (self.canJump != 0)
                {
                    self.animation = Player.AnimationIndex.Roll;
                }
                else
                {
                    self.animation = Player.AnimationIndex.Flip;
                }
                if ((self.room != null && self.room.gravity == 0f) || Mathf.Abs(spear.firstChunk.vel.x) < 1f)
                {
                    self.firstChunk.vel += spear.firstChunk.vel.normalized * 9f * 2f;
                }
                else
                {
                    self.rollDirection = (int)Mathf.Sign(spear.firstChunk.vel.x);
                    self.rollCounter = 0;
                    BodyChunk firstChunk3 = self.firstChunk;
                    firstChunk3.vel.x += Mathf.Sign(spear.firstChunk.vel.x) * 9f;
                }
            }
        }

        private void Player_ThrowToGetFree(On.Player.orig_ThrowToGetFree orig, Player self, bool eu)
        {
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && self.dangerGrasp != null && !self.dead && self.spearOnBack.HasASpear)
            {
                if (self.dangerGrasp == null) return;

                self.spearOnBack.DropSpear();
                for (int j = 0; j < self.dangerGrasp.grabber.grasps.Length; j++)
                {
                    self.dangerGrasp.grabber.ReleaseGrasp(j);
                }
                self.dangerGrasp.grabber.Stun(40);

                self.room.AddObject(new Slash(self.room, self, null, new Vector2(1, 0), 100f, 1f, 0.25f));
                self.room.PlaySound(Sounds.NAIL, self.firstChunk);
                self.room.PlaySound(Sounds.HERRO_PARRY, self.firstChunk);
            }
            orig(self, eu);
        }

        // Implement SuperJump
        private void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);

            if (SuperJump.TryGet(self, out var power))
            {
                self.jumpBoost *= 1f + power;
            }

            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && self.TryGetShaw(out ShawData shawData))
            {
                shawData.dashDropLock = 20;
            }
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadImage("atlas/slash");
            Futile.atlasManager.LoadImage("atlas/longslash");
            Sounds.Initialize();
        }
    }
}
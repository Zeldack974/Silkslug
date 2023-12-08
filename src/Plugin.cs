using System;
using System.Runtime.CompilerServices;
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
using UnityEngine.Experimental.Rendering;

namespace Slikslug
{
    [BepInPlugin(MOD_ID, "Slugcat Template", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "zeldak974.slikslug";

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("slikslug/super_jump");
        public static readonly PlayerFeature<bool> SpearAbilites = PlayerBool("slikslug/spear_abilities");
        public static readonly PlayerFeature<bool> SuperBlock = PlayerBool("slikslug/superblock");

        public static readonly SlugcatStats.Name ShawName = new SlugcatStats.Name("Shaw", false);

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            try { RegisterCommands(); } catch { }

            // Put your custom hooks here!
            On.Player.Jump += Player_Jump;
            On.Player.ctor += Player_ctor;
            On.Player.ThrowObject += Player_ThrowObject;
            On.Player.ThrowToGetFree += Player_ThrowToGetFree;
            On.Player.ThrownSpear += Player_ThrownSpear;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.ReleaseObject += Player_ReleaseObject;

            On.Spear.DrawSprites += Spear_DrawSprites;
            On.Spear.Update += Spear_Update;

            On.Spear.HitSomething += Spear_HitSomething;
            On.Player.CanBeGrabbed += Player_CanBeGrabbed;
            On.Player.Update += Player_Update;
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
            if (invisibleSpear.ContainsKey(self) && invisibleSpear[self] > 0)
            {
                sLeaser.sprites[0].isVisible = false;
            }
            else
            {
                sLeaser.sprites[0].isVisible = true;
            }
        }

        private void Player_ReleaseObject(On.Player.orig_ReleaseObject orig, Player self, int grasp, bool eu)
        {
            //if (self.TryGetShaw(out ShawData sd))
            //{
            //    Debug.Log($"release check: {sd.dashFrame}, {sd.attackCooldown}, {self.wantToJump}");
            //}
            if (!self.TryGetShaw(out ShawData shawData) || (shawData.dashFrame <= 0 && shawData.dashCooldown <= 0 && shawData.dashDropLock <= 0))
            {
                orig(self, grasp, eu);
            }
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            //if (self.slugcatStats.name.ToString() == "White")
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
            }
        }

        private bool Player_CanBeGrabbed(On.Player.orig_CanBeGrabbed orig, Player self, Creature grabber)
        {
            if (!self.dead && SuperBlock.TryGet(self, out bool block) && block)
            {
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i] != null)
                    {
                        Spear spear = self.grasps[i].grabbed as Spear;
                        if (spear != null)
                        {
                            for (int n = 17; n > 0; n--)
                            {
                                spear.room.AddObject(new Spark(spear.firstChunk.pos, Custom.RNV() * 2, Color.white, null, 10, 20));
                            }

                            spear.Destroy();
                            self.room.PlaySound(Sounds.hero_parry, self.firstChunk);

                            grabber.Stun(40);
                            Vector2 vector = (self.firstChunk.pos - grabber.firstChunk.pos).normalized * 9f * 3f;
                            self.firstChunk.vel += vector;

                            return false;
                        }
                    }
                }
            }

            return orig(self, grabber);
        }

        private bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj != null && result.obj is Creature && result.obj is Player)
            {
                Player player = (Player)result.obj;
                if (SuperBlock.TryGet(player, out bool block) && block)
                {
                    for (int i = 0; i < player.grasps.Length; i++)
                    {
                        if (player.grasps[i] != null)
                        {
                            Spear spear = player.grasps[i].grabbed as Spear;
                            if (spear != null)
                            {
                                for (int n = 17; n > 0; n--)
                                {
                                    spear.room.AddObject(new Spark(spear.firstChunk.pos, Custom.RNV() * 2, Color.white, null, 10, 20));
                                }

                                spear.Destroy();

                                player.firstChunk.vel += self.firstChunk.vel;

                                self.room.PlaySound(Sounds.hero_parry, self.firstChunk);
                                self.vibrate = 20;
                                self.ChangeMode(Weapon.Mode.Free);
                                self.firstChunk.vel = self.firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * self.firstChunk.vel.magnitude;
                                self.SetRandomSpin();

                                return false;
                            }
                        }
                    }
                }
            }
            return orig(self, result, eu);
        }

        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.SlugCatClass == ShawName)
            {

            }
        }

        private void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && self.TryGetShaw( out ShawData shawData ))
            {
                for (int grasp = 0; grasp < 2; grasp++)
                {
                    //Debug.Log($"[{grasp}]: {self.grasps[grasp]} {self.grasps[grasp] == null}");
                    if (shawData.dashFrame <= 0 && self.grasps[grasp] != null && self.grasps[grasp].grabbed is Spear)
                    {
                        float damageFac = 1f;
                        Spear spear = self.grasps[grasp].grabbed as Spear;
                        if (spear.abstractSpear.hue != 0f)
                        {
                            damageFac = 3f;
                        }

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
                        else if (self.input[0].thrw && !self.input[1].thrw && (grasp == 0 || self.grasps[0] == null))
                        {
                            shawData.chargeSlashCounter = 0;
                            self.room.PlaySound(Sounds.nail, self.firstChunk);
                            if (Random.value < 0.5f)
                            {
                                self.room.PlaySound(Sounds.Hornet_Attack, self.firstChunk);
                            }
                            self.room.AddObject(new Slash(self.room, self, spear, intVector.ToVector2(), 100f, 1f, 0.25f * damageFac));
                            spear.setInvisible(10);

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
                                    shawData.chargeLoopSound = self.room.PlaySound(Sounds.hero_nail_art_charge_loop, self.firstChunk, true, 1f, 1f);
                                }
                                self.room.AddObject(new ExplosionSpikes(self.room, self.firstChunk.pos, 16, 25f, 1f, 7f, 40f, Color.white));
                            }
                            else if (shawData.chargeSlashCounter > 12)
                            {
                                if (shawData.chargeInitSound == null)
                                {
                                    shawData.chargeInitSound = self.room.PlaySound(Sounds.hero_nail_art_charge_initiate, self.firstChunk);
                                }
                            }
                        }
                        else if (!self.input[0].thrw && self.input[1].thrw)
                        {
                            if (shawData.chargeSlashCounter > chargeSlashTime)
                            {
                                //self.ThrowObject(grasp, eu);
                                //self.room.AddObject(new ShockWave(self.firstChunk.pos + new Vector2(self.ThrowDirection, 0) * 10f + new Vector2(0f, 4f), 40f, 0.185f, 30, false));
                                self.room.PlaySound(Sounds.hero_nail_art_great_slash, self.firstChunk);
                                self.room.PlaySound(Sounds.Hornet_Great_Slash, self.firstChunk);
                                self.room.AddObject(new Slash(self.room, self, spear, intVector.ToVector2(), 150f, 1f, 0.75f * damageFac));
                                spear.setInvisible(10);
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

                            List<SoundID> sounds = new List<SoundID>();
                            sounds.Add(Sounds.Hornet_Fight_Yell_06);
                            sounds.Add(Sounds.Hornet_Fight_Yell_08);
                            sounds.Add(Sounds.Hornet_Fight_Yell_09);

                            self.room.PlaySound(sounds[Random.Range(0, sounds.Count)], self.firstChunk);
                            self.room.PlaySound(Sounds.hornet_dash, self.firstChunk);
                            self.room.AddObject(new DashSlash(self.room, self, spear, shawData.throwDir, 100f, 1f, 0.25f * damageFac));
                            spear.setInvisible(dashTotalFrame + DashSlash.lifeTime + 6);
                        }

                        if (shawData.dashDropLock > 0)
                        {
                            shawData.dashDropLock--;
                        }

                        break;
                    }
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
                    firstChunk3.vel.x = firstChunk3.vel.x + Mathf.Sign(spear.firstChunk.vel.x) * 9f;
                }
            }
        }

        private void Player_ThrowToGetFree(On.Player.orig_ThrowToGetFree orig, Player self, bool eu)
        {
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && self.dangerGrasp != null)
            {
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i] != null && self.grasps[i].grabbed is Spear)
                    {
                        if (self.dangerGrasp != null)
                        {
                            self.ReleaseGrasp(i);
                            for (int j = 0; j < self.dangerGrasp.grabber.grasps.Length; j++)
                            {
                                self.dangerGrasp.grabber.ReleaseGrasp(j);
                            }
                            self.dangerGrasp.grabber.Stun(40);

                            self.room.AddObject(new Slash(self.room, self, null, new Vector2(1, 0), 100f, 1f, 0.25f));
                            self.room.PlaySound(Sounds.nail, self.firstChunk);
                            self.room.PlaySound(Sounds.hero_parry, self.firstChunk);
                            return;
                        }
                        break;
                    }
                }
                return;
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
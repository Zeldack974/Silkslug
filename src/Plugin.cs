global using static Silkslug.MyDevConsole;
global using static Silkslug.Plugin;
//global using Debug = UnityEngine.Debug;
using BepInEx;
using UnityEngine;
using Random = UnityEngine.Random;
using SlugBase.Features;
using MoreSlugcats;
using RWCustom;
using static SlugBase.Features.FeatureTypes;
using static Silkslug.Shaw;
using System.Collections.Generic;
using Silkslug.ColosseumRubicon;
using BepInEx.Logging;
using Silkslug.ColosseumRubicon.Boss;
using Fisobs.Core;
using System.Xml.Schema;
using DressMySlugcat;
using System;
using System.IO;

namespace Silkslug
{
    [BepInPlugin(MOD_ID, "Silkslug", "1.0.4")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "zeldak974.silkslug";
        public static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("SilkSlug");

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("silkslug/super_jump");
        public static readonly PlayerFeature<bool> SpearAbilites = PlayerBool("silkslug/spear_abilities");

        public static readonly SlugcatStats.Name ShawName = new SlugcatStats.Name("Shaw");

        public static bool Initialized = false;

        public static void Log(object msg)
        {
            log.LogInfo(msg);
        }

        public static void LogError(object msg)
        {
            log.LogError(msg);
        }


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            try { RegisterCommands(); } catch { }
            ColosseumRubicon.Manager.OnEnable();
            BossHooks.Register();
            DevtoolObjects.Register();

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
            On.Player.Stun += Player_Stun;

            On.Spear.DrawSprites += Spear_DrawSprites;
            On.Spear.Update += Spear_Update;

            On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
            On.ProjectionScreen.AddImage_string += ProjectionScreen_AddImage_string;
            On.ProjectionScreen.AddImage_List1_int += ProjectionScreen_AddImage_List1_int;

            On.Menu.SlugcatSelectMenu.UpdateStartButtonText += SlugcatSelectMenu_UpdateStartButtonText;
            On.Menu.SlugcatSelectMenu.ContinueStartedGame += SlugcatSelectMenu_ContinueStartedGame;

            On.SlugcatStats.IsSlugcatFromMSC += SlugcatStats_IsSlugcatFromMSC;
            On.Music.MusicPlayer.GameRequestsSong += MusicPlayer_GameRequestsSong;

            On.Menu.MainMenu.ctor += MainMenu_ctor;
            On.RainWorldGame.BeatGameMode += RainWorldGame_BeatGameMode;
            On.MultiplayerUnlocks.IsLevelUnlocked += MultiplayerUnlocks_IsLevelUnlocked;

        }

        private bool MultiplayerUnlocks_IsLevelUnlocked(On.MultiplayerUnlocks.orig_IsLevelUnlocked orig, MultiplayerUnlocks self, string levelName)
        {
            return orig(self, levelName.ToLower());
        }


        private void RainWorldGame_BeatGameMode(On.RainWorldGame.orig_BeatGameMode orig, RainWorldGame game, bool standardVoidSea)
        {
            if (game.GetStorySession.saveState.saveStateNumber == ShawName)
            {
                if (game.rainWorld.progression.miscProgressionData.SetTokenCollected(new MultiplayerUnlocks.LevelUnlockID("CRHell")))
                {
                    game.manager.specialUnlockText = game.rainWorld.inGameTranslator.Translate("Colosseum Rubicon content is now available in Arena Mode.");
                }
            }
            orig(game, standardVoidSea);
        }

        private void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        {
            orig(self, manager, showRegionSpecificBkg);
            if (!Initialized && SkinApplyer.IsSlornetEnabled)
            {
                SkinApplyer.SetDefaults();
            }

            Initialized = true;
        }

        private void SlugcatSelectMenu_ContinueStartedGame(On.Menu.SlugcatSelectMenu.orig_ContinueStartedGame orig, Menu.SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter)
        {
            if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == ShawName && (bool)self.saveGameData[ShawName]?.ascended)
            {
                self.redSaveState = self.manager.rainWorld.progression.GetOrInitiateSaveState(ShawName, null, self.manager.menuSetup, false);
                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
                self.PlaySound(SoundID.MENU_Switch_Page_Out);
                return;
            }
            orig(self, storyGameCharacter);
        }

        private void SlugcatSelectMenu_UpdateStartButtonText(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, Menu.SlugcatSelectMenu self)
        {
            orig(self);
            if (self.startButton.menuLabel.text == self.Translate("CONTINUE") && self.slugcatPages[self.slugcatPageIndex].slugcatNumber == ShawName && (bool)self.saveGameData[ShawName]?.ascended)
            {
                self.startButton.menuLabel.text = self.Translate("STATISTICS");
                return;
            }
        }

        private void MusicPlayer_GameRequestsSong(On.Music.MusicPlayer.orig_GameRequestsSong orig, Music.MusicPlayer self, MusicEvent musicEvent)
        {
            Plugin.Log("MusicPlayer_GameRequestsSong");
            if (self.song != null)
                Plugin.Log("already playing " + self.song.name);
            if (!self.manager.rainWorld.setup.playMusic)
                Plugin.Log("playMusic is false");
            if (self.song != null && (self.song.priority >= musicEvent.prio))
                Plugin.Log($"song priority to low: {musicEvent.prio} < {self.song.priority}");

            if (self.manager.currentMainLoop.ID == ProcessManager.ProcessID.Game && (self.manager.currentMainLoop as RainWorldGame).session is StoryGameSession)
            {
                SaveState saveState2 = ((self.manager.currentMainLoop as RainWorldGame).session as StoryGameSession).saveState;
                for (int j = 0; j < saveState2.deathPersistentSaveData.songsPlayRecords.Count; j++)
                {
                    if (saveState2.deathPersistentSaveData.songsPlayRecords[j].songName == musicEvent.songName)
                        Plugin.Log($"no playing saved {saveState2.deathPersistentSaveData.songsPlayRecords[j].songName} == {musicEvent.songName}");
                }
            }

            orig(self, musicEvent);
            Plugin.Log("Playing song " + (self.song != null));
        }

        private bool SlugcatStats_IsSlugcatFromMSC(On.SlugcatStats.orig_IsSlugcatFromMSC orig, SlugcatStats.Name i)
        {
            return i.value == "Shaw" || orig(i);
        }

        private ProjectedImage ProjectionScreen_AddImage_List1_int(On.ProjectionScreen.orig_AddImage_List1_int orig, ProjectionScreen self, List<string> names, int cycleTime)
        {
            List<string> transformedNames = names.ConvertAll(name =>
            {
                if (self.room.game.GetStorySession.saveState.saveStateNumber == ShawName && name.ToLower().StartsWith("aiimg3")) return $"Shaw-{name}";

                return name;
            });

            return orig(self, transformedNames, cycleTime);
        }

        private ProjectedImage ProjectionScreen_AddImage_string(On.ProjectionScreen.orig_AddImage_string orig, ProjectionScreen self, string name)
        {
            if (self.room.game.GetStorySession.saveState.saveStateNumber == ShawName && name.ToLower().StartsWith("aiimg2"))
            {
                return orig(self, "Shaw-" + name);
            }
            return orig(self, name);
        }

        private void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
        {
            if (self.id == Conversation.ID.Pebbles_White && self.owner.oracle.room.game.GetStorySession.saveState.saveStateNumber == ShawName)
            {
                if (!self.owner.playerEnteredWithMark)
                {
                    self.events.Add(new Conversation.TextEvent(self, 0, ".  .  .", 0));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("...is this reaching you?"), 0));
                    self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 4));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("No?"), 0));
                    self.events.Add(new SSOracleBehavior.PebblesConversation.SpecialEvent(self, 0, "karma"));
                    //self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("It should work now."), 0));
                    self.owner.playerEnteredWithMark = true;
                    return;
                }
                else
                {
                    self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 70));
                    self.events.Add(new Conversation.TextEvent(self, 0, ". . .", 0));
                    self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 70));
                    self.events.Add(new Conversation.TextEvent(self, 0, ".  .  .", 0));
                    self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 70));

                }
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("A little insect, on the floor of my chamber. I think I know what you are looking for.\r\n"), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You're stuck in a cycle, a repeating pattern. You want a way out."), 0));
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 20));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("No?"), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("It speak now?"), 0));
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 20));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("What?<LINE>You seek to defeat the most powerful beings of this world?"), 0));
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 60));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("How boring."), 0));
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 10));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I don't need a killer right now, and I don't want a \"bug\" to cause more trouble in my systems."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate(".  .  ."), 0));
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 210));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I found the perfect place for you!<LINE>A place for all bloodthirsty murderers of yours."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Our creators called it by several names, but I personally prefer HELL."), 0));
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 10));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Go to the west past the Farm Arrays."), 0));
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 20));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("What?<LINE>You came from there?"), 0));
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 10));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Too bad to be you."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Anyway."), 0));
                if (self.owner.playerEnteredWithMark)
                {
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Then down into the earth<LINE>where the land fissures, as deep as you can reach, where the ancients built their temples and danced their silly rituals."), 0));
                }
                else
                {
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Then down into the earth<LINE>where the land fissures, as deep as you can reach, where the ancients built their temples and danced their silly rituals.<LINE>The mark I gave you will let you through."), 0));
                }
                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, self.convBehav, 20));
                //self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("If you can't find it, don't come back.<LINE>Next time I see you here, I'll turn you into a body."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I must resume my work."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("If you can't find it, don't come back."), 0));
                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Now go."), 0));
                return;
            }
            else
            {
                orig(self);
            }
        }


        private void Player_Stun(On.Player.orig_Stun orig, Player self, int st)
        {
            orig(self, st);
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities)
            {
                if (!self.CanPutSpearToBack && self.Stunned)
                {
                    self.spearOnBack.DropSpear();
                }

            }
        }

        private void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
        {
            //ConsoleWrite("RoomSpecificScript_AddRoomSpecificScript");
            //ConsoleWrite("Add scripts for: " + room.abstractRoom.name);
            //ColosseumRubiconManager.addScript(room);
            orig(room);
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
            if (SpearAbilites.TryGet(self, out bool customAbilities) && customAbilities && !wasDead && self.dead && ShawOptions.instance.shawVoice.Value)
            {
                if (Random.value <= 0.2)
                {
                    self.room.PlaySound(Sounds.HORNET_GIT_GUD, self.firstChunk.pos);
                }
                else
                {
                    self.room.PlaySound(Sounds.HORNET_DEATH, self.firstChunk.pos);
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
                        if (Random.value < 0.5f && ShawOptions.instance.shawVoice.Value)
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
                            if (ShawOptions.instance.shawVoice.Value)
                            {
                                self.room.PlaySound(Sounds.HORNET_GREAT_SLASH, self.firstChunk);
                            }
                            self.room.AddObject(new Slash(self.room, self, spear, intVector.ToVector2(), 175f, 1f, 0.75f * damageFac));
                            spear.SetInvisible(10);
                            shawData.chargeSlashCounter = 0;                        }
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

                    if (ShawOptions.instance.shawVoice.Value)
                    {
                        self.room.PlaySound(sounds[Random.Range(0, sounds.Count)], self.firstChunk);

                    }
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

            Plugin.Log("load assets !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            Futile.atlasManager.LoadImage("assets/slash1");
            Futile.atlasManager.LoadImage("assets/slash2");
            Futile.atlasManager.LoadImage("assets/longslash");

            Futile.atlasManager.LoadImage("illustrations/rubiconintrotext");
            Futile.atlasManager.LoadImage("illustrations/hkfront");
            Futile.atlasManager.LoadImage("illustrations/hkback");

            Futile.atlasManager.LoadImage("assets/eye0");
            Futile.atlasManager.LoadImage("assets/eye1");
            Futile.atlasManager.LoadImage("assets/eye2");

            Futile.atlasManager.LoadImage("illustrations/bossintro");
            Futile.atlasManager.LoadImage("assets/hellknightbody");
            Futile.atlasManager.LoadImage("illustrations/bluedream");

            Futile.atlasManager.LoadImage("assets/sawblade");

            Futile.atlasManager.LoadImage("illustrations/hornethead");

            Futile.atlasManager.LoadAtlas("atlases/memoriesaltas");

            Futile.atlasManager.LoadImage("illustrations/shaw-aiimg2");
            Futile.atlasManager.LoadImage("illustrations/shaw-aiimg3");
            Futile.atlasManager.LoadImage("illustrations/shaw-aiimg3a");
            Futile.atlasManager.LoadImage("illustrations/shaw-aiimg3b");

            Sounds.Initialize();
            MachineConnector.SetRegisteredOI(MOD_ID, ShawOptions.instance);

            //// Achievement manager
            //Futile.atlasManager.LoadImage("illustrations/achievement_background");
            //Futile.atlasManager.LoadImage("illustrations/achievement_image");
        }
    }
}
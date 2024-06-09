
using System.IO;
using IL.Menu;
using MoreSlugcats;
using Silkslug.ColosseumRubicon.Boss;
using static Silkslug.ColosseumRubicon.Warp;

namespace Silkslug.ColosseumRubicon
{
    public static class Manager
    {
        public static bool DisablePauseMenu;
        public static void OnEnable()
        {
            Warp.OnEnable();
            RegionPopup.OnEnable();

            Hooks();
        }

        public static void ResetValues()
        {
            ArenaChallenges.currentArena = 0;
        }

        public static void ResetRooms()
        {
            foreach (RoomManager manager in RoomManager.roomManagers)
            {
                if (manager.room != null) manager.ResetRoom();
            }
        }

        public static void ResetRoom(Room room)
        {
            RoomManager.GetRoomManager(room).ResetRoom();
        }

        public static void SpawnCreatures(Room room)
        {
            RoomManager.GetRoomManager(room).spawnCreatures();
        }

        public static void PlayMusic(Room room)
        {
            //room.game.manager.musicPlayer?.FadeOutAllSongs(0.5f);
            //room.game.manager.musicPlayer?.GameRequestsSong(new MusicEvent()
            //{
            //    prio = 100,
            //    songName = "RW_Threat_Metropolis",
            //    cyclesRest = 0,
            //    stopAtDeath = true,
            //    loop = true,
            //});
        }

        public static void SetChallengeCommand(string[] args)
        {
            if (args.Length == 0) { return; }

            ArenaChallenges.currentArena = int.Parse(args[0]) - 1;
        }


        ////////////////// HOOKS //////////////////
        public static void Hooks()
        {
            On.Room.Loaded += Room_Loaded;
            On.AbstractCreature.CheckVoidseaArena += AbstractCreature_CheckVoidseaArena;
            On.AbstractCreature.setCustomFlags += AbstractCreature_setCustomFlags;
            On.AbstractCreature.OpportunityToEnterDen += AbstractCreature_OpportunityToEnterDen;
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.Player.Update += Player_Update;
            On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
            On.Player.CanBeSwallowed += Player_CanBeSwallowed;
            On.Player.SlugcatGrab += Player_SlugcatGrab;
            On.RainCycle.GetDesiredCycleLength += RainCycle_GetDesiredCycleLength;
            
        }

        private static int RainCycle_GetDesiredCycleLength(On.RainCycle.orig_GetDesiredCycleLength orig, RainCycle self)
        {
            return ((self.world.name != "CR")) ? orig(self) : 2147483647;
        }

        private static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
        {
            if (self.room.abstractRoom.name == "CR_THEEND")
            {
                return;
            }
            orig(self, obj, graspUsed);
        }

        private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
        {
            return orig(self, testObj) && (self.room.world.name != "CR");
        }

        private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {

            if (ID == HKMainMenu.HKMainMenuID)
            {
                self.currentMainLoop = new HKMainMenu(self);
            }
            orig(self, ID);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            //UnityEngine.Debug.Log("pos: " + self.mainBodyChunk.pos);
            orig(self, eu);
        }

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            DisablePauseMenu = false;
            orig(self, manager);
        }

        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            //if (((RWInput.CheckPauseButton(0, false) && !self.lastPauseButton) || Platform.systemMenuShowing) && (self.cameras[0].hud == null || self.IsArenaSession || self.cameras[0].hud.map == null || self.cameras[0].hud.map.fade < 0.1f) && (self.cameras[0].hud == null || self.IsArenaSession || !self.cameras[0].hud.textPrompt.gameOverMode) && self.manager.fadeToBlack == 0f && self.cameras[0].roomSafeForPause)
            //{
            //    ConsoleWrite("open pause menu with button");
            //}
            orig(self);
            if (self.pauseMenu != null && DisablePauseMenu)
            {
                self.pauseMenu.ShutDownProcess();
                self.pauseMenu = null;
            }
        }

        private static void AbstractCreature_OpportunityToEnterDen(On.AbstractCreature.orig_OpportunityToEnterDen orig, AbstractCreature self, WorldCoordinate den)
        {
            if (self.world.name != "CR" && self.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                orig(self, den);
            }
        }

        private static void AbstractCreature_setCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature self)
        {
            orig(self);
            if (self.world.name == "CR" && self.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                self.voidCreature = true;
                self.lavaImmune = self.voidCreature;
            }
        }

        private static void AbstractCreature_CheckVoidseaArena(On.AbstractCreature.orig_CheckVoidseaArena orig, AbstractCreature self)
        {
            orig(self);
            if (self.world.name == "CR" && self.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
            {
                self.voidCreature = true;
                self.lavaImmune = self.voidCreature;
            }
        }

        //private static void Player_Die(On.Player.orig_Die orig, Player self)
        //{
        //    bool wasDead = self.dead;
        //    orig(self);
        //    if (!wasDead && self.dead)
        //    { 
        //        ResetValues();
        //        DisablePauseMenu = false;
        //    }
        //}

        public static bool playNewLocation = false;

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            //object obj = null;
            //obj.GetHashCode();
            if (self.abstractRoom.name == "SB_E05SAINT" && self.abstractRoom.firstTimeRealized)
            {
                Plugin.Log("Add RoomSpecificScript: " + (self.game.GetStorySession.saveState.saveStateNumber == Plugin.ShawName));
                if (self.game.GetStorySession.saveState.saveStateNumber == Plugin.ShawName)
                {
                    self.AddObject(new MSCRoomSpecificScript.VS_E05WrapAround(self));
                    Plugin.Log("RoomSpecificScript added");
                }
                //self.AddObject(new RoomSpecificScript.SB_A14KarmaIncrease(self));
            }
            if (self.abstractRoom.name == "CR_START" && self.abstractRoom.firstTimeRealized)
            {
                self.AddObject(new Warp.CR_STARTWarp(self));
            }

            if (self.abstractRoom.name.ToUpper() == "HOLLOW KNIGHT TEST" || self.abstractRoom.name == "CR_THEEND" && self.abstractRoom.firstTimeRealized)
            {
                self.AddObject(new HKLevel(self));
            }
            else if (self.abstractRoom.name == "CR_REST" && self.abstractRoom.firstTimeRealized)
            {
                Plugin.Log("adding rest warp");
                self.AddObject(new CR_RESTWarp(self));

            }
            else if (self.abstractRoom.name == "CR_VOSS" && self.abstractRoom.firstTimeRealized)
            {
                //Debug.Log("adding rest warp");
                //self.AddObject(new BossManager(self));

            }
            else if (self.world.name == "CR" && RoomManager.GetRoomManager(self) == null)
            {
                Plugin.Log("adding room manager");
                self.AddObject(new RoomManager(self));
            }

            orig(self);
            //if (playNewLocation)
            //{
            //    ConsoleWrite("new location !!!");
            //    self.PlaySound(Sounds.HORNET_FIGHT_YELL_06, 0.0f, 1.0f, 1f);
            //    playNewLocation = false;
            //}
        }
    }
}

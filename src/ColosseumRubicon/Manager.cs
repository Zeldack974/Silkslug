
using MoreSlugcats;
using static Silkslug.ColosseumRubicon.Warp;

namespace Silkslug.ColosseumRubicon
{
    public static class Manager
    {
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
            ConsoleWrite("room: " +  room);
            ConsoleWrite("manager: " + RoomManager.GetRoomManager(room));
            RoomManager.GetRoomManager(room).spawnCreatures();
        }

        public static void SetChallengeCommand(string[] args)
        {
            if (args.Length == 0) { return; }

            ArenaChallenges.currentArena = int.Parse(args[0]) - 1;
        }


        ////////////////// HOOKS //////////////////
        public static void Hooks()
        {
            On.Player.Die += Player_Die;
            On.Room.Loaded += Room_Loaded;
            On.AbstractCreature.CheckVoidseaArena += AbstractCreature_CheckVoidseaArena;
            On.AbstractCreature.setCustomFlags += AbstractCreature_setCustomFlags;
            On.AbstractCreature.OpportunityToEnterDen += AbstractCreature_OpportunityToEnterDen;
            On.Player.Update += Player_Update;
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            ConsoleWrite($"player pos: {self.mainBodyChunk.pos}");
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

        private static void Player_Die(On.Player.orig_Die orig, Player self)
        {
            bool wasDead = self.dead;
            orig(self);
            if (!wasDead && self.dead)
            { 
                ResetValues();
            }
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            //object obj = null;
            //obj.GetHashCode();

            if (self.abstractRoom.name == "SB_E05SAINT" && self.abstractRoom.firstTimeRealized)
            {
                ConsoleWrite("Add RoomSpecificScript: " + (self.game.GetStorySession.saveState.saveStateNumber == Plugin.ShawName));
                if (self.game.GetStorySession.saveState.saveStateNumber == Plugin.ShawName)
                {
                    self.AddObject(new MSCRoomSpecificScript.VS_E05WrapAround(self));
                    ConsoleWrite("RoomSpecificScript added");
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
                Debug.Log("adding rest warp");
                self.AddObject(new CR_RESTWarp(self));

            }
            else if (self.world.name == "CR" && RoomManager.GetRoomManager(self) == null)
            {
                Debug.Log("adding room manager");
                self.AddObject(new RoomManager(self));
            }

            orig(self);
        }
    }
}

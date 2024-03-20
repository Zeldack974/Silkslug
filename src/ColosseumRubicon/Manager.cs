
using MoreSlugcats;

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
            RoomManager.GetRoomManager(room).spawnCreatures();
        }


        ////////////////// HOOKS //////////////////
        public static void Hooks()
        {
            On.Player.Die += Player_Die;
            On.Room.Loaded += Room_Loaded;
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
            if (self.abstractRoom.firstTimeRealized)
            {
                if (self.abstractRoom.name == "SB_E05SAINT")
                {
                    ConsoleWrite("Add RoomSpecificScript: " + (self.game.GetStorySession.saveState.saveStateNumber == Plugin.ShawName));
                    if (self.game.GetStorySession.saveState.saveStateNumber == Plugin.ShawName)
                    {
                        self.AddObject(new MSCRoomSpecificScript.VS_E05WrapAround(self));
                        ConsoleWrite("RoomSpecificScript added");
                    }
                    //self.AddObject(new RoomSpecificScript.SB_A14KarmaIncrease(self));
                }
                if (self.abstractRoom.name == "CR_START")
                {
                    self.AddObject(new Warp.CR_STARTWarp(self));
                }
                else if (self.world.name == "CR")
                {
                    self.AddObject(new RoomManager(self));
                }
            }
            orig(self);
        }
    }
}

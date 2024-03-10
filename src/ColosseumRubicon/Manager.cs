
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

        public static void Initialize()
        {

        }


        ////////////////// HOOKS //////////////////
        public static void Hooks()
        {
            On.Room.Loaded += Room_Loaded;
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
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


using MoreSlugcats;

namespace Silkslug.ColosseumRubicon
{
    public static class Manager
    {
        public static void OnEnable()
        {
            Warp.OnEnable();

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
                }
                if (self.world.name == "CR")
                {
                    ConsoleWrite("Auto adding CR scripts");
                }
            }
            orig(self);
        }
    }
}

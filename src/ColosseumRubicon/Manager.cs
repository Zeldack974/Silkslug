
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

        public static void addScript(Room room)
        {
            if (room.abstractRoom.name == "SB_E05SAINT")
            {
                ConsoleWrite("Add RoomSpecificScript: " + (room.game.GetStorySession.saveState.saveStateNumber == Plugin.ShawName));
                if (room.game.GetStorySession.saveState.saveStateNumber == Plugin.ShawName)
                {
                    room.AddObject(new MSCRoomSpecificScript.VS_E05WrapAround(room));
                    ConsoleWrite("RoomSpecificScript added");
                }
            }
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
                addScript(self);
            }
            orig(self);
        }
    }
}

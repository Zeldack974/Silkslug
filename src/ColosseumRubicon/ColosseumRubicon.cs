
using MoreSlugcats;

namespace Silkslug.ColosseumRubicon
{
    public static class ColosseumRubicon
    {
        public static class ColosseumRubiconManager
        {
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
        }
    }
}

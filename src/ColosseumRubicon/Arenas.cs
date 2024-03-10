using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silkslug.ColosseumRubicon
{
    public static class Arenas
    {
        public static ArenaChallenge[] challenges
        {
            get
            {
                ArenaChallenge[] challenges = new ArenaChallenge[3];

                challenges[0] = new ArenaChallenge("CR_A01", [
                    new CreaturePlacment("green")
                ]);
                challenges[1] = new ArenaChallenge("CR_A01", [
                    new CreaturePlacment("green", 1, 2, false),
                    new CreaturePlacment("blue", 2, 1, false)
                ]);
                challenges[2] = new ArenaChallenge("CR_A02", [
                    new CreaturePlacment("cyan", -1, 2),
                ]);

                return challenges;
            }
        }

        public class ArenaChallenge
        {
            public ArenaChallenge(string room, CreaturePlacment[] creatures)
            {
                roomName = room;
                spawns = creatures;
            }

            public string roomName;

            public CreaturePlacment[] spawns;
        }

        public class CreaturePlacment
        {
            public CreaturePlacment(string creature, int den = -1, int num = 1, bool voidsea = true)
            {
                this.creature = creature;
                this.denId = den;
                this.number = num;
                this.voidsea = voidsea;

            }

            public string creature;
            public int number;
            public int denId;
            public bool voidsea;
            public int spawnDelay;

        }
    }
}

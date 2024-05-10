using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silkslug.ColosseumRubicon
{
    public static class ArenaChallenges
    {
        public static ArenaChallenge[] challenges
        {
            get
            {
                ArenaChallenge[] challenges = new ArenaChallenge[4];

                challenges[0] = new ArenaChallenge("CR_A01", new CreaturePlacment[]
                {
                    new CreaturePlacment("green")
                });

                challenges[1] = new ArenaChallenge("CR_A02", new CreaturePlacment[]
                {
                    new CreaturePlacment("green", 1, 1, false),
                }, "explosive");

                challenges[2] = new ArenaChallenge("CR_A01", new CreaturePlacment[]
                {
                    new CreaturePlacment("blue", 2, 2, false),
                    new CreaturePlacment("cyan", -1, 1),
                }, "hell");

                challenges[3] = new ArenaChallenge("CR_A01", new CreaturePlacment[]
                {
                });

                return challenges;
            }
        }

        public static int currentArena = 0;

        public static ArenaChallenge CurrentArena
        {
            get => challenges[ArenaChallenges.currentArena];
        }

        public class ArenaChallenge
        {
            public ArenaChallenge(string room, CreaturePlacment[] creatures, string spear = "spear")
            {
                roomName = room;
                spawns = creatures;
                this.spear = spear;
            }

            public string roomName;

            public CreaturePlacment[] spawns;

            public string spear;
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

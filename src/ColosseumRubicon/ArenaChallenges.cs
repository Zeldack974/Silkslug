using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silkslug.ColosseumRubicon
{
    public static class ArenaChallenges
    {
        public static List<ArenaChallenge> challenges
        {
            get
            {
                List<ArenaChallenge> challenges = new List<ArenaChallenge>();


                //challenges.Add(new ArenaChallenge("CR_A02", new CreaturePlacment[]
                //{
                //    new CreaturePlacment("RedCenti", -1, 10),
                //}, "hell"));

                challenges.Add(new ArenaChallenge("CR_A01", new CreaturePlacment[]
                {
                    new CreaturePlacment("cyan", -1, 3),
                }));

                challenges.Add(new ArenaChallenge("CR_frame", new CreaturePlacment[]
                {
                    new CreaturePlacment("BigSpider", -1, 8),
                    new CreaturePlacment("SpitterSpider", -1, 4),
                }, "hell", 2));

                challenges.Add(new ArenaChallenge("CR_A02", new CreaturePlacment[]
                {
                    new CreaturePlacment("KingVulture", -1, 1),
                    new CreaturePlacment("Vulture", -1, 1),

                }));

                challenges.Add(new ArenaChallenge("CR_stoneheads", new CreaturePlacment[]
                {
                    new CreaturePlacment("Scavenger", -1, 6),
                    new CreaturePlacment("Elite", -1, 4),
                }));

                challenges.Add(new ArenaChallenge("CR_frame", new CreaturePlacment[]
                {
                    new CreaturePlacment("MirosVulture", 12, 1),
                }, "hell", 2));

                //challenges.Add(new ArenaChallenge("cr_warehouse", new CreaturePlacment[]
                //{
                //    new CreaturePlacment("FireBug", -1, 4),
                //}, "hell"));

                challenges.Add(new ArenaChallenge("cr_warehouse", new CreaturePlacment[]
                {
                    new CreaturePlacment("red", 8, 1),
                }, "electric"));



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
            public ArenaChallenge(string room, CreaturePlacment[] creatures, string spear = "spear", int playerDen = 0)
            {
                roomName = room;
                spawns = creatures;
                this.spear = spear;
                this.playerDen = playerDen;
            }

            public string roomName;

            public CreaturePlacment[] spawns;

            public string spear;

            public int playerDen;
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

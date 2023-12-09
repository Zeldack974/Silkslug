using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Slikslug;
using UnityEngine;

namespace Silkslug
{
    public static class Shaw
    {
        public static int chargeSlashTime = 12 + 50;

        public static int dashTotalFrame = 6;
        public class ShawData
        {
            public readonly Player player;

            public int chargeSlashCounter;

            public int dashFrame;

            public int attackCooldown;

            public int dashCooldown;

            public int dashDropLock;

            public int lastThrowDir = 1;

            public Vector2 throwDir;

            public ChunkSoundEmitter chargeInitSound = null;

            public ChunkSoundEmitter chargeLoopSound = null;

            public ShawData(Player player)
            {
                this.player = player;
            }
        }

        public static readonly Dictionary<Spear, int> invisibleSpear = new Dictionary<Spear, int>();

        public static void SetInvisible(this Spear spear, int frames)
        {
            invisibleSpear[spear] = frames;
        }

        public static void SetVisible(this Spear spear)
        {
            if (invisibleSpear.ContainsKey(spear))
            {
                invisibleSpear.Remove(spear);
            }
        }

        public static readonly ConditionalWeakTable<Player, ShawData> ShawDeets = new ConditionalWeakTable<Player, ShawData>();
        public static ShawData GetShaw(this Player player) => ShawDeets.GetValue(player, _ => new(player));
        public static bool TryGetShaw(this Player player, out ShawData shawData)
        {
            if (player.SlugCatClass == Plugin.ShawName)
            {
                shawData = GetShaw(player);
                return true;
            }

            shawData = null;
            return false;
        }
    }
}

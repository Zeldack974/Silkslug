using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Silkslug.MyDevConsole;

namespace Silkslug
{
    public static class Sounds
    {
        public static SoundID hero_nail_art_charge_initiate { get; private set; }
        public static SoundID hero_nail_art_charge_loop { get; private set; }
        public static SoundID hero_nail_art_great_slash { get; private set; }
        public static SoundID hero_parry { get; private set; }
        public static SoundID hornet_dash { get; private set; }
        public static SoundID Hornet_Fight_Yell_06 { get; private set; }
        public static SoundID Hornet_Fight_Yell_08 { get; private set; }
        public static SoundID Hornet_Fight_Yell_09 { get; private set; }
        public static SoundID nail { get; private set; }
        public static SoundID Hornet_Great_Slash { get; private set; }

        public static SoundID Hornet_Attack { get; private set; }

        // Call Initialize() from your plugin's Awake method.
        internal static void Initialize()
        {
            hero_nail_art_charge_initiate = new SoundID("hero_nail_art_charge_initiate", true);
            hero_nail_art_charge_loop = new SoundID("hero_nail_art_charge_loop", true);
            hero_nail_art_great_slash = new SoundID("hero_nail_art_great_slash", true);
            hero_parry = new SoundID("hero_parry", true);
            hornet_dash = new SoundID("hornet_dash", true);
            Hornet_Fight_Yell_06 = new SoundID("Hornet_Fight_Yell_06", true);
            Hornet_Fight_Yell_08 = new SoundID("Hornet_Fight_Yell_08", true);
            Hornet_Fight_Yell_09 = new SoundID("Hornet_Fight_Yell_09", true);
            nail = new SoundID("nail", true);
            Hornet_Great_Slash = new SoundID("Hornet_Great_Slash", true);
            Hornet_Attack = new SoundID("Hornet_Attack", true);
        }

    }
}

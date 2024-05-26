namespace Silkslug
{
    public static class Sounds
    {
        public static SoundID HERO_NAIL_ART_CHARGE_INITIATE { get; private set; }
        public static SoundID HERO_NAIL_ART_CHARGE_LOOP { get; private set; }
        public static SoundID HERO_NAIL_ART_GREAT_SLASH { get; private set; }
        public static SoundID HERRO_PARRY { get; private set; }
        public static SoundID HORNET_DASH { get; private set; }
        public static SoundID HORNET_FIGHT_YELL_06 { get; private set; }
        public static SoundID HORNET_FIGHT_YELL_08 { get; private set; }
        public static SoundID HORNET_FIGHT_YELL_09 { get; private set; }
        public static SoundID NAIL { get; private set; }
        public static SoundID HORNET_GREAT_SLASH { get; private set; }
        public static SoundID HORNET_DEATH { get; private set; }
        public static SoundID HORNET_GIT_GUD { get; private set; }
        public static SoundID HORNET_ATTACK { get; private set; }
        public static SoundID BOXING_BELL { get; private set; }
        public static SoundID NEW_LOCATION { get; private set; }
        public static SoundID DOUBLE_DAMAGE { get; private set; }
        public static SoundID DAMAGE { get; private set; }



        // Call Initialize() from your plugin's Awake method.
        internal static void Initialize()
        {
            HERO_NAIL_ART_CHARGE_INITIATE = new SoundID("hero_nail_art_charge_initiate", true);
            HERO_NAIL_ART_CHARGE_LOOP = new SoundID("hero_nail_art_charge_loop", true);
            HERO_NAIL_ART_GREAT_SLASH = new SoundID("hero_nail_art_great_slash", true);
            HERRO_PARRY = new SoundID("hero_parry", true);
            HORNET_DASH = new SoundID("hornet_dash", true);
            HORNET_FIGHT_YELL_06 = new SoundID("Hornet_Fight_Yell_06", true);
            HORNET_FIGHT_YELL_08 = new SoundID("Hornet_Fight_Yell_08", true);
            HORNET_FIGHT_YELL_09 = new SoundID("Hornet_Fight_Yell_09", true);
            NAIL = new SoundID("nail", true);
            HORNET_GREAT_SLASH = new SoundID("Hornet_Great_Slash", true);
            HORNET_ATTACK = new SoundID("Hornet_Attack", true);
            HORNET_DEATH = new SoundID("Hornet_Death", true);
            HORNET_GIT_GUD = new SoundID("Hornet_Git_Gud", true);
            BOXING_BELL = new SoundID("Boxing_Bell", true);
            NEW_LOCATION = new SoundID("New_Location", true);
            DAMAGE = new SoundID("Damage", true);
            SAW_LOOP = new SoundID("Saw_Loop", true);

        }
    }
}

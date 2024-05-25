using DressMySlugcat;
using System.Linq;
using UnityEngine;

namespace Silkslug
{
    internal static class SkinApplyer
    {
        public static bool IsSlornetEnabled => (from mod in ModManager.InstalledMods where mod.enabled select mod.id).Contains("Slug Song");

        private static bool CanApply => IsSlornetEnabled;

        public static void SetSlornetSkin()
        {
            if (!CanApply) return;

            Plugin.Log("Applying slornet skin");

            Customization customization;
            if (SaveManager.Customizations.Any(x => x.Matches("Shaw", 0)))
            {
                customization = Customization.For("Shaw", 0);
            }
            else
            {
                customization = new Customization()
                {
                    Slugcat = "Shaw",
                    PlayerNumber = 0
                };
                SaveManager.Customizations.Add(customization);
            };

            SetSlugcatCustomization("HEAD", "dressmyslugcat.slugsong", customization);
            SetSlugcatCustomization("FACE", "dressmyslugcat.slugsong", customization);
            SetSlugcatCustomization("BODY", "dressmyslugcat.slugsong", customization);
            SetSlugcatCustomization("ARMS", "dressmyslugcat.empty", customization);
            SetSlugcatCustomization("HIPS", "dressmyslugcat.slugsong", customization);
            SetSlugcatCustomization("LEGS", "dressmyslugcat.slugsong", customization);
            SetSlugcatCustomization("TAIL", "dressmyslugcat.empty", customization);
            SetSlugcatCustomization("PIXEL", "dressmyslugcat.default", customization);
        }

        public static void SetSlugcatCustomization(string sprite, string spriteSheet, Customization customization)
        {
            Plugin.Log(string.Join(", ", DressMySlugcat.Plugin.SpriteSheets.ConvertAll(x => x.ID)));
            Plugin.Log(string.Join(", ", customization.CustomSprites.ConvertAll(x => x.Sprite)));


            CustomSprite customSprite = customization.CustomSprite(sprite, true);
            customSprite.SpriteSheetID = spriteSheet;
            customSprite.Color = Color.white;
        }
    }
}

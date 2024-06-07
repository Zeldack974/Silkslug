using DressMySlugcat;
using System;
using System.Linq;
using UnityEngine;

namespace Silkslug
{
    internal static class SkinApplyer
    {
        public static bool IsSlornetEnabled => (from mod in ModManager.InstalledMods where mod.enabled select mod.id).Contains("Slug Song");

        private static bool CanApply => IsSlornetEnabled;

        public static void SetDefaults()
        {
            for (int i = 0; i < 4; i++)
            {
                SlugcatStats.Name name = SlugcatStats.Name.White;
                if (i == 1) name = SlugcatStats.Name.Yellow;
                else if (i == 2) name = SlugcatStats.Name.Red;
                else if (i == 3) name = SlugcatStats.Name.Night;

                Color color = PlayerGraphics.DefaultSlugcatColor(name);

                SpriteDefinitions.SlugcatDefaults.Add(
                    new Customization()
                    {
                        Slugcat = "Shaw",
                        PlayerNumber = i,
                        CustomSprites = new()
                        {
                            new CustomSprite()
                            {
                                Sprite = "HEAD",
                                Color = color,
                                SpriteSheetID = "dressmyslugcat.slugsong"
                            },
                            new CustomSprite()
                            {
                                Sprite = "FACE",
                                Color = Color.white,
                                SpriteSheetID = "dressmyslugcat.slugsong"
                            },
                            new CustomSprite()
                            {
                                Sprite = "BODY",
                                Color = Color.white,
                                SpriteSheetID = "dressmyslugcat.slugsong"
                            },
                            new CustomSprite()
                            {
                                Sprite = "ARMS",
                                Color = Color.white,
                                SpriteSheetID = "dressmyslugcat.empty"
                            },
                            new CustomSprite()
                            {
                                Sprite = "HIPS",
                                Color = Color.white,
                                SpriteSheetID = "dressmyslugcat.slugsong"
                            },
                            new CustomSprite()
                            {
                                Sprite = "LEGS",
                                Color = Color.white,
                                SpriteSheetID = "dressmyslugcat.slugsong"
                            },
                            new CustomSprite()
                            {
                                Sprite = "TAIL",
                                Color = Color.white,
                                SpriteSheetID = "dressmyslugcat.empty"
                            },
                            new CustomSprite()
                            {
                                Sprite = "PIXEL",
                                Color = color,
                                SpriteSheetID = "rainworld.default"
                            },
                        }
                    }
                );
            }

        }

        public static void SetSlornetSkin()
        {
            SpriteDefinitions.SlugcatDefaults.ForEach(x =>
            {
                Plugin.Log($"{x.Slugcat} | {x.PlayerNumber}");
            });

            if (!CanApply) return;

            Plugin.Log("Applying slornet skin");

            for (int i = 0; i < 4; i++)
            {
                SlugcatStats.Name name = SlugcatStats.Name.White;
                if (i == 1) name = SlugcatStats.Name.Yellow;
                else if (i == 2) name = SlugcatStats.Name.Red;
                else if (i == 3) name = SlugcatStats.Name.Night;

                Color color = PlayerGraphics.DefaultSlugcatColor(name);

                Customization customization;
                if (SaveManager.Customizations.Any(x => x.Matches("Shaw", i)))
                {
                    customization = Customization.For("Shaw", i);
                }
                else
                {
                    customization = new Customization()
                    {
                        Slugcat = "Shaw",
                        PlayerNumber = i
                    };
                    SaveManager.Customizations.Add(customization);
                };

                SetSlugcatCustomization("HEAD", "dressmyslugcat.slugsong", customization, color);
                SetSlugcatCustomization("FACE", "dressmyslugcat.slugsong", customization);
                SetSlugcatCustomization("BODY", "dressmyslugcat.slugsong", customization);
                SetSlugcatCustomization("ARMS", "dressmyslugcat.empty", customization);
                SetSlugcatCustomization("HIPS", "dressmyslugcat.slugsong", customization);
                SetSlugcatCustomization("LEGS", "dressmyslugcat.slugsong", customization);
                SetSlugcatCustomization("TAIL", "dressmyslugcat.empty", customization);
                SetSlugcatCustomization("PIXEL", "rainworld.default", customization);
            }
        }

        public static void SetSlugcatCustomization(string sprite, string spriteSheet, Customization customization)
        {
            SetSlugcatCustomization(sprite, spriteSheet, customization, Color.white);
        }

        public static void SetSlugcatCustomization(string sprite, string spriteSheet, Customization customization, Color color)
        {
            CustomSprite customSprite = customization.CustomSprite(sprite, true);

            Plugin.Log(customSprite.SpriteSheetID);
            if (customSprite.SpriteSheetID == "rainworld.default")
            {
                customSprite.SpriteSheetID = spriteSheet;
                customSprite.Color = color;
            }
        }
    }
}

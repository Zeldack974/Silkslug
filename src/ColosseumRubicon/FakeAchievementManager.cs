using HUD;
using Menu;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using System.Reflection.Emit;
using System.IO;
using SlugBase;
using Newtonsoft.Json;

namespace Silkslug.ColosseumRubicon
{
    public class FakeAchievementManager : Menu.Menu
    {
        public State state = State.Appearing;

        public const float speedFactor = 1.5f;

        public int shownTime = 0;

        public enum State
        {
            Appearing,
            Showed,
            Disappearing,
            Hidden
        }

        public MenuLabel achievementTitle;
        public MenuLabel achievementSubTitle;

        public class Achievement
        {
            public string id;
            public Dictionary<string, Dictionary<string, string>> translations;
            public string imagePath;

            private string GetLocalization(string localization)
            {
                var transformedTranslations = translations.ToDictionary(x => x.Key.ToLower(), x => x.Value);
                if (transformedTranslations.ContainsKey(RW.options.language.value.ToLower())) return transformedTranslations[RW.options.language.value.ToLower()][localization];
                return transformedTranslations["english"][localization];
            }

            public string title => GetLocalization("title");
            public string description => GetLocalization("description");

            public Achievement(string id, Dictionary<string, Dictionary<string, string>> localizations)
            {
                this.id = id;
                this.translations = localizations;

                imagePath = Path.Combine("achievements", this.id, "image");
                Futile.atlasManager.LoadImage(imagePath);
            }
        }

        public static List<Achievement> achievements;

        public static Dictionary<string, object> ToDictionnary(object arg)
        {
            return arg.GetType().GetProperties().ToDictionary(property => property.Name, property => property.GetValue(arg));
        }

        public static void LoadAchievements()
        {
            Plugin.Log("Loading achievements");

            List<ModManager.Mod> mods = (from mod in ModManager.InstalledMods where mod.enabled select mod).ToList();

            achievements = new List<Achievement>();

            foreach (ModManager.Mod mod in mods)
            {
                string achievementsPath = Path.Combine(mod.path, "achievements");
                if (!Directory.Exists(achievementsPath)) continue;

                string[] directories = Directory.GetDirectories(achievementsPath);

                foreach (string directory in directories)
                {
                    string achievementId = new DirectoryInfo(directory).Name;
                    string achievementPath = Path.Combine(achievementsPath, achievementId);

                    Plugin.Log("Found achievement: " + achievementId + " | " + achievementPath);

                    string infoFile = Path.Combine(achievementPath, "info.json");
                    var localizations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(infoFile));

                    Plugin.Log("Before create achievement");

                    Achievement achievement = new Achievement(achievementId, localizations);

                    Plugin.Log("Created:" + achievement.id + " " + achievement.imagePath);

                    achievements.Add(achievement);
                }
            }
        }

        public FakeAchievementManager(ProcessManager manager, Achievement achievement) : base(manager, new ProcessManager.ProcessID("FakeAchievementMenu", true))
        {
            pages.Add(new Page(this, null, "main", 0));
            PlaySound(Sounds.STEAM_ACHIEVEMENT);

            container.y = -69;
            container.x = RW.options.ScreenSize.x - (282 - 1);

            container.AddChild(
                new FSprite("illustrations/achievement_background")
                {
                    x = 0,
                    y = 0,
                    width = 282,
                    height = 69,
                    anchorX = 0,
                    anchorY = 0,
                }
            );
            container.AddChild(
                new FSprite(achievement.imagePath)
                {
                    y = 13,
                    x = 11,
                    width = 44,
                    height = 44,
                    anchorX = 0,
                    anchorY = 0,
                }
            );

            achievementTitle = new MenuLabel(this, pages[0], achievement.title, new Vector2(70, 36), new Vector2(100, 10), false);
            achievementSubTitle = new MenuLabel(this, pages[0], achievement.description, new Vector2(70, 20), new Vector2(100, 10), false);
            achievementSubTitle.label.color = Color.gray;

            achievementTitle.label.alignment = FLabelAlignment.Left;
            achievementSubTitle.label.alignment = FLabelAlignment.Left;

            achievementTitle.label.anchorY = 0;
            achievementSubTitle.label.anchorY = 0;
        }

        public override void GrafUpdate(float timeStacker)
        {
            if (achievementTitle != null && achievementSubTitle != null)
            {
                achievementTitle.label.x = achievementTitle.DrawX(timeStacker);
                achievementTitle.label.y = achievementTitle.DrawY(timeStacker);

                achievementSubTitle.label.x = achievementSubTitle.DrawX(timeStacker);
                achievementSubTitle.label.y = achievementSubTitle.DrawY(timeStacker);
            }

            if (state == State.Disappearing)
            {
                this.container.y -= speedFactor;
            }
            else if (shownTime >= 300)
            {
                this.state = State.Disappearing;
            }
            else if (state == State.Showed)
            {
                shownTime++;
            }
            else if (container.y >= 0)
            {
                this.container.y = 0;
                this.state = State.Showed;
            }
            else if (state == State.Appearing)
            {
                this.container.y += speedFactor;
            }
            else if (container.y <= -69)
            {
                this.container.y = -69;
                this.state = State.Hidden;
            }

            this.pages[0].GrafUpdate(timeStacker);
        }

        public static void ShowAchievement(string achievementId)
        {
            UnityEngine.Debug.Log("Creating achievement");

            Achievement achievement = achievements.Find(achievement => achievement.id == achievementId);

            if (achievement == null) throw new Exception($"Achievement not found : {achievementId}");

            instance = new FakeAchievementManager(RW.processManager, achievement);
        }

        public static FakeAchievementManager instance;
    }
}

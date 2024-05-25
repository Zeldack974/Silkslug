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

namespace Silkslug.ColosseumRubicon
{
    public class FakeAchievementManager : Menu.Menu
    {
        public FContainer fContainer = new()
        { 
            y = -69,
            x = RW.options.ScreenSize.x - (282 - 1),
        };

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

        public FakeAchievementManager(ProcessManager manager) : base(manager, new ProcessManager.ProcessID("FakeAchievementMenu", true))
        {
            pages.Add(new Page(this, null, "main", 0));
            pages[0].Container.AddChild(fContainer);
            PlaySound(Sounds.STEAM_ACHIEVEMENT);

            fContainer.AddChild(
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
            fContainer.AddChild(
                new FSprite("illustrations/achievement_image")
                {
                    y = 13,
                    x = 11,
                    width = 44,
                    height = 44,
                    anchorX = 0,
                    anchorY = 0,
                }
            );

            fContainer.AddChild(
                new FLabel(Custom.GetDisplayFont(), "Embrace the Void")
                {
                    x = 71,
                    y = 36,
                    anchorX = 0,
                    anchorY = 0,
                    scale = 2f
                }
            );

            Plugin.Log(Custom.GetDisplayFont());

            fContainer.AddChild(
                new FLabel(Custom.GetFont(), "Deep", new FTextParams())
                {
                    x = 71,
                    y = 20,
                    anchorX = 0,
                    anchorY = 0,
                    color = Color.gray,
                    shader = RW.Shaders["Basic"]
                }
            );
        }

        public override void GrafUpdate(float timeStacker)
        {
            if (state == State.Disappearing)
            {
                this.fContainer.y -= speedFactor;
            }
            else if (shownTime >= 300)
            {
                this.state = State.Disappearing;
            }
            else if (state == State.Showed)
            {
                shownTime++;
            }
            else if (fContainer.y >= 0)
            {
                this.fContainer.y = 0;
                this.state = State.Showed;
            }
            else if (state == State.Appearing)
            {
                this.fContainer.y += speedFactor;
            }
            else if (fContainer.y <= -69)
            {
                this.fContainer.y = -69;
                this.state = State.Hidden;
            }

            this.pages[0].GrafUpdate(timeStacker);
        }

        public static void ShowAchievement()
        {
            UnityEngine.Debug.Log("Creating achievement");
            instance = new FakeAchievementManager(RW.processManager);
        }

        public static FakeAchievementManager instance;
    }
}

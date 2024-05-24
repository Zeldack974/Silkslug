using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Menu;
using RWCustom;
using UnityEngine;
using MoreSlugcats;
using System.Runtime.CompilerServices;

namespace Silkslug.ColosseumRubicon
{
    public class HKMainMenu : Menu.Menu
    {
        public static ProcessManager.ProcessID HKMainMenuID = new ProcessManager.ProcessID("HKMaineMenu", true);

        public HKMainMenu(ProcessManager manager) : base(manager, HKMainMenuID)
        {
            this.pages.Add(new Page(this, null, "main", 0));
            if (manager.musicPlayer != null)
            {
                manager.musicPlayer.FadeOutAllSongs(30f);
            }
            this.manager.musicPlayer.MenuRequestsSong("Hollow Knight Title Theme", 100, 0);
            this.manager.musicPlayer.song.Loop = true;

            //this.bg = new MenuIllustration(this, this.scene, "illustrations", "hkmainmenu", this.manager.rainWorld.options.ScreenSize / 2f, true, true);
            //this.bg.sprite.scaleX = this.manager.rainWorld.options.ScreenSize.x / 1200f;
            //this.bg.sprite.scaleY = this.manager.rainWorld.options.ScreenSize.x / 1200f;
            //this.bg.sprite.anchorX = 0.5f;
            //this.bg.sprite.anchorY = 0.5f;
            this.bg = new MenuIllustration(this, this.scene, "illustrations", "hklogo", this.manager.rainWorld.options.ScreenSize / 2f, true, true);
            this.bg.sprite.scale = (this.manager.rainWorld.options.ScreenSize.x / 2346) * 0.60f;
            this.bg.sprite.anchorX = 0.5f;
            this.bg.sprite.anchorY = 0.5f;

            this.pages[0].subObjects.Add(this.bg);
            this.startButton = new HoldButton(this, this.pages[0], base.Translate("NEW GAME"), "START", new Vector2(683f, 85f), 60f * 3f);
            this.pages[0].subObjects.Add(this.startButton);

            for (int i = 1; i < 150; i++)
            {
                Particle particle = new(this, this.pages[0]);
                this.pages[0].subObjects.Add(particle);
            }
        }

        //public override void Update()
        //{
        //    base.Update();
        //}

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == "START")
            {
                ConsoleWrite("START NEW GAME");
                this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
            }
        }

        private class Particle : MenuObject
        {
            public FSprite sprite = GenerateParticle();

            public float fadeOutStart = UnityEngine.Random.Range(0.7f, 0.9f);
            public float targetAlhpa = UnityEngine.Random.Range(0.2f, 0.8f);

            public Particle(Menu.Menu menu, MenuObject owner) : base(menu, owner)
            {
                owner.Container.AddChild(sprite);
                Reset();
            }

            public override void GrafUpdate(float timeStacker)
            {
                float screenWidth = RW.options.ScreenSize.x;

                Vector2 movementFactor = Custom.DegToVec(this.sprite.rotation);
                float progress = sprite.x / screenWidth;
                sprite.x += movementFactor.x;
                sprite.y += movementFactor.y;
                if (progress > 1 || (sprite.alpha <= 0 && progress >= fadeOutStart))
                {
                    Reset();
                } else if (progress >= fadeOutStart)
                {
                    sprite.alpha -= 0.01f;
                } else if (sprite.alpha < targetAlhpa)
                {
                    sprite.alpha += 0.02f;
                }
                base.GrafUpdate(timeStacker);
            }

            public static FSprite GenerateParticle()
            {
                FSprite particle = new FSprite("Futile_White");

                particle.shader = RW.Shaders["FlatLight"];

                return particle;
            }

            public void Reset()
            {
                if (sprite == null) return;

                float screenWidth = RW.options.ScreenSize.x;
                float screenHeight = RW.options.ScreenSize.y;
                float margin = screenWidth / 0.9f;
                float spawnX = UnityEngine.Random.Range(margin, screenWidth - margin);
                float spawnY = UnityEngine.Random.Range(margin, screenHeight - margin);
                sprite.scale = UnityEngine.Random.Range(0.5f, 1.5f);
                sprite.x = spawnX;
                sprite.y = spawnY;
                sprite.rotation = UnityEngine.Random.Range(85, 95);
                sprite.alpha = 0;
            }
        }

        public MenuIllustration bg;
        public MenuIllustration text;
        public HoldButton startButton;
    }
}

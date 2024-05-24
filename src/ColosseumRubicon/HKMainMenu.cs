using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Menu;
using RWCustom;
using UnityEngine;
using MoreSlugcats;

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

        public MenuIllustration bg;
        public MenuIllustration text;
        public HoldButton startButton;
    }
}

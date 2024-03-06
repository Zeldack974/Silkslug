using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HUD;
using System.Drawing;

namespace Silkslug.ColosseumRubicon;

public static class RegionPopup
{
    public static void OnEnable()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
    }


    private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);
        self.AddPart(new RubiconPopup(self));
    }
}

public class RubiconPopup : HudPart
{
    public static RubiconPopup staticHUD;

    public RubiconPopup(HUD.HUD hud) : base(hud)
    {
        staticHUD = this;

        sprite = new FSprite("illustrations/rubiconintrotext", true)
        {
            anchorX = 0.5f,
			x = hud.rainWorld.screenSize.x / 2f,
			y = hud.rainWorld.screenSize.y * 0.75f,
			scale = 1f

        };

        hud.fContainers[1].AddChild(sprite);
    }

    public override void Draw(float timeStacker)
    {
        base.Draw(timeStacker);
        if (visibilityTime > 0)
        {
            sprite.isVisible = true;

            sprite.alpha = 1 - Math.Max((visibilityTime - (visibleTime - fadeIn)) / fadeIn, 0);

            if (fadeOut >= visibilityTime)
            {
                sprite.alpha = Math.Min(visibilityTime / fadeOut, 1);
            }

            visibilityTime--;
        }
        else
        {
            sprite.isVisible = false;
        }
    }

    public override void Update()
    {
        base.Update();
    }

    public void StartAnimation()
    {
        if (visibilityTime == 0)
        {
            visibilityTime = visibleTime;

        }
    }

    public FSprite sprite;

    public float visibilityTime = 0;

    public float visibleTime = 720;

    public float fadeIn = 300;

    public float fadeOut = 180;

}
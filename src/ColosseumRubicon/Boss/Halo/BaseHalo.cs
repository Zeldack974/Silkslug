using UnityEngine;
using RWCustom;

namespace Silkslug.ColosseumRubicon.Boss.Halo
{
    /**
        Copyright (c) 2023 Maxi Mol & gruntonium
    **/

    public class BaseHalo : UpdatableAndDeletable, IDrawable
    {
        public HellKnight owner; //determines position of halo
        public int firstSprite = 0;
        public int totalSprites;
        public Vector2? connectionPos = null; //if not null, connections will fire
        public Vector2? overridePos = null; //if not null, halo position is different
        public Color color = RainWorld.SaturatedGold;
        public Color prevColor;

        public Vector2 pos, lastPos;
        public float radius;

        public bool suppressConnectionFires = false;
        public float boltFireChance = 0.3f;
        public int boltFireCounter = 0; //alternative to boltFireChance, a constant counter for bolt fires
        public bool randomBoltPositions = false;
        public bool shortestDistFromHalo = false;
        public float noiseSuppress = 0f;


        public BaseHalo(HellKnight owner)
        {
            this.owner = owner;

        }


        public override void Destroy()
        {
            base.Destroy();
        }


        public override void Update(bool eu)
        {
            base.Update(eu);

            //rgb cycle color type
            prevColor = color;
        }


        //added funtion to support interface IDrawable
        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            if (newContainer == null)
                newContainer = rCam.ReturnFContainer("BackgroundShortcuts");
            for (int i = 0; i < totalSprites; i++)
            {
                newContainer.AddChild(sLeaser.sprites[i]);
                //if (Options.shaderType.Value != "None")
                //    sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders[Options.shaderType.Value];
            }
        }


        //added funtion to support interface IDrawable
        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }


        //added funtion to support interface IDrawable
        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
        }


        //added funtion to support interface IDrawable
        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[0];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using RegionKit.Extras;
using Mono.Cecil.Cil;

namespace Silkslug
{
    public class FallingBlock : CosmeticSprite
    {
        public static int fallTicks = 40 * 2;
        public static int respawnTicks = 40 * 6;

        private readonly PlacedObject _pObj;
        private DevtoolObjects.FallingBlockData data => (_pObj.data as DevtoolObjects.FallingBlockData);
        public IntVector2 tileRect => data.scaleVec;
        public Vector2 rect => (data.scaleVec.ToVector2() + Vector2.one) * 20;

        Color color = Color.white;

        public int fallTimer = 0;
        public int respawnTimer = 0;
        public FallingBlock(Room room, PlacedObject _pObj)
        {
            this.room = room;
            this._pObj = _pObj;
            SetTiles(true);
            
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            this.pos = room.GetTilePosition(_pObj.pos).ToVector2() * 20;

            if (fallTimer == 0 && respawnTimer == 0)
            {
                for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
                {
                    AbstractCreature abtCreature = room.abstractRoom.creatures[i];
                    if (!abtCreature.slatedForDeletion)
                    {
                        Vector2 rect = tileRect.ToVector2();
                        Vector2 p = new Vector2(abtCreature.pos.x + 0f, abtCreature.pos.y + 0f);
                        Vector2 min = room.GetTilePosition(this.pos).ToVector2() + (rect * Vector2.up) + Vector2.up;
                        Vector2 max = min + (rect * Vector2.right) + Vector2.up;
                        if (p.y == min.y && p.x >= min.x && p.x <= max.x)
                        {
                            fallTimer = fallTicks;
                            break;
                        }
                    }
                }
            }

            if (fallTimer > 0)
            {
                respawnTimer = 0;
                fallTimer--;
                if (fallTimer == 0)
                {
                    SetTiles(false);
                    respawnTimer = respawnTicks;
                }
            }

            if (respawnTimer > 0)
            {
                fallTimer = 0;
                respawnTimer--;
                if (respawnTimer == 0)
                {
                    SetTiles(true);
                }
            }

        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);

            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].SetAnchor(Vector2.zero);
            AddToContainer(sLeaser, rCam, null);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            this.color = Color.Lerp(palette.blackColor, Color.black, 0.75f);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            data.scaleVec.x = Math.Max(data.scaleVec.x, 0);
            data.scaleVec.y = Math.Max(data.scaleVec.y, 0);

            Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
            sLeaser.sprites[0].scaleX = rect.x * 0.0625f;
            sLeaser.sprites[0].scaleY = rect.y * 0.0625f;
            sLeaser.sprites[0].color = color;

            if (fallTimer > 0)
            {
                sLeaser.sprites[0].color = Color.Lerp(Color.red, color, (float)fallTimer / (fallTicks));
            }

            if (respawnTimer > 0)
            {
                sLeaser.sprites[0].color = Color.red;
                sLeaser.sprites[0].alpha = Mathf.InverseLerp(Math.Max(0, respawnTicks - 40), respawnTicks, respawnTimer);
                sLeaser.sprites[0].y = Math.Max(0, sLeaser.sprites[0].y - timeStacker * 2f);
            }
            else
            {
                sLeaser.sprites[0].SetPosition(vector - camPos);
                sLeaser.sprites[0].alpha = 1f;
            }
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Water");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

        public void SetTiles(bool solid)
        {
            
            //ConsoleWrite($"setting tiles to {(solid ? "solid" : "air")}");
            IntVector2 startPos = room.GetTilePosition(_pObj.pos);
            for (int x = 0; x <= tileRect.x; x++)
            {
                for (int y = 0; y <= tileRect.y; y++)
                {
                    Room.Tile tile = room.GetTile(startPos + new IntVector2(x, y));
                    tile.Terrain = solid ? Room.Tile.TerrainType.Solid : Room.Tile.TerrainType.Air;
                }
            }
        }
    }
}

using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Silkslug.ColosseumRubicon.Boss
{
    public class Laser : UpdatableAndDeletable, IDrawable
    {
        public override void Update(bool eu)
        {
            base.Update(eu);
            this.life--;
            this.dir += this.dirTick;
            if (this.life <= 0)
            {
                this.life = 0;
                this.postLife++;
                if ((this.postLife < posLifeTime && this.type == Boss.Laser.Type.EXPLODE) || this.type == Boss.Laser.Type.LIGHTNING || this.type == Boss.Laser.Type.PILLAR)
                {
                    Vector2 normalized = Custom.PerpendicularVector(Custom.DegToVec(this.dir)).normalized;
                    Vector2 r = this.startPos + normalized * this.width * 1.25f;
                    Vector2 r2 = this.startPos - normalized * this.width * 1.25f;
                    Vector2 r3 = this.startPos + Custom.DegToVec(this.dir) * 400f * length - normalized * this.width * 1.25f;
                    Vector2 r4 = this.startPos + Custom.DegToVec(this.dir) * 400f * length + normalized * this.width * 1.25f;
                    if (this.type == Boss.Laser.Type.EXPLODE && (Custom.PointInPoly4(this.parent.player.firstChunk.pos, r, r2, r3, r4) || Custom.Dist(this.parent.player.firstChunk.pos, this.parent.pos) < 64f))
                    {
                        this.parent.player.firstChunk.vel = Custom.DegToVec(this.dir) * 16f;

                    }
                    if (this.type == Boss.Laser.Type.LIGHTNING && Custom.PointInPoly4(this.parent.player.firstChunk.pos, r, r2, r3, r4))
                    {
                        this.parent.player.firstChunk.vel = Custom.DegToVec(this.dir) * 16f;
                        if (this.postLife >= posLifeTime)
                        {
                            this.parent.player.Die();
                        }
                    }
                    if (this.type == Boss.Laser.Type.PILLAR && Custom.PointInPoly4(this.parent.player.firstChunk.pos, r, r2, r3, r4) && this.postLife < (posLifeTime - 5))
                    {
                        this.parent.player.Die();
                    }
                }
                if (this.postLife == posLifeTime)
                {
                    this.Impact();
                }
                if (this.postLife > posLifeTime + 5)
                {
                    base.slatedForDeletetion = true;
                }
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];
            for (int i = 0; i < 3; i++)
            {
                sLeaser.sprites[i] = new FSprite("Futile_White", true);
                sLeaser.sprites[i].rotation = this.dir;
                sLeaser.sprites[i].anchorY = 0f;
                sLeaser.sprites[i].scaleX = 0f;
                sLeaser.sprites[i].scaleY = 600f * length / sLeaser.sprites[i].height;
                if (i > 0)
                {
                    sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["Hologram"];
                }
            }
            this.AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = this.startPos - camPos;
            float perc = (float)this.life / (float)this.lifeTime;
            float num = Custom.LerpBackEaseOut(0f, (!this.hideGuides) ? (this.width * 1.5f) : 0f, perc);
            float num2 = Custom.LerpBackEaseOut(this.width * 0.75f, 0f, perc);
            if (this.postLife > 0)
            {
                if (this.type == Boss.Laser.Type.PILLAR && this.postLife < (posLifeTime - 5))
                {
                    num2 *= 2f;
                }
                else
                {
                    num2 = Custom.LerpElasticEaseOut(this.width * 0.75f, 0f, (float)this.postLife / 10f);
                }
            }
            float num3 = Custom.LerpBackEaseOut(2f, 0f, perc);
            Vector2 normalized = Custom.PerpendicularVector(Custom.DegToVec(this.dir)).normalized;
            sLeaser.sprites[0].x = vector.x;
            sLeaser.sprites[0].y = vector.y;
            sLeaser.sprites[0].scaleX = num2 / sLeaser.sprites[0].element.sourceRect.width;
            sLeaser.sprites[1].x = Mathf.Floor(vector.x + normalized.x * num);
            sLeaser.sprites[1].y = vector.y + normalized.y * num;
            sLeaser.sprites[1].scaleX = num3 / sLeaser.sprites[1].element.sourceRect.width;
            sLeaser.sprites[2].x = Mathf.Floor(vector.x - normalized.x * num);
            sLeaser.sprites[2].y = vector.y - normalized.y * num;
            sLeaser.sprites[2].scaleX = num3 / sLeaser.sprites[2].element.sourceRect.width;
            sLeaser.sprites[0].rotation = this.dir;
            sLeaser.sprites[1].rotation = this.dir;
            sLeaser.sprites[2].rotation = this.dir;
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[0]);
            rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[1]);
            rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[2]);
        }

        public void Impact()
        {
            Vector2 vector = this.startPos + Custom.DegToVec(this.dir) * 600f * length;
            Vector2 a = Custom.DegToVec(Custom.AimFromOneVectorToAnother(this.startPos, vector));
            IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, this.startPos, vector);
            if (intVector != null)
            {
                Vector2 vector2 = this.room.MiddleOfTile(intVector.Value) - a * 7f;
                if (this.type == Boss.Laser.Type.EXPLODE)
                {
                    this.room.AddObject(new Explosion(this.room, null, vector2, 7, 200f, 6.2f, 2f, 280f, 0f, null, 0.3f, 160f, 1f));
                    this.room.AddObject(new Explosion.ExplosionLight(vector2, 280f, 1f, 7, Color.white));
                    this.room.AddObject(new Explosion.ExplosionLight(vector2, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                    this.room.AddObject(new ShockWave(vector2, 330f, 0.045f, 5, false));
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 a2 = Custom.RNV();
                        if (this.room.GetTile(vector2 + a2 * 20f).Solid)
                        {
                            if (!this.room.GetTile(vector2 - a2 * 20f).Solid)
                            {
                                a2 *= -1f;
                            }
                            else
                            {
                                a2 = Custom.RNV();
                            }
                        }
                        this.room.AddObject(new Explosion.FlashingSmoke(vector2 + a2 * 40f * UnityEngine.Random.value, a2 * Mathf.Lerp(4f, 20f, Mathf.Pow(UnityEngine.Random.value, 2f)), 1f + 0.05f * UnityEngine.Random.value, new Color(1f, 1f, 1f), Color.white, UnityEngine.Random.Range(3, 11)));
                    }
                    this.room.PlaySound(SoundID.Bomb_Explode, vector2, 0.8f, 0.75f + UnityEngine.Random.value * 0.5f);
                    return;
                }
                if (this.type == Boss.Laser.Type.LIGHTNING)
                {
                    LightningBolt lightningBolt = new LightningBolt(this.startPos, vector2, 0, this.width / 60f, 2f, 0f, 0.2f, false);
                    lightningBolt.intensity = 1f;
                    this.room.AddObject(lightningBolt);
                    this.room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, vector2, 0.4f, 1.4f - UnityEngine.Random.value * 0.4f);
                    this.room.AddObject(new LightningMachine.Impact(vector2, 16f, Color.white, true));
                }
            }
        }

        // Token: 0x060040BE RID: 16574 RVA: 0x00480A48 File Offset: 0x0047EC48
        public Laser(HellKnight parent, Vector2 startPos, float dir, float width, int lifeTime, Boss.Laser.Type type)
        {
            this.parent = parent;
            this.life = lifeTime;
            this.lifeTime = lifeTime;
            this.startPos = startPos;
            this.dir = dir;
            this.type = type;
            this.width = width;
        }

        public float length = 2f;

        public int posLifeTime = 5;

        public HellKnight parent;

        public int life;

        public int lifeTime;

        public Vector2 startPos;

        public float dir;

        public int postLife;

        public Boss.Laser.Type type;

        public float width;

        public bool hideGuides;

        public float dirTick;

        public class Type : ExtEnum<Boss.Laser.Type>
        {
            // Token: 0x0600470E RID: 18190 RVA: 0x004BEE13 File Offset: 0x004BD013
            public Type(string value, bool register = false) : base(value, register)
            {
            }

            // Token: 0x04004C70 RID: 19568
            public static readonly Boss.Laser.Type EXPLODE = new Boss.Laser.Type("EXPLODE", true);

            // Token: 0x04004C71 RID: 19569
            public static readonly Boss.Laser.Type LIGHTNING = new Boss.Laser.Type("LIGHTNING", true);

            public static readonly Boss.Laser.Type PILLAR = new Boss.Laser.Type("PILLAR", true);
        }
    }

}

using UnityEngine;
using RWCustom;
using System;
using Random = UnityEngine.Random;
using static Silkslug.Shaw;
using System.Collections.Generic;
using System.Linq;
using Noise;
using IL.MoreSlugcats;

namespace Silkslug
{
    public class Slash : CosmeticSprite
    {
        public Slash(Room room, Player owner, Spear spear, Vector2 dir, float rad, float force, float damage)
        {
            this.room = room;
            this.owner = owner;
            this.spear = spear;
            this.dir = dir;
            this.rad = rad;
            this.force = force;
            this.damage = damage;
        }

        public Vector2 dir;

        public Vector2 pos;
        
        public Player owner;

        public Spear? spear;

        public static int lifeTime = 4;

        public float rad;

        public float hitRad {
            get { return this.rad * 1f; }
        }

        public float force;

        public float damage;

        public int frame;

        public bool hitSomething;

        public List<Creature> creaturesHit = new List<Creature>();

        public override void Update(bool eu)
        {
            base.Update(eu);

            this.frame++;
            if (this.frame > lifeTime + 1)
            {
                this.Destroy();
            }

            if (this.spear == null)
            {
                return;
            }

            bool hasHitSometing = this.hitSomething;

            if (!this.hitSomething)
            {
                foreach (IDrawable drawable in this.room.drawableObjects)
                {
                    if (drawable != null && drawable is Slash && (drawable as Slash) != this)
                    {
                        Slash slash = (Slash)drawable;
                        if ((slash.pos - this.pos).magnitude < (slash.hitRad + this.hitRad))
                        {
                            bool validDir1 = true;
                            if (this.dir.y != 0)
                            {
                                if (this.dir.y > 0 && slash.pos.y < this.owner.firstChunk.pos.y)
                                {
                                    validDir1 = false;
                                }
                                if (this.dir.y < 0 && slash.pos.y > this.owner.firstChunk.pos.y)
                                {
                                    validDir1 = false;
                                }
                            }
                            else if (this.dir.x != 0)
                            {
                                if (this.dir.x > 0 && slash.pos.x < this.owner.firstChunk.pos.x)
                                {
                                    validDir1 = false;
                                }
                                if (this.dir.x < 0 && slash.pos.x > this.owner.firstChunk.pos.x)
                                {
                                    validDir1 = false;
                                }
                            }

                            bool validDir2 = true;
                            if (slash.dir.y != 0)
                            {
                                if (slash.dir.y > 0 && this.pos.y < slash.owner.firstChunk.pos.y)
                                {
                                    validDir2 = false;
                                }
                                if (slash.dir.y < 0 && this.pos.y > slash.owner.firstChunk.pos.y)
                                {
                                    validDir2 = false;
                                }
                            }
                            else if (slash.dir.x != 0)
                            {
                                if (slash.dir.x > 0 && this.pos.x < slash.owner.firstChunk.pos.x)
                                {
                                    validDir2 = false;
                                }
                                if (slash.dir.x < 0 && this.pos.x > slash.owner.firstChunk.pos.x)
                                {
                                    validDir2 = false;
                                }
                            }

                            if (validDir1 && validDir2)
                            {
                                this.room.PlaySound(Sounds.hero_parry, slash.pos);
                                hitSomething = true;
                            }
                        }
                    }

                    if (drawable != null && drawable is DashSlash)
                    {
                        DashSlash dashSlash = (DashSlash)drawable;
                        if ((dashSlash.hitPos - this.pos).magnitude < (dashSlash.hitRad + this.hitRad))
                        {
                            bool validDir1 = true;
                            if (this.dir.y != 0)
                            {
                                if (this.dir.y > 0 && dashSlash.hitPos.y < this.owner.firstChunk.pos.y)
                                {
                                    validDir1 = false;
                                }
                                if (this.dir.y < 0 && dashSlash.hitPos.y > this.owner.firstChunk.pos.y)
                                {
                                    validDir1 = false;
                                }
                            }
                            else if (this.dir.x != 0)
                            {
                                if (this.dir.x > 0 && dashSlash.hitPos.x < this.owner.firstChunk.pos.x)
                                {
                                    validDir1 = false;
                                }
                                if (this.dir.x < 0 && dashSlash.hitPos.x > this.owner.firstChunk.pos.x)
                                {
                                    validDir1 = false;
                                }
                            }

                            if (validDir1)
                            {
                                this.room.PlaySound(Sounds.hero_parry, dashSlash.pos);
                                hitSomething = true;
                            }
                        }
                    }
                }
            }

            if (!this.hitSomething)
            {
                for (int j = 0; j < this.room.physicalObjects.Length; j++)
                {
                    for (int k = 0; k < this.room.physicalObjects[j].Count; k++)
                    {
                        if (this.owner != this.room.physicalObjects[j][k] && !this.room.physicalObjects[j][k].slatedForDeletetion)
                        {
                            for (int l = 0; l < this.room.physicalObjects[j][k].bodyChunks.Length; l++)
                            {
                                if (this.IsHit(this.room.physicalObjects[j][k].bodyChunks[l].pos, out float radValue, out float distance))
                                {
                                    float num = Mathf.InverseLerp(radValue, radValue * 0.25f, distance);
                                    int chuck = -1;
                                    if (!this.room.VisualContact(this.pos, this.room.physicalObjects[j][k].bodyChunks[l].pos))
                                    {
                                        num -= 0.5f;
                                    }
                                    if (num > 0f)
                                    {
                                        chuck = l;
                                    }



                                    if (chuck > -1)
                                    {
                                        if (this.room.physicalObjects[j][k] is Creature && !this.creaturesHit.Contains((this.room.physicalObjects[j][k] as Creature)) && (this.room.physicalObjects[j][k] is not Player || (ModManager.CoopAvailable && Custom.rainWorld.options.friendlyFire || this.room.game.IsArenaSession && this.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers)))
                                        {
                                            this.room.PlaySound(SoundID.Spear_Stick_In_Creature, this.room.physicalObjects[j][k].firstChunk);

                                            Vector2 force = this.dir * this.room.physicalObjects[j][k].TotalMass * 9f;
                                            if (ModManager.MSC && this.room.physicalObjects[j][k] is Player)
                                            {
                                                Player player = (this.room.physicalObjects[j][k] as Player);
                                                (this.room.physicalObjects[j][k] as Creature).SetKillTag(player.abstractCreature);
                                                player.playerState.permanentDamageTracking += (double)(this.damage / player.Template.baseDamageResistance);
                                                player.firstChunk.vel += force;
                                                if (player.playerState.permanentDamageTracking >= 1.0)
                                                {
                                                    player.Die();
                                                }
                                            }
                                            else
                                            {
                                                (this.room.physicalObjects[j][k] as Creature).Violence(this.owner.mainBodyChunk, force, this.room.physicalObjects[j][k].firstChunk, null, Creature.DamageType.Blunt, this.damage, 20f);
                                            }
                                            this.creaturesHit.Add(this.room.physicalObjects[j][k] as Creature);
                                            this.hitSomething = true;

                                            if (this.spear.abstractSpear.explosive)
                                            {
                                                this.owner.Stun(65);
                                                this.room.AddObject(new SootMark(this.room, this.room.physicalObjects[j][k].bodyChunks[l].pos, 50f, false));
                                                this.room.AddObject(new Explosion(this.room, this.owner, this.room.physicalObjects[j][k].bodyChunks[l].pos, 5, 110f, 5f, 2.9f, 60f, 0.3f, this.owner, 0.8f, 0f, 0.7f));
                                                for (int i = 0; i < 14; i++)
                                                {
                                                    this.room.AddObject(new Explosion.ExplosionSmoke(this.room.physicalObjects[j][k].bodyChunks[l].pos, Custom.RNV() * 5f * Random.value, 1f));
                                                }
                                                this.room.AddObject(new Explosion.ExplosionLight(this.room.physicalObjects[j][k].bodyChunks[l].pos, 160f, 1f, 3, (this.spear as ExplosiveSpear).explodeColor));
                                                this.room.AddObject(new ExplosionSpikes(this.room, this.room.physicalObjects[j][k].bodyChunks[l].pos, 9, 4f, 5f, 5f, 90f, (this.spear as ExplosiveSpear).explodeColor));
                                                this.room.AddObject(new ShockWave(this.room.physicalObjects[j][k].bodyChunks[l].pos, 60f, 0.045f, 4, false));
                                                for (int v = 0; v < 20; v++)
                                                {
                                                    Vector2 a = Custom.RNV();
                                                    this.room.AddObject(new Spark(this.room.physicalObjects[j][k].bodyChunks[l].pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), (this.spear as ExplosiveSpear).explodeColor, null, 4, 18));
                                                }
                                                this.room.ScreenMovement(new Vector2?(this.room.physicalObjects[j][k].bodyChunks[l].pos), default(Vector2), 0.7f);
                                                this.room.PlaySound(SoundID.Fire_Spear_Explode, this.room.physicalObjects[j][k].bodyChunks[l].pos);
                                                this.room.InGameNoise(new InGameNoise(this.room.physicalObjects[j][k].bodyChunks[l].pos, 8000f, spear, 1f));

                                                spear.Destroy();
                                                this.Destroy();
                                                return;
                                            }

                                            if (this.spear.abstractSpear.electric)
                                            {
                                                (this.spear as MoreSlugcats.ElectricSpear).Electrocute(this.room.physicalObjects[j][k]);
                                            }
                                        }
                                        if (this.room.physicalObjects[j][k] is Weapon && (this.room.physicalObjects[j][k] as Weapon).mode == Weapon.Mode.Thrown)
                                        {
                                            Weapon weapon = (this.room.physicalObjects[j][k] as Weapon);
                                            for (int n = 17; n > 0; n--)
                                            {
                                                weapon.room.AddObject(new Spark(weapon.firstChunk.pos, Custom.RNV() * 2, Color.white, null, 10, 20));
                                            }

                                            this.room.PlaySound(Sounds.hero_parry, weapon.firstChunk.pos);
                                            weapon.ChangeMode(Weapon.Mode.Free);
                                            weapon.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(this.pos, weapon.firstChunk.pos)) * 20f;
                                            weapon.SetRandomSpin();
                                        }
                                        else if (this.room.physicalObjects[j][k] is not Creature && this.spear != null)
                                        {
                                            this.room.physicalObjects[j][k].HitByWeapon(this.spear);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.hitSomething && !hasHitSometing)
            {
                if (this.dir.x != 0)
                {
                    this.owner.firstChunk.vel.x = this.dir.normalized.x * -25f * this.force;
                }

                if (this.dir.y < 0)
                {
                    this.owner.firstChunk.vel.y = this.dir.normalized.y * -35f * this.force;
                }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("atlas/slash", true);
            sLeaser.sprites[0].anchorY = 0;
            sLeaser.sprites[0].width = this.rad * 0.5895f;
            sLeaser.sprites[0].height = this.rad;
            //sLeaser.sprites[0].rotation = Custom.VecToDeg(this.dir.normalized);
            if (this.dir.y != 0)
            {
                sLeaser.sprites[0].rotation = 0f;
                sLeaser.sprites[0].height *= this.dir.y;
            }
            else
            {
                sLeaser.sprites[0].rotation = 90f;
                sLeaser.sprites[0].height *= this.dir.x;
            }
            //sLeaser.sprites[0].rotation = Custom.VecToDeg(this.dir);
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            this.pos = (Vector2.Lerp(owner.firstChunk.pos, owner.firstChunk.lastPos, 0.35f) - (this.dir * (this.rad / 3)));
            sLeaser.sprites[0].SetPosition(this.pos - camPos);

            sLeaser.sprites[0].color = Color.white;
            if (this.spear != null)
            {
                if (this.spear.abstractSpear.explosive)
                {
                    sLeaser.sprites[0].color = Color.red;
                }
                else if (this.spear.abstractSpear.electric && spear.abstractSpear.electricCharge > 0)
                {
                    sLeaser.sprites[0].color = new Color(0.5f, 0.80f, 1f);
                }
                else if (this.spear.abstractSpear.hue != 0)
                {
                    sLeaser.sprites[0].color = Custom.HSL2RGB(Custom.Decimal(this.spear.abstractSpear.hue + EggBugGraphics.HUE_OFF), 1f, 0.5f);
                }
            }

            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner = (newContatiner ?? rCam.ReturnFContainer("Midground"));
            foreach (FSprite sprite in sLeaser.sprites)
            {
                newContatiner.AddChild(sprite);
            }
        }

        public bool IsHit(Vector2 pos, out float radValue, out float distance)
        {
            radValue = this.hitRad * (0.25f + 0.75f * Mathf.Sin(Mathf.InverseLerp(0f, (float)lifeTime, (float)this.frame) * 3.1415927f));
            distance = Vector2.Distance(this.pos, pos);
            if (Mathf.Min(distance, float.MaxValue) < radValue)
            {
                bool validDir = true;
                if (this.dir.y != 0)
                {
                    if (this.dir.y > 0 && pos.y < this.owner.firstChunk.pos.y)
                    {
                        validDir = false;
                    }
                    if (this.dir.y < 0 && pos.y > this.owner.firstChunk.pos.y)
                    {
                        validDir = false;
                    }
                }
                else if (this.dir.x != 0)
                {
                    if (this.dir.x > 0 && pos.x < this.owner.firstChunk.pos.x)
                    {
                        validDir = false;
                    }
                    if (this.dir.x < 0 && pos.x > this.owner.firstChunk.pos.x)
                    {
                        validDir = false;
                    }
                }

                if (validDir)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class DashSlash : CosmeticSprite
    {
        public DashSlash(Room room, Player owner, Spear spear, Vector2 dir, float size, float force, float damage)
        {
            this.room = room;
            this.owner = owner;
            this.spear = spear;
            this.dir = dir.normalized;
            this.size = size;
            this.force = force;
            this.damage = damage;
        }

        public Vector2 dir;

        public Vector2 pos;

        public Player owner;

        public Spear spear;

        public static int lifeTime = 2;

        public float size;

        public float hitRad
        {
            get { return this.size * 0.5f; }
        }

        public Vector2 hitPos
        {
            get { return this.pos + this.dir * this.size; }
        }

        public float force;

        public float damage;

        public int frame;

        public bool hitSomething;

        public List<Creature> creaturesHit = new List<Creature>();

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (this.spear == null)
            {
                this.Destroy();
                return;
            }

            if (!this.hitSomething)
            {
                foreach (IDrawable drawable in this.room.drawableObjects)
                {
                    if (drawable != null && drawable is Slash)
                    {
                        Slash slash = (Slash)drawable;
                        if ((slash.pos - this.hitPos).magnitude < (slash.hitRad + this.hitRad))
                        {
                            bool validDir2 = true;
                            if (slash.dir.y != 0)
                            {
                                if (slash.dir.y > 0 && this.hitPos.y < slash.owner.firstChunk.pos.y)
                                {
                                    validDir2 = false;
                                }
                                if (slash.dir.y < 0 && this.hitPos.y > slash.owner.firstChunk.pos.y)
                                {
                                    validDir2 = false;
                                }
                            }
                            else if (slash.dir.x != 0)
                            {
                                if (slash.dir.x > 0 && this.hitPos.x < slash.owner.firstChunk.pos.x)
                                {
                                    validDir2 = false;
                                }
                                if (slash.dir.x < 0 && this.hitPos.x > slash.owner.firstChunk.pos.x)
                                {
                                    validDir2 = false;
                                }
                            }

                            if (validDir2)
                            {
                                this.room.PlaySound(Sounds.hero_parry, slash.pos);
                                hitSomething = true;
                            }
                        }
                    }
                    if (drawable != null && drawable is DashSlash && drawable as DashSlash != this)
                    {
                        DashSlash dashSlash = (DashSlash)drawable;
                        if ((dashSlash.hitPos - this.hitPos).magnitude < (dashSlash.hitRad + this.hitRad))
                        {
                            this.room.PlaySound(Sounds.hero_parry, dashSlash.pos);
                            hitSomething = true;
                        }
                    }
                }
            }

            if (!this.hitSomething)
            {
                for (int j = 0; j < this.room.physicalObjects.Length; j++)
                {
                    for (int k = 0; k < this.room.physicalObjects[j].Count; k++)
                    {
                        if (this.owner != this.room.physicalObjects[j][k] && !this.room.physicalObjects[j][k].slatedForDeletetion)
                        {
                            for (int l = 0; l < this.room.physicalObjects[j][k].bodyChunks.Length; l++)
                            {
                                if (this.IsHit(this.room.physicalObjects[j][k].bodyChunks[l].pos, out float radValue, out float distance))
                                {
                                    float num = Mathf.InverseLerp(radValue, radValue * 0.25f, distance);
                                    int chuck = -1;
                                    if (!this.room.VisualContact(this.pos, this.room.physicalObjects[j][k].bodyChunks[l].pos))
                                    {
                                        num -= 0.5f;
                                    }
                                    if (num > 0f)
                                    {
                                        chuck = l;
                                    }



                                    if (chuck > -1)
                                    {
                                        if (this.room.physicalObjects[j][k] is Creature && !this.creaturesHit.Contains((this.room.physicalObjects[j][k] as Creature)) && (this.room.physicalObjects[j][k] is not Player || (ModManager.CoopAvailable && Custom.rainWorld.options.friendlyFire || this.room.game.IsArenaSession && this.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers)))
                                        {
                                            this.room.PlaySound(SoundID.Spear_Stick_In_Creature, this.room.physicalObjects[j][k].firstChunk);

                                            Vector2 force = this.dir * this.room.physicalObjects[j][k].TotalMass * 9f;
                                            if (ModManager.MSC && this.room.physicalObjects[j][k] is Player)
                                            {
                                                Player player = (this.room.physicalObjects[j][k] as Player);
                                                (this.room.physicalObjects[j][k] as Creature).SetKillTag(player.abstractCreature);
                                                player.playerState.permanentDamageTracking += (double)(this.damage / player.Template.baseDamageResistance);
                                                player.firstChunk.vel += force;
                                                if (player.playerState.permanentDamageTracking >= 1.0)
                                                {
                                                    player.Die();
                                                }
                                            }
                                            else
                                            {
                                                (this.room.physicalObjects[j][k] as Creature).Violence(this.owner.mainBodyChunk, force, this.room.physicalObjects[j][k].firstChunk, null, Creature.DamageType.Blunt, this.damage, 20f);
                                            }
                                            this.creaturesHit.Add(this.room.physicalObjects[j][k] as Creature);
                                            this.hitSomething = true;

                                            if (this.spear.abstractSpear.explosive)
                                            {
                                                this.owner.Stun(65);
                                                this.room.AddObject(new SootMark(this.room, this.room.physicalObjects[j][k].bodyChunks[l].pos, 50f, false));
                                                this.room.AddObject(new Explosion(this.room, this.owner, this.room.physicalObjects[j][k].bodyChunks[l].pos, 5, 110f, 5f, 2.9f, 60f, 0.3f, this.owner, 0.8f, 0f, 0.7f));
                                                for (int i = 0; i < 14; i++)
                                                {
                                                    this.room.AddObject(new Explosion.ExplosionSmoke(this.room.physicalObjects[j][k].bodyChunks[l].pos, Custom.RNV() * 5f * Random.value, 1f));
                                                }
                                                this.room.AddObject(new Explosion.ExplosionLight(this.room.physicalObjects[j][k].bodyChunks[l].pos, 160f, 1f, 3, (this.spear as ExplosiveSpear).explodeColor));
                                                this.room.AddObject(new ExplosionSpikes(this.room, this.room.physicalObjects[j][k].bodyChunks[l].pos, 9, 4f, 5f, 5f, 90f, (this.spear as ExplosiveSpear).explodeColor));
                                                this.room.AddObject(new ShockWave(this.room.physicalObjects[j][k].bodyChunks[l].pos, 60f, 0.045f, 4, false));
                                                for (int v = 0; v < 20; v++)
                                                {
                                                    Vector2 a = Custom.RNV();
                                                    this.room.AddObject(new Spark(this.room.physicalObjects[j][k].bodyChunks[l].pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), (this.spear as ExplosiveSpear).explodeColor, null, 4, 18));
                                                }
                                                this.room.ScreenMovement(new Vector2?(this.room.physicalObjects[j][k].bodyChunks[l].pos), default(Vector2), 0.7f);
                                                this.room.PlaySound(SoundID.Fire_Spear_Explode, this.room.physicalObjects[j][k].bodyChunks[l].pos);
                                                this.room.InGameNoise(new InGameNoise(this.room.physicalObjects[j][k].bodyChunks[l].pos, 8000f, spear, 1f));

                                                spear.Destroy();
                                                this.Destroy();
                                                return;
                                            }

                                            if (this.spear.abstractSpear.electric)
                                            {
                                                (this.spear as MoreSlugcats.ElectricSpear).Electrocute(this.room.physicalObjects[j][k]);
                                            }
                                        }
                                        if (this.room.physicalObjects[j][k] is not Creature)
                                        {
                                            this.room.physicalObjects[j][k].HitByWeapon(this.spear);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.owner.TryGetShaw(out ShawData shawData))
            {
                if (this.hitSomething)
                {
                    shawData.dashFrame = 0;
                    this.owner.animation = Player.AnimationIndex.Flip;
                    this.owner.firstChunk.vel.y = 15f * this.force;
                    this.Destroy();
                }
                else
                {
                    if ((shawData.chargeSlashCounter < 0) || this.owner.bodyMode != Player.BodyModeIndex.ZeroG)
                    {
                        if (this.frame > lifeTime)
                        {
                            this.Destroy();
                        }
                        this.frame++;
                    }
                }
            }
            else
            {
                this.Destroy();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            if (this.spear != null)
            {
                this.spear.setVisible();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("atlas/longslash", true);
            sLeaser.sprites[0].anchorY = 0;
            sLeaser.sprites[0].width = this.size * 0.42f;
            sLeaser.sprites[0].height = this.size;
            sLeaser.sprites[0].rotation = Custom.VecToDeg(this.dir);
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            this.pos = (Vector2.Lerp(owner.firstChunk.pos, owner.firstChunk.lastPos, 0.35f) - (this.dir * (this.size / 3)));
            sLeaser.sprites[0].SetPosition(this.pos - camPos);

            sLeaser.sprites[0].color = Color.white;
            if (this.spear != null)
            {
                if (this.spear.abstractSpear.explosive)
                {
                    sLeaser.sprites[0].color = Color.red;
                }
                else if (this.spear.abstractSpear.electric && spear.abstractSpear.electricCharge > 0)
                {
                    sLeaser.sprites[0].color = new Color(0.5f, 0.80f, 1f);
                }
                else if (this.spear.abstractSpear.hue != 0)
                {
                    sLeaser.sprites[0].color = Custom.HSL2RGB(Custom.Decimal(this.spear.abstractSpear.hue + EggBugGraphics.HUE_OFF), 1f, 0.5f);
                }
            }

            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner = (newContatiner ?? rCam.ReturnFContainer("Midground"));
            foreach (FSprite sprite in sLeaser.sprites)
            {
                newContatiner.AddChild(sprite);
            }
        }

        public bool IsHit(Vector2 pos, out float radValue, out float distance)
        {
            radValue = this.hitRad * (0.25f + 0.75f * Mathf.Sin(Mathf.InverseLerp(0f, (float)lifeTime, (float)this.frame) * 3.1415927f));
            distance = Vector2.Distance(this.hitPos, pos);
            if (Mathf.Min(distance, float.MaxValue) < radValue)
            {
                return true;
            }
            return false;
        }
    }
}

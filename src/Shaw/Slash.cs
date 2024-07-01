using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using static Silkslug.Shaw;
using System.Collections.Generic;
using Noise;
using Silkslug.ColosseumRubicon.Boss;

namespace Silkslug
{
    public class Slash : CosmeticSprite
    {
        public Slash(Room room, Player owner, Spear spear, Vector2 dir, float size, float force, float damage, float stunFactor = 1f)
        {
            this.room = room;
            this.owner = owner;
            this.spear = spear;
            this.dir = dir;
            this.size = size;
            this.force = force;
            this.damage = damage;
            this.stunFactor = stunFactor;
            this.room.InGameNoise(new InGameNoise(this.owner.mainBodyChunk.pos, 8000f, this.owner, 1f));
        }

        public Vector2 dir;

        public new Vector2 pos;
        
        public Player owner;

        public Spear? spear;

        public Color color;

        public static int lifeTime = 4;

        public float size;

        public Vector2 hitVector
        {
            get { return new Vector2(this.size * 1.35f, this.size * 1f); }
        }

        public float hitRad {
            get { return this.size * 1f; }
        }

        public float force;

        public float damage;

        public float stunFactor;

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

            bool hasHitSometing = this.hitSomething;

            if (!this.hitSomething)
            {
                foreach (IDrawable drawable in this.room.drawableObjects)
                {
                    if (drawable != null && drawable is Slash && (drawable as Slash) != this)
                    {
                        Slash slash = (Slash)drawable;

                        slash.GetAABB(out Vector2 localMinB, out Vector2 localMaxB, true);
                        this.GetAABB(out Vector2 localMinA, out Vector2 localMaxA, true);
                        if (this.IsHit(slash.owner.firstChunk.pos + localMinB) || this.IsHit(slash.owner.firstChunk.pos + localMaxB) || this.IsHit(slash.owner.firstChunk.pos - localMinB) || this.IsHit(slash.owner.firstChunk.pos - localMaxB) || slash.IsHit(this.owner.firstChunk.pos + localMinA) || this.IsHit(this.owner.firstChunk.pos + localMaxA) || this.IsHit(this.owner.firstChunk.pos - localMinA) || this.IsHit(this.owner.firstChunk.pos - localMaxA))
                        {
                            this.hitSomething = true;
                            this.room.PlaySound(Sounds.HERRO_PARRY, this.owner.firstChunk.pos);
                        }
                    }

                    if (drawable != null && drawable is DashSlash)
                    {
                        DashSlash dashSlash = (DashSlash)drawable;
                        if (this.IsHit(dashSlash.hitPos, dashSlash.hitRad))
                        {
                            this.hitSomething = true;
                            this.room.PlaySound(Sounds.HERRO_PARRY, this.owner.firstChunk.pos);
                        }
                    }

                    if (drawable != null && drawable is HellKnight)
                    {
                        HellKnight boss = (HellKnight)drawable;
                        if (this.IsHit(boss.pos, boss.rad))
                        {
                            boss.TakeDamage(damage * 4f);
                            this.hitSomething = true;
                            this.room.PlaySound(SoundID.Spear_Stick_In_Creature, this.owner.firstChunk.pos);
                        }
                    }

                    if (drawable != null && drawable is SawBlade)
                    {
                        SawBlade saw = (SawBlade)drawable;
                        if (this.IsHit(saw.pos, saw.rad - 15))
                        {
                            this.hitSomething = true;
                            this.room.PlaySound(Sounds.HERRO_PARRY, saw.pos, 1f, 1.5f);
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
                                if (this.IsHit(this.room.physicalObjects[j][k].bodyChunks[l].pos, this.room.physicalObjects[j][k].bodyChunks[l].rad))
                                {
                                    if (this.room.physicalObjects[j][k] is Creature && !this.creaturesHit.Contains((this.room.physicalObjects[j][k] as Creature)) && (this.room.physicalObjects[j][k] is not Player || (ModManager.CoopAvailable && Custom.rainWorld.options.friendlyFire || this.room.game.IsArenaSession && this.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers)))
                                    {
                                        this.room.PlaySound(SoundID.Spear_Stick_In_Creature, this.room.physicalObjects[j][k].firstChunk);



                                        Vector2 force = this.dir * this.room.physicalObjects[j][k].TotalMass * 9f;
                                        if (ModManager.MSC && this.room.physicalObjects[j][k] is Player)
                                        {
                                            Player player = (this.room.physicalObjects[j][k] as Player);
                                            player.Violence(this.owner.mainBodyChunk, null, this.room.physicalObjects[j][k].firstChunk, null, Creature.DamageType.Stab, this.damage, 0f);
                                            player.stunDamageType = Creature.DamageType.Blunt;
                                            player.Stun(20);
                                            (player as Creature).SetKillTag(this.owner.abstractCreature);
                                            player.playerState.permanentDamageTracking += (double)(this.damage / player.Template.baseDamageResistance);
                                            player.firstChunk.vel += force;
                                            if (player.playerState.permanentDamageTracking >= 1.0)
                                            {
                                                player.Die();
                                            }
                                        }
                                        else
                                        {
                                            (this.room.physicalObjects[j][k] as Creature).Violence(this.owner.mainBodyChunk, force, this.room.physicalObjects[j][k].firstChunk, null, Creature.DamageType.Blunt, this.damage, 10f * stunFactor);
                                        }
                                        if (!(this.room.physicalObjects[j][k] as Creature).dead)
                                        {
                                            for (int v = 0; v < 20; v++)
                                            {
                                                Vector2 a = Custom.RNV();
                                                this.room.AddObject(new Spark(this.room.physicalObjects[j][k].bodyChunks[l].pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 10f, Random.value), this.color, null, 4, 18));
                                            }
                                        }

                                        this.creaturesHit.Add(this.room.physicalObjects[j][k] as Creature);
                                        this.hitSomething = true;

                                        if (this.spear != null && this.spear.abstractSpear.explosive)
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

                                        if (this.spear != null && this.spear.abstractSpear.electric)
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

                                        this.room.PlaySound(Sounds.HERRO_PARRY, weapon.firstChunk.pos);
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

            if (this.hitSomething && !hasHitSometing)
            {
                if (this.dir.x != 0)
                {
                    this.owner.firstChunk.vel.x = this.dir.normalized.x * -15f * this.force;
                }

                if (this.dir.y < 0)
                {
                    this.owner.firstChunk.vel.y = this.dir.normalized.y * -30f * this.force;
                }
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("assets/slash1", true);
            sLeaser.sprites[0].anchorY = 0;
            sLeaser.sprites[0].width = this.size * 0.5895f;
            sLeaser.sprites[0].height = this.size;;
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
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            if (frame > lifeTime)
            {
                sLeaser.sprites[0].SetElementByName("assets/slash2");
            }

            this.pos = (Vector2.Lerp(this.owner.firstChunk.pos, this.owner.firstChunk.lastPos, 0.35f) - (this.dir * (this.size / 3)));
            sLeaser.sprites[0].SetPosition(this.pos - camPos);

            this.color = Color.white;
            if (this.spear != null)
            {
                if (this.spear.abstractSpear.explosive)
                {
                    this.color = Color.red;
                }
                else if (this.spear.abstractSpear.electric && spear.abstractSpear.electricCharge > 0)
                {
                    this.color = new Color(0.5f, 0.80f, 1f);
                }
                else if (this.spear.abstractSpear.hue != 0)
                {
                    this.color = Custom.HSL2RGB(Custom.Decimal(this.spear.abstractSpear.hue + EggBugGraphics.HUE_OFF), 1f, 0.5f);
                }
            }
            sLeaser.sprites[0].color = this.color;

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

        public void GetAABB(out Vector2 min, out Vector2 max, bool local = false)
        {
            Vector2 ownerPos = this.owner.firstChunk.pos;
            Vector2 selfPos = this.pos;
            if (local)
            {
                ownerPos = Vector2.zero;
                selfPos = Vector2.zero;
            }
            min = ownerPos;
            max = ownerPos;

            if (this.dir.x > 0)
            {
                min = new Vector2(ownerPos.x, ownerPos.y - (this.hitVector.y / 2));
                max = new Vector2(selfPos.x + this.hitVector.x, selfPos.y + (this.hitVector.y / 2));
            }
            else if (this.dir.x < 0)
            {
                min = new Vector2(ownerPos.x - this.hitVector.x, ownerPos.y - (this.hitVector.y / 2));
                max = new Vector2(selfPos.x, selfPos.y + (this.hitVector.y / 2));
            }
            float downFac = 1.5f;
            if (this.dir.y > 0)
            {
                min = new Vector2(ownerPos.x - (this.hitVector.y / 2) * downFac, ownerPos.y);
                max = new Vector2(selfPos.x + (this.hitVector.y / 2) * downFac, selfPos.y + this.hitVector.x);
            }
            else if (this.dir.y < 0)
            {
                min = new Vector2(ownerPos.x - (this.hitVector.y / 2) * downFac, ownerPos.y - this.hitVector.x);
                max = new Vector2(selfPos.x + (this.hitVector.y / 2) * downFac, selfPos.y);
            }
        }
        public bool IsHit(Vector2 pos, float radius = 0)
        {
            float radValue = (this.size * 1.2f + radius) * (0.25f + 0.75f * Mathf.Sin(Mathf.InverseLerp(0f, (float)lifeTime, (float)this.frame) * 3.1415927f));
            float distance = Vector2.Distance(this.pos, pos);
            if (Mathf.Min(distance, float.MaxValue) > radValue)
            {
                return false;
            }

            GetAABB(out Vector2 min, out Vector2 max);

            min -= new Vector2(radius, radius);
            max += new Vector2(radius, radius);

            //Debug.Log($"in Min: {pos.x > min.x}, {pos.y > min.y}, in Max: ({pos.x < max.x}, {pos.y < max.y})");

            if ((pos.x > min.x && pos.y > min.y) && (pos.x < max.x && pos.y < max.y))
            {
                return true;
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
            this.room.InGameNoise(new InGameNoise(this.owner.mainBodyChunk.pos, 8000f, this.owner, 1f));
        }

        public Vector2 dir;

        public new Vector2 pos;

        public Player owner;

        public Spear spear;

        public Color color;

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
                        if (slash.IsHit(this.hitPos, this.hitRad))
                        {
                            this.room.PlaySound(Sounds.HERRO_PARRY, slash.pos);
                            hitSomething = true;
                        }
                    }
                    if (drawable != null && drawable is DashSlash && drawable as DashSlash != this)
                    {
                        DashSlash dashSlash = (DashSlash)drawable;
                        if ((dashSlash.hitPos - this.hitPos).magnitude < (dashSlash.hitRad + this.hitRad))
                        {
                            this.room.PlaySound(Sounds.HERRO_PARRY, dashSlash.pos);
                            hitSomething = true;
                        }
                    }

                    if (drawable != null && drawable is HellKnight )
                    {
                        HellKnight boss = (HellKnight)drawable;
                        if ((boss.pos - this.hitPos).magnitude < (boss.rad + this.hitRad))
                        {
                            boss.TakeDamage(damage * 4f);
                            this.room.PlaySound(SoundID.Spear_Stick_In_Creature, boss.pos);
                            hitSomething = true;
                        }
                    }

                    if (drawable != null && drawable is SawBlade)
                    {
                        SawBlade saw = (SawBlade)drawable;
                        if ((saw.pos - this.hitPos).magnitude < (saw.rad + this.hitRad))
                        {
                            this.room.PlaySound(Sounds.HERRO_PARRY, saw.pos, 1f, 2f);
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
                                                player.Stun(20);
                                                (this.room.physicalObjects[j][k] as Creature).SetKillTag(this.owner.abstractCreature);
                                                player.playerState.permanentDamageTracking += (double)(this.damage / player.Template.baseDamageResistance);
                                                player.firstChunk.vel += force;
                                                if (player.playerState.permanentDamageTracking >= 1.0)
                                                {
                                                    player.Die();
                                                }
                                            }
                                            else
                                            {
                                                (this.room.physicalObjects[j][k] as Creature).Violence(this.owner.mainBodyChunk, force, this.room.physicalObjects[j][k].firstChunk, null, Creature.DamageType.Blunt, this.damage, 10f);
                                            }
                                            if (!(this.room.physicalObjects[j][k] as Creature).dead)
                                            {
                                                for (int v = 0; v < 20; v++)
                                                {
                                                    Vector2 a = Custom.RNV();
                                                    this.room.AddObject(new Spark(this.room.physicalObjects[j][k].bodyChunks[l].pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 10f, Random.value), this.color, null, 4, 18));
                                                }
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
                    //ConsoleWrite("hit something");
                    shawData.dashFrame = 0;
                    this.owner.animation = Player.AnimationIndex.Flip;
                    foreach (var chuck in this.owner.bodyChunks)
                    {
                        chuck.vel.y = 0;
                    }
                    this.owner.firstChunk.vel.y = 15f * 2f; // * this.force;
                    this.Destroy();
                }
                else
                {
                    if ((shawData.chargeSlashCounter <= 0) || this.owner.bodyMode != Player.BodyModeIndex.ZeroG)
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
                this.spear.SetVisible();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("assets/longslash", true);
            sLeaser.sprites[0].anchorY = 0;
            sLeaser.sprites[0].width = this.size * 0.42f;
            sLeaser.sprites[0].height = this.size;
            sLeaser.sprites[0].rotation = Custom.VecToDeg(this.dir);
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            this.pos = (Vector2.Lerp(this.owner.firstChunk.pos, this.owner.firstChunk.lastPos, 0.35f) - (this.dir * (this.size / 3)));
            sLeaser.sprites[0].SetPosition(this.pos - camPos);

            this.color = Color.white;
            if (this.spear != null)
            {
                if (this.spear.abstractSpear.explosive)
                {
                    this.color = Color.red;
                }
                else if (this.spear.abstractSpear.electric && spear.abstractSpear.electricCharge > 0)
                {
                    this.color = new Color(0.5f, 0.80f, 1f);
                }
                else if (this.spear.abstractSpear.hue != 0)
                {
                    this.color = Custom.HSL2RGB(Custom.Decimal(this.spear.abstractSpear.hue + EggBugGraphics.HUE_OFF), 1f, 0.5f);
                }
            }
            sLeaser.sprites[0].color = this.color;

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

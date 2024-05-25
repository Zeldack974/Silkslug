using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using D = UnityEngine.Debug;
using RWCustom;
using UnityEngine.PlayerLoop;
using MoreSlugcats;
using static Silkslug.ColosseumRubicon.Boss.HellKnight.Attack;
using System.Diagnostics;

namespace Silkslug.ColosseumRubicon.Boss
{
    public class HellKnight : UpdatableAndDeletable, IDrawable
    {
        public Vector2 pos = Vector2.zero;
        public int rad = 35;
        public Halo.TempleGuardHalo halo;
        public bool showHitbox = false;
        public Player player;

        public float maxHealth = 215f;
        public float health;
        public bool dead = false;
        public Attack currentAttack;


        public static bool UpperArena => BossManager.Instance.upperArena;
        public static int Phase { get => BossManager.Instance.phase; set => BossManager.Instance.phase = value; }

        public List<Vector2> arena1Pos = new List<Vector2>();
        public List<Vector2> arena2Pos = new List<Vector2>();

        public static List<string> arena1Attacks = new List<string>();
        public static List<string> arena2Attacks = new List<string>();

        public FadeOut fadeOut;

        public HellKnight(Room room, Vector2 pos)
        {
            this.room = room;
            this.pos = pos;
            halo = new Halo.TempleGuardHalo(this);
            room.AddObject(halo);
            health = maxHealth;

            player = base.room.game.Players[0].realizedCreature as Player;

            arena1Pos.Add(new Vector2(350, 372));
            arena1Pos.Add(new Vector2(700, 372));
            arena1Pos.Add(new Vector2(1050, 372));

            arena2Pos.Add(new Vector2(700, 1335));
            arena2Pos.Add(new Vector2(700, 1020));
            arena2Pos.Add(new Vector2(1160, 1180));
            arena2Pos.Add(new Vector2(240, 1180));

            arena1Attacks.AddRange(new List<string>()
            {
                "lasercage",
                "floorbeam",
                "laserwall",
                "laserhalf",
                "lasercircle"
            });

            arena2Attacks.AddRange(new List<string>()
            {
                "lasercircle",
                "lasercanon",
                "doublelaser",
                "floorbeam",
                "karma10"
            });

            currentAttack = new Attack.Cooldown(this);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);




            halo.overridePos = pos + new Vector2(0, 10);
            //UnityEngine.Debug.Log($"mousePos: {new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + room.game.cameras[0].pos}");
            //ConsoleWrite($"HP: {health}, damage taken: {maxHealth - health}, Phase: {Phase}");

            //if (Phase == 2)
            //{
            //    pos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + room.game.cameras[0].pos;
            //    return;
            //}

            if (dead)
            {
                //for (int i = 0; i < room.updateList.Count; i++)
                //{
                //    if (room.updateList[i] is Laser || room.updateList[i] is KarmicBomb)
                //    {
                //        room.updateList[i].Destroy();
                //    }
                //}
                return;
            }

            if (Phase == 1 && fadeOut != null)
            {
                if (fadeOut.IsDoneFading())
                {
                    Teleport(arena2Pos[1]);
                    player.SuperHardSetPosition(arena2Pos[1]);
                    Phase = 2;
                    fadeOut.Destroy();
                    fadeOut = null;
                    room.AddObject(new FadeOut(room, Color.white, 60 * 1, true));
                }
                return;
            }

            ConsoleWrite($"phase {Phase} need to go to phase: [2]{health <= (maxHealth - 100f)} [3]{health <= ((maxHealth - 100f) - 6f * 3f)}");


            if (health <= (maxHealth - 100f) && Phase == 0)
            {
                Phase = 1;
                health = maxHealth - 100f;
                this.Teleport(arena1Pos[1]);
                currentAttack = new Attack.Cooldown(this);
            }

            bool attackFinished = currentAttack.Update();
            if (attackFinished)
            {

                if (Phase == 1)
                {
                    if (currentAttack.name == "deathcircle")
                    {
                        ConsoleWrite("go to phase 3");
                        currentAttack = new Attack.Cooldown(this, 60 * 3);
                        fadeOut = new FadeOut(room, Color.white, 60 * 3, false);
                        room.AddObject(fadeOut);
                        (room.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = Math.Min(player.Karma + 5, 9);
                        room.game.cameras[0].hud.karmaMeter.reinforceAnimation = 1;
                }
                    else
                    {
                        ConsoleWrite("start death circle");
                        currentAttack = new DeathCircle(this);
                    }
                    return;
                }

                //ConsoleWrite($"attack finished: {currentAttack.name}");
                if (currentAttack.name == "cooldown")
                {
                    if (Phase == 0)
                    {
                        this.Teleport(arena1Pos[UnityEngine.Random.Range(0, arena1Pos.Count)]);
                        currentAttack = Attack.GetAttack(this, arena1Attacks[UnityEngine.Random.Range(0, arena1Attacks.Count)]);
                    }
                    else if (Phase == 2)
                    {
                        this.Teleport(arena2Pos[UnityEngine.Random.Range(0, arena2Pos.Count)]);
                        currentAttack = Attack.GetAttack(this, arena2Attacks[UnityEngine.Random.Range(0, arena2Attacks.Count)]);
                    }
                }
                else
                {
                    currentAttack = new Attack.Cooldown(this, currentAttack.cooldown);
                }
            }
        }

        public void TakeDamage(float damage)
        {
            ConsoleWrite("teleport to " + pos);
            //if (Phase == 0)
            //{
            //    damage = 100;
            //}
            //if (Phase == 1)
            //{
            //    damage = 6 * 3;
            //}
            //if (Phase == 2)
            //{
            //    damage = health;
            //}
            damage = health;
            UnityEngine.Debug.Log($"HellKnight took {damage} damage {health}/{maxHealth}");

            health = Math.Max(health - damage, 0);
            if (health == 0 && !dead)
            {
                ConsoleWrite("Die! PURGATORY GUARDIAN HELL KNIGHT");
                dead = true;
                currentAttack = null;
                room.AddObject(new TimeInverter22000(room));
            }
        }

        public void Teleport(Vector2 pos)
        {
            if (Custom.DistLess(pos, this.pos, 10)) return;
            player.room.AddObject(new ShockWave(this.pos, 750f, 0.35f, 15, true));
            this.room.PlaySound(SoundID.HUD_Pause_Game, this.pos, 2f, UnityEngine.Random.Range(0.5f, 0.75f));
            this.pos = pos;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];
            sLeaser.sprites[0] = new FSprite("atlas/hellknightbody", true);
            sLeaser.sprites[1] = new FSprite("Futile_White", true); // debug
            sLeaser.sprites[1].shader = this.room.game.rainWorld.Shaders["VectorCircle"];
            sLeaser.sprites[1].isVisible = showHitbox;
            sLeaser.sprites[2] = new FSprite("guardEye", true);
            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = this.pos - camPos;

            sLeaser.sprites[0].SetPosition(pos - new Vector2(3, 50f));
            sLeaser.sprites[0].scale = 0.8f;

            if (showHitbox)
            {
                sLeaser.sprites[1].SetPosition(pos);
                sLeaser.sprites[1].scale = rad / 4;
            }

            sLeaser.sprites[2].color = new Color(Mathf.Lerp(0.75f, 1f, UnityEngine.Random.value), 0f, 0f);
            sLeaser.sprites[2].SetPosition(pos + new Vector2(0, 23));
            sLeaser.sprites[2].scale = 2.25f;


            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Background");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContainer.AddChild(fsprite);
            }
        }

        public class Attack
        {
            public string name;
            public int cooldown;
            public HellKnight owner;
            public Attack(HellKnight owner)
            {
                this.owner = owner;
            }

            public virtual bool Update()
            {
                return true;
            }

            public static Attack GetAttack(HellKnight boss, string name)
            {
                switch (name.ToLower())
                {
                    case "cooldown":
                        return new Cooldown(boss);
                    case "floorbeam":
                        return new FloorBeam(boss);
                    case "laserwall":
                        return new LaserWall(boss);
                    case "lasercage":
                        return new LaserCage(boss);
                    case "laserhalf":
                        return new LaserHalf(boss);
                    case "lasercircle":
                        return new LaserCircle(boss);
                    case "deathcircle":
                        return new DeathCircle(boss);
                    case "lasercanon":
                        return new LaserCanon(boss);
                    case "doublelaser":
                        return new DoubleLaser(boss);
                    case "karma10":
                        return new Karma10(boss);
                    default:
                        Plugin.LogError($"Unknow attack {name}");
                        return new Attack(boss);
                }
            }


            public class Cooldown : Attack
            {
                public int attackTicks;
                public Cooldown(HellKnight ower, int num = 60) : base(ower) 
                {
                    attackTicks = num;
                    name = "cooldown";
                    cooldown = 60;
                }

                public override bool Update()
                {
                    attackTicks--;
                    return attackTicks <= 0;
                }
            }

            public class FloorBeam : Attack
            {
                public int attackTicks;
                public int remainingBeams = 6;
                public int spawnCooldown = 0;
                public FloorBeam(HellKnight ower) : base(ower)
                {
                    name = "floorbeam";
                    cooldown = 25;
                }

                public override bool Update()
                {
                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }

                    float up = owner.room.RoomRect.bottom + (owner.room.RoomRect.top - owner.room.RoomRect.bottom) / 2;
                    if (UpperArena)
                    {
                        up = owner.room.RoomRect.top;
                    }
                    Vector2 pos = new Vector2(this.owner.player.firstChunk.pos.x, up);
                    owner.room.AddObject(new Laser(this.owner, pos, Custom.VecToDeg(new Vector2(0, -1)), 20f, 40, Laser.Type.PILLAR)
                    {
                        posLifeTime = 20,
                    });

                    remainingBeams--;
                    spawnCooldown = 30;

                    return remainingBeams <= 0;
                }
            }

            public class LaserWall : Attack
            {
                public int attackTicks;
                public int remainingBeams = 3;
                public int spawnCooldown = 0;

                public LaserWall(HellKnight ower) : base(ower)
                {
                    name = "laserwall";
                    cooldown = 60 * 3;
                    remainingBeams = Random.Range(1, 2 + 1);
                }

                public override bool Update()
                {
                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }

                    int empltyIndex = UnityEngine.Random.Range(1, 8);
                    int laserLifeTime = 60;
                    laserLifeTime += (int)(115 * Math.Abs(owner.player.mainBodyChunk.pos.x - (180 + 40 + (empltyIndex * 40 * 3))) * 0.001f);


                    for (int i = 0; i < 9; i++)
                    {
                        if (i == empltyIndex) continue;
                        for (int j = 0; j < 3; j++)
                        {
                            Vector2 pos = new Vector2(180 + (j * 40) + (i * 40 * 3), owner.room.RoomRect.bottom + (owner.room.RoomRect.top - owner.room.RoomRect.bottom) / 2);
                            owner.room.AddObject(new Laser(this.owner, pos, Custom.VecToDeg(new Vector2(0, -1)), 15f, laserLifeTime, Laser.Type.LIGHTNING));
                        }

                    }

                    remainingBeams--;
                    spawnCooldown = laserLifeTime + 30;


                    return remainingBeams <= 0;
                }
            }

            public class LaserCage : Attack
            {
                public int attackTicks;
                public int remainingBeams = 3;
                public int spawnCooldown = 0;

                public LaserCage(HellKnight ower) : base(ower)
                {
                    name = "lasercage";
                    cooldown = 60; // 60 * 2
                    remainingBeams = 3;
                }

                public override bool Update()
                {
                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }

                    int lasers = 18;
                    int startAdd = 20;
                    if (remainingBeams%2 == 0)
                    {
                        lasers = 17;
                        startAdd = 40 + 10;
                    }

                    for (int i = 0; i < lasers; i++)
                    {

                        Vector2 pos = new Vector2(180 + startAdd + (i * 20 * 3), owner.room.RoomRect.bottom + (owner.room.RoomRect.top - owner.room.RoomRect.bottom) / 2);
                        owner.room.AddObject(new Laser(this.owner, pos, Custom.VecToDeg(new Vector2(0, -1)), 5f, 30, Laser.Type.LIGHTNING));

                    }

                    remainingBeams--;
                    spawnCooldown = 45;


                    return remainingBeams <= 0;
                }
            }

            public class LaserHalf : Attack
            {
                public LaserHalf(HellKnight ower) : base(ower)
                {
                    name = "laserhalf";
                    cooldown = 60;
                }

                public override bool Update()
                {
                    int num = 40;
                    if (Random.Range(0, 2) == 0)
                    {
                        num = -num;
                    }


                    for (int i = 0; i < 14; i++)
                    {

                        Vector2 pos = new Vector2(700 + 0 + (i * num), owner.room.RoomRect.bottom + (owner.room.RoomRect.top - owner.room.RoomRect.bottom) / 2);
                        owner.room.AddObject(new Laser(this.owner, pos, Custom.VecToDeg(new Vector2(0, -1)), 20f, 90, Laser.Type.LIGHTNING));

                    }


                    return true;
                }
            }

            public class LaserCircle : Attack
            {
                public int attackTicks;
                public int remainingBeams = 3;
                public int spawnCooldown = 0;

                public LaserCircle(HellKnight ower) : base(ower)
                {
                    name = "lasercircle";
                    cooldown = 60; // 60 * 2
                    remainingBeams = 3;
                }

                public override bool Update()
                {
                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }

                    float num = 0f;
                    if (remainingBeams % 2 == 0)
                    {
                        num = 0.5f;
                    }
                    num += Random.value * 0.125f;

                    for (int i = 0; i < 8; i++)
                    {
                        owner.room.AddObject(new Laser(this.owner, this.owner.pos, (((i + num) /8f) * 360f), 20f, 30, Laser.Type.LIGHTNING));

                    }

                    remainingBeams--;
                    spawnCooldown = 35;


                    return remainingBeams <= 0;
                }
            }

            public class DeathCircle : Attack
            {
                public int spawnCooldown = 0;
                public float angle = Custom.VecToDeg(new Vector2(-1, 0));

                public DeathCircle(HellKnight ower) : base(ower)
                {
                    name = "deathcircle";
                    cooldown = 0;
                }

                public override bool Update()
                {
                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }



                    owner.room.AddObject(new Laser(this.owner, this.owner.pos, angle, 10f, 45, Laser.Type.LIGHTNING));
                    angle += 8f;
                    spawnCooldown = 1;


                    return owner.health < ((owner.maxHealth - 100f) - 6f * 3f);
                }
            }

            public class LaserCanon : Attack
            {
                public int attackTicks;
                public int remainingBeams = 3;
                public int spawnCooldown = 0;
                public List<Laser> lasers = new List<Laser>();

                public LaserCanon(HellKnight ower) : base(ower)
                {
                    name = "lasercanon";
                    cooldown = 60;
                    remainingBeams = 6;
                }

                public override bool Update()
                {
                    for (int i = 0; i < lasers.Count; i++)
                    {
                        //if (lasers[i] == null) continue;
                        if (lasers[i].slatedForDeletetion)
                        {
                            lasers[i].Destroy();
                            lasers.Remove(lasers[i]);
                        }
                        else
                        {
                            if (lasers[i].life > 10)
                            {
                                lasers[i].dir = Custom.VecToDeg(Custom.DirVec(this.owner.pos, this.owner.player.mainBodyChunk.pos));
                            }
                        }
                    }

                    if (remainingBeams > 0)
                    {
                        if (spawnCooldown > 0)
                        {
                            spawnCooldown--;
                            return false;
                        }

                        lasers.Add(new Laser(this.owner, this.owner.pos, Custom.VecToDeg(Custom.DirVec(this.owner.pos, this.owner.player.mainBodyChunk.pos)), 20f, 90, Laser.Type.LIGHTNING));
                        owner.room.AddObject(lasers.Last());

                        remainingBeams--;
                        spawnCooldown = 30;
                    }

                    return remainingBeams <= 0 && lasers.Count == 0;
                }
            }

            public class DoubleLaser : Attack
            {
                public int attackTicks;
                public int spawnCooldown = 0;
                public int steps = 6;
                public int step;
                public bool fromCenter;

                public DoubleLaser(HellKnight ower) : base(ower)
                {
                    name = "doublelaser";
                    cooldown = 60;
                    step = steps;
                    fromCenter = Random.value < 0.5f;
                }

                public override bool Update()
                {
                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }


                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }

                    int lifetime = 20;
                    if (step == steps)
                    {
                        lifetime = 60;
                    }

                    for (int i = -1; i <= 1; i += 2)
                    {

                        float num = Mathf.Lerp(1000f, 0f, (float)step/(float)steps) * i;
                        if (!fromCenter)
                        {
                            num = Mathf.Lerp(0f, 1000f, (float)step / (float)steps) * i;
                        }

                        for (int j = -1; j <= 1; j++)
                        {
                            Vector2 pos = new Vector2(700f + num + (40f * j), owner.room.RoomRect.top);
                            owner.room.AddObject(new Laser(this.owner, pos, Custom.VecToDeg(Custom.DirVec(pos, new Vector2(pos.x, owner.room.RoomRect.bottom))), 20f, lifetime, Laser.Type.LIGHTNING));
                        }
                    }

                    step--;
                    spawnCooldown = lifetime;
                    return step < 0;
                }
            }

            public class Karma10 : Attack
            {
                public int attackTicks;
                public int remainingBeams = 5;
                public int spawnCooldown = 0;

                public Karma10(HellKnight ower) : base(ower)
                {
                    name = "karma10";
                    cooldown = 60;
                    remainingBeams = 3;
                }

                public override bool Update()
                {
                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }

                    Vector2 pos = owner.arena2Pos[Random.Range(0, owner.arena2Pos.Count)];
                    owner.room.AddObject(new KarmicBomb(this.owner, pos, 400f, 60, true));

                    

                    remainingBeams--;
                    spawnCooldown = 30;


                    return remainingBeams <= 0;
                }
            }
        }
    }
}

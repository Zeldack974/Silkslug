using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using UnityEngine.PlayerLoop;
using MoreSlugcats;

namespace Silkslug.ColosseumRubicon.Boss
{
    public class HellKnight : UpdatableAndDeletable, IDrawable
    {
        public Vector2 pos = Vector2.zero;
        public int rad = 35;
        public Halo.TempleGuardHalo halo;
        public bool showHitbox = false;
        public Player player;

        public float maxHealth = 300f;
        public float health = 300f;
        public bool dead = false;
        public Attack currentAttack;

        public static bool UpperArena => BossManager.Instance.upperArena;
        public static int Phase { get => BossManager.Instance.phase; set => BossManager.Instance.phase = value; }

        public List<Vector2> arena1Pos = new List<Vector2>();
        public List<Vector2> arena2Pos = new List<Vector2>();

        public static List<string> arena1Attacks = new List<string>();
        public static List<string> arena2Attacks = new List<string>();

        public HellKnight(Room room, Vector2 pos)
        {
            this.room = room;
            this.pos = pos;
            halo = new Halo.TempleGuardHalo(this);
            room.AddObject(halo);

            player = base.room.game.Players[0].realizedCreature as Player;

            arena1Pos.Add(new Vector2(350, 372));
            arena1Pos.Add(new Vector2(700, 372));
            arena1Pos.Add(new Vector2(1050, 372));

            arena2Pos.Add(new Vector2(700, 1185));
            arena2Pos.Add(new Vector2(700, 910));
            arena2Pos.Add(new Vector2(1160, 1200));
            arena2Pos.Add(new Vector2(240, 1200));

            arena1Attacks.Add("cooldown");

            currentAttack = new Attack.Cooldown(this);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            halo.overridePos = pos + new Vector2(0, 10);

            bool attackFinished = currentAttack.Update();
            if (attackFinished)
            {
                if (currentAttack is Attack.Cooldown)
                {
                    if (Phase == 0)
                    {
                        currentAttack = Attack.GetAttack(this, arena1Attacks[UnityEngine.Random.Range(0, arena1Attacks.Count)]);
                    }
                    else if (Phase == 2)
                    {
                        currentAttack = Attack.GetAttack(this, arena2Attacks[UnityEngine.Random.Range(0, arena2Attacks.Count)]);
                    }
                    else
                    {
                        //currentAttack
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
            health = Math.Max(health - damage, 0);
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
                    name = "none";
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
                public int remainingBeams = 10;
                public int spawnCooldown = 0;
                public FloorBeam(HellKnight ower) : base(ower)
                {
                    name = "floorbeam";
                    cooldown = 60 * 2;
                }

                public override bool Update()
                {
                    if (spawnCooldown > 0)
                    {
                        spawnCooldown--;
                        return false;
                    }

                    ConsoleWrite("new laser");
                    Vector2 pos = new Vector2(this.owner.player.firstChunk.pos.x, owner.room.RoomRect.bottom + (owner.room.RoomRect.top - owner.room.RoomRect.bottom) / 2);
                    owner.room.AddObject(new Laser(this.owner, pos, Custom.VecToDeg(new Vector2(0, -1)), 30f, 40, Laser.Type.PILLAR)
                    {
                        posLifeTime = 20,
                    });

                    remainingBeams--;
                    spawnCooldown = 30;

                    return remainingBeams <= 0;
                }
            }
        }
    }
}

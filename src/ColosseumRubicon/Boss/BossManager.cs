using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System.Threading;


namespace Silkslug.ColosseumRubicon.Boss
{
    public class BossManager : UpdatableAndDeletable
    {
        public static BossManager Instance;

        public bool initialize = false;

        public Spear playerSpear = null;

        public FadeOut fadeOut;

        public DemonEye eye;

        public int phase = 0;


        public bool upperArena { get => phase > 1; }

        public BossManager(Room room)
        {
            ConsoleWrite("Creating BossManager");
            this.room = room;
            Instance = this;

        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (fadeOut != null && fadeOut.IsDoneFading())
            {
                ConsoleWrite("open the eye");
                fadeOut.Destroy();
                fadeOut = null;
                eye.Open();
            }
        }

        public override void Destroy()
        {
            ConsoleWrite("BossManager Destroyed");
            Instance = null;
            base.Destroy();
        }

        public static void Initialize(Room room)
        {
            if (Instance != null)
            {
                Instance.Destroy();
            }
            room.AddObject(new BossManager(room));
            Instance.Init();
        }

        public void Init()
        {
            ConsoleWrite("Initialize BossManager");

            foreach (var obj in room.updateList)
            {
                if (obj is FadeOut)
                    obj.Destroy();
            }

            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count(); j++)
                {
                    if (room.physicalObjects[i][j] is not Player)
                        room.physicalObjects[i][j].Destroy();
                }
            }

            fadeOut = new FadeOut(room, Color.white, 210, true);
            Manager.DisablePauseMenu = true;
            eye = new DemonEye(room);
            room.AddObject(fadeOut);
            room.AddObject(eye);

            Plugin.Log("request music");
            this.room.game.manager.musicPlayer.GameRequestsSong(new MusicEvent()
            {
                prio = 100,
                songName = "RW_91 - Sheer Ice Torrent",
                loop = true,
                cyclesRest = 0,
                stopAtDeath = true,
            });
            this.room.game.manager.musicPlayer.song.subTracks[0].source.time = 3;


            //ConsoleWrite($"room: {room.abstractRoom.realizedRoom}, ready for players: {room.ReadyForPlayer}, room obj: {room.updateList.Count}");
            GiveSpears();

            initialize = true;
        }

        public void SpawnBoss()
        {
            ConsoleWrite("spawning boss");
            room.AddObject(new HellKnight(room, eye.eyeCenter - new Vector2(0, 50)));
            Manager.DisablePauseMenu = false;
        }

        public void GiveSpears()
        {
            ConsoleWrite($"giving spears to {room.game.AlivePlayers.Count} players");

            for (int i = 0; i < room.game.AlivePlayers.Count; i++)
            {
                for (int g = 0; g < room.game.AlivePlayers[i].realizedCreature.grasps.Length; g++)
                {
                    if (room.game.AlivePlayers[i].realizedCreature.grasps[g] != null && room.game.AlivePlayers[i].realizedCreature.grasps[g].grabbed != null && !room.game.AlivePlayers[i].realizedCreature.grasps[g].discontinued)
                    {
                        if (room.game.AlivePlayers[i].realizedCreature.grasps[g].grabbed is Creature)
                        {
                            room.game.AlivePlayers[i].realizedCreature.ReleaseGrasp(g);
                        }
                        else
                        {
                            room.game.AlivePlayers[i].realizedCreature.grasps[g].grabbed.Destroy();
                        }
                    }
                }

                AbstractSpear spear = new AbstractSpear(this.room.world, null, this.room.abstractRoom.entities[i].pos, this.room.game.GetNewID(), false, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));

                //this.room.abstractRoom.entities.Add(spear);
                spear.RealizeInRoom();
                playerSpear = spear.realizedObject as Spear;
                (room.game.AlivePlayers[i].realizedCreature as Player).SlugcatGrab(spear.realizedObject, 0);

            }
        }
    }
}

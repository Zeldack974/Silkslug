using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RWCustom;


namespace Silkslug.ColosseumRubicon
{
    public class RoomManager : UpdatableAndDeletable
    {
        public ArenaChallenges.ArenaChallenge challenge
        {
            get { return ArenaChallenges.challenges[ArenaChallenges.currentArena]; }
        }

        public static List<RoomManager> roomManagers = new List<RoomManager>();

        public static RoomManager GetRoomManager(Room room) => roomManagers.Find(r => r.room == room);

        public RoomManager(Room room)
        {
            this.room = room;
            roomManagers.Add(this);


        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            //ConsoleWrite("[update Update]");
            if (creatureSpawned)
            {
                bool allCreatureDead = true;
                int count = 0;

                for (int j = 0; j < this.room.physicalObjects.Length; j++)
                {
                    for (int k = 0; k < this.room.physicalObjects[j].Count; k++)
                    {
                        if (this.room.physicalObjects[j][k] is Creature && this.room.physicalObjects[j][k] is not Player && !this.room.physicalObjects[j][k].slatedForDeletetion)
                        {
                            allCreatureDead = allCreatureDead && (this.room.physicalObjects[j][k] as Creature).dead;
                            count++;
                        }
                    }
                }

                for (int i = this.room.game.shortcuts.transportVessels.Count - 1; i >= 0; i--)
                {
                    if (this.room.game.shortcuts.transportVessels[i].creature != null && this.room.game.shortcuts.transportVessels[i].creature is not Player && !this.room.game.shortcuts.transportVessels[i].creature.slatedForDeletetion)
                    {
                        allCreatureDead = allCreatureDead && this.room.game.shortcuts.transportVessels[i].creature.dead;
                        count++;
                    }
                }

                //ConsoleWrite($"{count} creatures alive");


                if (allCreatureDead)
                {
                    if (creatureDetected && !transition)
                    {
                        ConsoleWrite("next challenge");
                        NextChallenge();
                        transition = true;
                    }
                }
                else
                {
                    creatureDetected = true;
                    //for (int i = this.room.game.shortcuts.transportVessels.Count - 1; i >= 0; i--)
                    //{
                    //    this.room.game.shortcuts.transportVessels[i].wait = 2;
                    //}
                }
            }
        }

        public void NextChallenge()
        {
            ConsoleWrite("previous challenge " + ArenaChallenges.currentArena);
            ArenaChallenges.currentArena++;
            ConsoleWrite($"go to challenge {ArenaChallenges.currentArena} in arena {ArenaChallenges.challenges[ArenaChallenges.currentArena].roomName}");
            room.AddObject(new Warp.NextChallengeTeleport(room));
        }

        public void spawnCreatures()
        {
            ConsoleWrite($"Spawn creatures for challenge {ArenaChallenges.currentArena}");
            foreach (ArenaChallenges.CreaturePlacment spawn in challenge.spawns)
            {
                ConsoleWrite();
                ConsoleWrite($"try spawn {spawn.number} {spawn.creature} in {spawn.denId} :");
                for (int i = 1; i <= spawn.number; i++)
                {
                    int node = spawn.denId;

                    CreatureTemplate.Type type = WorldLoader.CreatureTypeFromString(spawn.creature);
                    if (node == -1)
                        node = Array.IndexOf(room.abstractRoom.nodes, room.abstractRoom.nodes.Where(n => n.type == AbstractRoomNode.Type.Den).OrderBy(x => Guid.NewGuid()).First()); // StaticWorld.GetCreatureTemplate(type).mappedNodeTypes[n.type.Index]

                    if (node == -1)
                    {
                        Debug.LogError("try to spawn creature on node -1");
                        return;
                    }

                    AbstractCreature ent = new AbstractCreature(room.game.world, StaticWorld.GetCreatureTemplate(type), null, room.LocalCoordinateOfNode(node), room.game.GetNewID());
                    //room.abstractRoom.MoveEntityToDen(ent);
                    ent.Move(room.LocalCoordinateOfNode(node));
                    ent.RealizeInRoom();

                    ConsoleWrite($"spawn {type.value} in {node}");
                }
            }

            CreatePlayerHate();

            creatureSpawned = true;
            GiveSpear();
        }

        public void ResetRoom()
        {
            AbstractRoom abstractRoom = room.abstractRoom;
            abstractRoom.Abstractize();
            abstractRoom.RealizeRoom(abstractRoom.world, abstractRoom.world.game);
            abstractRoom.realizedRoom.AddObject(this);
            this.room = abstractRoom.realizedRoom;

            creatureSpawned = false;
            creatureDetected = false;
            transition = false;
        }

        public void GiveSpear()
        {

            for (int i = 0; i < room.game.AlivePlayers.Count; i++)
            {
                for (int g = 0; g < room.game.AlivePlayers[i].realizedCreature.grasps.Length; g++)
                {
                    if (room.game.AlivePlayers[i].realizedCreature.grasps[g] != null && room.game.AlivePlayers[i].realizedCreature.grasps[g].grabbed != null && !room.game.AlivePlayers[i].realizedCreature.grasps[g].discontinued) // (room.game.AlivePlayers[i].realizedCreature.grasps[g] != null && room.game.AlivePlayers[i].realizedCreature.grasps[g].grabbed != null && !room.game.AlivePlayers[i].realizedCreature.grasps[g].discontinued && room.game.AlivePlayers[i].realizedCreature.grasps[g].grabbed is Creature && (!(room.game.AlivePlayers[i].realizedCreature.grasps[g].grabbed is Player) || !(room.game.AlivePlayers[i].realizedCreature.grasps[g].grabbed as Player).isSlugpup))
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
                AbstractSpear spear;
                switch (ArenaChallenges.CurrentArena.spear)
                {
                    case "explosive":
                        spear = new AbstractSpear(this.room.world, null, this.room.abstractRoom.entities[i].pos, this.room.game.GetNewID(), true, false);
                        break;
                    case "hell":
                        spear = new AbstractSpear(this.room.world, null, this.room.abstractRoom.entities[i].pos, this.room.game.GetNewID(), false, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
                        break;
                    case "electric":
                        spear = new AbstractSpear(this.room.world, null, this.room.abstractRoom.entities[i].pos, this.room.game.GetNewID(), false, true);
                        break;
                    default:
                        spear = new AbstractSpear(this.room.world, null, this.room.abstractRoom.entities[i].pos, this.room.game.GetNewID(), false, false);
                        break;
                }

                this.room.abstractRoom.entities.Add(spear);
                spear.RealizeInRoom();
                (room.game.AlivePlayers[i].realizedCreature as Player).SlugcatGrab(spear.realizedObject, 0);
                    
            }

        }

        public void CreatePlayerHate()
        {
            for (int i = 0; i < base.room.abstractRoom.creatures.Count; i++)
            {
                if (base.room.abstractRoom.creatures[i].state.socialMemory != null && base.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
                {
                    for (int j = 0; j < this.room.game.Players.Count; j++)
                    {
                        base.room.abstractRoom.creatures[i].state.socialMemory.GetOrInitiateRelationship(this.room.game.Players[j].ID).like = -1f;
                        base.room.abstractRoom.creatures[i].state.socialMemory.GetOrInitiateRelationship(this.room.game.Players[j].ID).tempLike = -1f;
                    }
                }
            }
        }

        public bool creatureSpawned;
        public bool creatureDetected;
        public bool transition;

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Silkslug.ColosseumRubicon
{
    public class RoomManager : UpdatableAndDeletable
    {
        public ArenaChallenges.ArenaChallenge challenge
        {
            get { return ArenaChallenges.challenges.First(e => e.roomName.ToLower() == room.abstractRoom.name.ToLower()); }
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


            if (creatureSpawned)
            {
                Debug.Log($"{room.abstractRoom.creatures.Where(c => c.realizedCreature == null || (c.realizedCreature is not Player && !c.realizedCreature.dead)).Count()} / {room.abstractRoom.creatures.Count()} creatures alive");
                bool allCreatureDead = true;
                foreach (AbstractCreature abstractCreature in room.abstractRoom.creatures) // ERREUR : contient que le joeur
                {
                    allCreatureDead = allCreatureDead && (abstractCreature.realizedCreature == null || abstractCreature.realizedCreature is Player || abstractCreature.realizedCreature.dead);
                }

                if (allCreatureDead)
                {
                    Debug.Log("next challenge");
                    //NextChallenge();
                }
            }
        }

        public void NextChallenge()
        {
            ArenaChallenges.currentArena++;
            room.AddObject(new Warp.NextChallengeTeleport(room));
        }

        public void spawnCreatures()
        {
            ConsoleWrite("spawnCreatures");
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

            creatureSpawned = true;
        }

        public void ResetRoom()
        {
            AbstractRoom abstractRoom = room.abstractRoom;
            abstractRoom.Abstractize();
            abstractRoom.RealizeRoom(abstractRoom.world, abstractRoom.world.game);
            abstractRoom.realizedRoom.AddObject(this);
            this.room = abstractRoom.realizedRoom;

            creatureSpawned = false;
        }

        public bool creatureSpawned;
    }
}

using MoreSlugcats;
using System;
using UnityEngine;
using RWCustom;
using HUD;
using Silkslug.ColosseumRubicon.Boss;

namespace Silkslug.ColosseumRubicon;

internal class Warp
{
    public static void OnEnable()
    {
        //On.World.GetAbstractRoom_string += World_GetAbstractRoom_string;

        On.MoreSlugcats.MSCRoomSpecificScript.VS_E05WrapAround.Update += VS_E05WrapAround_Update;
        On.OverWorld.InitiateSpecialWarp += OverWorld_InitiateSpecialWarp;

        On.OverWorld.WorldLoaded += OverWorld_WorldLoaded;
        On.Player.Update += Player_Update;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        //ConsoleWrite($"pos: {self.mainBodyChunk.pos}");
        //ConsoleWrite($"WorldPos: {self.abstractCreature.pos.x} {self.abstractCreature.pos.y} ; {self.abstractCreature.pos} ; {self.abstractCreature.pos.Tile}");
        orig(self, eu);
    }

    private static void OverWorld_WorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld self)
    {

        if (self.game.GetStorySession.saveState.saveStateNumber == ShawName && self.reportBackToGate == null && self.specialWarpCallback != null && self.currentSpecialWarp == OverWorld.SpecialWarpType.WARP_VS_HR)
        {
            Manager.playNewLocation = true;
            try
            {

                Plugin.Log("New World loaded");
                World world = self.activeWorld;
                World world2 = self.worldLoader.ReturnWorld();
                AbstractRoom abstractRoom = null;
                AbstractRoom abstractRoom2 = world2.GetAbstractRoom("CR_START"); ;

                if (self.currentSpecialWarp == OverWorld.SpecialWarpType.WARP_SINGLEROOM)
                {
                    abstractRoom2 = world2.GetAbstractRoom(self.singleRoomWorldWarpGoal);
                }
                self.activeWorld = world2;
                if (self.game.roomRealizer != null)
                {
                    self.game.roomRealizer = new RoomRealizer(self.game.roomRealizer.followCreature, world2);
                }
                abstractRoom2.RealizeRoom(world2, self.game);
                for (int i = 0; i < self.game.Players.Count; i++)
                {
                    if (self.game.Players[i].realizedCreature != null)
                    {
                        self.game.Players[i].realizedCreature.room.RemoveObject(self.game.Players[i].realizedCreature);
                        world.GetAbstractRoom(self.game.Players[i].pos).RemoveEntity(self.game.Players[i]);
                        self.game.Players[i].world = world2;
                        WorldCoordinate worldCoordinate = new WorldCoordinate(abstractRoom2.index, 0, 0, -1);
                        worldCoordinate.Tile = new IntVector2(15, 103);
                        self.game.Players[i].pos = worldCoordinate;
                        abstractRoom2.AddEntity(self.game.Players[i]);
                        self.game.Players[i].realizedCreature.PlaceInRoom(abstractRoom2.realizedRoom);
                        if (abstractRoom2.realizedRoom.game.session is StoryGameSession && world2.region != null && !(abstractRoom2.realizedRoom.game.session as StoryGameSession).saveState.regionStates[world2.region.regionNumber].roomsVisited.Contains(abstractRoom2.realizedRoom.abstractRoom.name))
                        {
                            (abstractRoom2.realizedRoom.game.session as StoryGameSession).saveState.regionStates[world2.region.regionNumber].roomsVisited.Add(abstractRoom2.realizedRoom.abstractRoom.name);
                        }
                    }
                }
                for (int j = 0; j < self.game.cameras.Length; j++)
                {
                    //if (self.currentSpecialWarp == OverWorld.SpecialWarpType.WARP_VS_HR)
                    //{
                    //    self.game.cameras[j].virtualMicrophone.AllQuiet();
                    //    self.game.cameras[j].MoveCamera(abstractRoom2.realizedRoom, 1);
                    //}
                    //else
                    //{
                        self.game.cameras[j].virtualMicrophone.AllQuiet();
                        self.game.cameras[j].MoveCamera(abstractRoom2.realizedRoom, -1);
                    //}
                }
                for (int m = 0; m < self.game.Players.Count; m++)
                {
                    if (self.game.Players[m].realizedCreature != null && (self.game.Players[m].realizedCreature as Player).objectInStomach != null)
                    {
                        (self.game.Players[m].realizedCreature as Player).objectInStomach.world = world2;
                    }
                }
                for (int n = self.game.shortcuts.transportVessels.Count - 1; n >= 0; n--)
                {
                    if (!self.activeWorld.region.IsRoomInRegion(self.game.shortcuts.transportVessels[n].room.index))
                    {
                        self.game.shortcuts.transportVessels.RemoveAt(n);
                    }
                }
                for (int num = self.game.shortcuts.betweenRoomsWaitingLobby.Count - 1; num >= 0; num--)
                {
                    if (!self.activeWorld.region.IsRoomInRegion(self.game.shortcuts.betweenRoomsWaitingLobby[num].room.index))
                    {
                        self.game.shortcuts.betweenRoomsWaitingLobby.RemoveAt(num);
                    }
                }
                for (int num2 = self.game.shortcuts.borderTravelVessels.Count - 1; num2 >= 0; num2--)
                {
                    if (!self.activeWorld.region.IsRoomInRegion(self.game.shortcuts.borderTravelVessels[num2].room.index))
                    {
                        self.game.shortcuts.borderTravelVessels.RemoveAt(num2);
                    }
                }
                bool flag = false;
                if (self.reportBackToGate == null)
                {
                    if (self.specialWarpCallback != null)
                    {
                        self.specialWarpCallback.NewWorldLoaded();
                    }
                    self.specialWarpCallback = null;
                    flag = true;
                }
                else
                {
                    self.reportBackToGate.NewWorldLoaded();
                    self.reportBackToGate = null;
                }
                self.worldLoader = null;
                for (int num3 = 0; num3 < self.game.cameras.Length; num3++)
                {
                    self.game.cameras[num3].hud.ResetMap(new Map.MapData(world2, self.game.rainWorld));
                    self.game.cameras[num3].dayNightNeedsRefresh = true;
                    if (self.game.cameras[num3].hud.textPrompt.subregionTracker != null)
                    {
                        self.game.cameras[num3].hud.textPrompt.subregionTracker.lastShownRegion = 0;
                    }
                }
                if (!flag)
                {
                    world.regionState.AdaptRegionStateToWorld(-1, abstractRoom2.index);
                    world.regionState.gatesPassedThrough[world.GetAbstractRoom(abstractRoom.name).gateIndex] = true;
                    world2.regionState.gatesPassedThrough[world2.GetAbstractRoom(abstractRoom.name).gateIndex] = true;
                }
                if (world.regionState != null)
                {
                    world.regionState.world = null;
                }
                if (ModManager.MSC)
                {
                    world2.SpawnPupNPCs();
                }
                world2.rainCycle.baseCycleLength = world.rainCycle.baseCycleLength;
                world2.rainCycle.cycleLength = world.rainCycle.cycleLength;
                world2.rainCycle.timer = world.rainCycle.timer;
                world2.rainCycle.duskPalette = world.rainCycle.duskPalette;
                world2.rainCycle.nightPalette = world.rainCycle.nightPalette;
                world2.rainCycle.dayNightCounter = world.rainCycle.dayNightCounter;
                if (ModManager.MSC)
                {
                    if (world.rainCycle.timer == 0)
                    {
                        world2.rainCycle.preTimer = world.rainCycle.preTimer;
                        world2.rainCycle.maxPreTimer = world.rainCycle.maxPreTimer;
                    }
                    else
                    {
                        world2.rainCycle.preTimer = 0;
                        world2.rainCycle.maxPreTimer = 0;
                    }
                }
                if (ModManager.MMF)
                {
                    GC.Collect();
                }

                Plugin.Log("start CR intro");
                RubiconPopup.staticHUD.StartAnimation();
            }
            catch (Exception e)
            {
                Plugin.log.LogError(e.ToString());
            }
        }
        else
        {
            orig(self);
        }
        Plugin.Log("New world load finished");
    }

    private static void VS_E05WrapAround_Update(On.MoreSlugcats.MSCRoomSpecificScript.VS_E05WrapAround.orig_Update orig, MSCRoomSpecificScript.VS_E05WrapAround self, bool eu)
    {
        for (int i = 0; i < self.room.game.Players.Count; i++)
        {
            if (self.room.game.Players[i].realizedCreature != null && (self.room.game.Players[i].realizedCreature as Player).room == self.room)
            {
                if (!self.loadStarted)
                {
                    //ConsoleWrite("check 1");
                    if (self.StoredEffect == null)
                    {
                        self.StoredEffect = new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.VoidSpawn, 0, false);
                    }
                    //ConsoleWrite("StoredEffect: " + self.StoredEffect);
                    //ConsoleWrite("end: " + self.phaseTimer + ", " + self.loadStarted);
                    //ConsoleWrite("overWorld: " + self.room.game.overWorld);
                    //ConsoleWrite("load: " + self.loadStarted);
                }
            }
        }

        bool loadStarted = self.loadStarted;
        orig(self, eu);
        if (!loadStarted && self.loadStarted)
        {
            Plugin.Log("Load started");
            self.room.world.game.globalRain.ResetRain();
            RainWorldGame.ForceSaveNewDenLocation(self.room.game, "CR_START", true);

        }
    }

    //private static AbstractRoom World_GetAbstractRoom_string(On.World.orig_GetAbstractRoom_string orig, World self, string room)
    //{
    //    ConsoleWrite("name: " + self.name);
    //    if (room == "HR_C01" && self.game.GetStorySession.saveState.saveStateNumber == ShawName)
    //    {
    //        return orig(self, "CR_START");
    //    }
    //    return orig(self, room);
    //}

    private static void OverWorld_InitiateSpecialWarp(On.OverWorld.orig_InitiateSpecialWarp orig, OverWorld self, OverWorld.SpecialWarpType warp, ISpecialWarp callback)
    {
        Plugin.Log("OverWorld_InitiateSpecialWarp " + RainWorld.ShowLogs);
        Plugin.Log("world loader: " + self.worldLoader);
        if (warp == OverWorld.SpecialWarpType.WARP_VS_HR && self.game.GetStorySession.saveState.saveStateNumber == ShawName)
        {
            self.reportBackToGate = null;
            self.currentSpecialWarp = warp;
            self.specialWarpCallback = callback;
            if (RainWorld.ShowLogs)
            {
                Plugin.Log("Switch Worlds Special! " + warp.ToString());
            }
            Plugin.Log("Switch Worlds Special! CR");
            self.worldLoader = new WorldLoader(self.game, self.PlayerCharacterNumber, false, "CR", self.GetRegion("CR"), self.game.setupValues);
            self.worldLoader.NextActivity();
            //ConsoleWrite("Change special warp WorldLoader from HR to CR");
        }
        else
        {
            orig(self, warp, callback);
        }
    }

    public class CR_STARTWarp : UpdatableAndDeletable
    {
        public CR_STARTWarp(Room room)
        {
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (room != null && room.ReadyForPlayer && room.readyForAI)
            {
                if (Manager.playNewLocation)
                {
                    Plugin.Log("new location !!!");
                    this.room.PlaySound(Sounds.NEW_LOCATION, 0.0f, 0.25f, 1f);
                    Manager.playNewLocation = false;
                }
            }

            for (int index = 0; index < this.room.game.Players.Count; index++)
            {
                if (this.room.game.Players[index].realizedCreature != null && (this.room.game.Players[index].realizedCreature as Player).room == this.room)
                {
                    Player player = this.room.game.Players[index].realizedCreature as Player;

                    if (player.abstractCreature.pos.x == 15 && player.abstractCreature.pos.y < 30)
                    {
                        player.SuperHardSetPosition(new Vector2(315, 2060));
                    }

                    if (player.mainBodyChunk.pos.y < 445)
                    {
                        if (player.mainBodyChunk.pos.y > 350)
                        {
                            fadeObj = new FadeOut(room, Color.white, 130f, false); // 130
                            room.AddObject(fadeObj);
                        }
                        player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, 345));
                        player.mainBodyChunk.vel = Vector2.zero;

                        if (fadeObj != null && fadeObj.IsDoneFading())
                        {
                            if (newRoom == null)
                            {
                                newRoom = room.game.world.GetAbstractRoom(ArenaChallenges.challenges[0].roomName);
                            }

                            if (newRoom.realizedRoom == null)
                            {
                                newRoom.RealizeRoom(room.game.world, room.game);
                            }

                            if (newRoom.realizedRoom != null && newRoom.realizedRoom.ReadyForPlayer && player != null)
                            {
                                Plugin.Log("teleport players");
                                for (int i = 0; i < room.game.AlivePlayers.Count; i++)
                                {
                                    room.game.AlivePlayers[i].realizedCreature.abstractCreature.Move(newRoom.realizedRoom.LocalCoordinateOfNode(0));
                                    room.game.AlivePlayers[i].realizedCreature.PlaceInRoom(newRoom.realizedRoom);
                                    room.game.AlivePlayers[i].realizedCreature.abstractCreature.ChangeRooms(player.room.GetWorldCoordinate(player.mainBodyChunk.pos));
                                }

                                player.room.AddObject(new FadeOut(player.room, Color.white, 60f, true));
                                fadeObj.Destroy();
                                room.game.cameras[0].virtualMicrophone.AllQuiet();
                                room.game.cameras[0].MoveCamera(player.room, 0);
                                fadeObj = null;
                                newRoom = null;
                                Manager.ResetValues();
                                Manager.SpawnCreatures(player.room);
                                Manager.PlayMusic(player.room);
                                //room.abstractRoom.Abstractize();
                                //newRoom.realizedRoom.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 0.95f);
                            }
                        }
                    }
                }
            }
        }

        public FadeOut fadeObj;
        public AbstractRoom newRoom;
    }

    public class CR_RESTWarp : UpdatableAndDeletable
    {
        public CR_RESTWarp(Room room)
        {
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            for (int index = 0; index < this.room.game.Players.Count; index++)
            {
                if (this.room.game.Players[index].realizedCreature != null && (this.room.game.Players[index].realizedCreature as Player).room == this.room)
                {
                    Player player = this.room.game.Players[index].realizedCreature as Player;

                    if (player.abstractCreature.pos.x == 15 && player.abstractCreature.pos.y > 22)
                    {
                        player.SuperHardSetPosition(new Vector2((float)room.LocalCoordinateOfNode(0).x * 20f, (float)room.LocalCoordinateOfNode(0).y * 20f));
                    }

                    if (player.mainBodyChunk.pos.x > 1111)
                    {
                        if (fadeObj == null)
                        {
                            fadeObj = new FadeOut(room, Color.white, 30f, false); // 130
                            room.AddObject(fadeObj);
                        }
                    }
                    if (fadeObj != null)
                    {
                        player.mainBodyChunk.vel = new Vector2(10, 0);

                        if (player.mainBodyChunk.pos.x > 1180)
                        {
                            player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, 345));
                        }
                        if (fadeObj.IsDoneFading())
                        {
                            if (newRoom == null)
                            {
                                newRoom = room.game.world.GetAbstractRoom("CR_BOSS");
                            }

                            if (newRoom.realizedRoom == null)
                            {
                                newRoom.RealizeRoom(room.game.world, room.game);
                            }

                            if (newRoom.realizedRoom != null && newRoom.realizedRoom.ReadyForPlayer && player != null)
                            {
                                Plugin.Log($"teleport {room.game.AlivePlayers.Count} players");
                                for (int i = 0; i < room.game.AlivePlayers.Count; i++)
                                {
                                    room.game.AlivePlayers[i].realizedCreature.abstractCreature.Move(newRoom.realizedRoom.GetWorldCoordinate(new Vector2(650, 145)));
                                    room.game.AlivePlayers[i].realizedCreature.PlaceInRoom(newRoom.realizedRoom);
                                    room.game.AlivePlayers[i].realizedCreature.abstractCreature.ChangeRooms(player.room.GetWorldCoordinate(player.mainBodyChunk.pos));
                                }

                                fadeObj.Destroy();
                                room.game.cameras[0].virtualMicrophone.AllQuiet();
                                room.game.cameras[0].MoveCamera(player.room, 0);
                                fadeObj = null;
                                newRoom = null;
                                BossManager.Initialize(player.room);
                            }
                        }
                    }
                }
            }
        }

        public FadeOut fadeObj;
        public AbstractRoom newRoom;
    }

    public class NextChallengeTeleport : UpdatableAndDeletable
    {
        public NextChallengeTeleport(Room room)
        {
            this.room = room;
            if (ArenaChallenges.currentArena >= ArenaChallenges.challenges.Count)
            {
                Plugin.Log($"creating teleporter from {room.abstractRoom.name} to CR_REST");
            }
            else
            {
                Plugin.Log($"creating teleporter from {room.abstractRoom.name} to {ArenaChallenges.challenges[ArenaChallenges.currentArena].roomName}");
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (fadeObj == null)
            {
                fadeObj = new FadeOut(room, Color.white, 130f, false); // 130
                room.AddObject(fadeObj);
            }

            if (fadeObj != null && fadeObj.IsDoneFading())
            {
                bool exitChallenge = ArenaChallenges.currentArena >= ArenaChallenges.challenges.Count;
                if (newRoom == null)
                {
                    if (exitChallenge)
                    {
                        newRoom = room.game.world.GetAbstractRoom("CR_REST");
                    }
                    else
                    {
                        //Plugin.Log($"Try get room {ArenaChallenges.challenges[ArenaChallenges.currentArena].roomName}");
                        newRoom = room.game.world.GetAbstractRoom(ArenaChallenges.challenges[ArenaChallenges.currentArena].roomName);
                        if (newRoom == null)
                        {
                            Plugin.LogError($"Fail to get {ArenaChallenges.challenges[ArenaChallenges.currentArena].roomName}");
                            return;
                        }
                    }
                }

                if (newRoom.realizedRoom == null)
                {
                    newRoom.RealizeRoom(room.game.world, room.game);
                }

                if (newRoom.realizedRoom != null && newRoom.realizedRoom.ReadyForPlayer)
                {
                    int den = 0; 
                    if (!exitChallenge)
                    {
                        den = ArenaChallenges.challenges[ArenaChallenges.currentArena].playerDen;
                    }
                    Plugin.Log($"teleporting players from {room.abstractRoom.name} to " + newRoom.name + " in den " + den);
                    //Plugin.Log($"worldPos: {newRoom.realizedRoom.LocalCoordinateOfNode(den)}, validPos: {new Vector2((float)newRoom.realizedRoom.LocalCoordinateOfNode(den).x * 20f, (float)newRoom.realizedRoom.LocalCoordinateOfNode(den).y * 20f)}, tilePos: {newRoom.realizedRoom.LocalCoordinateOfNode(den).Tile}");




                    for (int i = 0; i < room.game.AlivePlayers.Count; i++)
                    {

                        if (room.abstractRoom.name.ToLower() != newRoom.name.ToLower())
                        {
                            room.game.AlivePlayers[i].realizedCreature.Abstractize();
                            room.game.AlivePlayers[i].Realize();
                            room.game.AlivePlayers[i].realizedCreature.PlaceInRoom(newRoom.realizedRoom);
                            room.game.AlivePlayers[i].realizedCreature.abstractCreature.Move(newRoom.realizedRoom.LocalCoordinateOfNode(den));
                            room.game.AlivePlayers[i].realizedCreature.abstractCreature.ChangeRooms(newRoom.realizedRoom.GetWorldCoordinate(room.game.AlivePlayers[i].realizedCreature.mainBodyChunk.pos));
                        }
                        //else
                        //{
                        //    //room.game.AlivePlayers[i].Move(room.LocalCoordinateOfNode(den));
                        //    //(room.game.AlivePlayers[i].realizedCreature as Player).SuperHardSetPosition(new Vector2((float)this.room.LocalCoordinateOfNode(den).x * 20f, (float)this.room.LocalCoordinateOfNode(den).y * 20f));
                        //    //Vector2 pos = new Vector2(room.LocalCoordinateOfNode(den).x, room.LocalCoordinateOfNode(den).y);r
                        //    //(room.game.AlivePlayers[i].realizedCreature as Player).SuperHardSetPosition(pos);
                        //}

                        (room.game.AlivePlayers[i].realizedCreature as Player).SuperHardSetPosition(new Vector2((float)newRoom.realizedRoom.LocalCoordinateOfNode(den).x * 20f, (float)newRoom.realizedRoom.LocalCoordinateOfNode(den).y * 20f) + new Vector2(0, 10));
                        room.game.AlivePlayers[i].realizedCreature.mainBodyChunk.vel = Vector2.zero;
                        room.game.AlivePlayers[i].realizedCreature.mainBodyChunk.lastPos = room.game.AlivePlayers[i].realizedCreature.mainBodyChunk.pos;

                    }

                    Manager.ResetRoom(room);

                    newRoom.realizedRoom.AddObject(new FadeOut(newRoom.realizedRoom, Color.white, 60f, true)); // not sure if this is deleted       // dont worry i fixed that

                    fadeObj.Destroy();
                    room.game.cameras[0].virtualMicrophone.AllQuiet();
                    room.game.cameras[0].MoveCamera(newRoom.realizedRoom, 0);

                    if (room != newRoom.realizedRoom)
                    {
                        room.abstractRoom.Abstractize();
                    }

                    if (!exitChallenge)
                    {
                        Plugin.Log("spawn creatures in " + newRoom.realizedRoom);
                        Manager.SpawnCreatures(newRoom.realizedRoom);
                    }
                    else
                    {
                        RainWorldGame.ForceSaveNewDenLocation(this.room.game, "CR_REST", true);
                        newRoom.realizedRoom.game.manager.musicPlayer?.FadeOutAllSongs(6f);
                    }

                    fadeObj = null;
                    newRoom = null;

                    Destroy();
                    //room.abstractRoom.Abstractize();
                    //newRoom.realizedRoom.PlaySound(MoreSlugcatsEnums.MSCSoundID.Sat_Interference3, 0f, 1f, 0.95f);
                }
            }
        }

        public FadeOut fadeObj;
        public AbstractRoom newRoom;
    }
}


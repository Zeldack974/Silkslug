using MoreSlugcats;
using System;
using UnityEngine;
using RWCustom;
using HUD;

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
        //WorldCoordinate worldPos = self.abstractCreature.pos;
        //ConsoleWrite($"WorldPos: {worldPos.x} {worldPos.y} ; {worldPos} ; {worldPos.Tile}");
        orig(self, eu);
    }

    private static void OverWorld_WorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld self)
    {

        if (self.game.GetStorySession.saveState.saveStateNumber == ShawName && self.reportBackToGate == null && self.specialWarpCallback != null && self.currentSpecialWarp == OverWorld.SpecialWarpType.WARP_VS_HR)
        {
            try
            {

                ConsoleWrite("New World loaded");
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
                        worldCoordinate.Tile = new IntVector2(30, 29);
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
            } catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        else
        {
            orig(self);
        }
        ConsoleWrite("New World Load Finish");
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
            ConsoleWrite("Load Started");
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
        ConsoleWrite("OverWorld_InitiateSpecialWarp " + RainWorld.ShowLogs);
        ConsoleWrite("world loader: " + self.worldLoader);
        if (warp == OverWorld.SpecialWarpType.WARP_VS_HR && self.game.GetStorySession.saveState.saveStateNumber == ShawName)
        {
            self.reportBackToGate = null;
            self.currentSpecialWarp = warp;
            self.specialWarpCallback = callback;
            if (RainWorld.ShowLogs)
            {
                Debug.Log("Switch Worlds Special! " + warp.ToString());
            }
            ConsoleWrite("Switch Worlds Special! CR");
            self.worldLoader = new WorldLoader(self.game, self.PlayerCharacterNumber, false, "CR", self.GetRegion("CR"), self.game.setupValues);
            self.worldLoader.NextActivity();
            //ConsoleWrite("Change special warp WorldLoader from HR to CR");
        }
        else
        {
            orig(self, warp, callback);
        }
    }
}


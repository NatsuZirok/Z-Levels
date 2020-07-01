﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ZLevels
{
    [StaticConstructorOnStartup]
    public static class RoofAndFloorPatches
    {

        [HarmonyPatch(typeof(DamageWorker), "ExplosionDamageTerrain")]
        internal static class Patch_ExplosionDamageTerrain
        {
            private static bool Prefix(DamageWorker __instance, Explosion explosion, IntVec3 c)
            {
                if (c.GetTerrain(explosion.Map) == ZLevelsDefOf.ZL_RoofTerrain)
                {
                    if ((float)explosion.GetDamageAmountAt(c) >= ZLevelsDefOf.ZL_RoofTerrain.destroyOnBombDamageThreshold)
                    {
                        explosion.Map.terrainGrid.Notify_TerrainDestroyed(c);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TerrainGrid), "RemoveTopLayer")]
        public static class DestroyedTerrain
        {
            public static bool Prefix(TerrainGrid __instance, IntVec3 c, bool doLeavings, Map ___map)
            {
                try
                {
                    var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                    if (ZTracker.GetZIndexFor(___map) > 0)
                    {
                        int num = ___map.cellIndices.CellToIndex(c);
                        if (doLeavings)
                        {
                            GenLeaving.DoLeavingsFor(__instance.topGrid[num], c, ___map);
                        }
                        TerrainDef[] underGrid = Traverse.Create(__instance).Field("underGrid").GetValue<TerrainDef[]>();
                        if (underGrid[num] != null)
                        {
                            __instance.topGrid[num] = underGrid[num];
                            underGrid[num] = null;
                            Traverse.Create(__instance).Method("DoTerrainChangedEffects", new object[]
                            {
                            c
                            }).GetValue();
                        }
                        if (c.GetTerrain(___map) == TerrainDefOf.Sand)
                        {
                            __instance.SetTerrain(c, ZLevelsDefOf.ZL_OutsideTerrain);
                        }

                        Map lowerMap = ZTracker.GetLowerLevel(___map.Tile, ___map);
                        bool firstTime = false;
                        if (lowerMap == null)
                        {
                            lowerMap = ZTracker.CreateLowerLevel(___map, c);
                            firstTime = true;
                        }

                        var thingList = c.GetThingList(___map);
                        if (thingList.Where(x => x is Blueprint || x is Frame).Count() == 0)
                        {
                            for (int i = thingList.Count - 1; i >= 0; i--)
                            {
                                if (!(thingList[i] is Mineable || thingList[i] is Blueprint || thingList[i] is Frame))
                                {
                                    //Log.Message(thingList[i] + " going down 1");
                                    ZTracker.SimpleTeleportThing(thingList[i], c, lowerMap, firstTime, 10);
                                }
                            }
                        }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("[Z-Levels] DestroyedTerrain patch produced an error. That should not happen and will break things. Send a Hugslib log to the Z-Levels developers. Error message: " + ex, true);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("SpawnSetup")]
        public static class Patch_SpawnSetup
        {
            [HarmonyPostfix]
            public static void Postfix(Thing __instance)
            {
                try
                {
                    if (__instance.Position.GetTerrain(__instance.Map) == ZLevelsDefOf.ZL_OutsideTerrain)
                    {
                        var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                        Map lowerMap = ZTracker.GetLowerLevel(__instance.Map.Tile, __instance.Map);
                        bool firstTime = false;
                        if (lowerMap == null)
                        {
                            lowerMap = ZTracker.CreateLowerLevel(__instance.Map, __instance.Position);
                            firstTime = true;
                        }
                        var thingList = __instance.Position.GetThingList(__instance.Map);
                        if (thingList.Where(x => x is Blueprint || x is Frame).Count() == 0)
                        {
                            for (int i = thingList.Count - 1; i >= 0; i--)
                            {
                                if (!(thingList[i] is Mineable || thingList[i] is Blueprint || thingList[i] is Frame))
                                {
                                    //Log.Message(thingList[i] + " going down 2");
                                    ZTracker.SimpleTeleportThing(thingList[i], __instance.Position, lowerMap, firstTime, 10);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("[Z-Levels] Patch_SpawnSetup patch produced an error. That should not happen and will break things. Send a Hugslib log to the Z-Levels developers. Error message: " + ex, true);
                }
            }
        }

        [HarmonyPatch(typeof(RoofGrid), "SetRoof")]
        internal static class Patch_SetRoof
        {
            private static void Prefix(RoofGrid __instance, ref IntVec3 c, ref RoofDef def)
            {
                try
                {
                    if (def != null && !def.isNatural)
                    {
                        Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                        var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                        var upperMap = ZTracker.GetUpperLevel(map.Tile, map);
                        if (upperMap != null && upperMap.terrainGrid.TerrainAt(c) == ZLevelsDefOf.ZL_OutsideTerrain)
                        {
                            upperMap.terrainGrid.SetTerrain(c, ZLevelsDefOf.ZL_RoofTerrain);
                        }
                    }
                    else if (def == null)
                    {
                        Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                        if (c.GetRoof(map) == RoofDefOf.RoofConstructed)
                        {
                            var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                            Map upperMap = ZTracker.GetUpperLevel(map.Tile, map);
                            if (upperMap != null)
                            {
                                upperMap.terrainGrid.SetTerrain(c, ZLevelsDefOf.ZL_OutsideTerrain);
                                var thingList = c.GetThingList(upperMap);
                                for (int i = thingList.Count - 1; i >= 0; i--)
                                {
                                    if (!(thingList[i] is Mineable || thingList[i] is Blueprint || thingList[i] is Frame))
                                    {
                                        //Log.Message(thingList[i] + " going down 3");
                                        ZTracker.SimpleTeleportThing(thingList[i], c, map, false, 10);
                                    }
                                }
                                ZLogger.Message("Removing roof " + c.GetRoof(map), true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Log.Error("Error in Patch_SetRoof: " + ex);
                };
            }
        }

        [HarmonyPatch(typeof(TerrainGrid), "SetTerrain")]
        internal static class Patch_SetTerrain
        {
            private static void Postfix(TerrainGrid __instance, ref IntVec3 c, ref TerrainDef newTerr)
            {
                try
                {
                    if (newTerr != null && newTerr.Removable)
                    {
                        var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                        Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                        var lowerMap = ZTracker.GetLowerLevel(map.Tile, map);
                        if (lowerMap != null && lowerMap.roofGrid.RoofAt(c) != RoofDefOf.RoofConstructed)
                        {
                            lowerMap.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error in Patch_SetTerrain: " + ex);
                };
            }
        }
    }
}


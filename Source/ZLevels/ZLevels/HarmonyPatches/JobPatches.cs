﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ZLevels
{
    [StaticConstructorOnStartup]
    public static class JobManagerPatches
    {
        public static bool manualDespawn = false;

        //static JobManagerPatches()
        //{
        //    MethodInfo method = typeof(JobManagerPatches).GetMethod("LogScanner");
        //    //MethodInfo method2 = typeof(JobManagerPatches).GetMethod("LogScanner2");
        //    foreach (Type type in GenTypes.AllSubclasses(typeof(ThinkNode_JobGiver)))
        //    {
        //        try
        //        {
        //            new Harmony("test.test.tst").Patch(type.GetMethod("TryGiveJob", BindingFlags.Instance | BindingFlags.NonPublic
        //                | BindingFlags.GetField), null, new HarmonyMethod(method), null);
        //            ZLogger.Message("Patch: " + type);
        //        }
        //
        //        catch (Exception ex)
        //        {
        //            ZLogger.Message("Error patching: " + ex);
        //        }
        //    }
        //
        //}
        //
        //public static void LogScanner(ThinkNode_JobGiver __instance, Job __result, Pawn pawn)
        //{
        //    //if (__result != null && pawn.def.race.Humanlike)
        //    //{
        //    //    ZLogger.Message(__instance + " - " + __result + " - " + pawn, true);
        //    //}
        //    if (pawn.Faction == Faction.OfPlayer && !pawn.def.race.Humanlike)
        //    {
        //    }
        //}
        //
        //[HarmonyPatch(typeof(JobQueue), "EnqueueFirst")]
        //internal static class Patch_JobQueue
        //{
        //    private static void Postfix(Job j, JobTag? tag = null)
        //    {
        //        ZLogger.Message("Switching to " + j, true);
        //    }
        //}
        //
        //[HarmonyPatch(typeof(JobQueue), "EnqueueLast")]
        //internal static class Patch_JobQueue2
        //{
        //    private static void Postfix(Job j, JobTag? tag = null)
        //    {
        //        ZLogger.Message("Switching to " + j, true);
        //    }
        //}
        //
        //
        //[HarmonyPatch(typeof(JobGiver_Work), "GiverTryGiveJobPrioritized")]
        //internal static class Patch_JobGiver_Work
        //{
        //    private static void Postfix(Pawn pawn, WorkGiver giver, IntVec3 cell)
        //    {
        //        ZLogger.Message("Switching to " + pawn, true);
        //    }
        //}

        //[HarmonyPatch(typeof(JobGiver_AIFollowMaster), "TryGiveJob")]
        //public class JobGiver_AIFollowMasterPatch
        //{
        //    [HarmonyPostfix]
        //    private static void JobGiver_AIFollowMasterPostfix(JobGiver_AIFollowMaster __instance, ref Job __result, Pawn pawn)
        //    {
        //        try
        //        {
        //            var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
        //            if (__result == null && ZTracker?.ZLevelsTracker[pawn.Map.Tile]?.ZLevels?.Count > 1)
        //            {
        //                if (ZTracker.jobTracker == null)
        //                {
        //                    ZTracker.jobTracker = new Dictionary<Pawn, JobTracker>();
        //                }
        //                if (ZTracker.jobTracker.ContainsKey(pawn))
        //                {
        //                    try
        //                    {
        //                        if (ZTracker.jobTracker[pawn]?.activeJobs?.Count() > 0)
        //                        {
        //                            if (!pawn.jobs.jobQueue.Contains(ZTracker.jobTracker[pawn].activeJobs[0]))
        //                            {
        //                                ZLogger.Message("Queue: " + ZTracker.jobTracker[pawn].activeJobs[0]);
        //                                pawn.jobs.jobQueue.EnqueueLast(ZTracker.jobTracker[pawn].activeJobs[0]);
        //                                return;
        //                            }
        //                            else if (pawn.jobs.jobQueue[0].job.targetA.Thing.Map != pawn.Map)
        //                            {
        //                                ZTracker.ResetJobTrackerFor(pawn);
        //                            }
        //                            else
        //                            {
        //                                return;
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        ZLogger.Message("Error2: " + ex);
        //                    };
        //                }
        //                else
        //                {
        //                    ZTracker.jobTracker[pawn] = new JobTracker();
        //                }
        //                if (Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTick < 200)
        //                // minimal job interval check per pawn is 200 ticks
        //                {
        //                    return;
        //                }
        //                Job result = null;
        //                var oldMap = pawn.Map;
        //                var oldPosition = pawn.Position;
        //                bool select = false;
        //
        //                foreach (var otherMap in ZTracker.ZLevelsTracker[pawn.Map.Tile].ZLevels.Values)
        //                {
        //                    if (otherMap != oldMap)
        //                    {
        //                        if (Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTick < 200)
        //                        {
        //                            return;
        //                        }
        //                        ZLogger.Message("Searching figth job for " + pawn);
        //                        var stairs = new List<Thing>();
        //
        //                        ZLogger.Message("Searching job for " + pawn + " in " + ZTracker.GetMapInfo(otherMap)
        //                            + " for " + ZTracker.GetMapInfo(oldMap));
        //
        //                        if (ZTracker.GetZIndexFor(otherMap) > ZTracker.GetZIndexFor(oldMap))
        //                        {
        //                            Map lowerMap = ZTracker.GetLowerLevel(otherMap.Tile, otherMap);
        //                            if (lowerMap != null)
        //                            {
        //                                ZLogger.Message("Searching stairs up in " + ZTracker.GetMapInfo(otherMap));
        //                                stairs = lowerMap.listerThings.AllThings.Where(x => x is Building_StairsUp).ToList();
        //                            }
        //                            else
        //                            {
        //                                ZLogger.Message("Lower map is null in " + ZTracker.GetMapInfo(otherMap));
        //                            }
        //                        }
        //                        else if (ZTracker.GetZIndexFor(otherMap) < ZTracker.GetZIndexFor(oldMap))
        //                        {
        //                            Map upperMap = ZTracker.GetUpperLevel(otherMap.Tile, otherMap);
        //                            if (upperMap != null)
        //                            {
        //                                ZLogger.Message("Searching stairs down in " + ZTracker.GetMapInfo(otherMap));
        //                                stairs = upperMap.listerThings.AllThings.Where(x => x is Building_StairsDown).ToList();
        //                            }
        //                            else
        //                            {
        //                                ZLogger.Message("Upper map is null in " + ZTracker.GetMapInfo(otherMap));
        //                            }
        //                        }
        //                        if (Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTick < 200)
        //                        // minimal job interval check per pawn is 200 ticks
        //                        {
        //                            return;
        //                        }
        //
        //                        if (stairs != null && stairs.Count() > 0)
        //                        {
        //                            var selectedStairs = GenClosest.ClosestThing_Global(pawn.Position, stairs, 99999f);
        //                            var position = selectedStairs.Position;
        //
        //                            if (Find.Selector.SelectedObjects.Contains(pawn)) select = true;
        //                            JobManagerPatches.manualDespawn = true;
        //                            pawn.DeSpawn();
        //                            JobManagerPatches.manualDespawn = false;
        //                            GenPlace.TryPlaceThing(pawn, position, otherMap, ThingPlaceMode.Direct);
        //
        //                            result = JobGiver_AIFollowMasterPatch.TryGiveJob(pawn, __instance);
        //
        //                            if (result != null)
        //                            {
        //                                ZLogger.Message(pawn + " gets food job " + result);
        //                                ZLogger.Message("TryIssueJobPackage: " + pawn + " - map: " + ZTracker.GetMapInfo(pawn.Map)
        //                                    + " - " + pawn.Position + " result " + result);
        //                                JobManagerPatches.manualDespawn = true;
        //                                pawn.DeSpawn();
        //                                JobManagerPatches.manualDespawn = false;
        //                                GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
        //                                if (select) Find.Selector.Select(pawn);
        //                                ZTracker.BuildJobListFor(pawn, oldMap, otherMap, result, null);
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (pawn.Map != oldMap)
        //                {
        //                    JobManagerPatches.manualDespawn = true;
        //                    pawn.DeSpawn();
        //                    JobManagerPatches.manualDespawn = false;
        //                    GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
        //                    if (select) Find.Selector.Select(pawn);
        //                }
        //                try
        //                {
        //                    ZTracker.jobTracker[pawn].lastTick = Find.TickManager.TicksGame;
        //                }
        //                catch { }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error("Some kind of error occurred in Z-Levels JobManager: " + ex);
        //        }
        //    }
        //
        //    public static Job TryGiveJob(Pawn pawn, JobGiver_AIFollowMaster __instance)
        //    {
        //        Pawn followee = Traverse.Create(__instance).Method("GetFollowee", new object[]
        //                            {
        //                                pawn
        //                            }).GetValue<Pawn>();
        //        if (followee == null)
        //        {
        //            return null;
        //        }
        //        if (!followee.Spawned || !pawn.CanReach(followee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
        //        {
        //            return null;
        //        }
        //
        //        float radius = Traverse.Create(__instance).Method("GetRadius", new object[]
        //                        {
        //                            pawn
        //                        }).GetValue<float>();
        //        if (!JobDriver_FollowClose.FarEnoughAndPossibleToStartJob(pawn, followee, radius))
        //        {
        //            return null;
        //        }
        //        Job job = JobMaker.MakeJob(JobDefOf.FollowClose, followee);
        //        job.expiryInterval = Traverse.Create(__instance).Field("FollowJobExpireInterval").GetValue<int>(); ;
        //        job.checkOverrideOnExpire = true;
        //        job.followRadius = radius;
        //        return job;
        //    }
        //}

        //[HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGiveJob")]
        //public class JobGiver_ConfigurableHostilityResponsePatch
        //{
        //    [HarmonyPostfix]
        //    private static void JobGiver_ConfigurableHostilityResponsePostfix(JobGiver_ConfigurableHostilityResponse __instance, ref Job __result, Pawn pawn)
        //    {
        //        try
        //        {
        //            var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
        //            if (pawn.def.race.Humanlike && pawn.Faction != Faction.OfPlayer && __result == null && ZTracker?.ZLevelsTracker[pawn.Map.Tile]?.ZLevels?.Count > 1)
        //            {
        //                if (ZTracker.jobTracker == null)
        //                {
        //                    ZTracker.jobTracker = new Dictionary<Pawn, JobTracker>();
        //                }
        //                if (ZTracker.jobTracker.ContainsKey(pawn))
        //                {
        //                    try
        //                    {
        //                        if (ZTracker.jobTracker[pawn]?.activeJobs?.Count() > 0)
        //                        {
        //                            if (!pawn.jobs.jobQueue.Contains(ZTracker.jobTracker[pawn].activeJobs[0]))
        //                            {
        //                                ZLogger.Message("Queue: " + ZTracker.jobTracker[pawn].activeJobs[0]);
        //                                pawn.jobs.jobQueue.EnqueueLast(ZTracker.jobTracker[pawn].activeJobs[0]);
        //                                return;
        //                            }
        //                            else if (pawn.jobs.jobQueue[0].job.targetA.Thing.Map != pawn.Map)
        //                            {
        //                                ZTracker.ResetJobTrackerFor(pawn);
        //                            }
        //                            else
        //                            {
        //                                return;
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        ZLogger.Message("Error2: " + ex);
        //                    };
        //                }
        //                else
        //                {
        //                    ZTracker.jobTracker[pawn] = new JobTracker();
        //                }
        //                if (Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTick < 200)
        //                // minimal job interval check per pawn is 200 ticks
        //                {
        //                    return;
        //                }
        //
        //                Map playerMap = ZTracker.GetMapByIndex(pawn.Map.Tile, 0);
        //                if (pawn.Map != playerMap)
        //                {
        //                    ZTracker.BuildJobListFor(pawn, pawn.Map, playerMap, JobMaker.MakeJob(JobDefOf.Wait_Combat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true), null);
        //                    try
        //                    {
        //                        ZLogger.Message("3 lastTick");
        //                        ZTracker.jobTracker[pawn].lastTick = Find.TickManager.TicksGame;
        //                    }
        //                    catch { }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error("Some kind of error occurred in Z-Levels JobManager: " + ex);
        //        }
        //    }
        //}
        //
        //[HarmonyPatch(typeof(JobGiver_AIGotoNearestHostile), "TryGiveJob")]
        //public class JobGiver_AIGotoNearestHostilePatch
        //{
        //    [HarmonyPostfix]
        //    private static void JobGiver_AIGotoNearestHostilePostfix(JobGiver_AIGotoNearestHostile __instance, ref Job __result, Pawn pawn)
        //    {
        //        try
        //        {
        //            var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
        //            if (pawn.def.race.Humanlike && pawn.Faction != Faction.OfPlayer && __result == null && ZTracker?.ZLevelsTracker[pawn.Map.Tile]?.ZLevels?.Count > 1)
        //            {
        //                if (ZTracker.jobTracker == null)
        //                {
        //                    ZTracker.jobTracker = new Dictionary<Pawn, JobTracker>();
        //                }
        //                if (ZTracker.jobTracker.ContainsKey(pawn))
        //                {
        //                    try
        //                    {
        //                        if (ZTracker.jobTracker[pawn]?.activeJobs?.Count() > 0)
        //                        {
        //                            if (!pawn.jobs.jobQueue.Contains(ZTracker.jobTracker[pawn].activeJobs[0]))
        //                            {
        //                                ZLogger.Message("Queue: " + ZTracker.jobTracker[pawn].activeJobs[0]);
        //                                pawn.jobs.jobQueue.EnqueueLast(ZTracker.jobTracker[pawn].activeJobs[0]);
        //                                return;
        //                            }
        //                            else if (pawn.jobs.jobQueue[0].job.targetA.Thing.Map != pawn.Map)
        //                            {
        //                                ZTracker.ResetJobTrackerFor(pawn);
        //                            }
        //                            else
        //                            {
        //                                return;
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        ZLogger.Message("Error2: " + ex);
        //                    };
        //                }
        //                else
        //                {
        //                    ZTracker.jobTracker[pawn] = new JobTracker();
        //                }
        //                if (Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTick < 200)
        //                // minimal job interval check per pawn is 200 ticks
        //                {
        //                    return;
        //                }
        //                Job result = null;
        //                var oldMap = pawn.Map;
        //                var oldPosition = pawn.Position;
        //                bool select = false;
        //
        //                foreach (var otherMap in ZTracker.ZLevelsTracker[pawn.Map.Tile].ZLevels.Values)
        //                {
        //                    if (otherMap != oldMap)
        //                    {
        //                        if (Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTick < 200)
        //                        {
        //                            return;
        //                        }
        //                        ZLogger.Message("Searching figth job for " + pawn);
        //                        var stairs = new List<Thing>();
        //
        //                        ZLogger.Message("Searching job for " + pawn + " in " + ZTracker.GetMapInfo(otherMap)
        //                            + " for " + ZTracker.GetMapInfo(oldMap));
        //
        //                        if (ZTracker.GetZIndexFor(otherMap) > ZTracker.GetZIndexFor(oldMap))
        //                        {
        //                            Map lowerMap = ZTracker.GetLowerLevel(otherMap.Tile, otherMap);
        //                            if (lowerMap != null)
        //                            {
        //                                ZLogger.Message("Searching stairs up in " + ZTracker.GetMapInfo(otherMap));
        //                                stairs = lowerMap.listerThings.AllThings.Where(x => x is Building_StairsUp).ToList();
        //                            }
        //                            else
        //                            {
        //                                ZLogger.Message("Lower map is null in " + ZTracker.GetMapInfo(otherMap));
        //                            }
        //                        }
        //                        else if (ZTracker.GetZIndexFor(otherMap) < ZTracker.GetZIndexFor(oldMap))
        //                        {
        //                            Map upperMap = ZTracker.GetUpperLevel(otherMap.Tile, otherMap);
        //                            if (upperMap != null)
        //                            {
        //                                ZLogger.Message("Searching stairs down in " + ZTracker.GetMapInfo(otherMap));
        //                                stairs = upperMap.listerThings.AllThings.Where(x => x is Building_StairsDown).ToList();
        //                            }
        //                            else
        //                            {
        //                                ZLogger.Message("Upper map is null in " + ZTracker.GetMapInfo(otherMap));
        //                            }
        //                        }
        //                        if (Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTick < 200)
        //                        // minimal job interval check per pawn is 200 ticks
        //                        {
        //                            return;
        //                        }
        //
        //                        if (stairs != null && stairs.Count() > 0)
        //                        {
        //                            var selectedStairs = GenClosest.ClosestThing_Global(pawn.Position, stairs, 99999f);
        //                            var position = selectedStairs.Position;
        //
        //                            if (Find.Selector.SelectedObjects.Contains(pawn)) select = true;
        //                            JobManagerPatches.manualDespawn = true;
        //                            pawn.DeSpawn();
        //                            JobManagerPatches.manualDespawn = false;
        //                            GenPlace.TryPlaceThing(pawn, position, otherMap, ThingPlaceMode.Direct);
        //
        //                            result = JobGiver_AIGotoNearestHostilePatch.TryGiveJob(pawn);
        //
        //                            if (result != null)
        //                            {
        //                                ZLogger.Message(pawn + " gets food job " + result);
        //                                ZLogger.Message("TryIssueJobPackage: " + pawn + " - map: " + ZTracker.GetMapInfo(pawn.Map)
        //                                    + " - " + pawn.Position + " result " + result);
        //                                JobManagerPatches.manualDespawn = true;
        //                                pawn.DeSpawn();
        //                                JobManagerPatches.manualDespawn = false;
        //                                GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
        //                                if (select) Find.Selector.Select(pawn);
        //                                ZTracker.BuildJobListFor(pawn, oldMap, otherMap, result, null);
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (pawn.Map != oldMap)
        //                {
        //                    JobManagerPatches.manualDespawn = true;
        //                    pawn.DeSpawn();
        //                    JobManagerPatches.manualDespawn = false;
        //                    GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
        //                    if (select) Find.Selector.Select(pawn);
        //                }
        //                if (result == null)
        //                {
        //                    Map playerMap = ZTracker.GetMapByIndex(oldMap.Tile, 0);
        //                    if (pawn.Map != playerMap)
        //                    {
        //                        ZTracker.BuildJobListFor(pawn, oldMap, playerMap, JobMaker.MakeJob(JobDefOf.Wait_Combat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true), null);
        //                    }
        //                }
        //                try
        //                {
        //                    ZLogger.Message("4 lastTick");
        //                    ZTracker.jobTracker[pawn].lastTick = Find.TickManager.TicksGame;
        //                }
        //                catch { }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error("Some kind of error occurred in Z-Levels JobManager: " + ex);
        //        }
        //    }
        //
        //    public static Job TryGiveJob(Pawn pawn)
        //    {
        //        float num = float.MaxValue;
        //        Thing thing = null;
        //        List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
        //        for (int i = 0; i < potentialTargetsFor.Count; i++)
        //        {
        //            IAttackTarget attackTarget = potentialTargetsFor[i];
        //            if (!attackTarget.ThreatDisabled(pawn) && AttackTargetFinder.IsAutoTargetable(attackTarget))
        //            {
        //                Thing thing2 = (Thing)attackTarget;
        //                int num2 = thing2.Position.DistanceToSquared(pawn.Position);
        //                if ((float)num2 < num && pawn.CanReach(thing2, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
        //                {
        //                    num = (float)num2;
        //                    thing = thing2;
        //                }
        //            }
        //        }
        //        if (thing != null)
        //        {
        //            Job job = JobMaker.MakeJob(JobDefOf.Goto, thing);
        //            job.checkOverrideOnExpire = true;
        //            job.expiryInterval = 500;
        //            job.collideWithPawns = true;
        //            return job;
        //        }
        //        return null;
        //    }
        //}

        [HarmonyPatch(typeof(JobDriver_Ingest))]
        [HarmonyPatch("PrepareToIngestToils_ToolUser")]
        public static class Patch_PrepareToIngestToils_ToolUser_Postfix
        {
            private static void Postfix(ref IEnumerable<Toil> __result, JobDriver_Ingest __instance, Toil chewToil)
            {
                var list = __result.ToList<Toil>();

                Thing IngestibleSource = __instance.job.GetTarget(TargetIndex.A).Thing;
                Pawn actor = __instance.pawn;
                Thing thing = null;
                IntVec3 intVec = IntVec3.Invalid;
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                var oldMap = actor.Map;
                bool select = false;
                if (Find.Selector.SelectedObjects.Contains(actor)) select = true;

                Predicate<Thing> baseChairValidator = delegate (Thing t)
                {
                    if (t.def.building == null || !t.def.building.isSittable)
                    {
                        return false;
                    }
                    if (t.IsForbidden(actor))
                    {
                        return false;
                    }
                    if (!actor.CanReserve(t))
                    {
                        return false;
                    }
                    if (!t.IsSociallyProper(actor))
                    {
                        return false;
                    }
                    if (t.IsBurning())
                    {
                        return false;
                    }
                    if (t.HostileTo(actor))
                    {
                        return false;
                    }
                    bool flag = false;
                    for (int i = 0; i < 4; i++)
                    {
                        Building edifice = (t.Position + GenAdj.CardinalDirections[i]).GetEdifice(t.Map);
                        if (edifice != null && edifice.def.surfaceType == SurfaceType.Eat)
                        {
                            flag = true;
                            break;
                        }
                    }
                    return flag ? true : false;
                };

                foreach (var otherMap in ZTracker.GetAllMapsInClosestOrder(actor.Map))
                {
                    if (actor.Map != otherMap)
                    {
                        Traverse.Create(actor).Field("mapIndexOrState")
                            .SetValue((sbyte)Find.Maps.IndexOf(otherMap));
                    }

                    if (IngestibleSource.def.ingestible.chairSearchRadius > 0f)
                    {
                        thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map,
                            ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell,
                            TraverseParms.For(actor), IngestibleSource.def.ingestible.chairSearchRadius,
                            (Thing t) => baseChairValidator(t) && t.Position.GetDangerFor(actor, t.Map) == Danger.None);
                    }
                    if (thing == null)
                    {
                        try
                        {
                            intVec = RCellFinder.SpotToChewStandingNear(actor, actor.CurJob.GetTarget(TargetIndex.A).Thing);
                        }
                        catch
                        {
                            intVec = actor.Position;
                        }
                        Danger chewSpotDanger = intVec.GetDangerFor(actor, actor.Map);
                        if (chewSpotDanger != Danger.None)
                        {
                            thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map,
                                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell,
                                TraverseParms.For(actor), IngestibleSource.def.ingestible.chairSearchRadius, (Thing t) =>
                                baseChairValidator(t) && (int)t.Position.GetDangerFor(actor, t.Map) <= (int)chewSpotDanger);
                        }
                    }

                    if (thing != null)
                    {
                        intVec = thing.Position;
                        actor.Reserve(thing, actor.CurJob);
                        ZLogger.Message(actor + " - Found: " + thing);
                        break;
                    }
                }
                if (actor.Map != oldMap)
                {
                    Traverse.Create(actor).Field("mapIndexOrState")
                        .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                }
                if (select) Find.Selector.Select(actor);

                if (thing != null && thing.Map != null && thing.Map != actor.Map)
                {
                    if (!IngestibleSource.def.IsDrug)
                    {
                        list.InsertRange(list.Count - 2, JobDriver_GoToThingMap.Toils_GoToThingMap(__instance.GetActor()
                            , null, thing, __instance));
                        ZLogger.Message("Adding: " + thing + " in " + actor);
                    }
                    else
                    {
                        list.InsertRange(list.Count - 1, JobDriver_GoToThingMap.Toils_GoToThingMap(__instance.GetActor()
                            , null, thing, __instance));
                        ZLogger.Message("Adding 2: " + thing + " in " + actor);
                    }
                }
                __result = list;
            }
        }

        [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob")]
        public class JobGiver_GetFoodPatch
        {
            [HarmonyPrefix]
            private static bool JobGiver_GetFoodPrefix(JobGiver_GetFood __instance, ref Job __result, Pawn pawn)
            {
                ZLogger.Message(pawn + " starting food search");
                if (pawn.def.race.Humanlike)
                {
                    try
                    {
                        var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                        if (ZTracker.jobTracker == null)
                        {
                            ZTracker.jobTracker = new Dictionary<Pawn, JobTracker>();
                        }
                        if (ZTracker.jobTracker.ContainsKey(pawn))
                        {
                            try
                            {
                                if (ZTracker.jobTracker[pawn]?.activeJobs?.Count() > 0)
                                {
                                    if (pawn.needs.food.CurCategory < HungerCategory.Starving
                                        && !pawn.jobs.jobQueue.Contains(ZTracker.jobTracker[pawn].activeJobs[0]))
                                    {
                                        if (ZTracker.jobTracker[pawn].activeJobs[0].def.defName != "UnloadYourHauledInventory"
                                            && ZTracker.jobTracker[pawn].activeJobs[0].TryMakePreToilReservations(pawn, false))
                                        {
                                            ZLogger.Message("Queue: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                                            pawn.jobs.jobQueue.EnqueueLast(ZTracker.jobTracker[pawn].activeJobs[0]);
                                            return false;
                                        }
                                    }
                                    else if (pawn.needs.food.CurCategory < HungerCategory.Starving
                                        && pawn.jobs.curJob == null
                                        && ZTracker.jobTracker[pawn].activeJobs[0].TryMakePreToilReservations(pawn, false))
                                    {
                                        ZLogger.Message("1 START JOB "
                                            + ZTracker.jobTracker[pawn].activeJobs[0] + " FOR " + pawn);
                                        __result = ZTracker.jobTracker[pawn].activeJobs[0];
                                        ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);

                                        return false;
                                    }
                                    else if (pawn.jobs.curJob == null && ZTracker.jobTracker[pawn].activeJobs[0] != null)
                                    {
                                        ZLogger.Message("2 START JOB " + ZTracker.jobTracker[pawn].activeJobs[0]
                                            + " FOR " + pawn);
                                        __result = ZTracker.jobTracker[pawn].activeJobs[0];
                                        ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);
                                        return false;
                                        //ZLogger.Message("1 RESETTING JOB TRACKER FOR " + pawn);
                                        //ZLogger.Message(pawn + " - pawn.jobs.curJob: " + pawn.jobs.curJob);
                                        //ZLogger.Message(pawn + " - ZTracker.jobTracker[pawn].activeJobs[0]: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                                        //foreach (var job in pawn.jobs.jobQueue)
                                        //{
                                        //    ZLogger.Message(pawn + " - job in pawn queue: " + job.job);
                                        //}
                                        //foreach (var job in ZTracker.jobTracker[pawn].activeJobs)
                                        //{
                                        //    ZLogger.Message(pawn + " - job in ZTracker queue: " + job);
                                        //}
                                        //ZTracker.ResetJobTrackerFor(pawn);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ZLogger.Message("Error2: " + ex);
                            };
                        }
                        else
                        {
                            ZTracker.jobTracker[pawn] = new JobTracker();
                        }

                        Job result;
                        var oldMap = pawn.Map;
                        var oldPosition = pawn.Position;
                        bool select = false;
                        if (Find.Selector.SelectedObjects.Contains(pawn)) select = true;

                        float maxLevelPercentage = Traverse.Create(__instance).Field("maxLevelPercentage").GetValue<float>();
                        HungerCategory minCategory = Traverse.Create(__instance).Field("minCategory").GetValue<HungerCategory>();

                        foreach (var otherMap in ZTracker.GetAllMapsInClosestOrder(oldMap))
                        {
                            var stairs = new List<Thing>();

                            ZLogger.Message("Searching food job for " + pawn + " in " + ZTracker.GetMapInfo(otherMap)
                                + " for " + ZTracker.GetMapInfo(oldMap));

                            if (ZTracker.GetZIndexFor(otherMap) > ZTracker.GetZIndexFor(oldMap))
                            {
                                Map lowerMap = ZTracker.GetLowerLevel(otherMap.Tile, otherMap);
                                if (lowerMap != null)
                                {
                                    ZLogger.Message("Searching stairs up in " + ZTracker.GetMapInfo(otherMap));
                                    stairs = ZTracker.stairsUp[lowerMap];
                                    //stairs = lowerMap.listerThings.AllThings.Where(x => x is Building_StairsUp).ToList();
                                }
                                else
                                {
                                    ZLogger.Message("Lower map is null in " + ZTracker.GetMapInfo(otherMap));
                                }
                            }
                            else if (ZTracker.GetZIndexFor(otherMap) < ZTracker.GetZIndexFor(oldMap))
                            {
                                Map upperMap = ZTracker.GetUpperLevel(otherMap.Tile, otherMap);
                                if (upperMap != null)
                                {
                                    ZLogger.Message("Searching stairs down in " + ZTracker.GetMapInfo(otherMap));
                                    stairs = ZTracker.stairsDown[upperMap];
                                    //stairs = upperMap.listerThings.AllThings.Where(x => x is Building_StairsDown).ToList();
                                }
                                else
                                {
                                    ZLogger.Message("Upper map is null in " + ZTracker.GetMapInfo(otherMap));
                                }
                            }

                            if (stairs != null && stairs.Count() > 0)
                            {
                                var selectedStairs = stairs.MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position));
                                var position = selectedStairs.Position;

                                //JobManagerPatches.manualDespawn = true;
                                //pawn.DeSpawn();
                                //JobManagerPatches.manualDespawn = false;
                                //GenPlace.TryPlaceThing(pawn, position, otherMap, ThingPlaceMode.Direct);

                                Traverse.Create(pawn).Field("mapIndexOrState")
                                    .SetValue((sbyte)Find.Maps.IndexOf(otherMap));
                                Traverse.Create(pawn).Field("positionInt")
                                    .SetValue(position);
                                ZLogger.Message("19 SetPosition for " + pawn + " to " + position);

                            }
                            else if (pawn.Map != oldMap && otherMap == oldMap)
                            {
                                Traverse.Create(pawn).Field("mapIndexOrState")
                                    .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                                Traverse.Create(pawn).Field("positionInt")
                                    .SetValue(oldPosition);
                                ZLogger.Message("4 SetPosition for " + pawn + " to " + oldPosition);

                            }

                            result = JobGiver_GetFoodPatch.TryGiveJob(pawn, __instance.forceScanWholeMap, maxLevelPercentage, minCategory);

                            if (result != null)
                            {
                                ZLogger.Message(pawn + " got food job " + result + " - map: "
                                    + ZTracker.GetMapInfo(pawn.Map) + " - " + pawn.Position);

                                Traverse.Create(pawn).Field("mapIndexOrState")
                                    .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                                Traverse.Create(pawn).Field("positionInt")
                                    .SetValue(oldPosition);
                                ZLogger.Message("3 SetPosition for " + pawn + " to " + oldPosition);

                                //JobManagerPatches.manualDespawn = true;
                                //pawn.DeSpawn();
                                //JobManagerPatches.manualDespawn = false;
                                //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);

                                ZTracker.BuildJobListFor(pawn, oldMap, otherMap, result, null);
                                __result = ZTracker.jobTracker[pawn].activeJobs[0];
                                ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);
                                break;
                            }
                        }

                        if (pawn.Map != oldMap)
                        {
                            Traverse.Create(pawn).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                            Traverse.Create(pawn).Field("positionInt")
                                .SetValue(oldPosition);
                            ZLogger.Message("19 SetPosition for " + pawn + " to " + oldPosition);


                            //JobManagerPatches.manualDespawn = true;
                            //    pawn.DeSpawn();
                            //    JobManagerPatches.manualDespawn = false;
                            //    GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
                        }
                        try
                        {
                            ZTracker.jobTracker[pawn].lastTickFood = Find.TickManager.TicksGame;
                        }
                        catch { }
                        if (select) Find.Selector.Select(pawn);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Some kind of error occurred in Z-Levels JobManager: " + ex);
                    }
                }
                return true;
            }

            private static Job TryGiveJob(Pawn pawn, bool forceScanWholeMap, float maxLevelPercentage, HungerCategory minCategory)
            {
                Need_Food food = pawn.needs.food;
                if (food == null || food.CurCategory < minCategory || food.CurLevelPercentage > maxLevelPercentage)
                {
                    return null;
                }
                bool allowCorpse;
                if (pawn.AnimalOrWildMan())
                {
                    allowCorpse = true;
                }
                else
                {
                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
                    allowCorpse = (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.4f);
                }
                bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
                Thing thing;
                ThingDef thingDef;
                if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out thing, out thingDef, true, true, false, allowCorpse, false, pawn.IsWildMan(), forceScanWholeMap, false, FoodPreferability.Undefined))
                {
                    return null;
                }
                Pawn pawn2 = thing as Pawn;
                if (pawn2 != null)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.PredatorHunt, pawn2);
                    job.killIncappedTarget = true;
                    return job;
                }
                if (thing is Plant && thing.def.plant.harvestedThingDef == thingDef)
                {
                    return JobMaker.MakeJob(JobDefOf.Harvest, thing);
                }
                Building_NutrientPasteDispenser building_NutrientPasteDispenser = thing as Building_NutrientPasteDispenser;
                if (building_NutrientPasteDispenser != null && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())
                {
                    Building building = building_NutrientPasteDispenser.AdjacentReachableHopper(pawn);
                    if (building != null)
                    {
                        ISlotGroupParent hopperSgp = building as ISlotGroupParent;
                        Job job2 = WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, hopperSgp);
                        if (job2 != null)
                        {
                            return job2;
                        }
                    }
                    thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn, desperate, out thingDef, FoodPreferability.MealLavish, false, !pawn.IsTeetotaler(), false, false, false, false, false, false, forceScanWholeMap, false, FoodPreferability.Undefined);
                    if (thing == null)
                    {
                        return null;
                    }
                }
                float nutrition = FoodUtility.GetNutrition(thing, thingDef);
                Job job3 = JobMaker.MakeJob(JobDefOf.Ingest, thing);
                job3.count = FoodUtility.WillIngestStackCountOf(pawn, thingDef, nutrition);
                return job3;
            }
        }

        [HarmonyPatch(typeof(JobGiver_GetJoy), "TryGiveJob")]
        public class JobGiver_GetJoyPatch
        {
            [HarmonyPrefix]
            private static bool JobGiver_GetJoyPrefix(JobGiver_GetJoy __instance, ref Job __result, Pawn pawn)
            {
                try
                {
                    var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                    try
                    {
                        //    if (pawn.jobs.jobQueue.Count > 0)
                        //    {
                        //        //ZLogger.Message(pawn + " 1 taking job instead: " + pawn.jobs.jobQueue[0].job);
                        //        __result = null;
                        //        //pawn.jobs.jobQueue.Dequeue();
                        //        try
                        //        {
                        //            ZTracker.jobTracker[pawn].lastTickJoy = Find.TickManager.TicksGame;
                        //        }
                        //        catch { }
                        //        //try
                        //        //{
                        //        //    ZLogger.Message("ZTracker.jobTracker[pawn].activeJobs.Count > 0");
                        //        //    ZLogger.Message(pawn + " - pawn.jobs.curJob: " + pawn.jobs.curJob);
                        //        //    ZLogger.Message(pawn + " - ZTracker.jobTracker[pawn].activeJobs[0]: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                        //        //    foreach (var job in pawn.jobs.jobQueue)
                        //        //    {
                        //        //        ZLogger.Message(pawn + " - job in pawn queue: " + job.job);
                        //        //    }
                        //        //    foreach (var job in ZTracker.jobTracker[pawn].activeJobs)
                        //        //    {
                        //        //        ZLogger.Message(pawn + " - job in ZTracker queue: " + job);
                        //        //    }
                        //        //}
                        //        //catch { };
                        //        return;
                        //    }

                        //if (ZTracker.jobTracker[pawn].activeJobs.Count > 0)
                        //{
                        //    ZLogger.Message(pawn + " 2 taking job instead: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                        //    __result = ZTracker.jobTracker[pawn].activeJobs[0];
                        //    ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);
                        //    try
                        //    {
                        //        ZLogger.Message("ZTracker.jobTracker[pawn].activeJobs.Count > 0");
                        //        ZLogger.Message(pawn + " - pawn.jobs.curJob: " + pawn.jobs.curJob);
                        //        ZLogger.Message(pawn + " - ZTracker.jobTracker[pawn].activeJobs[0]: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                        //        foreach (var job in pawn.jobs.jobQueue)
                        //        {
                        //            ZLogger.Message(pawn + " - job in pawn queue: " + job.job);
                        //        }
                        //        foreach (var job in ZTracker.jobTracker[pawn].activeJobs)
                        //        {
                        //            ZLogger.Message(pawn + " - job in ZTracker queue: " + job);
                        //        }
                        //    }
                        //    catch { };
                        //    return false;
                        //}

                        //    else
                        //    {
                        //        ZLogger.Message(pawn + " cant start new job");
                        //    }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error: " + ex);
                    }

                    if (ZTracker.jobTracker == null)
                    {
                        ZTracker.jobTracker = new Dictionary<Pawn, JobTracker>();
                    }
                    if (ZTracker.jobTracker.ContainsKey(pawn))
                    {
                        try
                        {
                            if (ZTracker.jobTracker[pawn]?.activeJobs?.Count() > 0)
                            {
                                if (pawn?.needs?.joy?.CurCategory > JoyCategory.Low
                                    && !pawn.jobs.jobQueue.Contains(ZTracker.jobTracker[pawn].activeJobs[0]))
                                {
                                    if (ZTracker.jobTracker[pawn].activeJobs[0].def.defName != "UnloadYourHauledInventory"
                                        && ZTracker.jobTracker[pawn].activeJobs[0].TryMakePreToilReservations(pawn, false))
                                    {
                                        ZLogger.Message("Queue: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                                        pawn.jobs.jobQueue.EnqueueLast(ZTracker.jobTracker[pawn].activeJobs[0]);

                                        return false;
                                    }

                                }
                                else if (pawn?.needs?.joy?.CurCategory > JoyCategory.Low && pawn.jobs.curJob == null
                                    && ZTracker.jobTracker[pawn].activeJobs[0].TryMakePreToilReservations(pawn, false))
                                {

                                    //ZLogger.Message("2 START JOB "
                                    //    + ZTracker.jobTracker[pawn].activeJobs[0] + " FOR " + pawn);
                                    //pawn.jobs.StartJob(ZTracker.jobTracker[pawn].activeJobs[0]);
                                    //ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);
                                    ZLogger.Message("Return 4");

                                    return false;
                                }
                                else if (pawn?.needs?.joy?.CurCategory <= JoyCategory.Low && pawn.jobs.curJob != ZTracker.jobTracker[pawn].activeJobs[0])
                                {
                                    ZLogger.Message("2 RESETTING JOB TRACKER FOR " + pawn);
                                    ZLogger.Message(pawn + " - pawn.jobs.curJob: " + pawn.jobs.curJob);
                                    ZLogger.Message(pawn + " - ZTracker.jobTracker[pawn].activeJobs[0]: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                                    foreach (var job in pawn.jobs.jobQueue)
                                    {
                                        ZLogger.Message(pawn + " - job in pawn queue: " + job.job);
                                    }
                                    foreach (var job in ZTracker.jobTracker[pawn].activeJobs)
                                    {
                                        ZLogger.Message(pawn + " - job in ZTracker queue: " + job);
                                    }
                                    ZTracker.ResetJobTrackerFor(pawn);

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ZLogger.Message("Error2: " + ex);
                        };
                    }
                    else
                    {
                        ZTracker.jobTracker[pawn] = new JobTracker();
                    }
                    //if (Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTickJoy < 200)
                    //// minimal job interval check per pawn is 200 ticks
                    //{
                    //    ZLogger.Message(pawn + " exceeded joy tick limit");
                    //    return;
                    //}

                    Job result = null;
                    var oldMap = pawn.Map;
                    var oldPosition = pawn.Position;
                    bool select = false;
                    if (Find.Selector.SelectedObjects.Contains(pawn)) select = true;
                    var jobList = new Dictionary<Job, Map>();

                    bool CanDoDuringMedicalRest = Traverse.Create(__instance).Field("CanDoDuringMedicalRest").GetValue<bool>();
                    DefMap<JoyGiverDef, float> joyGiverChances = Traverse.Create(__instance).Field("joyGiverChances").GetValue<DefMap<JoyGiverDef, float>>();

                    foreach (var otherMap in ZTracker.GetAllMapsInClosestOrder(oldMap))
                    {
                        ZLogger.Message(pawn + " - other map: " + otherMap);

                        var stairs = new List<Thing>();

                        ZLogger.Message("Searching joy job for " + pawn + " in " + ZTracker.GetMapInfo(otherMap)
                            + " for " + ZTracker.GetMapInfo(oldMap));

                        if (ZTracker.GetZIndexFor(otherMap) > ZTracker.GetZIndexFor(oldMap))
                        {
                            Map lowerMap = ZTracker.GetLowerLevel(otherMap.Tile, otherMap);
                            if (lowerMap != null)
                            {
                                //ZLogger.Message("Searching stairs up in " + ZTracker.GetMapInfo(otherMap));
                                stairs = ZTracker.stairsUp[lowerMap];
                                //stairs = lowerMap.listerThings.AllThings.Where(x => x is Building_StairsUp).ToList();
                            }
                            else
                            {
                                ZLogger.Message("Lower map is null in " + ZTracker.GetMapInfo(otherMap));
                            }
                        }
                        else if (ZTracker.GetZIndexFor(otherMap) < ZTracker.GetZIndexFor(oldMap))
                        {
                            Map upperMap = ZTracker.GetUpperLevel(otherMap.Tile, otherMap);
                            if (upperMap != null)
                            {
                                //ZLogger.Message("Searching stairs down in " + ZTracker.GetMapInfo(otherMap));
                                stairs = ZTracker.stairsDown[upperMap];
                                //stairs = upperMap.listerThings.AllThings.Where(x => x is Building_StairsDown).ToList();
                            }
                            else
                            {
                                ZLogger.Message("Upper map is null in " + ZTracker.GetMapInfo(otherMap));
                            }
                        }

                        if (stairs != null && stairs.Count() > 0)
                        {
                            var selectedStairs = stairs.MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position));
                            //var selectedStairs = GenClosest.ClosestThing_Global(pawn.Position, stairs, 99999f);
                            var position = selectedStairs.Position;

                            Traverse.Create(pawn).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(otherMap));
                            Traverse.Create(pawn).Field("positionInt")
                                .SetValue(position);
                            ZLogger.Message("5 SetPosition for " + pawn + " to " + position);

                        }
                        else if (pawn.Map != oldMap && otherMap == oldMap)
                        {
                            Traverse.Create(pawn).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                            Traverse.Create(pawn).Field("positionInt")
                                .SetValue(oldPosition);
                            ZLogger.Message("6 SetPosition for " + pawn + " to " + oldPosition);

                        }
                        result = JobGiver_GetJoyPatch.TryGiveJob(pawn, CanDoDuringMedicalRest, joyGiverChances, __instance);
                        if (result != null)
                        {
                            ZLogger.Message(pawn + " got joy job " + result + " - map: "
                                + ZTracker.GetMapInfo(pawn.Map) + " - " + pawn.Position);
                            jobList[result] = otherMap;

                            //JobManagerPatches.manualDespawn = true;
                            //pawn.DeSpawn();
                            //JobManagerPatches.manualDespawn = false;
                            //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);

                            //ZTracker.BuildJobListFor(pawn, oldMap, otherMap, result, null);
                            //__result = ZTracker.jobTracker[pawn].activeJobs[0];
                            //ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);
                            //break;
                        }
                    }

                    if (pawn.Map != oldMap)
                    {
                        Traverse.Create(pawn).Field("mapIndexOrState")
                            .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                        Traverse.Create(pawn).Field("positionInt")
                            .SetValue(oldPosition);
                        ZLogger.Message("7 SetPosition for " + pawn + " to " + oldPosition);


                        //JobManagerPatches.manualDespawn = true;
                        //pawn.DeSpawn();
                        //JobManagerPatches.manualDespawn = false;
                        //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
                    }

                    if (jobList.Count > 0)
                    {
                        //var job = jobList.MaxBy(j => j.Key.def.joyGainRate);
                        var job = jobList.RandomElement();
                        ZTracker.BuildJobListFor(pawn, oldMap, job.Value, job.Key, null);
                        __result = ZTracker.jobTracker[pawn].activeJobs[0];
                        ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);
                    }
                    else
                    {
                        ZLogger.Message(pawn + " cant find joy job");
                    }
                    try
                    {
                        ZTracker.jobTracker[pawn].lastTickJoy = Find.TickManager.TicksGame;
                    }
                    catch { }

                    if (select) Find.Selector.Select(pawn);

                    return false;
                }
                catch (Exception ex)
                {
                    Log.Error("Some kind of error occurred in Z-Levels JobManager: " + ex);
                }
                return true;
            }
            public static Job TryGiveJob(Pawn pawn, bool CanDoDuringMedicalRest, DefMap<JoyGiverDef, float> joyGiverChances, JobGiver_GetJoy __instance)
            {
                if (!CanDoDuringMedicalRest && pawn.InBed() && HealthAIUtility.ShouldSeekMedicalRest(pawn))
                {
                    return null;
                }
                List<JoyGiverDef> allDefsListForReading = DefDatabase<JoyGiverDef>.AllDefsListForReading;
                JoyToleranceSet tolerances = pawn.needs.joy.tolerances;
                for (int i = 0; i < allDefsListForReading.Count; i++)
                {
                    JoyGiverDef joyGiverDef = allDefsListForReading[i];
                    joyGiverChances[joyGiverDef] = 0f;
                    if (pawn.needs.joy.tolerances.BoredOf(joyGiverDef.joyKind) || !joyGiverDef.Worker.CanBeGivenTo(pawn))
                    {
                        continue;
                    }
                    if (joyGiverDef.pctPawnsEverDo < 1f)
                    {
                        Rand.PushState(pawn.thingIDNumber ^ 0x3C49C49);
                        if (Rand.Value >= joyGiverDef.pctPawnsEverDo)
                        {
                            Rand.PopState();
                            continue;
                        }
                        Rand.PopState();
                    }
                    float num = tolerances[joyGiverDef.joyKind];
                    float b = Mathf.Pow(1f - num, 5f);
                    b = Mathf.Max(0.001f, b);
                    joyGiverChances[joyGiverDef] = joyGiverDef.Worker.GetChance(pawn) * b;
                }
                for (int j = 0; j < joyGiverChances.Count; j++)
                {
                    if (!allDefsListForReading.TryRandomElementByWeight((JoyGiverDef d) => joyGiverChances[d], out JoyGiverDef result))
                    {
                        break;
                    }
                    Job job = result.Worker.TryGiveJob(pawn);
                    if (job != null)
                    {
                        return job;
                    }
                    joyGiverChances[result] = 0f;
                }
                return null;
            }
        }


        [HarmonyPatch(typeof(JobGiver_GetRest), "TryGiveJob")]
        public class JobGiver_GetRestPatch
        {
            [HarmonyPrefix]
            private static bool JobGiver_GetRestPrefix(JobGiver_GetFood __instance, ref Job __result, Pawn pawn)
            {
                try
                {
                    var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                    if (pawn.Faction == Faction.OfPlayer)
                    {
                        ZLogger.Message(pawn + " search for rest job");
                        if (ZTracker.jobTracker == null)
                        {
                            ZTracker.jobTracker = new Dictionary<Pawn, JobTracker>();
                        }
                        if (ZTracker.jobTracker.ContainsKey(pawn))
                        {
                            try
                            {
                                if (ZTracker.jobTracker[pawn]?.activeJobs?.Count() > 0)
                                {
                                    if (pawn.needs.rest.CurCategory < RestCategory.Exhausted &&
                                        !pawn.jobs.jobQueue.Contains(ZTracker.jobTracker[pawn].activeJobs[0]))
                                    {
                                        if (ZTracker.jobTracker[pawn].activeJobs[0].def.defName != "UnloadYourHauledInventory"
                                            && ZTracker.jobTracker[pawn].activeJobs[0].TryMakePreToilReservations(pawn, false))
                                        {
                                            ZLogger.Message("Queue: " + ZTracker.jobTracker[pawn].activeJobs[0]);

                                            try
                                            {
                                                ZLogger.Message("--------------------------");
                                                for (int i = ZTracker.jobTracker[pawn].mainJob.targetQueueB.Count - 1; i >= 0; i--)
                                                {
                                                    var target = ZTracker.jobTracker[pawn].mainJob.targetQueueB[i];

                                                    ZLogger.Message("31 job.targetQueueB: " + target.Thing);
                                                    ZLogger.Message("31 job.targetQueueB.Map: " + target.Thing.Map);
                                                    ZLogger.Message("31 job.targetQueueB.stackCount: " + target.Thing.stackCount);
                                                    ZLogger.Message("31 job.targetQueueB.countQueue: " + ZTracker.jobTracker[pawn].mainJob.countQueue[i]);
                                                }
                                            }
                                            catch { }

                                            pawn.jobs.jobQueue.EnqueueLast(ZTracker.jobTracker[pawn].activeJobs[0]);

                                            return false;
                                        }
                                    }
                                    else if (pawn.needs.rest.CurCategory < RestCategory.Exhausted &&
                                        pawn.jobs.curJob == null)
                                    {
                                        ZLogger.Message("3 START JOB "
                                            + ZTracker.jobTracker[pawn].activeJobs[0] + " FOR " + pawn);
                                        __result = ZTracker.jobTracker[pawn].activeJobs[0];
                                        ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);

                                        return false;
                                    }
                                    else if (pawn.needs.rest.CurCategory >= RestCategory.Exhausted &&
                                        pawn.jobs.curJob != ZTracker.jobTracker[pawn].activeJobs[0])
                                    {
                                        ZLogger.Message("3 RESETTING JOB TRACKER FOR " + pawn);
                                        ZLogger.Message(pawn + " - pawn.jobs.curJob: " + pawn.jobs.curJob);
                                        ZLogger.Message(pawn + " - ZTracker.jobTracker[pawn].activeJobs[0]: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                                        foreach (var job in pawn.jobs.jobQueue)
                                        {
                                            ZLogger.Message(pawn + " - job in pawn queue: " + job.job);
                                        }
                                        foreach (var job in ZTracker.jobTracker[pawn].activeJobs)
                                        {
                                            ZLogger.Message(pawn + " - job in ZTracker queue: " + job);
                                        }
                                        ZTracker.ResetJobTrackerFor(pawn);

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ZLogger.Message("Error2: " + ex);
                            };
                        }
                        else
                        {
                            ZTracker.jobTracker[pawn] = new JobTracker();
                        }

                        //if (pawn.needs?.rest?.CurCategory < RestCategory.VeryTired
                        //    && Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTickRest < 200)
                        //// minimal job interval check per pawn is 200 ticks
                        //{
                        //    return false;
                        //}

                        Job result = null;
                        var oldMap = pawn.Map;
                        var oldPosition = pawn.Position;
                        bool select = false;
                        if (Find.Selector.SelectedObjects.Contains(pawn)) select = true;

                        RestCategory minCategory = Traverse.Create(__instance).Field("minCategory").GetValue<RestCategory>();
                        float maxLevelPercentage = Traverse.Create(__instance).Field("maxLevelPercentage").GetValue<float>();

                        foreach (var otherMap in ZTracker.GetAllMapsInClosestOrder(oldMap))
                        {
                            //if (pawn?.needs?.rest?.CurCategory < RestCategory.VeryTired
                            //    && Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTickRest < 200)
                            //{
                            //    return false;
                            //}

                            ZLogger.Message("Searching rest job for " + pawn + " in " + ZTracker.GetMapInfo(otherMap)
                                + " for " + ZTracker.GetMapInfo(oldMap));

                            var stairs = new List<Thing>();

                            if (ZTracker.GetZIndexFor(otherMap) > ZTracker.GetZIndexFor(oldMap))
                            {
                                Map lowerMap = ZTracker.GetLowerLevel(otherMap.Tile, otherMap);
                                if (lowerMap != null)
                                {
                                    ZLogger.Message("Searching stairs up in " + ZTracker.GetMapInfo(otherMap));
                                    stairs = ZTracker.stairsUp[lowerMap];
                                    //stairs = lowerMap.listerThings.AllThings.Where(x => x is Building_StairsUp).ToList();
                                }
                                else
                                {
                                    ZLogger.Message("Lower map is null in " + ZTracker.GetMapInfo(otherMap));
                                }
                            }
                            else if (ZTracker.GetZIndexFor(otherMap) < ZTracker.GetZIndexFor(oldMap))
                            {
                                Map upperMap = ZTracker.GetUpperLevel(otherMap.Tile, otherMap);
                                if (upperMap != null)
                                {
                                    ZLogger.Message("Searching stairs down in " + ZTracker.GetMapInfo(otherMap));
                                    stairs = ZTracker.stairsDown[upperMap];
                                    //stairs = upperMap.listerThings.AllThings.Where(x => x is Building_StairsDown).ToList();
                                }
                                else
                                {
                                    ZLogger.Message("Upper map is null in " + ZTracker.GetMapInfo(otherMap));
                                }
                            }

                            //if (pawn?.needs?.rest?.CurCategory < RestCategory.VeryTired &&
                            //    Find.TickManager.TicksGame - ZTracker.jobTracker[pawn].lastTickRest < 200)
                            //// minimal job interval check per pawn is 200 ticks
                            //{
                            //    return false;
                            //}

                            if (stairs != null && stairs.Count() > 0)
                            {
                                var selectedStairs = stairs.MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position));
                                //var selectedStairs = GenClosest.ClosestThing_Global(pawn.Position, stairs, 99999f);
                                var position = selectedStairs.Position;

                                Traverse.Create(pawn).Field("mapIndexOrState")
                                    .SetValue((sbyte)Find.Maps.IndexOf(otherMap));
                                Traverse.Create(pawn).Field("positionInt")
                                    .SetValue(position);
                                ZLogger.Message("8 SetPosition for " + pawn + " to " + position);


                                //JobManagerPatches.manualDespawn = true;
                                //pawn.DeSpawn();
                                //JobManagerPatches.manualDespawn = false;
                                //GenPlace.TryPlaceThing(pawn, position, otherMap, ThingPlaceMode.Direct);
                            }
                            else if (pawn.Map != oldMap && otherMap == oldMap)
                            {
                                Traverse.Create(pawn).Field("mapIndexOrState")
                                    .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                                Traverse.Create(pawn).Field("positionInt")
                                    .SetValue(oldPosition);
                                ZLogger.Message("2 SetPosition for " + pawn + " to " + oldPosition);

                            }

                            result = JobGiver_GetRestPatch.TryGiveJob(pawn, minCategory, maxLevelPercentage);
                            if (result != null && result.targetA.Thing != null)
                            {
                                ZLogger.Message(pawn + " got rest job " + result + " - map: "
                                    + ZTracker.GetMapInfo(pawn.Map) + " - " + pawn.Position);
                                if (pawn.Map != oldMap)
                                {
                                    Traverse.Create(pawn).Field("mapIndexOrState")
                                        .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                                    Traverse.Create(pawn).Field("positionInt")
                                        .SetValue(oldPosition);
                                    ZLogger.Message("1 SetPosition for " + pawn + " to " + oldPosition);
                                    //JobManagerPatches.manualDespawn = true;
                                    //pawn.DeSpawn();
                                    //JobManagerPatches.manualDespawn = false;
                                    //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
                                }
                                ZTracker.BuildJobListFor(pawn, oldMap, result.targetA.Thing.Map, result, null);
                                __result = ZTracker.jobTracker[pawn].activeJobs[0];
                                ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);
                                break;
                            }
                        }

                        if (pawn.Map != oldMap)
                        {
                            Traverse.Create(pawn).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                            Traverse.Create(pawn).Field("positionInt")
                                .SetValue(oldPosition);
                            ZLogger.Message("9 SetPosition for " + pawn + " to " + oldPosition);


                            //JobManagerPatches.manualDespawn = true;
                            //pawn.DeSpawn();
                            //JobManagerPatches.manualDespawn = false;
                            //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
                        }
                        if (result.targetA.Thing == null)
                        {
                            ZLogger.Message(pawn + " taking rest on the ground");
                            __result = JobMaker.MakeJob(JobDefOf.LayDown, FindGroundSleepSpotFor(pawn));
                        }

                        if (select) Find.Selector.Select(pawn);

                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Some kind of error occurred in Z-Levels JobManager: " + ex);
                }
                return true;
            }

            private static Job TryGiveJob(Pawn pawn, RestCategory minCategory, float maxLevelPercentage = 1f)
            {
                Need_Rest rest = pawn.needs.rest;
                if (rest == null || rest.CurCategory < minCategory || rest.CurLevelPercentage > maxLevelPercentage)
                {
                    return null;
                }
                if (RestUtility.DisturbancePreventsLyingDown(pawn))
                {
                    return null;
                }
                Lord lord = pawn.GetLord();
                Building_Bed building_Bed;
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                if ((lord != null && lord.CurLordToil != null && !lord.CurLordToil.AllowRestingInBed) || pawn.IsWildMan())
                {
                    building_Bed = null;
                }
                else
                {
                    if (pawn.ownership?.OwnedBed != null && ZTracker.GetAllMaps(pawn.Map.Tile)
                        .Contains(pawn.ownership?.OwnedBed?.Map))
                    {
                        building_Bed = pawn.ownership.OwnedBed;
                    }
                    else
                    {
                        building_Bed = RestUtility.FindBedFor(pawn);
                    }
                }
                if (building_Bed != null)
                {
                    ZLogger.Message("Found " + building_Bed + " for " + pawn + " in " + ZTracker.GetMapInfo(building_Bed.Map));
                    return JobMaker.MakeJob(JobDefOf.LayDown, building_Bed);
                }
                return JobMaker.MakeJob(JobDefOf.LayDown, FindGroundSleepSpotFor(pawn));
            }

            private static IntVec3 FindGroundSleepSpotFor(Pawn pawn)
            {
                Map map = pawn.Map;
                for (int i = 0; i < 2; i++)
                {
                    int radius = (i == 0) ? 4 : 12;
                    if (CellFinder.TryRandomClosewalkCellNear(pawn.Position, map, radius, out IntVec3 result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander))
                    {
                        return result;
                    }
                }
                return CellFinder.RandomClosewalkCellNearNotForbidden(pawn.Position, map, 4, pawn);
            }
        }

        [HarmonyPatch(typeof(JobGiver_Work), "TryIssueJobPackage")]
        public class TryIssueJobPackagePatch
        {
            private static bool Prefix(JobGiver_Work __instance, bool ___emergency, ref ThinkResult __result, Pawn pawn, JobIssueParams jobParams)
            {
                ZLogger.Message(pawn + " emergency " + ___emergency);

                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                try
                {
                    ZLogger.Message("--------------------------");
                    for (int i = ZTracker.jobTracker[pawn].mainJob.targetQueueB.Count - 1; i >= 0; i--)
                    {
                        var target = ZTracker.jobTracker[pawn].mainJob.targetQueueB[i];
                        ZLogger.Message("45 job.targetQueueB: " + target.Thing);
                        ZLogger.Message("45 job.targetQueueB.Map: " + target.Thing.Map);
                        ZLogger.Message("45 job.targetQueueB.stackCount: " + target.Thing.stackCount);
                        ZLogger.Message("45 job.targetQueueB.countQueue: " + ZTracker.jobTracker[pawn].mainJob.countQueue[i]);
                    }
                }
                catch { }

                ZLogger.Message(pawn + " start work search 1");
                try
                {
                    foreach (var d in ZTracker.jobTracker[pawn].activeJobs)
                    {
                        ZLogger.Message("Active jobs: " + d + " - " + pawn);
                    }
                    foreach (var t in pawn.jobs.jobQueue)
                    {
                        ZLogger.Message("Active jobQueue: " + pawn + " - " + t.job);
                    }
                }
                catch { }
                ZLogger.Message(pawn + " start work search 2");
                ZLogger.Message("=============================");

                //JobManagerPatches.searchJobs = false;
                try
                {

                    if (ZTracker.jobTracker == null)
                    {
                        ZTracker.jobTracker = new Dictionary<Pawn, JobTracker>();
                    }
                    //try
                    //{
                    //    if (ZTracker.jobTracker.ContainsKey(pawn) && ZTracker.jobTracker[pawn]?.activeJobs?.Count() == 0
                    //        && (pawn.needs.food.CurCategory >= HungerCategory.UrgentlyHungry
                    //        || pawn.needs.joy.CurCategory < JoyCategory.Low
                    //        || pawn.needs.rest.CurCategory >= RestCategory.VeryTired))
                    //    {
                    //        ZLogger.Message(pawn + " refusing to work because low needs");
                    //        ZLogger.Message("pawn.needs.food.CurCategory: " + pawn.needs.food.CurCategory);
                    //        ZLogger.Message("pawn.needs.joy.CurCategory: " + pawn.needs.joy.CurCategory);
                    //        ZLogger.Message("pawn.needs.rest.CurCategory: " + pawn.needs.rest.CurCategory);
                    //        __result = ThinkResult.NoJob;
                    //        return false;
                    //    }
                    //}
                    //catch { };

                    if (ZTracker.jobTracker.ContainsKey(pawn))
                    {
                        try
                        {
                            if (ZTracker.jobTracker[pawn]?.activeJobs?.Count() > 0)
                            {
                                foreach (var activeJob in ZTracker.jobTracker[pawn]?.activeJobs)
                                {
                                    ZLogger.Message(pawn + " - active job: " + activeJob);
                                }
                                if (!pawn.jobs.jobQueue.Contains(ZTracker.jobTracker[pawn].activeJobs[0]))
                                {
                                    if (ZTracker.jobTracker[pawn].activeJobs[0].def.defName != "UnloadYourHauledInventory"
                                        && ZTracker.jobTracker[pawn].activeJobs[0].TryMakePreToilReservations(pawn, false))
                                    {
                                        ZLogger.Message("1 Queue: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                                        try
                                        {
                                            ZLogger.Message("--------------------------");
                                            for (int i = ZTracker.jobTracker[pawn].mainJob.targetQueueB.Count - 1; i >= 0; i--)
                                            {
                                                var target = ZTracker.jobTracker[pawn].mainJob.targetQueueB[i];
                                                ZLogger.Message("50 job.targetQueueB: " + target.Thing);
                                                ZLogger.Message("50 job.targetQueueB.Map: " + target.Thing.Map);
                                                ZLogger.Message("50 job.targetQueueB.stackCount: " + target.Thing.stackCount);
                                                ZLogger.Message("50 job.targetQueueB.countQueue: " + ZTracker.jobTracker[pawn].mainJob.countQueue[i]);
                                            }
                                        }
                                        catch { }
                                        if (pawn?.carryTracker?.CarriedThing != null)
                                        {
                                            ZLogger.Message(pawn + " carrying " + pawn?.carryTracker?.CarriedThing);
                                        }

                                        //if (ZTracker.jobTracker[pawn].activeJobs[0] == ZTracker.jobTracker[pawn].mainJob)
                                        //{
                                        //    ZTracker.FixMainJobIfThereIsProblems(pawn);
                                        //}

                                        ZLogger.Message("--------------------------");
                                        try
                                        {
                                            for (int i = ZTracker.jobTracker[pawn].mainJob.targetQueueB.Count - 1; i >= 0; i--)
                                            {
                                                var target = ZTracker.jobTracker[pawn].mainJob.targetQueueB[i];
                                                ZLogger.Message("51 job.targetQueueB: " + target.Thing);
                                                ZLogger.Message("51 job.targetQueueB.Map: " + target.Thing.Map);
                                                ZLogger.Message("51 job.targetQueueB.stackCount: " + target.Thing.stackCount);
                                                ZLogger.Message("51 job.targetQueueB.countQueue: " + ZTracker.jobTracker[pawn].mainJob.countQueue[i]);
                                            }
                                        }
                                        catch { };
                                        __result = new ThinkResult(ZTracker.jobTracker[pawn].activeJobs[0], ZTracker.jobTracker[pawn].activeJobs[0].jobGiver);
                                        ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);

                                        //pawn.jobs.jobQueue.EnqueueLast(ZTracker.jobTracker[pawn].activeJobs[0]);

                                        return false;
                                    }
                                    else
                                    {
                                        ZLogger.Message(pawn + " failed job queue " + ZTracker.jobTracker[pawn].activeJobs[0] + " in " + ZTracker.GetMapInfo(pawn.Map));
                                    }
                                }
                                else if (pawn.jobs.curJob == null)
                                {
                                    //ZLogger.Message("4 START JOB "
                                    //    + ZTracker.jobTracker[pawn].activeJobs[0] + " FOR " + pawn);
                                    //
                                    //ZLogger.Message(pawn + " - pawn.jobs.curJob: " + pawn.jobs.curJob);
                                    //ZLogger.Message(pawn + " - ZTracker.jobTracker[pawn].activeJobs[0]: " + ZTracker.jobTracker[pawn].activeJobs[0]);

                                    //foreach (var job in ZTracker.jobTracker[pawn].activeJobs)
                                    //{
                                    //    ZLogger.Message(pawn + " - job in ZTracker queue: " + job);
                                    //}

                                    //pawn.jobs.jobQueue.EnqueueFirst(ZTracker.jobTracker[pawn].activeJobs[0]);
                                    //ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);
                                    ZLogger.Message(pawn + " - return 3");

                                    return false;
                                }
                                else if (pawn.jobs.curJob != ZTracker.jobTracker[pawn].activeJobs[0])
                                {
                                    ZLogger.Message("4 RESETTING JOB TRACKER FOR " + pawn);
                                    ZLogger.Message(pawn + " - pawn.jobs.curJob: " + pawn.jobs.curJob);
                                    ZLogger.Message(pawn + " - ZTracker.jobTracker[pawn].activeJobs[0]: " + ZTracker.jobTracker[pawn].activeJobs[0]);
                                    foreach (var job in pawn.jobs.jobQueue)
                                    {
                                        ZLogger.Message(pawn + " - job in pawn queue: " + job.job);
                                    }
                                    foreach (var job in ZTracker.jobTracker[pawn].activeJobs)
                                    {
                                        ZLogger.Message(pawn + " - job in ZTracker queue: " + job);
                                    }
                                    ZTracker.ResetJobTrackerFor(pawn);
                                }
                            }
                            else
                            {
                                ZLogger.Message(pawn + " has no active jobs");
                            }
                        }
                        catch (Exception ex)
                        {
                            ZLogger.Message("Error2: " + ex);
                        };
                    }
                    else
                    {
                        ZTracker.jobTracker[pawn] = new JobTracker();
                    }
                    ThinkResult result;
                    var oldMap = pawn.Map;
                    var oldPosition = pawn.Position;
                    bool select = false;
                    if (Find.Selector.SelectedObjects.Contains(pawn)) select = true;

                    Map dest = null;
                    result = TryIssueJobPackage(pawn, jobParams, __instance, ___emergency, ref dest, oldMap, oldPosition);
                    if (result.Job != null)
                    {

                        if (pawn.Map != oldMap)
                        {
                            //JobManagerPatches.manualDespawn = true;
                            //pawn.DeSpawn();
                            //JobManagerPatches.manualDespawn = false;
                            //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);

                            Traverse.Create(pawn).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                            Traverse.Create(pawn).Field("positionInt")
                                .SetValue(oldPosition);
                            ZLogger.Message("10 SetPosition for " + pawn + " to " + oldPosition);

                            //pawn.Position = oldPosition;

                        }
                        ZLogger.Message(pawn + " got job " + result + " - map: "
                            + ZTracker.GetMapInfo(pawn.Map) + " - " + pawn.Position);

                        //pawn.ClearReservationsForJob(result.Job);

                        if (dest != null)
                        {
                            ZTracker.BuildJobListFor(pawn, oldMap, dest, result.Job, null);
                        }
                        else
                        {
                            ZTracker.BuildJobListFor(pawn, oldMap, oldMap, result.Job, null);
                        }
                        foreach (var job in ZTracker.jobTracker[pawn].activeJobs)
                        {
                            ZLogger.Message("Active job: " + job);
                        }
                        __result = new ThinkResult(ZTracker.jobTracker[pawn].activeJobs[0], ZTracker.jobTracker[pawn].activeJobs[0].jobGiver);
                        ZTracker.jobTracker[pawn].activeJobs.RemoveAt(0);

                    }
                    else
                    {
                        __result = ThinkResult.NoJob;
                        ZLogger.Message(pawn + " failed to find job");
                    }

                    if (pawn.Map != oldMap)
                    {
                        Traverse.Create(pawn).Field("mapIndexOrState")
                            .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                        Traverse.Create(pawn).Field("positionInt")
                            .SetValue(oldPosition);
                        ZLogger.Message("11 SetPosition for " + pawn + " to " + oldPosition);


                        //pawn.Position = oldPosition;

                        //JobManagerPatches.manualDespawn = true;
                        //pawn.DeSpawn();
                        //JobManagerPatches.manualDespawn = false;
                        //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
                    }
                    if (pawn.Position != oldPosition)
                    {
                        Traverse.Create(pawn).Field("positionInt")
                            .SetValue(oldPosition);
                        ZLogger.Message("12 SetPosition for " + pawn + " to " + oldPosition);

                        //pawn.Position = oldPosition;
                    }

                    if (select) Find.Selector.Select(pawn);
                    return false;
                }
                catch (Exception ex)
                {
                    Log.Error("Some kind of error occurred in Z-Levels JobManager: " + ex);
                }
                return true;
            }

            public static bool TryFindBestBetterStoreCellForValidator(Thing t, Pawn carrier, Map mapToSearch,
                StoragePriority currentPriority, Faction faction, out IntVec3 foundCell, bool needAccurateResult = true)
            {
                Map temp = null;
                return TryFindBestBetterStoreCellFor(t, carrier, mapToSearch, currentPriority, faction, out foundCell, ref temp);
            }

            public static bool TryFindBestBetterStoreCellFor(Thing t, Pawn carrier, Map mapToSearch, StoragePriority currentPriority, Faction faction, out IntVec3 foundCell, ref Map dest, bool needAccurateResult = true)
            {
                bool result = true;
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                List<SlotGroup> allGroupsListInPriorityOrder = new List<SlotGroup>();
                foreach (var map in ZTracker.GetAllMaps(carrier.Map.Tile))
                {
                    allGroupsListInPriorityOrder.AddRange(map.haulDestinationManager.AllGroupsListInPriorityOrder);
                }

                if (allGroupsListInPriorityOrder.Count == 0)
                {
                    foundCell = IntVec3.Invalid;
                    result = false;
                }
                StoragePriority storagePriority = currentPriority;
                float num = 2.14748365E+09f;
                IntVec3 invalid = IntVec3.Invalid;
                int count = allGroupsListInPriorityOrder.Count;

                foreach (var slotGroup in allGroupsListInPriorityOrder.OrderByDescending(x => x.Settings.Priority))
                {
                    ZLogger.Message("Checking on : " + t + " - " + ZTracker.GetMapInfo(t.Map) + " for " + carrier + " - " + carrier.Map + " - " + slotGroup.parent
                        + " - priority: " + slotGroup.Settings.Priority + " - " + slotGroup.parent.Map);
                    StoragePriority priority = slotGroup.Settings.Priority;
                    if (priority < storagePriority || priority <= currentPriority)
                    {
                        ZLogger.Message("Break on - " + slotGroup.parent + " - priority: " + slotGroup.Settings.Priority + " - " + slotGroup.parent.Map);
                        break;
                    }
                    TryIssueJobPackagePatch.TryFindBestBetterStoreCellForWorker
                    (t, carrier, mapToSearch, faction, slotGroup, needAccurateResult,
                    ref invalid, ref num, ref storagePriority, slotGroup.parent.Map, ref dest);
                }
                if (!invalid.IsValid)
                {
                    foundCell = IntVec3.Invalid;
                    result = false;
                }
                foundCell = invalid;

                return result;
            }

            private static void TryFindBestBetterStoreCellForWorker(Thing t, Pawn carrier, Map map, Faction faction, SlotGroup slotGroup, bool needAccurateResult, ref IntVec3 closestSlot, ref float closestDistSquared, ref StoragePriority foundPriority, Map oldMap, ref Map dest)
            {
                if (slotGroup == null)
                {
                    return;
                }
                if (!slotGroup.parent.Accepts(t))
                {
                    return;
                }

                if (slotGroup.HeldThings.Contains(t))
                {
                    ZLogger.Message(t + " already in stockpile " + slotGroup.parent);
                    return;
                }
                ZLogger.Message(slotGroup.parent + " - priority: " + slotGroup.Settings.Priority
                    + " - " + slotGroup.parent.Map + " accepts " + t + " - " + t.Map, true);

                IntVec3 a = t.SpawnedOrAnyParentSpawned ? t.PositionHeld : carrier.PositionHeld;
                List<IntVec3> cellsList = slotGroup.CellsList;
                int count = cellsList.Count;
                int num;
                if (needAccurateResult)
                {
                    num = Mathf.FloorToInt((float)count * Rand.Range(0.005f, 0.018f));
                }
                else
                {
                    num = 0;
                }
                for (int i = 0; i < count; i++)
                {
                    IntVec3 intVec = cellsList[i];
                    //ZLogger.Message("Checking " + intVec + " in " + slotGroup + " in " + slotGroup.parent.Map);
                    float num2 = (float)(a - intVec).LengthHorizontalSquared;
                    if (num2 <= closestDistSquared && IsGoodStoreCell(intVec, slotGroup.parent.Map, t, carrier, faction, oldMap))
                    {
                        closestSlot = intVec;
                        dest = slotGroup.parent.Map;
                        closestDistSquared = num2;
                        foundPriority = slotGroup.Settings.Priority;
                        var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                        ZLogger.Message("RESULT: " + carrier + " - " + slotGroup.parent + " - priority: "
                            + slotGroup.Settings.Priority + " - " + ZTracker.GetMapInfo(slotGroup.parent.Map)
                            + " accepts " + t + " in " + ZTracker.GetMapInfo(dest), true);
                        if (i >= num)
                        {
                            break;
                        }
                    }
                }
            }

            public static bool IsGoodStoreCell(IntVec3 c, Map map, Thing t, Pawn carrier, Faction faction, Map oldMap)
            {
                if (carrier != null && c.IsForbidden(carrier))
                {
                    return false;
                }
                if (!NoStorageBlockersIn(c, map, t))
                {
                    return false;
                }
                if (carrier != null)
                {
                    if (!carrier.CanReserveNew(c))
                    {
                        return false;
                    }
                }
                else if (faction != null && map.reservationManager.IsReservedByAnyoneOf(c, faction))
                {
                    return false;
                }
                if (c.ContainsStaticFire(map))
                {
                    return false;
                }
                List<Thing> thingList = c.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is IConstructible && GenConstruct.BlocksConstruction(thingList[i], t))
                    {
                        return false;
                    }
                }

                return true;// carrier == null || carrier.Map.reachability.CanReach(t.SpawnedOrAnyParentSpawned ? t.PositionHeld : carrier.PositionHeld, c, PathEndMode.ClosestTouch, TraverseParms.For(carrier, Danger.Deadly, TraverseMode.ByPawn, false));
            }

            private static bool NoStorageBlockersIn(IntVec3 c, Map map, Thing thing)
            {
                List<Thing> list = map.thingGrid.ThingsListAt(c);
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing2 = list[i];
                    if (thing2.def.EverStorable(false))
                    {
                        if (!thing2.CanStackWith(thing))
                        {
                            return false;
                        }
                        if (thing2.stackCount >= thing.def.stackLimit)
                        {
                            return false;
                        }
                    }
                    if (thing2.def.entityDefToBuild != null && thing2.def.entityDefToBuild.passability != Traversability.Standable)
                    {
                        return false;
                    }
                    if (thing2.def.surfaceType == SurfaceType.None && thing2.def.passability != Traversability.Standable)
                    {
                        return false;
                    }
                }
                return true;
            }

            private static Job StartOrResumeBillJob(WorkGiver_DoBill scanner, Pawn pawn, IBillGiver giver)
            {
                List<ThingCount> chosenIngThings =
                    Traverse.Create(scanner).Field("chosenIngThings").GetValue<List<ThingCount>>();
                for (int i = 0; i < giver.BillStack.Count; i++)
                {
                    Bill bill = giver.BillStack[i];
                    if ((bill.recipe.requiredGiverWorkType != null && bill.recipe.requiredGiverWorkType
                        != scanner.def.workType) || (Find.TickManager.TicksGame < bill.lastIngredientSearchFailTicks
                        + Traverse.Create(scanner).Field("ReCheckFailedBillTicksRange").GetValue<IntRange>().RandomInRange
                        && FloatMenuMakerMap.makingFor != pawn))
                    {
                        continue;
                    }
                    bill.lastIngredientSearchFailTicks = 0;
                    if (!bill.ShouldDoNow() || !bill.PawnAllowedToStartAnew(pawn))
                    {
                        continue;
                    }
                    SkillRequirement skillRequirement = bill.recipe.FirstSkillRequirementPawnDoesntSatisfy(pawn);
                    if (skillRequirement != null)
                    {
                        JobFailReason.Is("UnderRequiredSkill".Translate(skillRequirement.minLevel), bill.Label);
                        continue;
                    }
                    Bill_ProductionWithUft bill_ProductionWithUft = bill as Bill_ProductionWithUft;

                    if (bill_ProductionWithUft != null)
                    {
                        if (bill_ProductionWithUft.BoundUft != null)
                        {
                            if (bill_ProductionWithUft.BoundWorker == pawn && pawn.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly) && !bill_ProductionWithUft.BoundUft.IsForbidden(pawn))
                            {
                                return Traverse.Create(scanner).Method("FinishUftJob", new object[]
                                {
                                    pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft
                                }).GetValue<Job>();

                                //return FinishUftJob(pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft);
                            }
                            continue;
                        }
                        UnfinishedThing unfinishedThing = Traverse.Create(scanner).Method("ClosestUnfinishedThingForBill", new object[]
                            {
                                    pawn, bill_ProductionWithUft
                            }).GetValue<UnfinishedThing>();

                        //UnfinishedThing unfinishedThing = ClosestUnfinishedThingForBill(pawn, bill_ProductionWithUft);

                        if (unfinishedThing != null)
                        {
                            return Traverse.Create(scanner).Method("FinishUftJob", new object[]
                            {
                                    pawn, unfinishedThing, bill_ProductionWithUft
                            }).GetValue<Job>();
                            //return FinishUftJob(pawn, unfinishedThing, bill_ProductionWithUft);
                        }
                    }

                    bool flag = false;
                    var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                    var origMap = giver.Map;
                    var origMap2 = pawn.Map;
                    ZLogger.Message(giver + " - billGiver.Map: " + ZTracker.GetMapInfo(giver.Map));
                    foreach (var map in ZTracker.GetAllMapsInClosestOrder(giver.Map))
                    {
                        try
                        {
                            Traverse.Create(giver).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(map));
                            Traverse.Create(pawn).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(map));

                            ZLogger.Message("pawn.Map: " + ZTracker.GetMapInfo(pawn.Map));
                            ZLogger.Message("billGiver.Map: " + ZTracker.GetMapInfo(giver.Map));
                            flag = Traverse.Create(scanner).Method("TryFindBestBillIngredients", new object[]
                                    {
                                        bill, pawn, (Thing)giver, chosenIngThings
                                    }).GetValue<bool>();
                            if (flag) break;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Z-Levels failed to process HasJobOnThing of DoBill workgiver. Report about it to devs and provide Hugslib log. Error: " + ex);
                        }
                    }

                    Traverse.Create(giver).Field("mapIndexOrState")
                        .SetValue((sbyte)Find.Maps.IndexOf(origMap));
                    Traverse.Create(pawn).Field("mapIndexOrState")
                        .SetValue((sbyte)Find.Maps.IndexOf(origMap2));

                    if (!flag)
                    {
                        if (FloatMenuMakerMap.makingFor != pawn)
                        {
                            bill.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
                        }
                        else
                        {
                            JobFailReason.Is("MissingMaterials".Translate(), bill.Label);
                        }
                        chosenIngThings.Clear();
                        continue;
                    }
                    Job haulOffJob = null;
                    foreach (var t in chosenIngThings)
                    {
                    }
                    Job result = TryStartNewDoBillJob(pawn, bill, giver, chosenIngThings, out haulOffJob);
                    chosenIngThings.Clear();
                    ZLogger.Message("StartOrResumeBillJob result: " + result);
                    return result;
                }
                chosenIngThings.Clear();
                return null;
            }

            public static Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, out Job haulOffJob, bool dontCreateJobIfHaulOffRequired = true)
            {
                haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
                if (haulOffJob != null && dontCreateJobIfHaulOffRequired)
                {
                    return haulOffJob;
                }
                Job job = JobMaker.MakeJob(JobDefOf.DoBill, (Thing)giver);
                job.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
                job.countQueue = new List<int>(chosenIngThings.Count);
                for (int i = 0; i < chosenIngThings.Count; i++)
                {
                    job.targetQueueB.Add(chosenIngThings[i].Thing);
                    job.countQueue.Add(chosenIngThings[i].Count);
                }
                job.haulMode = HaulMode.ToCellNonStorage;
                job.bill = bill;
                return job;
            }

            public static Job JobOnThing(WorkGiver_DoBill scanner, Pawn pawn, Thing thing, bool forced = false)
            {
                IBillGiver billGiver = thing as IBillGiver;
                if (billGiver == null || !scanner.ThingIsUsableBillGiver(thing)
                    || !billGiver.BillStack.AnyShouldDoNow || !billGiver.UsableForBillsAfterFueling()
                    || !pawn.CanReserve(thing, 1, -1, null, forced) || thing.IsBurning()
                    || thing.IsForbidden(pawn))
                {
                    return null;
                }
                CompRefuelable compRefuelable = thing.TryGetComp<CompRefuelable>();
                if (compRefuelable == null || compRefuelable.HasFuel)
                {
                    billGiver.BillStack.RemoveIncompletableBills();
                    Job job = StartOrResumeBillJob(scanner, pawn, billGiver);
                    return job;
                }
                if (!RefuelWorkGiverUtility.CanRefuel(pawn, thing, forced))
                {
                    return null;
                }
                return RefuelWorkGiverUtility.RefuelJob(pawn, thing, forced, null, null);
            }

            public static Job JobOnThing(WorkGiver_ConstructDeliverResourcesToBlueprints scanner, Pawn pawn, Thing t, bool forced = false)
            {
                if (t.Faction != pawn.Faction)
                {
                    return null;
                }
                Blueprint blueprint = t as Blueprint;
                if (blueprint == null)
                {
                    return null;
                }
                if (GenConstruct.FirstBlockingThing(blueprint, pawn) != null)
                {
                    return GenConstruct.HandleBlockingThingJob(blueprint, pawn, forced);
                }
                bool flag = scanner.def.workType == WorkTypeDefOf.Construction;
                if (!GenConstruct.CanConstruct(blueprint, pawn, flag, forced))
                {
                    return null;
                }

                if (!flag && blueprint.def.entityDefToBuild is TerrainDef
                    && pawn.Map.terrainGrid.CanRemoveTopLayerAt(blueprint.Position))
                {
                    return null;
                }
                Job job = Traverse.Create(scanner).Method("RemoveExistingFloorJob", new object[]
                {
                    pawn, blueprint
                }).GetValue<Job>();

                if (job != null)
                {
                    return job;
                }
                var method = Traverse.Create(scanner).Method("ResourceDeliverJobFor", new object[]
                {
                    pawn, blueprint, true
                });

                Job job2 = method.GetValue<Job>();
                if (job2 == null)
                {
                    var oldMap = pawn.Map;
                    var oldPosition = pawn.Position;

                    var ZTracker = Current.Game.GetComponent<ZLevelsManager>();

                    foreach (var otherMap in ZTracker.ZLevelsTracker[pawn.Map.Tile].ZLevels.Values)
                    {
                        if (otherMap != oldMap)
                        {
                            var stairs = otherMap.listerThings.AllThings.Where(x => x is Building_StairsDown
                            || x is Building_StairsUp).ToList();

                            if (stairs != null && stairs.Count() > 0)
                            {
                                //var selectedStairs = GenClosest.ClosestThing_Global(pawn.Position, stairs, 99999f);
                                var selectedStairs = stairs.MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position));
                                var position = selectedStairs.Position;

                                Traverse.Create(pawn).Field("mapIndexOrState")
                                    .SetValue((sbyte)Find.Maps.IndexOf(otherMap));
                                Traverse.Create(pawn).Field("positionInt")
                                    .SetValue(position);
                                ZLogger.Message("13 SetPosition for " + pawn + " to " + position);

                                //pawn.Position = position;

                                //JobManagerPatches.manualDespawn = true;
                                //pawn.DeSpawn();
                                //JobManagerPatches.manualDespawn = false;
                                //GenPlace.TryPlaceThing(pawn, position, otherMap, ThingPlaceMode.Direct);

                                job2 = method.GetValue<Job>();
                                if (job2 != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (pawn.Map != oldMap)
                    {
                        Traverse.Create(pawn).Field("mapIndexOrState")
                            .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                        Traverse.Create(pawn).Field("positionInt")
                            .SetValue(oldPosition);
                        ZLogger.Message("14 SetPosition for " + pawn + " to " + oldPosition);

                        //pawn.Position = oldPosition;

                        //JobManagerPatches.manualDespawn = true;
                        //pawn.DeSpawn();
                        //JobManagerPatches.manualDespawn = false;
                        //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);

                    }
                }

                if (job2 != null)
                {
                    return job2;
                }
                if (scanner.def.workType != WorkTypeDefOf.Hauling)
                {
                    Job job3 = Traverse.Create(scanner).Method("NoCostFrameMakeJobFor", new object[]
                    {
                        pawn, blueprint
                    }).GetValue<Job>();
                    if (job3 != null)
                    {
                        return job3;
                    }
                }
                return null;
            }

            public static bool NoOneHasJobOn(Thing t, Pawn pawn)
            {
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                try
                {
                    foreach (var jobPawn in ZTracker.jobTracker.Keys)
                    {
                        if (jobPawn.Spawned && !jobPawn.Dead && pawn != jobPawn)
                        {
                            var mainJob = ZTracker.jobTracker[jobPawn].mainJob;
                            if (mainJob != null)
                            {
                                if (mainJob.targetA.Thing == t || mainJob.targetB.Thing == t)
                                {
                                    ZLogger.Message(pawn + "Someone has job on " + t + " - " + jobPawn);
                                    return false;
                                }
                            }
                        }
                    }
                }
                catch { };
                ZLogger.Message("No one has job on " + t);
                return true;
            }

            public static Job JobOnThing(WorkGiver_ConstructDeliverResourcesToFrames scanner, Pawn pawn, Thing t, bool forced = false)
            {
                if (t.Faction != pawn.Faction)
                {
                    return null;
                }
                Frame frame = t as Frame;
                if (frame == null)
                {
                    return null;
                }

                if (GenConstruct.FirstBlockingThing(frame, pawn) != null)
                {
                    return GenConstruct.HandleBlockingThingJob(frame, pawn, forced);
                }
                bool checkSkills = scanner.def.workType == WorkTypeDefOf.Construction;
                if (!GenConstruct.CanConstruct(frame, pawn, checkSkills, forced))
                {
                    return null;
                }
                var method = Traverse.Create(scanner).Method("ResourceDeliverJobFor", new object[]
                {
                    pawn, frame, true
                });

                Job job2 = method.GetValue<Job>();
                if (job2 == null)
                {
                    var oldMap = pawn.Map;
                    var oldPosition = pawn.Position;

                    var ZTracker = Current.Game.GetComponent<ZLevelsManager>();

                    foreach (var otherMap in ZTracker.ZLevelsTracker[pawn.Map.Tile].ZLevels.Values)
                    {
                        if (otherMap != oldMap)
                        {
                            var stairs = otherMap.listerThings.AllThings.Where(x => x is Building_StairsDown
                            || x is Building_StairsUp).ToList();

                            if (stairs != null && stairs.Count() > 0)
                            {
                                //var selectedStairs = GenClosest.ClosestThing_Global(pawn.Position, stairs, 99999f);
                                var selectedStairs = stairs.MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position));
                                var position = selectedStairs.Position;

                                Traverse.Create(pawn).Field("mapIndexOrState")
                                    .SetValue((sbyte)Find.Maps.IndexOf(otherMap));
                                Traverse.Create(pawn).Field("positionInt")
                                    .SetValue(position);
                                ZLogger.Message("15 SetPosition for " + pawn + " to " + position);

                                //pawn.Position = position;

                                //JobManagerPatches.manualDespawn = true;
                                //pawn.DeSpawn();
                                //JobManagerPatches.manualDespawn = false;
                                //GenPlace.TryPlaceThing(pawn, position, otherMap, ThingPlaceMode.Direct);

                                job2 = method.GetValue<Job>();
                                if (job2 != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (pawn.Map != oldMap)
                    {
                        Traverse.Create(pawn).Field("mapIndexOrState")
                            .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                        Traverse.Create(pawn).Field("positionInt")
                            .SetValue(oldPosition);
                        ZLogger.Message("16 SetPosition for " + pawn + " to " + oldPosition);

                        //pawn.Position = oldPosition;

                        //JobManagerPatches.manualDespawn = true;
                        //pawn.DeSpawn();
                        //JobManagerPatches.manualDespawn = false;
                        //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);

                    }
                }

                if (job2 != null)
                {
                    return job2;
                }
                else
                {
                    return null;
                }

                //return ResourceDeliverJobFor(pawn, frame);
            }

            public static bool PawnCanAutomaticallyHaulFast(Pawn p, Thing t, bool forced)
            {
                UnfinishedThing unfinishedThing = t as UnfinishedThing;
                Building building;
                if (unfinishedThing != null && unfinishedThing.BoundBill != null
                    && ((building = (unfinishedThing.BoundBill.billStack.billGiver as Building)) == null
                    || (building.Spawned && building.OccupiedRect().ExpandedBy(1).Contains(unfinishedThing.Position))))
                {
                    return false;
                }
                if (!p.CanReserve(t, 1, -1, null, forced))
                {
                    return false;
                }
                if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    return false;
                }
                if (t.def.IsNutritionGivingIngestible && t.def.ingestible.HumanEdible && !t.IsSociallyProper(p, forPrisoner: false, animalsCare: true))
                {
                    JobFailReason.Is("ReservedForPrisoners".Translate());
                    return false;
                }
                if (t.IsBurning())
                {
                    JobFailReason.Is("BurningLower".Translate());
                    return false;
                }
                return true;
            }

            public static Job HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
            {
                Map dest = null;
                return JobOnThing(pawn, t, ref dest, forced);
            }
            public static Job JobOnThing(Pawn pawn, Thing t, ref Map dest, bool forced = false)
            {
                if (!PawnCanAutomaticallyHaulFast(pawn, t, forced))
                {
                    return null;
                }
                return HaulToStorageJob(pawn, t, ref dest);
            }

            public static Job HaulToStorageJob(Pawn p, Thing t, ref Map dest)
            {
                StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(t);
                if (!TryFindBestBetterStorageFor(t, p, p.Map, currentPriority, p.Faction, out IntVec3 foundCell, out IHaulDestination haulDestination, ref dest))
                {
                    JobFailReason.Is("NoEmptyPlaceLower".Translate());
                    return null;
                }
                if (haulDestination is ISlotGroupParent)
                {
                    return HaulToCellStorageJob(p, t, foundCell, fitInStoreCell: false);
                }
                Thing thing = haulDestination as Thing;
                if (thing != null && thing.TryGetInnerInteractableThingOwner() != null)
                {
                    return HaulToContainerJob(p, t, thing);
                }
                Log.Error("Don't know how to handle HaulToStorageJob for storage " + haulDestination.ToStringSafe() + ". thing=" + t.ToStringSafe());
                return null;
            }
            public static bool TryFindBestBetterStorageFor(Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, out IntVec3 foundCell, out IHaulDestination haulDestination, ref Map dest, bool needAccurateResult = true)
            {
                IntVec3 foundCell2 = IntVec3.Invalid;

                StoragePriority storagePriority = StoragePriority.Unstored;
                if (TryFindBestBetterStoreCellFor(t, carrier, map, currentPriority, faction, out foundCell2, ref dest, needAccurateResult))
                {
                    storagePriority = foundCell2.GetSlotGroup(dest).Settings.Priority;
                }

                if (!TryFindBestBetterNonSlotGroupStorageFor(t, carrier, map, currentPriority, faction, out IHaulDestination haulDestination2, ref dest))
                {
                    haulDestination2 = null;
                }
                if (storagePriority == StoragePriority.Unstored && haulDestination2 == null)
                {
                    foundCell = IntVec3.Invalid;
                    haulDestination = null;
                    return false;
                }
                if (haulDestination2 != null && (storagePriority == StoragePriority.Unstored || (int)haulDestination2.GetStoreSettings().Priority > (int)storagePriority))
                {
                    foundCell = IntVec3.Invalid;
                    haulDestination = haulDestination2;
                    return true;
                }
                foundCell = foundCell2;
                haulDestination = foundCell2.GetSlotGroup(dest).parent;
                return true;
            }

            public static bool TryFindBestBetterNonSlotGroupStorageFor(Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, out IHaulDestination haulDestination, ref Map dest, bool acceptSamePriority = false)
            {
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                List<IHaulDestination> allHaulDestinationsListInPriorityOrder = map.haulDestinationManager.AllHaulDestinationsListInPriorityOrder;
                List<IHaulDestination> allHaulDestinations = new List<IHaulDestination>();
                foreach (var map2 in ZTracker.GetAllMaps(map.Tile))
                {
                    allHaulDestinations.AddRange(map2.haulDestinationManager.AllHaulDestinationsListInPriorityOrder);
                }
                IntVec3 intVec = t.SpawnedOrAnyParentSpawned ? t.PositionHeld : carrier.PositionHeld;
                float num = float.MaxValue;
                StoragePriority storagePriority = StoragePriority.Unstored;
                haulDestination = null;
                foreach (var container in allHaulDestinations.OrderByDescending(x => x.GetStoreSettings().Priority))
                {
                    ZLogger.Message(container + " - 1");
                    if (container is ISlotGroupParent)
                    {
                        continue;
                    }
                    ZLogger.Message(container + " - 2");

                    StoragePriority priority = container.GetStoreSettings().Priority;
                    if ((int)priority < (int)storagePriority || (acceptSamePriority && (int)priority < (int)currentPriority) || (!acceptSamePriority && (int)priority <= (int)currentPriority))
                    {
                        break;
                    }

                    ZLogger.Message(container + " - 3");

                    float num2 = intVec.DistanceToSquared(container.Position);
                    ZLogger.Message(container + " accepts " + t + " - " + (container.Accepts(t)).ToString());
                    ZLogger.Message("num2 > num: " + (num2 > num).ToString());
                    if (num2 > num || !container.Accepts(t))
                    {
                        continue;
                    }
                    Thing thing = container as Thing;
                    if (thing != null && thing.Faction != faction)
                    {
                        continue;
                    }
                    if (thing != null)
                    {
                        if (carrier != null)
                        {
                            if (thing.IsForbidden(carrier))
                            {
                                continue;
                            }
                        }
                        else if (faction != null && thing.IsForbidden(faction))
                        {
                            continue;
                        }
                    }
                    if (thing != null)
                    {
                        if (carrier != null)
                        {
                            //if (!carrier.CanReserveNew(thing))
                            //{
                            //    continue;
                            //}
                        }
                        else if (faction != null && map.reservationManager.IsReservedByAnyoneOf(thing, faction))
                        {
                            continue;
                        }
                    }
                    if (carrier != null)
                    {
                        if (thing != null)
                        {
                            //if (!carrier.Map.reachability.CanReach(intVec, thing, PathEndMode.ClosestTouch, TraverseParms.For(carrier)))
                            //{
                            //    continue;
                            //}
                        }
                        else if (!carrier.Map.reachability.CanReach(intVec, container.Position, PathEndMode.ClosestTouch, TraverseParms.For(carrier)))
                        {
                            continue;
                        }
                    }
                    num = num2;
                    storagePriority = priority;
                    haulDestination = container;
                    dest = map;
                }
                return haulDestination != null;
            }

            public static Job HaulToCellStorageJob(Pawn p, Thing t, IntVec3 storeCell, bool fitInStoreCell)
            {
                Map oldMap = null;
                Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, t, storeCell);
                SlotGroup slotGroup = p.Map.haulDestinationManager.SlotGroupAt(storeCell);
                if (slotGroup != null)
                {
                    Thing thing = p.Map.thingGrid.ThingAt(storeCell, t.def);
                    if (thing != null)
                    {
                        job.count = t.def.stackLimit;
                        if (fitInStoreCell)
                        {
                            job.count -= thing.stackCount;
                        }
                    }
                    else
                    {
                        job.count = 99999;
                    }
                    int num = 0;
                    float statValue = p.GetStatValue(StatDefOf.CarryingCapacity);
                    List<IntVec3> cellsList = slotGroup.CellsList;
                    for (int i = 0; i < cellsList.Count; i++)
                    {
                        if (IsGoodStoreCell(cellsList[i], p.Map, t, p, p.Faction, oldMap))
                        {
                            Thing thing2 = p.Map.thingGrid.ThingAt(cellsList[i], t.def);
                            num = ((thing2 == null || thing2 == t) ? (num + t.def.stackLimit) :
                                (num + Mathf.Max(t.def.stackLimit - thing2.stackCount, 0)));
                            if (num >= job.count || (float)num >= statValue)
                            {
                                break;
                            }
                        }
                    }
                    job.count = Mathf.Min(job.count, num);
                }
                else
                {
                    job.count = 99999;
                }
                job.haulOpportunisticDuplicates = true;
                job.haulMode = HaulMode.ToCellStorage;
                return job;
            }

            public static Job HaulToContainerJob(Pawn p, Thing t, Thing container)
            {
                ThingOwner thingOwner = container.TryGetInnerInteractableThingOwner();
                if (thingOwner == null)
                {
                    Log.Error(container.ToStringSafe() + " gave null ThingOwner.");
                    return null;
                }
                Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, t, container);
                job.count = Mathf.Min(t.stackCount, thingOwner.GetCountCanAccept(t));
                job.haulMode = HaulMode.ToContainer;
                return job;
            }

            public static ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams, JobGiver_Work instance, bool emergency, ref Map dest, Map oldMap, IntVec3 oldPosition)
            {
                if (emergency && pawn.mindState.priorityWork.IsPrioritized)
                {
                    List<WorkGiverDef> workGiversByPriority =
                        pawn.mindState.priorityWork.WorkGiver.workType.workGiversByPriority;
                    for (int i = 0; i < workGiversByPriority.Count; i++)
                    {
                        WorkGiver worker = workGiversByPriority[i].Worker;
                        if (WorkGiversRelated(pawn.mindState.priorityWork.WorkGiver, worker.def))
                        {
                            Job job = GiverTryGiveJobPrioritized(pawn, worker, pawn.mindState.priorityWork.Cell);
                            if (job != null)
                            {
                                job.playerForced = true;
                                return new ThinkResult(job, instance, workGiversByPriority[i].tagToGive);
                            }
                        }
                    }
                    pawn.mindState.priorityWork.Clear();
                }
                List<WorkGiver> list = (!emergency) ? pawn.workSettings.WorkGiversInOrderNormal : pawn.workSettings.WorkGiversInOrderEmergency;
                ZLogger.Message(pawn + " - " + emergency + " - workgiver count: " + list.Count);
                int num = -999;
                TargetInfo bestTargetOfLastPriority = TargetInfo.Invalid;
                WorkGiver_Scanner scannerWhoProvidedTarget = null;
                WorkGiver_Scanner scanner;
                IntVec3 pawnPosition;
                float closestDistSquared;
                float bestPriority;
                bool prioritized;
                bool allowUnreachable;
                Danger maxPathDanger;
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                var entryPoints = new Dictionary<Map, IntVec3>();
                for (int j = 0; j < list.Count; j++)
                {
                    WorkGiver workGiver = list[j];
                    foreach (var otherMap in ZTracker.GetAllMapsInClosestOrder(oldMap))
                    {
                        //ZLogger.Message("Searching job for " + pawn + " in " + ZTracker.GetMapInfo(otherMap)
                        //        + " for " + ZTracker.GetMapInfo(oldMap));
                        var stairs = new List<Thing>();
                        if (ZTracker.GetZIndexFor(otherMap) > ZTracker.GetZIndexFor(oldMap))
                        {
                            Map lowerMap = ZTracker.GetLowerLevel(otherMap.Tile, otherMap);
                            if (lowerMap != null)
                            {
                                //ZLogger.Message("Searching stairs up in " + ZTracker.GetMapInfo(lowerMap));
                                stairs = ZTracker.stairsUp[lowerMap];
                                //lowerMap.listerThings.AllThings.Where(x => x is Building_StairsUp).ToList();
                            }
                            else
                            {
                                ZLogger.Message("Lower map is null in " + ZTracker.GetMapInfo(otherMap));
                            }
                        }
                        else if (ZTracker.GetZIndexFor(otherMap) < ZTracker.GetZIndexFor(oldMap))
                        {
                            Map upperMap = ZTracker.GetUpperLevel(otherMap.Tile, otherMap);
                            if (upperMap != null)
                            {
                                //ZLogger.Message("Searching stairs down in " + ZTracker.GetMapInfo(upperMap));
                                stairs = ZTracker.stairsDown[upperMap];
                                //upperMap.listerThings.AllThings.Where(x => x is Building_StairsDown).ToList();
                            }
                            else
                            {
                                ZLogger.Message("Upper map is null in " + ZTracker.GetMapInfo(otherMap));
                            }
                        }
                        if (stairs != null && stairs.Count() > 0)
                        {
                            IntVec3 position = IntVec3.Invalid;
                            if (!entryPoints.ContainsKey(otherMap))
                            {
                                //var selectedStairs = GenClosest.ClosestThing_Global(pawn.Position, stairs, 99999f);
                                var selectedStairs = stairs.MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position));
                                position = selectedStairs.Position;
                                entryPoints[otherMap] = position;
                            }
                            else
                            {
                                position = entryPoints[otherMap];
                            }

                            Traverse.Create(pawn).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(otherMap));
                            Traverse.Create(pawn).Field("positionInt")
                                .SetValue(position);
                            ZLogger.Message("17 SetPosition for " + pawn + " to " + position);

                            //pawn.Position = position;

                            //JobManagerPatches.manualDespawn = true;
                            //pawn.DeSpawn();
                            //JobManagerPatches.manualDespawn = false;
                            //GenPlace.TryPlaceThing(pawn, position, otherMap, ThingPlaceMode.Direct);

                        }
                        else if (pawn.Map != oldMap && otherMap == oldMap)
                        {

                            //JobManagerPatches.manualDespawn = true;
                            //pawn.DeSpawn();
                            //JobManagerPatches.manualDespawn = false;

                            Traverse.Create(pawn).Field("mapIndexOrState")
                                .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                            Traverse.Create(pawn).Field("positionInt")
                                .SetValue(oldPosition);
                            ZLogger.Message("18 SetPosition for " + pawn + " to " + oldPosition);


                            //if (oldPosition.GetTerrain(oldMap) == ZLevelsDefOf.ZL_OutsideTerrain)
                            //{
                            //    IntVec3 position = IntVec3.Invalid;
                            //    if (!entryPoints.ContainsKey(oldMap))
                            //    {
                            //        //var selectedStairs = GenClosest.ClosestThing_Global(pawn.Position, stairs, 99999f);
                            //        var selectedStairs = stairs.MinBy(x => IntVec3Utility.DistanceTo(pawn.Position, x.Position));
                            //        position = selectedStairs.Position;
                            //        entryPoints[oldMap] = position;
                            //        if (CellFinder.TryFindRandomCellNear(oldPosition, oldMap, 100,
                            //            (IntVec3 c) => c.Walkable(oldMap), out position))
                            //        {
                            //            Traverse.Create(pawn).Field("mapIndexOrState")
                            //                .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                            //            Traverse.Create(pawn).Field("positionInt")
                            //                .SetValue(position);
                            //            //pawn.Position = position;
                            //
                            //            //GenPlace.TryPlaceThing(pawn, position, oldMap, ThingPlaceMode.Direct);
                            //        }
                            //    }
                            //    else
                            //    {
                            //        position = entryPoints[oldMap];
                            //        Traverse.Create(pawn).Field("mapIndexOrState")
                            //            .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                            //        Traverse.Create(pawn).Field("positionInt")
                            //            .SetValue(position);
                            //        //pawn.Position = position;
                            //
                            //        //GenPlace.TryPlaceThing(pawn, position, oldMap, ThingPlaceMode.Direct);
                            //    }
                            //}
                            //else
                            //{
                            //    Traverse.Create(pawn).Field("mapIndexOrState")
                            //        .SetValue((sbyte)Find.Maps.IndexOf(oldMap));
                            //    Traverse.Create(pawn).Field("positionInt")
                            //        .SetValue(oldPosition);
                            //    //pawn.Position = oldPosition;
                            //
                            //    //GenPlace.TryPlaceThing(pawn, oldPosition, oldMap, ThingPlaceMode.Direct);
                            //}
                        }
                        if (workGiver.def.priorityInType != num && bestTargetOfLastPriority.IsValid)
                        {
                            break;
                        }

                        if (!PawnCanUseWorkGiver(pawn, workGiver))
                        {
                            continue;
                        }
                        try
                        {

                            Job job2 = workGiver.NonScanJob(pawn);
                            if (job2 != null)
                            {
                                return new ThinkResult(job2, instance, list[j].def.tagToGive);
                            }

                            scanner = (workGiver as WorkGiver_Scanner);
                            if (scanner != null)
                            {
                                if (scanner.def.scanThings)
                                {
                                    Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) &&
                                    scanner.HasJobOnThing(pawn, t);

                                    Predicate<Thing> deliverResourcesValidator = (Thing t) => !t.IsForbidden(pawn) &&
                                        JobOnThing((WorkGiver_ConstructDeliverResourcesToBlueprints)scanner, pawn, t) != null;

                                    Predicate<Thing> deliverResourcesValidator2 = (Thing t) => !t.IsForbidden(pawn) &&
                                        JobOnThing((WorkGiver_ConstructDeliverResourcesToFrames)scanner, pawn, t) != null;

                                    Predicate<Thing> billValidator = (Thing t) => !t.IsForbidden(pawn)
                                    && TryIssueJobPackagePatch.JobOnThing((WorkGiver_DoBill)scanner, pawn, t) != null
                                    && NoOneHasJobOn(t, pawn);

                                    Predicate<Thing> haulingValidator = (Thing t) => !t.IsForbidden(pawn)
                                    && HasJobOnThing(pawn, t, false) != null;

                                    IEnumerable<Thing> enumerable;

                                    if (scanner is WorkGiver_HaulGeneral || scanner is WorkGiver_HaulCorpses)
                                    {
                                        ZLogger.Message(pawn + " - pawn.map: " + pawn.Map);
                                        var allZones = new Dictionary<Zone_Stockpile, Map>();
                                        var allMaps = ZTracker.GetAllMaps(pawn.Map.Tile);
                                        foreach (Map map in allMaps)
                                        {
                                            foreach (var zone in map.zoneManager.AllZones.Where(x => x is Zone_Stockpile))
                                            {
                                                //ZLogger.Message("Adding " + zone + " in " + ZTracker.GetMapInfo(zone.Map) + " - " + ZTracker.GetMapInfo(map));
                                                allZones[(Zone_Stockpile)zone] = map;
                                            }
                                        }

                                        List<Zone_Stockpile> copiedZones = new List<Zone_Stockpile>();
                                        IntVec3 newPosition2 = IntVec3.Invalid;
                                        foreach (Map map in allMaps)
                                        {
                                            foreach (var zone in allZones)
                                            {
                                                if (zone.Key.Map != pawn.Map)
                                                {
                                                    var newZone = new Zone_Stockpile
                                                    {
                                                        settings = zone.Key.settings,
                                                        label = zone.Key.label + " - Copy"
                                                    };
                                                    foreach (IntVec3 intVec in zone.Key.cells)
                                                    {
                                                        var newPosition = (intVec - zone.Key.Position) + pawn.Position;
                                                        if (newPosition.GetZone(pawn.Map) == null && newPosition.GetSlotGroup(pawn.Map) == null)
                                                        {
                                                            newZone.cells.Add(newPosition);
                                                        }
                                                        else if (CellFinder.TryFindRandomCellNear
                                                            (newPosition, map, 1000, (IntVec3 c) => c.Walkable(map)
                                                            && c.GetZone(map) == null && c.GetSlotGroup(pawn.Map) == null
                                                            && GenGrid.InBounds(c, map), out newPosition2))
                                                        {
                                                            newZone.cells.Add(newPosition2);
                                                        }
                                                    }
                                                    newZone.zoneManager = pawn.Map.zoneManager;
                                                    //ZLogger.Message("Registering " + newZone + " in " + ZTracker.GetMapInfo(pawn.Map));
                                                    pawn.Map.zoneManager.RegisterZone(newZone);
                                                    //ZLogger.Message("Adding " + newZone + " to " + ZTracker.GetMapInfo(pawn.Map));
                                                    copiedZones.Add(newZone);
                                                }
                                            }
                                        }

                                        foreach (var t in pawn.Map.listerThings.AllThings.Where(x => x.def.EverHaulable))
                                        {
                                            pawn.Map.listerHaulables.RecalcAllInCell(t.Position);
                                            pawn.Map.listerMergeables.RecalcAllInCell(t.Position);
                                        }

                                        enumerable = pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();

                                        foreach (var zone in copiedZones)
                                        {
                                            pawn.Map.zoneManager.DeregisterZone(zone);
                                        }
                                        ZLogger.Message("--------------------", true);

                                    }
                                    else
                                    {
                                        enumerable = scanner.PotentialWorkThingsGlobal(pawn);
                                    }
                                    Thing thing = null;
                                    if (scanner.Prioritized)
                                    {
                                        IEnumerable<Thing> enumerable2 = enumerable;
                                        if (enumerable2 == null)
                                        {
                                            enumerable2 = pawn.Map.listerThings.ThingsMatching(scanner.PotentialWorkThingRequest);
                                        }
                                        thing = ((!scanner.AllowUnreachable) ? GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, enumerable2, scanner.PathEndMode, TraverseParms.For(pawn, scanner.MaxPathDanger(pawn)), 9999f, validator, (Thing x) => scanner.GetPriority(pawn, x)) : GenClosest.ClosestThing_Global(pawn.Position, enumerable2, 99999f, validator, (Thing x) => scanner.GetPriority(pawn, x)));
                                    }
                                    else if (scanner.AllowUnreachable)
                                    {
                                        IEnumerable<Thing> enumerable3 = enumerable;
                                        if (enumerable3 == null)
                                        {
                                            enumerable3 = pawn.Map.listerThings.ThingsMatching(scanner.PotentialWorkThingRequest);
                                        }
                                        thing = GenClosest.ClosestThing_Global(pawn.Position, enumerable3, 99999f, validator);
                                    }
                                    else
                                    {
                                        if (scanner is WorkGiver_HaulGeneral || scanner is WorkGiver_HaulCorpses)
                                        {
                                            thing = GenClosest.ClosestThingReachable
                                                (pawn.Position, pawn.Map, scanner.PotentialWorkThingRequest,
                                                scanner.PathEndMode, TraverseParms.For(pawn, scanner.MaxPathDanger(pawn)),
                                                9999f, haulingValidator, enumerable, 0, scanner.MaxRegionsToScanBeforeGlobalSearch,
                                                enumerable != null);
                                            try
                                            {
                                                if (enumerable != null)
                                                {
                                                    ZLogger.Message("Try get thing from enumerable: " + enumerable.Count(), true);
                                                    foreach (var t in enumerable)
                                                    {
                                                        ZLogger.Message(t + " in enumerable: " + ZTracker.GetMapInfo(pawn.Map));
                                                    }
                                                }

                                                if (thing != null)
                                                {
                                                    ZLogger.Message("Get thing: " + thing + " in " + thing.Map + " for " + pawn + " in " + ZTracker.GetMapInfo(pawn.Map), true);
                                                }
                                                else
                                                {
                                                    ZLogger.Message("Cant get thing for " + pawn + " in " + ZTracker.GetMapInfo(pawn.Map), true);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ZLogger.Message(ex.ToString());
                                            };
                                        }

                                        else if (scanner is WorkGiver_DoBill)
                                        {
                                            try
                                            {
                                                thing = GenClosest.ClosestThingReachable
                                                    (pawn.Position, pawn.Map, scanner.PotentialWorkThingRequest,
                                                    scanner.PathEndMode, TraverseParms.For(pawn,
                                                    scanner.MaxPathDanger(pawn)), 9999f, billValidator,
                                                    enumerable, 0, scanner.MaxRegionsToScanBeforeGlobalSearch,
                                                    enumerable != null);
                                                //ZLogger.Message("Get thing: " + thing + " in " + pawn.Map);
                                            }
                                            catch { };

                                        }

                                        else if (scanner is WorkGiver_ConstructDeliverResourcesToBlueprints)
                                        {
                                            thing = GenClosest.ClosestThingReachable
                                                (pawn.Position, pawn.Map, scanner.PotentialWorkThingRequest,
                                                scanner.PathEndMode, TraverseParms.For(pawn,
                                                scanner.MaxPathDanger(pawn)), 9999f, deliverResourcesValidator,
                                                enumerable, 0, scanner.MaxRegionsToScanBeforeGlobalSearch,
                                                enumerable != null);
                                        }

                                        else if (scanner is WorkGiver_ConstructDeliverResourcesToFrames)
                                        {
                                            thing = GenClosest.ClosestThingReachable
                                                (pawn.Position, pawn.Map, scanner.PotentialWorkThingRequest,
                                                scanner.PathEndMode, TraverseParms.For(pawn,
                                                scanner.MaxPathDanger(pawn)), 9999f, deliverResourcesValidator2,
                                                enumerable, 0, scanner.MaxRegionsToScanBeforeGlobalSearch,
                                                enumerable != null);
                                        }
                                        else
                                        {
                                            thing = GenClosest.ClosestThingReachable
                                                (pawn.Position, pawn.Map, scanner.PotentialWorkThingRequest,
                                                scanner.PathEndMode, TraverseParms.For(pawn,
                                                scanner.MaxPathDanger(pawn)), 9999f, validator,
                                                enumerable, 0, scanner.MaxRegionsToScanBeforeGlobalSearch,
                                                enumerable != null);
                                        }

                                        if (thing != null)
                                        {
                                            ZLogger.Message(pawn + " - " + ZTracker.GetMapInfo(pawn.Map) + " - " + scanner + " Selected thing: " + thing);
                                        }
                                    }
                                    if (thing != null)
                                    {
                                        bestTargetOfLastPriority = thing;
                                        scannerWhoProvidedTarget = scanner;
                                    }
                                }
                                if (scanner.def.scanCells)
                                {
                                    pawnPosition = pawn.Position;
                                    closestDistSquared = 99999f;
                                    bestPriority = float.MinValue;
                                    prioritized = scanner.Prioritized;
                                    allowUnreachable = scanner.AllowUnreachable;
                                    maxPathDanger = scanner.MaxPathDanger(pawn);
                                    IEnumerable<IntVec3> enumerable4 = scanner.PotentialWorkCellsGlobal(pawn);
                                    IList<IntVec3> list2;
                                    if ((list2 = (enumerable4 as IList<IntVec3>)) != null)
                                    {
                                        for (int k = 0; k < list2.Count; k++)
                                        {
                                            ProcessCell(list2[k]);
                                        }
                                    }
                                    else
                                    {
                                        foreach (IntVec3 item in enumerable4)
                                        {
                                            ProcessCell(item);
                                        }
                                    }
                                }
                            }

                            void ProcessCell(IntVec3 c)
                            {
                                bool flag = false;
                                float num2 = (c - pawnPosition).LengthHorizontalSquared;
                                float num3 = 0f;
                                if (prioritized)
                                {
                                    if (!c.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, c))
                                    {
                                        if (!allowUnreachable && !pawn.CanReach(c, scanner.PathEndMode, maxPathDanger))
                                        {
                                            return;
                                        }
                                        num3 = scanner.GetPriority(pawn, c);
                                        if (num3 > bestPriority || (num3 == bestPriority && num2 < closestDistSquared))
                                        {
                                            flag = true;
                                        }
                                    }
                                }
                                else if (num2 < closestDistSquared && !c.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, c))
                                {
                                    if (!allowUnreachable && !pawn.CanReach(c, scanner.PathEndMode, maxPathDanger))
                                    {
                                        return;
                                    }
                                    flag = true;
                                }
                                if (flag)
                                {
                                    bestTargetOfLastPriority = new TargetInfo(c, pawn.Map);
                                    scannerWhoProvidedTarget = scanner;
                                    closestDistSquared = num2;
                                    bestPriority = num3;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Concat(pawn, " threw exception in WorkGiver ", workGiver.def.defName, ": ", ex.ToString()));
                        }
                        finally
                        {

                        }
                        if (bestTargetOfLastPriority.IsValid)
                        {
                            Job job3 = null;
                            if (scannerWhoProvidedTarget is WorkGiver_HaulGeneral || scannerWhoProvidedTarget is WorkGiver_HaulCorpses)
                            {
                                job3 = (!bestTargetOfLastPriority.HasThing) ?
                                    scannerWhoProvidedTarget.JobOnCell(pawn, bestTargetOfLastPriority.Cell)
                                    : JobOnThing(pawn, bestTargetOfLastPriority.Thing, ref dest);
                                ZLogger.Message("1 - " + scannerWhoProvidedTarget + " - " + job3);
                            }
                            else if (scannerWhoProvidedTarget is WorkGiver_DoBill scanner1)
                            {
                                job3 = (!bestTargetOfLastPriority.HasThing) ?
                                    scannerWhoProvidedTarget.JobOnCell(pawn, bestTargetOfLastPriority.Cell)
                                    : JobOnThing(scanner1, pawn, bestTargetOfLastPriority.Thing);
                                ZLogger.Message("2 - " + scannerWhoProvidedTarget + " - " + job3);
                            }
                            else if (scannerWhoProvidedTarget is WorkGiver_ConstructDeliverResourcesToBlueprints scanner2)
                            {
                                job3 = (!bestTargetOfLastPriority.HasThing) ?
                                    scannerWhoProvidedTarget.JobOnCell(pawn, bestTargetOfLastPriority.Cell)
                                    : JobOnThing(scanner2, pawn, bestTargetOfLastPriority.Thing);
                                ZLogger.Message("3 - " + scannerWhoProvidedTarget + " - " + job3);
                            }
                            else if (scannerWhoProvidedTarget is WorkGiver_ConstructDeliverResourcesToFrames scanner3)
                            {
                                job3 = (!bestTargetOfLastPriority.HasThing) ?
                                    scannerWhoProvidedTarget.JobOnCell(pawn, bestTargetOfLastPriority.Cell)
                                    : JobOnThing(scanner3, pawn, bestTargetOfLastPriority.Thing);
                                ZLogger.Message("4 - " + scannerWhoProvidedTarget + " - " + job3);
                            }
                            else
                            {
                                job3 = (!bestTargetOfLastPriority.HasThing) ?
                                    scannerWhoProvidedTarget.JobOnCell(pawn, bestTargetOfLastPriority.Cell)
                                    : scannerWhoProvidedTarget.JobOnThing(pawn, bestTargetOfLastPriority.Thing);
                                ZLogger.Message("5 - " + scannerWhoProvidedTarget + " - " + job3);
                            }

                            if (job3 != null)
                            {
                                job3.workGiverDef = scannerWhoProvidedTarget.def;
                                if (dest == null)
                                {
                                    dest = otherMap;
                                }
                                ZLogger.Message("Main job stats: ");
                                ZLogger.Message("Job: " + job3);
                                ZLogger.Message("Job.targetA: " + job3.targetA);
                                ZLogger.Message("Job.targetB: " + job3.targetB);
                                ZLogger.Message("Job.targetC: " + job3.targetC);
                                ZLogger.Message("Job.targetQueueA: " + job3.targetQueueA);
                                ZLogger.Message("Job.targetQueueB: " + job3.targetQueueB);
                                ZLogger.Message("Job.countQueue: " + job3.countQueue);
                                return new ThinkResult(job3, instance, list[j].def.tagToGive);
                            }
                            //Log.ErrorOnce(string.Concat(scannerWhoProvidedTarget, " provided target ", bestTargetOfLastPriority, " but yielded no actual job for pawn ", pawn, ". The CanGiveJob and JobOnX methods may not be synchronized."), 6112651);
                        }
                        num = workGiver.def.priorityInType;
                    }
                }
                return ThinkResult.NoJob;
            }

            private static bool HaulShouldSkip(Pawn pawn, WorkGiver giver)
            {
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                foreach (var map in ZTracker.GetAllMaps(pawn.Map.Tile))
                {
                    if (map.listerHaulables.ThingsPotentiallyNeedingHauling().Count > 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            private static bool PawnCanUseWorkGiver(Pawn pawn, WorkGiver giver)
            {
                if (!giver.def.nonColonistsCanDo && !pawn.IsColonist)
                {
                    //ZLogger.Message(pawn + " cannot use " + giver + " - reason: !giver.def.nonColonistsCanDo && !pawn.IsColonist");
                    return false;
                }
                if (pawn.WorkTagIsDisabled(giver.def.workTags))
                {
                    //ZLogger.Message(pawn + " cannot use " + giver + " - reason: pawn.WorkTagIsDisabled(giver.def.workTags)");
                    return false;
                }
                if (giver is WorkGiver_Haul || giver is WorkGiver_HaulCorpses)
                {

                }
                else if (giver.ShouldSkip(pawn))
                {
                    //ZLogger.Message(pawn + " cannot use " + giver + " - reason: giver.ShouldSkip(pawn)");
                    return false;
                }
                if (giver.MissingRequiredCapacity(pawn) != null)
                {
                    //ZLogger.Message(pawn + " cannot use " + giver + " - reason: giver.MissingRequiredCapacity(pawn) != null");
                    return false;
                }
                //ZLogger.Message(pawn + " can use " + giver);
                return true;
            }

            private static bool WorkGiversRelated(WorkGiverDef current, WorkGiverDef next)
            {
                if (next == WorkGiverDefOf.Repair)
                {
                    return current == WorkGiverDefOf.Repair;
                }
                return true;
            }

            private static Job GiverTryGiveJobPrioritized(Pawn pawn, WorkGiver giver, IntVec3 cell)
            {
                if (!PawnCanUseWorkGiver(pawn, giver))
                {
                    return null;
                }
                try
                {
                    Job job = giver.NonScanJob(pawn);
                    if (job != null)
                    {
                        return job;
                    }
                    WorkGiver_Scanner scanner = giver as WorkGiver_Scanner;
                    if (scanner != null)
                    {
                        if (giver.def.scanThings)
                        {
                            Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn) &&
                            scanner.HasJobOnThing(pawn, t);
                            List<Thing> thingList = cell.GetThingList(pawn.Map);
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                Thing thing = thingList[i];
                                if (scanner.PotentialWorkThingRequest.Accepts(thing) && predicate(thing))
                                {
                                    Job job2 = scanner.JobOnThing(pawn, thing);
                                    if (job2 != null)
                                    {
                                        job2.workGiverDef = giver.def;
                                    }
                                    return job2;
                                }
                            }
                        }
                        if (giver.def.scanCells && !cell.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, cell))
                        {
                            Job job3 = scanner.JobOnCell(pawn, cell);
                            if (job3 != null)
                            {
                                job3.workGiverDef = giver.def;
                            }
                            return job3;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(string.Concat(pawn, " threw exception in GiverTryGiveJobTargeted on WorkGiver ", giver.def.defName, ": ", ex.ToString()));
                }
                return null;
            }
        }
    }
}
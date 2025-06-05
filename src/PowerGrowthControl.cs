using HarmonyLib;
using MGSC;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FactionGrowthControl.Properties
{
    public class PowerGrowthControl
    {
        [HarmonyPatch(typeof(ProcMissionRecord), "get_PowerCost")]
        public static class MissionPowerCostOverrides
        {
            public static void Postfix(ProcMissionRecord __instance, ref int __result)
            {
                if (__instance.ProcMissionType == ProceduralMissionType.RaiderCapture)
                    __result = 200;
                else if (__instance.ProcMissionType == ProceduralMissionType.Sabotage ||
                         __instance.ProcMissionType == ProceduralMissionType.Robbery)
                    __result = 100;
                else if (__instance.ProcMissionType == ProceduralMissionType.Espionage ||
                         __instance.ProcMissionType == ProceduralMissionType.Elimination)
                    __result = 50;
            }
        }

        [HarmonyPatch(typeof(ProcMissionRecord), "get_PowerBuff")]
        public static class MissionPowerBuffOverrides
        {
            public static void Postfix(ProcMissionRecord __instance, ref int __result)
            {
                if (__instance.ProcMissionType == ProceduralMissionType.Sabotage ||
                    __instance.ProcMissionType == ProceduralMissionType.Ritual ||
                    __instance.ProcMissionType == ProceduralMissionType.Elimination ||
                    __instance.ProcMissionType == ProceduralMissionType.Robbery)
                    __result = 200;
                else if (__instance.ProcMissionType == ProceduralMissionType.Espionage)
                    __result = 100;
                else if (__instance.ProcMissionType == ProceduralMissionType.RaiderCapture)
                    __result = 0;
            }
        }
    }

    [HarmonyPatch(typeof(MissionSystem), "TryAddPower")]
    public static class TryAddPowerPatch
    {
        public static bool Prefix(
            Stations stations,
            Factions factions,
            Faction faction,
            int value,
            ref bool __result)
        {
            int ownedCount = stations.Values.Count(s => s.OwnerFactionId == faction.Id);
            float multiplier = CalculateGainMultiplier(ownedCount);
            int adjustedValue = (int)Mathf.Ceil(value * multiplier);

            Plugin.Logger.Log($"[TryAddPower] Faction: {faction.Id}, Owned: {ownedCount}, Multiplier: {multiplier:F2}, Value: {value} → {adjustedValue}");

            MissionSystem._stationsCache.Clear();
            Station selected = null;
            int minPower = int.MaxValue;

            foreach (var station in stations.Values)
            {
                if (station.OwnerFactionId == faction.Id)
                {
                    MissionSystem._stationsCache.Add(station);
                    if (station.GainedPower <= minPower)
                    {
                        selected = station;
                        minPower = selected.GainedPower;
                    }
                }
            }

            if (selected == null)
            {
                __result = false;
                return false;
            }

            Plugin.Logger.Log($"[TryAddPower] ✅ Selected Station: {selected.Id}, Old Power: {selected.GainedPower}");
            StationSystem.GainPower(factions, selected, adjustedValue);
            __result = true;
            return false;
        }

        private static float CalculateGainMultiplier(int count)
        {
            if (count >= 9) return 1f;
            int level = 8 - count;
            float result = Mathf.Pow(1.3f, level);
            Plugin.Logger.Log($"[GainMultiplier] StationCount={count}, Level={level}, Multiplier={result:F2}");
            return result;
        }
    }

    [HarmonyPatch(typeof(MissionSystem), "TryExtractPower")]
    public static class TryExtractPowerPatch
    {
        public static bool Prefix(
            Stations stations,
            Factions factions,
            Faction faction,
            int value,
            List<Station> stationsCache,
            ref bool __result)
        {
            stationsCache.Clear();
            int totalPower = 0;
            int ownedCount = 0;

            foreach (Station station in stations.Values)
            {
                if (station.OwnerFactionId == faction.Id)
                {
                    ownedCount++;
                    totalPower += station.GainedPower;
                    stationsCache.Add(station);
                }
            }

            float multiplier = CalculateExtractMultiplier(ownedCount);
            int adjustedValue = (int)Mathf.Ceil(value * multiplier);

            Plugin.Logger.Log($"[TryExtractPower] Faction: {faction.Id}, Owned: {ownedCount}, TotalPower: {totalPower}, Multiplier: {multiplier:F2}, Value: {value} → {adjustedValue}");

            if (stationsCache.Count == 0 || totalPower < adjustedValue)
            {
                Plugin.Logger.Log("[TryExtractPower] ❌ Not enough power or stations.");
                __result = false;
                return false;
            }

            stationsCache.Sort((x, y) => y.GainedPower.CompareTo(x.GainedPower));

            for (int i = 0; i < stationsCache.Count && adjustedValue > 0; ++i)
            {
                Station s = stationsCache[i];
                int extract = Mathf.Min(adjustedValue, s.GainedPower);
                Plugin.Logger.Log($"[TryExtractPower] Extracting {extract} from Station {s.Id} (Current: {s.GainedPower})");
                StationSystem.ExtractPower(factions, s, extract);
                adjustedValue -= extract;
            }

            stationsCache.Clear();
            __result = true;
            return false;
        }

        private static float CalculateExtractMultiplier(int count)
        {
            float result;

            if ( count <= 7)
            {
                int level = 8 - count;
                result = Mathf.Pow(0.7f, level);
            }
            else if (count <= 12)
            {
                result = 1.0f;
            }
            else // (13 <= count_ 
            {
                int level = count - 12;
                result = Mathf.Pow(1.1f, level);
            }

            Plugin.Logger.Log($"[ExtractMultiplier] StationCount={count}, Multiplier={result:F2}");
            return Mathf.Clamp(result, 0.2f, 1.9f);
        }
    }
}

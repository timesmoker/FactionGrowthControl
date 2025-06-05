using HarmonyLib;
using MGSC;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FactionGrowthControl
{
    public static class MissionSys_PowerDeltaPatch
    {
        private static int PowerBonusThreshold => Plugin.Config.PowerBonusThreshold;
        private static float PowerGainBonusMultiplier => Plugin.Config.PowerGainBonusMultiplier;
        private static float PowerLossBonusMultiplier => Plugin.Config.PowerLossBonusMultiplier;

        private static int PowerPenaltyThreshold => Plugin.Config.PowerPenaltyThreshold;
        private static float PowerPenaltyMultiplier => Plugin.Config.PowerPenaltyMultiplier;

        private static float MultiplierMin => Plugin.Config.MultiplierMin;
        private static float MultiplierMax => Plugin.Config.MultiplierMax;

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.TryAddPower))]
        public class TryAddPower_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(
                Stations stations,
                Factions factions,
                Faction faction,
                int value,
                ref bool __result)
            {
                MissionSystem._stationsCache.Clear();

                Station selected = null;
                int minPower = int.MaxValue;
                int ownedCount = 0;

                foreach (var station in stations.Values)
                {
                    if (station.OwnerFactionId != faction.Id)
                        continue;

                    ownedCount++;
                    MissionSystem._stationsCache.Add(station);

                    if (station.GainedPower <= minPower)
                    {
                        selected = station;
                        minPower = station.GainedPower;
                    }
                }

                if (selected == null)
                {
                    __result = false;
                    return false;
                }

                value = AdjustGainValue(value, ownedCount);

                StationSystem.GainPower(factions, selected, value);
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.TryExtractPower))]
        public class TryExtractPower_Patch
        {
            [HarmonyPrefix]
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

                value = AdjustExtractValue(value, ownedCount);

                if (stationsCache.Count == 0 || totalPower < value)
                {
                    __result = false;
                    return false;
                }

                stationsCache.Sort((x, y) => y.GainedPower.CompareTo(x.GainedPower));

                for (int i = 0; i < stationsCache.Count && value > 0; ++i)
                {
                    Station s = stationsCache[i];
                    int extract = Mathf.Min(value, s.GainedPower);
                    StationSystem.ExtractPower(factions, s, extract);
                    value -= extract;
                }

                stationsCache.Clear();
                __result = true;
                return false;
            }
        }

        private static int AdjustGainValue(int baseValue, int count)
        {
            int adjustedValue;
            float multiplier = 1f;

            if (count < PowerBonusThreshold)
            {
                int level = PowerBonusThreshold - count;
                multiplier = Mathf.Pow(PowerGainBonusMultiplier, level);
                multiplier = Mathf.Clamp(multiplier, MultiplierMin, MultiplierMax);
                adjustedValue = (int)(baseValue * multiplier);
            }
            else
            {
                adjustedValue = baseValue;
            }

            return adjustedValue;
        }

        private static int AdjustExtractValue(int baseValue, int count)
        {
            int adjustedValue;
            float multiplier = 1f;

            if (count < PowerBonusThreshold)
            {
                int level = PowerBonusThreshold - count;
                multiplier = Mathf.Pow(PowerLossBonusMultiplier, level);
                multiplier = Mathf.Clamp(multiplier, MultiplierMin, MultiplierMax);
                adjustedValue = (int)(baseValue * multiplier);
            }
            else if (count <= PowerPenaltyThreshold)
            {
                adjustedValue = baseValue;
            }
            else
            {
                int level = count - PowerPenaltyThreshold;
                multiplier = Mathf.Pow(PowerPenaltyMultiplier, level);
                multiplier = Mathf.Clamp(multiplier, MultiplierMin, MultiplierMax);
                adjustedValue = (int)(baseValue * multiplier);
            }

            return adjustedValue;
        }
    }
}

using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FactionGrowthControl
{
    public static class FactionSys_MissionCapPatch
    {
        private static int MaxMissionsCap => (int)(Data.Stations.Count * Plugin.Config.TotalMissionCapRate);
        private static readonly System.Random Rng = new System.Random();

        [HarmonyPatch(typeof(FactionSystem), nameof(FactionSystem.FactionsTick))]
        public class FactionsTickPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(
                MissionFactory missionFactory,
                Missions missions,
                Stations stations,
                News news,
                Factions factions,
                SpaceTime spaceTime,
                SpaceTickTimers spaceTickTimers,
                TravelMetadata travelMetadata,
                FactionCachedData cache)
            {
                double totalHours = (spaceTime.Time - spaceTickTimers.LastFactionsTick).TotalHours;
                float tickIntervalHours = Data.Global.FactionTickIntervalHours;
                if (totalHours < tickIntervalHours)
                    return false;

                CacheSystem.CacheStationOwners(cache, stations);

                for (; totalHours >= tickIntervalHours; totalHours -= tickIntervalHours)
                {
                    spaceTickTimers.LastFactionsTick = spaceTickTimers.LastFactionsTick.AddHours(tickIntervalHours);

                    if (missions.Values.Count < MaxMissionsCap)
                    {
                        var factionList = factions.Values.ToList();
                        Shuffle(factionList);
                        foreach (var faction in factionList)
                        {
                            if (
                                !factions.IsEnabledFaction(faction) ||
                                !FactionSystem.IsMissionChanceApproved(faction) ||
                                FactionSystem.IsMissionsCountReachTechLevelLimit(faction, missions) ||
                                !cache.factionsOwnedStations.TryGetValue(faction.Id, out var stationList) ||
                                stationList.Count == 0)
                                continue;

                            FactionSystem.TryCreateMission(
                                faction,
                                missionFactory,
                                missions,
                                stations,
                                news,
                                factions,
                                spaceTime,
                                travelMetadata,
                                cache
                            );
                        }
                    }
                }

                return false; 
            }
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}

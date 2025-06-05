using HarmonyLib;
using MGSC;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FactionGrowthControl
{
    
    [HarmonyPatch(typeof(MissionSystem), "Update")]
    public static class MissionWinrateControl
    {
        static bool Prefix(
        Missions missions,
        Stations stations,
        News news,
        SpaceTime spaceTime,
        PopulationDebugData populationDebugData,
        TravelMetadata travelMetadata,
        Factions factions,
        ItemsPrices itemsPrices,
        Difficulty difficulty)
    {
            for (int index = missions.Values.Count - 1; index >= 0 && missions.Values.Count != 0; --index)
            {
                var mission = missions.Values[index];
                if (mission.ExpireTime > spaceTime.Time) continue;

                Plugin.Logger.Log($"[MissionUpdate] Processing mission: {mission.ProcMissionType} at station {mission.StationId}");

                if (mission.IsStoryMission)
                {
                    Plugin.Logger.Log("[MissionUpdate] Skipped story mission");
                    MissionSystem.RemoveMission(missions, mission.StationId);
                    continue;
                }

                var station = stations.Get(mission.StationId);
                var faction1 = factions.Get(mission.BeneficiaryFactionId);
                var faction2 = factions.Get(mission.VictimFactionId);

                float A = faction1.Power;
                float B = faction2.Power;

                Plugin.Logger.Log($"[MissionUpdate] Faction power - Beneficiary: {A}, Victim: {B}");

                float chance = 0.2f;

                if (A > 0 && B > 0)
                {
                    float logRatio = 0.2f * Mathf.Log10(Mathf.Max(A, B) / Mathf.Min(A, B));
                    float logDiff = 0.0325f * Mathf.Log10(Mathf.Abs(A - B));

                    Plugin.Logger.Log($"[MissionUpdate] logRatio={logRatio:F4}, logDiff={logDiff:F4}");

                    if (A > B)
                    {
                        chance += logRatio + logDiff;
                    }
                    else if (A < B)
                    {
                        chance -= logRatio + logDiff;
                    }
                }

                chance = Mathf.Clamp(chance, 0.2f, 0.8f);
                Plugin.Logger.Log($"[MissionUpdate] Final chance = {chance:P1}");

                bool isSuccess = UnityEngine.Random.value < chance;
                Plugin.Logger.Log($"[MissionUpdate] Mission {(isSuccess ? "Succeeded" : "Failed")}");

                var newsEvent = new NewsEvent
                {
                    Factions = { faction1.Id, faction2.Id },
                    Parameters = { station.Id }
                };

                var rec = Data.ProcMissions.Get(mission.ProcMissionType);
                if (isSuccess)
                {
                    newsEvent.NewsType = rec.NewsTypeEndGood;
                    Plugin.Logger.Log($"[MissionUpdate] NewsType = {newsEvent.NewsType} (Success)");
                    MissionSystem.ProcessMissionSuccessActions(
                        stations, spaceTime, populationDebugData, travelMetadata, factions, itemsPrices, difficulty, mission);
                }
                else
                {
                    newsEvent.NewsType = rec.NewsTypeEndFail;
                    Plugin.Logger.Log($"[MissionUpdate] NewsType = {newsEvent.NewsType} (Failure)");
                    MissionSystem.ProcessMissionFailureActions(stations, spaceTime, travelMetadata, factions, mission);

                    if (travelMetadata.CurrentSpaceObject.Equals(station.SpaceObjectId) && !travelMetadata.IsInTravel)
                    {
                        UI.Get<SpaceHudScreen>().RefreshUIOnArrival(travelMetadata.CurrentSpaceObject);
                        Plugin.Logger.Log("[MissionUpdate] UI refreshed due to mission failure at current location.");
                    }
                }

                NewsSystem.AddNews(news, spaceTime, travelMetadata, newsEvent);
                Plugin.Logger.Log("[MissionUpdate] News event added.");

                MissionSystem.RemoveMission(missions, mission.StationId);
                Plugin.Logger.Log("[MissionUpdate] Mission removed.");
            }

            return false; 
        }
    }
}



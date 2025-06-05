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
            float chance = 0.2f;
            float logRatioWeight = 0.2f;
            float logDiffWeight = 0.0325f;
            float chanceMin = 0.2f;
            float chanceMax = 0.8f;
            
            for (int index = missions.Values.Count - 1; index >= 0 && missions.Values.Count != 0; --index)
            {
                var mission = missions.Values[index];
                if (mission.ExpireTime > spaceTime.Time) continue;

                Plugin.Logger.Log($"[MissionUpdate] Processing mission: {mission.ProcMissionType} at station {mission.StationId}");

                if (mission.IsStoryMission)
                {
                    MissionSystem.RemoveMission(missions, mission.StationId);
                    continue;
                }

                var station = stations.Get(mission.StationId);
                var faction1 = factions.Get(mission.BeneficiaryFactionId);
                var faction2 = factions.Get(mission.VictimFactionId);

                if (faction1.Power > 0 && faction2.Power > 0)
                {
                    float max = Mathf.Max(faction1.Power, faction2.Power);
                    float min = Mathf.Min(faction1.Power, faction2.Power);
                    float diff = Mathf.Abs(faction1.Power - faction2.Power);
                    int sign = faction1.Power > faction2.Power ? 1 : (faction1.Power < faction2.Power ? -1 : 0);

                    float logRatio = logRatioWeight * Mathf.Log10(max / min);
                    float logDiff  = logDiffWeight  * Mathf.Log10(diff);

                    chance += sign * (logRatio + logDiff);
                }
                
                chance = Mathf.Clamp(chance, chanceMin, chanceMax);
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
                    MissionSystem.ProcessMissionSuccessActions(
                        stations, spaceTime, populationDebugData, travelMetadata, factions, itemsPrices, difficulty, mission);
                }
                else
                {
                    newsEvent.NewsType = rec.NewsTypeEndFail;
                    MissionSystem.ProcessMissionFailureActions(stations, spaceTime, travelMetadata, factions, mission);

                    if (travelMetadata.CurrentSpaceObject.Equals(station.SpaceObjectId) && !travelMetadata.IsInTravel)
                    {
                        UI.Get<SpaceHudScreen>().RefreshUIOnArrival(travelMetadata.CurrentSpaceObject);
                    }
                }

                NewsSystem.AddNews(news, spaceTime, travelMetadata, newsEvent);

                MissionSystem.RemoveMission(missions, mission.StationId);
            }

            return false; 
        }
    }
}



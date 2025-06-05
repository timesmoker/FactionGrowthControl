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
    public static class MissionSys_WinRatePatch
    {
        static float BaseWinRate => Plugin.Config.BaseWinRate;
        static float WinRateLogRatioWeight => Plugin.Config.WinRateLogRatioWeight;
        static float WinRateLogDiffWeight => Plugin.Config.WinRateLogDiffWeight;
        static float WinRateMin => Plugin.Config.WinRateMin;
        static float WinRateMax => Plugin.Config.WinRateMax;

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.Update))]
        public class UpdatePatch
        {
            [HarmonyPrefix]
            public static bool Prefix(
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

                    if (mission.IsStoryMission)
                    {
                        MissionSystem.RemoveMission(missions, mission.StationId);
                        continue;
                    }

                    var station = stations.Get(mission.StationId);
                    var faction1 = factions.Get(mission.BeneficiaryFactionId);
                    var faction2 = factions.Get(mission.VictimFactionId);

                    float max = Math.Max(1, Math.Max(faction1.Power, faction2.Power));
                    int min = Math.Max(1, Math.Min(faction1.Power, faction2.Power));
                    int diff = Math.Max(1, Math.Abs(faction1.Power - faction2.Power));
                    int sign = faction1.Power > faction2.Power ? 1 : (faction1.Power < faction2.Power ? -1 : 0);

                    float logRatio = Mathf.Log10(max / min);
                    float logDiff = Mathf.Log10(diff);
                    
                    float winrate = BaseWinRate + sign * (logRatio * WinRateLogRatioWeight + logDiff * WinRateLogDiffWeight);
                    winrate = Mathf.Clamp(winrate, WinRateMin, WinRateMax);

                    bool isSuccess = UnityEngine.Random.value < winrate;

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
                            stations, spaceTime, populationDebugData, travelMetadata,
                            factions, itemsPrices, difficulty, mission);
                    }
                    else
                    {
                        newsEvent.NewsType = rec.NewsTypeEndFail;
                        MissionSystem.ProcessMissionFailureActions(
                            stations, spaceTime, travelMetadata, factions, mission);

                        if (travelMetadata.CurrentSpaceObject.Equals(station.SpaceObjectId)
                            && !travelMetadata.IsInTravel)
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
}

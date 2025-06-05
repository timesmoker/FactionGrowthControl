using System.Collections.Generic;
using HarmonyLib;
using MGSC;

namespace FactionGrowthControl
{
    public static class MissionRecord_PowerOverride
    {
        static int CaptureMissionCost => Plugin.Config.CaptureMissionCost;
        static int CaptureMissionBuff => Plugin.Config.CaptureMissionBuff;

        static int LowRiskMissionCost => Plugin.Config.LowRiskMissionCost;
        static int LowRiskMissionBuff => Plugin.Config.LowRiskMissionBuff;

        static int HighRiskMissionCost => Plugin.Config.HighRiskMissionCost;
        static int HighRiskMissionBuff => Plugin.Config.HighRiskMissionBuff;

        private static readonly HashSet<ProceduralMissionType> HighRiskMissions = new HashSet<ProceduralMissionType>
        {
            ProceduralMissionType.Sabotage,
            ProceduralMissionType.Ritual,
            ProceduralMissionType.Robbery
        };

        private static readonly HashSet<ProceduralMissionType> LowRiskMissions = new HashSet<ProceduralMissionType>
        {
            ProceduralMissionType.Espionage,
            ProceduralMissionType.Elimination
        };

        [HarmonyPatch(typeof(ProcMissionRecord), "get_PowerCost")]
        [HarmonyPostfix]
        public static void MissionPowerCostOverrides(ProcMissionRecord __instance, ref int __result)
        {
            if (HighRiskMissions.Contains(__instance.ProcMissionType))
                __result = HighRiskMissionCost;
            else if (LowRiskMissions.Contains(__instance.ProcMissionType))
                __result = LowRiskMissionCost;
            else // capture Mission
                __result = CaptureMissionCost;
        }
        
        [HarmonyPatch(typeof(ProcMissionRecord), "get_PowerBuff")]
        [HarmonyPostfix]
        public static void MissionPowerBuffOverrides(ProcMissionRecord __instance, ref int __result)
        {
            if (HighRiskMissions.Contains(__instance.ProcMissionType))
                __result = HighRiskMissionBuff;
            else if (LowRiskMissions.Contains(__instance.ProcMissionType))
                __result = LowRiskMissionBuff;
            else // capture Mission
                __result = CaptureMissionBuff;
        }
        
    }
}
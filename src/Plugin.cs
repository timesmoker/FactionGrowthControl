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
    public static class Plugin
    {
        public static ConfigDirectories ConfigDirectories = new ConfigDirectories();

        public static ModConfig Config { get; private set; }

        public static Logger Logger = new Logger();

        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfig(IModContext context)
        {
            Directory.CreateDirectory(ConfigDirectories.ModPersistenceFolder);
    
            Config = ModConfig.LoadConfig(ConfigDirectories.ConfigPath);
            Logger.Log("FactionGrowthControl loaded.goooooood!");
            new Harmony("timesmoker" + ConfigDirectories.ModAssemblyName).PatchAll();
        }


        [Hook(ModHookType.SpaceStarted)]
        public static void OnSpaceStarted(IModContext context)
        {
            var missions = Data.ProcMissions;

            if (missions == null)
            {
                Logger.Log("[MyMod] Data.ProcMissions is null ❌");
                return;
            }

            Logger.Log($"[MyMod] Found {missions.Count} missions");

            foreach (var mission in missions)
            {
                Logger.Log($"--- Mission ---");
                Logger.Log($"Type: {mission.ProcMissionType}");
                Logger.Log($"PowerCost: {mission.PowerCost}, Buff: {mission.PowerBuff}, Debuff: {mission.PowerDebuff}");

                if (mission.WeightsByStrategy != null && mission.WeightsByStrategy.Count > 0)
                {
                    foreach (var (weight, strategy) in mission.WeightsByStrategy)
                    {
                        Logger.Log($"Weight: {weight}, Strategy: {strategy}");
                    }
                }
                else
                {
                    Logger.Log("WeightsByStrategy: (empty)");
                }
            }
        }
    }
}
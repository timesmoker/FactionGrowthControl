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
            Logger.Log("Mod Loaded");
            Directory.CreateDirectory(ConfigDirectories.ModPersistenceFolder);
            Config = ModConfig.LoadConfig(ConfigDirectories.ConfigPath);
            new Harmony("timesmoker" + ConfigDirectories.ModAssemblyName).PatchAll();
        }
    }
}
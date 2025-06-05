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
    public class ConfigDirectories
    {
        public string ModAssemblyName { get; private set; }

        /// <summary>
        /// The full path to the config file.  Stored in the mod's persistence folder.
        /// </summary>
        public string ConfigPath { get; private set; }

        /// <summary>
        /// This mod's persistence folder.
        /// </summary>
        public string ModPersistenceFolder { get; private set; }

        /// <summary>
        /// The Quasimorph_Mods folder that is parallel to the game's folder.
        /// This is a workaround for Quasimorph syncing and overwriting all files in the 
        /// Game's App Data folder.
        /// </summary>
        public string AllModsConfigFolder { get; set; }

        /// <summary>
        /// The name of the config file name.  Defaults to config.json
        /// </summary>
        public string ConfigFileName { get; set; }

        public ConfigDirectories(string configFileName = "config.json")
        {
            ModAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            AllModsConfigFolder = Path.Combine(Application.persistentDataPath, "../Quasimorph_ModConfigs/");
            ModPersistenceFolder = Path.Combine(AllModsConfigFolder, ModAssemblyName);
            ConfigPath = Path.Combine(ModPersistenceFolder, configFileName);
            ConfigFileName = configFileName;
        }

        /// <summary>
        /// Moves the config files from the legacy directory to the new directory.
        /// </summary>
        public void UpgradeModDirectory()
        {
            try
            {
                string oldDirectory = Path.Combine(Application.persistentDataPath,
                    ModAssemblyName);

                if (!Directory.Exists(oldDirectory)) return;

                Plugin.Logger.LogWarning($"Moving config folder from '{oldDirectory}' to '{ModPersistenceFolder}");
                Directory.Move(oldDirectory, ModPersistenceFolder);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Log($"Unable to move the config files.  Exception: {ex.ToString()}");
            }
        }

        /// <summary>
        /// Moves a standalone file to a directory config path.
        /// </summary>
        /// <param name="configFileName"></param>
        public void UpgradeFile(string configFileName = "config.json")
        {
            try
            {
                string oldConfigFile = Path.Combine(Application.persistentDataPath, configFileName);

                if (!File.Exists(oldConfigFile)) return;

                Plugin.Logger.LogWarning($"Moving config file from '{oldConfigFile}' to '{ModPersistenceFolder}");
                Directory.CreateDirectory(ModPersistenceFolder);
                File.Move(oldConfigFile, ConfigPath);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Log($"Unable to move the config files.  Exception: {ex.ToString()}");
            }
        }
    }
}
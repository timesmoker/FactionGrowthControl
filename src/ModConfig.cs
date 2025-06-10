using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace FactionGrowthControl
{
    public class ModConfig
    {
        [JsonProperty]
        public int ConfigVersion { get; private set; } = 2;
        [JsonProperty]
        public int CaptureMissionCost { get; private set;  } = 300;
        [JsonProperty]
        public int CaptureMissionBuff { get; private set;  } = 0;
 
        [JsonProperty]
        public int LowRiskMissionCost { get; private set;  } = 50;
        [JsonProperty]
        public int LowRiskMissionBuff { get; private set;  } = 200;

        [JsonProperty]
        public int HighRiskMissionCost { get; private set;  } = 100;
        [JsonProperty]
        public int HighRiskMissionBuff { get; private set;  } = 400;

        [JsonProperty]
        public int PowerBonusThreshold { get; private set;  }  = 8;
        [JsonProperty]
        public float PowerGainBonusMultiplier { get; private set;  }  = 1.3f;
        [JsonProperty]
        public float PowerLossBonusMultiplier { get; private set;  }  = 0.8f;
        
        [JsonProperty]
        public int PowerPenaltyThreshold { get; private set;  }  = 12;
        [JsonProperty]
        public float PowerPenaltyMultiplier { get; private set;  }  = 1.07f;
        
        [JsonProperty]
        public float MultiplierMin { get; private set;  }  = 0.2f;
        [JsonProperty]
        public float MultiplierMax { get; private set;  }  = 2.0f;
        
        [JsonProperty]
        public float BaseWinRate { get; private set;  }  = 0.2f;
        [JsonProperty]
        public float WinRateLogRatioWeight { get; private set;  }  = 0.2f;
        [JsonProperty]
        public float WinRateLogDiffWeight { get; private set;  }  = 0.0325f;
        [JsonProperty]
        public float WinRateMin { get; private set;  }  = 0.25f;
        [JsonProperty]
        public float WinRateMax { get; private set;  }  = 0.75f;
        
        [JsonProperty]
        public float TotalMissionCapRate { get; private set;  } = 0.4f;
        
        
        public static ModConfig LoadConfig(string configPath)
        {
            
            ModConfig defaultConfig = new ModConfig();

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            if (File.Exists(configPath))
            {
                try
                {
                    string sourceJson = File.ReadAllText(configPath);
                    var loadedRaw = JsonConvert.DeserializeObject<Dictionary<string, object>>(sourceJson);

                    int existingVersion = -1;

                    bool hasVersion =
                        loadedRaw.TryGetValue(nameof(ConfigVersion), out var versionObj) &&
                        int.TryParse(versionObj?.ToString(), out existingVersion);

                    if (!hasVersion || existingVersion < defaultConfig.ConfigVersion)
                    {
                        Plugin.Logger.Log($"[Config] Outdated or missing version (found: v{existingVersion}). Overwriting with v{defaultConfig.ConfigVersion}");
                        string updatedJson = JsonConvert.SerializeObject(defaultConfig, serializerSettings);
                        File.WriteAllText(configPath, updatedJson);
                        return defaultConfig;
                    }

                    ModConfig config = JsonConvert.DeserializeObject<ModConfig>(sourceJson, serializerSettings);
                    Plugin.Logger.Log("Config loaded.");
                    return config;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError("Error parsing configuration.  Ignoring config file and using defaults");
                    Plugin.Logger.LogException(ex);

                    Plugin.Logger.LogError("[Config] Failed to load config. Using default.");
                    Plugin.Logger.LogException(ex);
                    return defaultConfig;
                }
            }
            else
            {
                string json = JsonConvert.SerializeObject(defaultConfig, serializerSettings);
                File.WriteAllText(configPath, json);
                Plugin.Logger.Log("[Config] No config found. Created new one.");
                return defaultConfig;
            }
        }
    }
}
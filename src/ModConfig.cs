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
        public int CaptureMissionCost { get; private set;  } = 300;
        [JsonProperty]
        public int CaptureMissionBuff { get; private set;  } = 0;
 
        [JsonProperty]
        public int LowRiskMissionCost { get; private set;  } = 50;
        [JsonProperty]
        public int LowRiskMissionBuff { get; private set;  } = 100;

        [JsonProperty]
        public int HighRiskMissionCost { get; private set;  } = 100;
        [JsonProperty]
        public int HighRiskMissionBuff { get; private set;  } = 200;

        [JsonProperty]
        public int PowerBonusThreshold { get; private set;  }  = 8;
        [JsonProperty]
        public float PowerGainBonusMultiplier { get; private set;  }  = 1.3f;
        [JsonProperty]
        public float PowerLossBonusMultiplier { get; private set;  }  = 0.8f;
        
        [JsonProperty]
        public int PowerPenaltyThreshold { get; private set;  }  = 13;
        [JsonProperty]
        public float PowerPenaltyMultiplier { get; private set;  }  = 1.09f;
        
        [JsonProperty]
        public float MultiplierMin { get; private set;  }  = 0.2f;
        [JsonProperty]
        public float MultiplierMax { get; private set;  }  = 1.7f;
        
        [JsonProperty]
        public float BaseWinRate { get; private set;  }  = 0.2f;
        [JsonProperty]
        public float WinRateLogRatioWeight { get; private set;  }  = 0.2f;
        [JsonProperty]
        public float WinRateLogDiffWeight { get; private set;  }  = 0.0325f;
        [JsonProperty]
        public float WinRateMin { get; private set;  }  = 0.2f;
        [JsonProperty]
        public float WinRateMax { get; private set;  }  = 0.8f;
        
        [JsonProperty]
        public float TotalMissionCapRate { get; private set;  } = 0.5f;
        
        
        public static ModConfig LoadConfig(string configPath)
        {
            ModConfig config;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            if (File.Exists(configPath))
            {
                try
                {
                    string sourceJson = File.ReadAllText(configPath);

                    config = JsonConvert.DeserializeObject<ModConfig>(sourceJson, serializerSettings);

                    //Add any new elements that have been added since the last mod version the user had.
                    string upgradeConfig = JsonConvert.SerializeObject(config, serializerSettings);

                    if (upgradeConfig != sourceJson)
                    {
                        Plugin.Logger.Log("Updating config with missing elements");
                        //re-write
                        File.WriteAllText(configPath, upgradeConfig);
                    }

                    Plugin.Logger.Log("Config loaded.");
                    return config;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError("Error parsing configuration.  Ignoring config file and using defaults");
                    Plugin.Logger.LogException(ex);

                    //Not overwriting in case the user just made a typo.
                    config = new ModConfig();
                    return config;
                }
            }
            else
            {
                config = new ModConfig();

                string json = JsonConvert.SerializeObject(config, serializerSettings);
                File.WriteAllText(configPath, json);

                return config;
            }
        }
    }
}
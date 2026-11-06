# FactionGrowthControl Configuration

[한국어](CONFIG.ko.md)

The config file is created on the first game run:

`%AppData%\..\LocalLow\Magnum Scriptum Ltd\Quasimorph_ModConfigs\FactionGrowthControl\config.json`

- The file is read once at game start. Restart the game after editing.
- If the JSON is invalid, the mod logs an error to `Player.log` and uses the defaults. The file is left untouched.
- Do not edit `ConfigVersion`. When a mod update raises the version, the whole file is replaced with the new defaults, so back up your custom values before updating.

## Default config

```json
{
  "ConfigVersion": 2,
  "CaptureMissionCost": 300,
  "CaptureMissionBuff": 0,
  "LowRiskMissionCost": 50,
  "LowRiskMissionBuff": 200,
  "HighRiskMissionCost": 100,
  "HighRiskMissionBuff": 400,
  "PowerBonusThreshold": 8,
  "PowerGainBonusMultiplier": 1.3,
  "PowerLossBonusMultiplier": 0.8,
  "PowerPenaltyThreshold": 12,
  "PowerPenaltyMultiplier": 1.07,
  "MultiplierMin": 0.2,
  "MultiplierMax": 2.0,
  "BaseWinRate": 0.2,
  "WinRateLogRatioWeight": 0.2,
  "WinRateLogDiffWeight": 0.0325,
  "WinRateMin": 0.25,
  "WinRateMax": 0.75,
  "TotalMissionCapRate": 0.4
}
```

## 1. Mission power cost and reward

Every procedural mission belongs to one of three groups:

| Group | Mission types |
|---|---|
| Capture | RaiderCapture, and every type not listed below |
| Low risk | Espionage, Elimination |
| High risk | Sabotage, Ritual, Robbery |

| Key | Default | Meaning |
|---|---|---|
| `CaptureMissionCost` | 300 | Power the attacking faction pays to start a capture mission |
| `CaptureMissionBuff` | 0 | Power the attacking faction gains when a capture mission succeeds |
| `LowRiskMissionCost` | 50 | Cost of a low risk mission |
| `LowRiskMissionBuff` | 200 | Reward of a low risk mission |
| `HighRiskMissionCost` | 100 | Cost of a high risk mission |
| `HighRiskMissionBuff` | 400 | Reward of a high risk mission |

- **Cost** is taken from the attacker's stations, starting with the station that holds the most power. If the attacker's stations do not hold enough power in total, the faction cannot start that mission.
- **Reward** goes to the attacker's station with the least power. It is multiplied by the difficulty's *Faction Growth Speed* and by the station count multiplier (section 2).
- A capture mission already gives the attacker a whole station, which is why its reward is 0 by default.
- Only the attacking faction's gain is changed here. Other success effects (station power, population, tech level, victim's power loss) keep the game's values.

For reference, the vanilla values in game 1.0.4:

| Mission | Vanilla cost | Vanilla reward |
|---|---|---|
| RaiderCapture | 180 | 180 |
| Elimination | 90 | 90 |
| Espionage | 0 | 200 |
| Sabotage | 90 | 90 |
| Ritual | 90 | 300 |
| Robbery | 60 | 150 |

## 2. Station count multiplier

Factions with few stations spawn few missions, so the player rarely gets to play them. This multiplier helps small factions grow and slows down factions that own too many stations.

`n` is the number of stations the faction owns, including uncapturable ones.

| Key | Default | Meaning |
|---|---|---|
| `PowerBonusThreshold` | 8 | Below this many stations, the faction is treated as small |
| `PowerGainBonusMultiplier` | 1.3 | Gain multiplier per missing station for small factions |
| `PowerLossBonusMultiplier` | 0.8 | Loss multiplier per missing station for small factions |
| `PowerPenaltyThreshold` | 12 | Above this many stations, the faction is treated as large |
| `PowerPenaltyMultiplier` | 1.07 | Loss multiplier per extra station for large factions |
| `MultiplierMin` | 0.2 | Lower limit of any multiplier |
| `MultiplierMax` | 2.0 | Upper limit of any multiplier |

Formulas:

- Gain multiplier: `PowerGainBonusMultiplier ^ (PowerBonusThreshold - n)` when `n < PowerBonusThreshold`, otherwise 1.
- Loss multiplier:
  - `PowerLossBonusMultiplier ^ (PowerBonusThreshold - n)` when `n < PowerBonusThreshold`
  - 1 when `PowerBonusThreshold <= n <= PowerPenaltyThreshold`
  - `PowerPenaltyMultiplier ^ (n - PowerPenaltyThreshold)` when `n > PowerPenaltyThreshold`
- Both are clamped between `MultiplierMin` and `MultiplierMax`.

Where they apply:

- **Gain multiplier**: the mission reward from section 1.
- **Loss multiplier**: the mission cost from section 1, and the faction-wide power loss when an enemy mission against this faction succeeds.
- Not affected: losing a captured station, station-level power loss, strategy passive income, and power gained from player barter.

With the default values:

| Stations | Gain | Loss |
|---|---|---|
| 1-5 | 2.0 | 0.2-0.51 |
| 6 | 1.69 | 0.64 |
| 7 | 1.3 | 0.8 |
| 8-12 | 1.0 | 1.0 |
| 13 | 1.0 | 1.07 |
| 15 | 1.0 | 1.23 |
| 18 | 1.0 | 1.50 |
| 20 | 1.0 | 1.72 |
| 23+ | 1.0 | 2.0 |

## 3. Auto-resolve win rate

When a mission ends without the player playing it, the game rolls whether the attacker succeeded. This happens when the mission expires, and, depending on difficulty settings, when the player evacuates from it or dies in it. The mod replaces the vanilla roll with this formula:

```
ratio   = stronger power / weaker power
diff    = |attacker power - victim power|
sign    = +1 if attacker is stronger, -1 if weaker, 0 if equal
winrate = BaseWinRate + sign * (log10(ratio) * WinRateLogRatioWeight
                              + log10(diff)  * WinRateLogDiffWeight)
winrate = clamp(winrate, WinRateMin, WinRateMax)
```

| Key | Default | Meaning |
|---|---|---|
| `BaseWinRate` | 0.2 | Starting win rate before the power comparison |
| `WinRateLogRatioWeight` | 0.2 | How much the power ratio matters |
| `WinRateLogDiffWeight` | 0.0325 | How much the absolute power gap matters |
| `WinRateMin` | 0.25 | Lowest possible win rate |
| `WinRateMax` | 0.75 | Highest possible win rate |

Examples with the default values:

| Attacker vs victim | Win rate |
|---|---|
| Weaker or equal | 25% |
| 3000 vs 2500 | about 30% |
| 6000 vs 3000 | about 37% |
| 10000 vs 2000 | about 47% |
| 100x stronger | 75% |

- The default `BaseWinRate` is below `WinRateMin`, so a weaker or equally strong attacker always gets `WinRateMin`.
- Missions involving the player's faction (CEO Elimination, Bramfatura Invasion) keep the vanilla roll.
- Missions the player actually completes are not rolled at all.

## 4. Total mission cap

| Key | Default | Meaning |
|---|---|---|
| `TotalMissionCapRate` | 0.4 | Maximum active missions, as a fraction of all stations in the game data |

- Cap = `number of stations in the game data x TotalMissionCapRate`, rounded down. With game 1.0.4 that is 176 x 0.4 = 70 missions.
- Factions stop starting new missions while the number of active missions is at or above the cap. Story missions and player-faction missions count toward it.
- Factions get to start missions in a random order every hour, so no faction always goes first.

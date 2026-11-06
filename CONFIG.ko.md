# FactionGrowthControl 설정 가이드

[English](CONFIG.md)

설정 파일은 게임을 처음 실행할 때 아래 위치에 만들어집니다.

`%AppData%\..\LocalLow\Magnum Scriptum Ltd\Quasimorph_ModConfigs\FactionGrowthControl\config.json`

- 설정은 게임을 시작할 때 한 번만 읽습니다. 수정했다면 게임을 다시 시작해야 적용됩니다.
- JSON 형식이 깨져 있으면 `Player.log`에 오류를 남기고 기본값으로 실행합니다. 파일은 건드리지 않습니다.
- `ConfigVersion`은 수정하지 마세요. 모드 업데이트로 버전이 올라가면 파일 전체가 새 기본값으로 덮어써지니, 직접 바꾼 값이 있다면 업데이트 전에 백업해 두세요.

## 기본 설정

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

## 1. 미션 파워 비용과 보상

세력이 만드는 미션은 세 그룹으로 나뉩니다.

| 그룹 | 미션 종류 |
|---|---|
| 점령 | RaiderCapture, 그리고 아래에 없는 모든 종류 |
| 저위험 | Espionage, Elimination |
| 고위험 | Sabotage, Ritual, Robbery |

| 키 | 기본값 | 의미 |
|---|---|---|
| `CaptureMissionCost` | 300 | 공격 세력이 점령 미션을 시작할 때 내는 파워 |
| `CaptureMissionBuff` | 0 | 점령 미션 성공 시 공격 세력이 얻는 파워 |
| `LowRiskMissionCost` | 50 | 저위험 미션 비용 |
| `LowRiskMissionBuff` | 200 | 저위험 미션 보상 |
| `HighRiskMissionCost` | 100 | 고위험 미션 비용 |
| `HighRiskMissionBuff` | 400 | 고위험 미션 보상 |

- **비용**은 공격 세력의 거점에서 빠져나가며, 파워가 가장 많이 쌓인 거점부터 빠집니다. 거점들에 쌓인 파워를 다 합쳐도 비용보다 적으면 그 미션은 시작할 수 없습니다.
- **보상**은 공격 세력의 거점 중 파워가 가장 적은 곳으로 들어갑니다. 난이도 설정의 *세력 성장 속도*와 거점 수 배율(2장)이 곱해집니다.
- 점령 미션은 성공하면 거점 하나를 통째로 얻기 때문에 기본 보상이 0입니다.
- 이 설정이 바꾸는 건 공격 세력이 얻는 파워뿐입니다. 거점 파워 감소, 인구 감소, 기술 레벨 변화, 피해 세력의 파워 감소 같은 나머지 효과는 게임 원래 값을 따릅니다.

참고용 바닐라 값 (게임 1.0.4 기준):

| 미션 | 바닐라 비용 | 바닐라 보상 |
|---|---|---|
| RaiderCapture | 180 | 180 |
| Elimination | 90 | 90 |
| Espionage | 0 | 200 |
| Sabotage | 90 | 90 |
| Ritual | 90 | 300 |
| Robbery | 60 | 150 |

## 2. 거점 수 배율

거점이 적은 세력은 미션을 거의 만들지 못해서 플레이어가 그 세력의 미션을 즐기기 어렵습니다. 이 배율은 작은 세력이 성장하도록 돕고, 거점을 너무 많이 가진 세력의 성장은 늦춥니다.

아래에서 `n`은 세력이 가진 거점 수이며, 점령 불가 거점도 포함합니다.

| 키 | 기본값 | 의미 |
|---|---|---|
| `PowerBonusThreshold` | 8 | 거점이 이보다 적으면 약소 세력으로 취급 |
| `PowerGainBonusMultiplier` | 1.3 | 약소 세력이 거점 1개 부족할 때마다 얻는 양에 곱해지는 값 |
| `PowerLossBonusMultiplier` | 0.8 | 약소 세력이 거점 1개 부족할 때마다 잃는 양에 곱해지는 값 |
| `PowerPenaltyThreshold` | 12 | 거점이 이보다 많으면 대형 세력으로 취급 |
| `PowerPenaltyMultiplier` | 1.07 | 대형 세력이 거점 1개 초과할 때마다 잃는 양에 곱해지는 값 |
| `MultiplierMin` | 0.2 | 배율의 하한 |
| `MultiplierMax` | 2.0 | 배율의 상한 |

계산식:

- 얻는 배율: `n < PowerBonusThreshold`이면 `PowerGainBonusMultiplier ^ (PowerBonusThreshold - n)`, 아니면 1
- 잃는 배율:
  - `n < PowerBonusThreshold`이면 `PowerLossBonusMultiplier ^ (PowerBonusThreshold - n)`
  - `PowerBonusThreshold <= n <= PowerPenaltyThreshold`이면 1
  - `n > PowerPenaltyThreshold`이면 `PowerPenaltyMultiplier ^ (n - PowerPenaltyThreshold)`
- 두 배율 모두 `MultiplierMin`과 `MultiplierMax` 사이로 제한됩니다.

적용되는 곳:

- **얻는 배율**: 1장의 미션 보상
- **잃는 배율**: 1장의 미션 비용, 그리고 적의 미션이 성공해서 이 세력 전체 파워가 깎일 때
- 적용되지 않는 곳: 거점을 점령당해 잃는 파워, 거점 단위 파워 감소, 전략별 패시브 파워 증가, 플레이어와의 물물교환으로 오르는 파워

기본값 기준 배율:

| 거점 수 | 얻는 배율 | 잃는 배율 |
|---|---|---|
| 1~5 | 2.0 | 0.2~0.51 |
| 6 | 1.69 | 0.64 |
| 7 | 1.3 | 0.8 |
| 8~12 | 1.0 | 1.0 |
| 13 | 1.0 | 1.07 |
| 15 | 1.0 | 1.23 |
| 18 | 1.0 | 1.50 |
| 20 | 1.0 | 1.72 |
| 23 이상 | 1.0 | 2.0 |

## 3. 자동 판정 승률

플레이어가 직접 하지 않은 미션이 끝나면, 게임이 공격 세력의 성공 여부를 확률로 정합니다. 미션이 시간 만료됐을 때 이 판정이 일어나고, 난이도 설정에 따라 플레이어가 미션 도중 철수하거나 사망했을 때도 일어납니다. 모드는 바닐라 판정을 아래 공식으로 대체합니다.

```
ratio   = 강한 쪽 파워 / 약한 쪽 파워
diff    = |공격 세력 파워 - 피해 세력 파워|
sign    = 공격 세력이 강하면 +1, 약하면 -1, 같으면 0
winrate = BaseWinRate + sign * (log10(ratio) * WinRateLogRatioWeight
                              + log10(diff)  * WinRateLogDiffWeight)
winrate = clamp(winrate, WinRateMin, WinRateMax)
```

| 키 | 기본값 | 의미 |
|---|---|---|
| `BaseWinRate` | 0.2 | 파워 비교 전 기본 승률 |
| `WinRateLogRatioWeight` | 0.2 | 파워 비율이 승률에 주는 영향 |
| `WinRateLogDiffWeight` | 0.0325 | 파워 차이(절대값)가 승률에 주는 영향 |
| `WinRateMin` | 0.25 | 최저 승률 |
| `WinRateMax` | 0.75 | 최고 승률 |

기본값 기준 예시:

| 공격 세력 vs 피해 세력 | 승률 |
|---|---|
| 공격 세력이 약하거나 같음 | 25% |
| 3000 vs 2500 | 약 30% |
| 6000 vs 3000 | 약 37% |
| 10000 vs 2000 | 약 47% |
| 100배 강함 | 75% |

- 기본 `BaseWinRate`가 `WinRateMin`보다 낮아서, 공격 세력이 약하거나 파워가 같으면 항상 `WinRateMin`이 됩니다.
- 플레이어 세력이 관련된 미션(CEO 암살, 브람파투라 침공)은 바닐라 판정을 그대로 씁니다.
- 플레이어가 직접 클리어한 미션에는 이 판정이 적용되지 않습니다.

## 4. 미션 총량 상한

| 키 | 기본값 | 의미 |
|---|---|---|
| `TotalMissionCapRate` | 0.4 | 동시에 존재할 수 있는 미션 수를 게임 데이터의 전체 거점 수에 대한 비율로 지정 |

- 상한 = `게임 데이터의 전체 거점 수 x TotalMissionCapRate` (소수점 버림). 게임 1.0.4 기준 176 x 0.4 = 70개입니다.
- 진행 중인 미션 수가 상한 이상이면 세력들은 새 미션을 시작하지 않습니다. 스토리 미션과 플레이어 세력 미션도 이 수에 포함됩니다.
- 세력들이 미션을 시작하는 순서는 매시간 무작위로 섞이므로, 특정 세력이 항상 먼저 기회를 갖지는 않습니다.

# Traumar.NET 使用手冊

本手冊提供 Traumar .NET 類別庫（適用於 .NET Framework 4.7.2+）中所有公開方法（Public Methods）的詳細說明、參數定義與程式碼範例。

## 目錄
- [核心計算模組 (Core)](#核心計算模組-core)
  - [SurvivalCalculator.ProbabilityOfSurvival](#survivalcalculatorprobabilityofsurvival)
  - [PerformanceCalculator.CalculateTraumaPerformance](#performancecalculatorcalculatetraumaperformance)
  - [RmmCalculator.CalculateRmm](#rmmcalculatorcalculatermm)
  - [StatHelper.CalculateIntervals](#stathelpercalculateintervals)
- [SEQIC 品質指標模組 (Seqic)](#seqic-品質指標模組-seqic)
  - [Indicators1to4.CalculateIndicator1-4](#indicators1to4calculateindicator1-4)
  - [Indicators5to8.CalculateIndicator5-8](#indicators5to8calculateindicator5-8)
  - [Indicators9to13.CalculateIndicator9-13](#indicators9to13calculateindicator9-13)
- [附錄：列舉與模型說明](#附錄列舉與模型說明)

---

## 核心計算模組 (Core)

### SurvivalCalculator.ProbabilityOfSurvival
計算單筆病患的存活機率 (Ps)，基於 TRISS (Trauma and Injury Severity Score) 方法論。

*   **回傳結果**: `PatientRecord` (包含計算出的 `Ps` 屬性)。
*   **參數說明**:
    *   `input` (`PatientInput`): 輸入病患資料。
    *   **必填欄位**: `PatientInput` 物件的 `Age`, `InjuryType`, `Iss`, `Rts` 欄位為必填。
*   **邊界值與限制**:
    *   `Age`: 必須 $\ge 0$。若 $\ge 55$ 歲會觸發 TRISS 的年齡加權。
    *   `Rts`: 必須在 $0$ 到 $7.8408$ 之間。
    *   `Iss`: 必須在 $0$ 到 $75$ 之間。
    *   `InjuryType`: 僅支援 `Blunt` (鈍傷) 或 `Penetrating` (穿刺傷)。

#### 程式碼範例

**範例 1：基本成人鈍傷案件 (最常見情況)**
```csharp
// 初始化完整的參數物件
var input = new PatientInput 
{ 
    Age = 40,               // 年齡：40 歲
    InjuryType = InjuryType.Blunt, // 傷害類型：鈍傷
    Iss = 16,               // ISS (0-75)：16 分 (中度傷勢)
    Rts = 7.84              // RTS (0-7.8408)：7.84 分 (生理徵象正常)
};

var result = SurvivalCalculator.ProbabilityOfSurvival(input);

// 回傳結果說明：
// result.Ps 為 0.9634... (代表 96.3% 存活率)
Console.WriteLine($"病患存活機率: {result.Ps:P2}");
```

**範例 2：高齡穿刺傷案件 (觸發 55 歲以上與穿刺傷係數)**
```csharp
var input = new PatientInput 
{ 
    Age = 70,               // 年齡：70 歲 (觸發 Age >= 55 係數)
    InjuryType = InjuryType.Penetrating, // 傷害類型：穿刺傷
    Iss = 25,               // ISS：25 分 (重傷)
    Rts = 5.967             // RTS：5.967 分 (呼吸或血壓不穩定)
};

var result = SurvivalCalculator.ProbabilityOfSurvival(input);
// result.Ps 會顯著降低，約為 0.584... (58.4%)
Console.WriteLine($"高齡穿刺傷存活機率: {result.Ps:P2}");
```

**範例 3：極端重傷案例 (ISS 75)**
```csharp
var input = new PatientInput 
{ 
    Age = 25, 
    InjuryType = InjuryType.Blunt, 
    Iss = 75,               // ISS 最大值
    Rts = 1.0               // 極低生理分數
};
var result = SurvivalCalculator.ProbabilityOfSurvival(input);
Console.WriteLine($"極端重傷存活機率: {result.Ps:P4}");
```

---

### PerformanceCalculator.CalculateTraumaPerformance
計算創傷中心的整體績效評分，包含 W-Score, M-Score 與 Z-Score。

*   **回傳結果**: `TraumaPerformanceResult`。
*   **參數說明**:
    *   `records` (`IEnumerable<PatientRecord>`): 包含病患紀錄的集合。
    *   **必填欄位**: `PatientRecord` 物件的 `Ps` 與 `Outcome` 欄位為必填。
    *   `minSampleSize` (`int`): 最小計算樣本數，預設為 $10$。
*   **邊界值與限制**:
    *   樣本數小於 `minSampleSize` 時會拋出 `ArgumentException`。
    *   若所有病患 Ps 皆為 $0$ 或 $1$，計算 Z-Score 可能導致分母為零。

#### 程式碼範例

**範例 1：計算中型資料集的績效 (W-Score 與 Z-Score)**
```csharp
// 準備病患資料集清單 (包含預測存活率 Ps 與實際存活結果 Outcome)
var dataset = new List<PatientRecord> 
{
    new PatientRecord { Ps = 0.95, Outcome = 1 }, // 預測 95% 存活，實際存活
    new PatientRecord { Ps = 0.40, Outcome = 0 }, // 預測 40% 存活，實際死亡
    new PatientRecord { Ps = 0.85, Outcome = 1 },
    new PatientRecord { Ps = 0.98, Outcome = 1 },
    new PatientRecord { Ps = 0.10, Outcome = 0 },
    new PatientRecord { Ps = 0.92, Outcome = 1 },
    new PatientRecord { Ps = 0.77, Outcome = 1 },
    new PatientRecord { Ps = 0.45, Outcome = 1 }, // 超額存活案例
    new PatientRecord { Ps = 0.91, Outcome = 1 },
    new PatientRecord { Ps = 0.88, Outcome = 1 }
};

var result = PerformanceCalculator.CalculateTraumaPerformance(dataset, minSampleSize: 10);

// 回傳結果說明：
// result.WScore: 代表每百人超額存活人數 (本例為正值)
// result.ZScore: 績效顯著性 (高於 1.96 代表統計上顯著優於基準)
Console.WriteLine($"W-Score: {result.WScore:F2}, Z-Score: {result.ZScore:F2}");
```

**範例 2：處理資料量不足的情況**
```csharp
var smallList = new List<PatientRecord> { new PatientRecord { Ps = 0.5, Outcome = 1 } };
try 
{
    // minSampleSize 預設為 10
    var result = PerformanceCalculator.CalculateTraumaPerformance(smallList);
} 
catch (ArgumentException ex) 
{
    Console.WriteLine($"錯誤: {ex.Message}"); // 輸出 "資料量不足：需要至少 10 筆..."
}
```

**範例 3：提取各分區 M-Score (MTOS 相似度)**
```csharp
var result = PerformanceCalculator.CalculateTraumaPerformance(dataset);
// M-Score 是長度為 6 的陣列，代表不同 Ps 區間的病患佔比
// Index 0: 0.96 - 1.00
// Index 5: 0.00 - 0.25
Console.WriteLine($"高品質資料佔比 (Ps > 0.96): {result.MScore[0]:P2}");
```

---

### RmmCalculator.CalculateRmm
計算相對死亡率指標 (Relative Mortality Metric, RMM)，採用非線性分箱演算法與拔靴法 (Bootstrap)。

*   **回傳結果**: `RmmResult` (包含 Population RMM 與 Bootstrap RMM 及其信賴區間)。
*   **參數說明**:
    *   `records` (`IEnumerable<PatientRecord>`): 包含病患紀錄的集合。
    *   **必填欄位**: `PatientRecord` 物件的 `Ps` 與 `Outcome` 欄位為必填。
    *   `minSampleSize` (`int`): 同上。
    *   `nSamples` (`int`): 拔靴採樣次數，預設為 $100$。
    *   `seed` (`int?`): 隨機種子，用於確保測試結果可重現。

#### 程式碼範例

**範例 1：執行標準 RMM 計算 (Population RMM)**
```csharp
var dataset = GetPatientData(); // 取得病患集合
var rmm = RmmCalculator.CalculateRmm(dataset);

// 回傳結果說明：
// rmm.PopulationRMM: 由全體資料直接計算的 RMM (-1.0 ~ 1.0)
// rmm.PopulationRMM_LL: 95% 信賴區間下限
Console.WriteLine($"母體 RMM: {rmm.PopulationRMM:F4} (範圍: {rmm.PopulationRMM_LL:F4} ~ {rmm.PopulationRMM_UL:F4})");
```

**範例 2：使用隨機種子執行 Bootstrap 模擬**
```csharp
// nSamples: 250 次拔靴採樣
// seed: 999 確保結果可重現 (Parity 測試必備)
var rmm = RmmCalculator.CalculateRmm(dataset, nSamples: 250, seed: 999);

// 回傳結果說明：
// rmm.BootstrapRMM: 250 次採樣結果的平均值
// rmm.BootstrapCI: 95% 信賴區間的半分寬
Console.WriteLine($"Bootstrap RMM: {rmm.BootstrapRMM:F4} ± {rmm.BootstrapCI:F4}");
```

**範例 3：根據 RMM 判斷院內表現**
```csharp
var rmm = RmmCalculator.CalculateRmm(dataset);
if (rmm.PopulationRMM > 0.05) 
    Console.WriteLine("臨床表現顯著優於預期基準。");
else if (rmm.PopulationRMM < -0.05) 
    Console.WriteLine("臨床表現低於預期基準，建議進行 PI 審查。");
```

---

### StatHelper.CalculateIntervals
提供適用於比例 (Proportion) 的信賴區間計算，支援 Wilson Score 與 Clopper-Pearson 方法。

*   **回傳結果**: `(double Lower, double Upper)` 元組。
*   **參數說明**:
    *   `x` (`int`): 成功次數 (分子)。
    *   `n` (`int`): 總次數 (分母)。
    *   `confLevel` (`double`): 信賴水準，預設為 $0.95$。
*   **方法清單**:
    1.  `CalculateWilsonInterval`: 適用於中大型樣本，與 R 語言 `prop.test` 一致。
    2.  `CalculateClopperPearsonInterval`: 確切二項式區間，適用於小型樣本，較保守。

#### 程式碼範例

**範例 1：計算 Wilson 區間 (10/100)**
```csharp
var (lower, upper) = StatHelper.CalculateWilsonInterval(10, 100);
Console.WriteLine($"95% CI: [{lower:F4}, {upper:F4}]");
```

**範例 2：計算 Clopper-Pearson 區間**
```csharp
var (lower, upper) = StatHelper.CalculateClopperPearsonInterval(5, 50);
Console.WriteLine($"CP CI: [{lower:F4}, {upper:F4}]");
```

**範例 3：自定義信賴水準 (99% CI)**
```csharp
var (lower, upper) = StatHelper.CalculateWilsonInterval(20, 100, confLevel: 0.99);
```

---

## SEQIC 品質指標模組 (Seqic)

### Indicators1to4.CalculateIndicator1-4
計算 SEQIC 第 1 至第 4 項指標，主要關於反應時間、時間戳完整度與解剖率。

#### 方法 1：CalculateIndicator1 (創傷小組反應時間)
*   **用途**: 計算各級創傷小組在不同時限內的抵達率 (1a-1f)。
*   **參數說明**: 需要傳入 `Indicator1Input` 物件集合，其 `UniqueIncidentId`, `ActivationLevel`, `ServiceType`, `Level`, `ResponseTime`, `ActivationProvider` 欄位為必填。
*   **範例**:
    ```csharp
    var inputs = new List<Indicator1Input>
    {
        new Indicator1Input 
        { 
            UniqueIncidentId = "INC-001",
            ActivationLevel = TraumaTeamActivationLevel.Level1, // 一級啟動
            ServiceType = PhysicianServiceType.SurgeryTrauma,   // 外科醫師
            Level = TraumaLevel.I,                             // 一級創傷中心
            ResponseTime = 12.5,                               // 抵達時間：12.5 分 (符合 1a 規範)
            ActivationProvider = "Dr. Smith"
        },
        new Indicator1Input 
        { 
            UniqueIncidentId = "INC-002",
            ActivationLevel = TraumaTeamActivationLevel.Level1,
            ServiceType = PhysicianServiceType.SurgeryTrauma,
            Level = TraumaLevel.I,
            ResponseTime = 25.0,                               // 超過 15 分但低於 30 分 (不符 1a 但符 1b)
            ActivationProvider = "Dr. Doe"
        }
    };

    var result = Indicators1to4.CalculateIndicator1(inputs, ciMethod: CiMethod.Wilson);

    // 回傳結果說明：
    // result.Indicator1A.Rate: 15 分鐘內抵達比例
    // result.Indicator1B.Rate: 30 分鐘內抵達比例
    Console.WriteLine($"指標 1a 通過率: {result.Indicator1A.Rate:P2}");
    ```

#### 方法 2：CalculateIndicator2 (事件時間缺漏率)
*   **用途**: 統計創傷事件發生時間缺漏的比例。
*   **參數說明**: 需要傳入 `Indicator2Input` 物件集合，其 `UniqueIncidentId`, `Level`, `IncidentTime` 欄位為必填。
*   **範例**:
    ```csharp
    var result = Indicators1to4.CalculateIndicator2(input);
    Console.WriteLine($"缺漏筆數: {result.Indicator2.Numerator}");
    ```

#### 方法 3：CalculateIndicator3 (存活機率紀錄率)
*   **用途**: 檢查病患紀錄中是否包含 Ps 數值。
*   **參數說明**: 需要傳入 `Indicator3Input` 物件集合，其 `UniqueIncidentId`, `Level`, `TraumaType`, `ProbabilityOfSurvival` 欄位為必填。
*   **範例**:
    ```csharp
    var input = dataset.Select(d => new Indicator3Input { UniqueIncidentId = d.Id, ProbabilityOfSurvival = d.Ps });
    var result = Indicators1to4.CalculateIndicator3(input);
    ```

#### 方法 4：CalculateIndicator4 (死亡解剖與長期滯院)
*   **用途**: 計算死亡病患的解剖率 (4a) 與非預期長期滯留 (4b)。
*   **參數說明**: 需要傳入 `Indicator4Input` 物件集合，其 `UniqueIncidentId`, `Level`, `EdDisposition`, `EdLos`, `HospitalDisposition`, `HospitalLos`, `Autopsy` 欄位為必填。
*   **範例**:
    ```csharp
    var result = Indicators1to4.CalculateIndicator4(input);
    Console.WriteLine($"解剖率: {result.Indicator4A.Rate:P2}");
    ```

---

### Indicators5to8.CalculateIndicator5-8
計算 SEQIC 第 5 至第 8 項指標，包含毒物篩檢與低 GCS 指標。

#### 方法 1：CalculateIndicator5 (酒精與藥物篩檢)
*   **用途**: 統計病患執行酒精濃度 (BAC) 與藥物篩檢 (Drug Screen) 的比例及陽性率。
*   **參數說明**: 需要傳入 `Indicator5Input` 物件集合，其 `UniqueIncidentId`, `Level`, `BloodAlcoholContent`, `DrugScreen` 欄位為必填。
*   **範例**:
    ```csharp
    var result = Indicators5to8.CalculateIndicator5(input);
    Console.WriteLine($"藥物篩檢執行率: {result.Indicator5C.Rate:P2}");
    ```

#### 方法 2：CalculateIndicator6 (低 GCS 延遲抵達)
*   **用途**: 計算 GCS < 8 且受傷後超過 3 小時才抵達非適當設施的比例。
*   **參數說明**: 需要傳入 `Indicator6Input` 物件集合，其 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `ReceivingIndicator`, `LowGcsIndicator`, `TimeFromInjuryToArrival` 欄位為必填。
*   **範例**:
    ```csharp
    var result = Indicators5to8.CalculateIndicator6(input);
    ```

#### 方法 3：CalculateIndicator7 (延遲獲得決定性照護)
*   **用途**: 統計所有受傷後超過 3 小時抵達的個案比例。
*   **參數說明**: 需要傳入 `Indicator7Input` 物件集合，其 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `TimeFromInjuryToArrival` 欄位為必填。

#### 方法 4：CalculateIndicator8 (分層死亡率)
*   **用途**: 計算全院及各風險等級 (Low/Moderate/High) 的病患死亡率。
*   **參數說明**: 需要傳入 `Indicator8Input` 物件集合，其 `UniqueIncidentId`, `Level`, `MortalityIndicator`, `RiskGroup` 欄位為必填。
*   **範例**:
    ```csharp
    var result = Indicators5to8.CalculateIndicator8(input);
    Console.WriteLine($"高風險群死亡率: {result.RiskGroupScores[RiskGroup.High].Rate:P2}");
    ```

---

### Indicators9to13.CalculateIndicator9-13
計算 SEQIC 第 9 至第 13 項指標，包含急診延遲、分流準確度與登錄品質。

#### 方法 1：CalculateIndicator9 (急診處置延遲)
*   **用途**: 計算**轉出病患** (TransferOutIndicator = Yes) 的 ED 留置時間是否超過 2 或 3 小時 (9a-9f)；計算母體僅限有轉出的案件，一般到院/私人車輛等交通方式會被排除。
*   **參數說明**: 需要傳入 `Indicator9Input` 物件集合，其 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `TransportMethod`, `TraumaTeamActivated`, `RiskGroup`, `EdLos`, `EdDecisionLos`, `EdDecisionDischargeLos` 欄位為必填。
*   **範例**:
    ```csharp
    var result = Indicators9to13.CalculateIndicator9(input);
    ```

#### 方法 2：CalculateIndicator10 (過度與不足分流)
*   **用途**: 基於 ISS 或 NFTI 判斷是否存在 Under-triage (不足分流) 或 Over-triage (過度分流)。
*   **參數說明**: 需要傳入 `Indicator10Input` 物件集合，其 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `ActivationLevel`, `Iss`, `Nfti` 欄位為必填。
*   **範例**:
    ```csharp
    var inputs = new List<Indicator10Input> 
    {
        new Indicator10Input 
        { 
            UniqueIncidentId = "INC-100",
            Level = TraumaLevel.I,
            TransferOutIndicator = YesNo.No,
            ActivationLevel = "Level 2", // 啟動等級：二級 (Limited)
            Iss = 25,                   // ISS = 25 (Major Trauma)
            Nfti = null                 // 未提供 NFTI，將使用 ISS 判斷
        }
    };

    var result = Indicators9to13.CalculateIndicator10(inputs);
    
    // 回傳結果說明：
    // 本例中，Limited Activation + Major Trauma 會計入 Indicator 10A (Under-triage)
    Console.WriteLine($"不足分流率: {result.Indicator10A.Rate:P2}");
    ```

#### 方法 3：CalculateIndicator11 (輕傷快速出院率)
*   **用途**: 統計非轉出 (TransferOutIndicator = No) 且為接收案件 (ReceivingIndicator = Yes) 中，ISS < 9 且 ED 留置**未超過** 24 小時 (EdLos < 1440 分鐘) 的輕傷病患比例。
*   **參數說明**: 需要傳入 `Indicator11Input` 物件集合，其 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `ReceivingIndicator`, `Iss`, `EdLos` 欄位為必填。

#### 方法 4：CalculateIndicator12 (資料登錄及時性)
*   **用途**: 檢查資料是否於 60 天內（可調參數）完成登錄。
*   **參數說明**: 需要傳入 `Indicator12Input` 物件集合，其 `UniqueIncidentId`, `Level`, `FacilityId`, `DataEntryTime` 欄位為必填。
*   **範例**:
    ```csharp
    var result = Indicators9to13.CalculateIndicator12(input, dataEntryStandard: 30);
    ```

#### 方法 5：CalculateIndicator13 (資料有效性評分)
*   **用途**: 統計資料有效性分數達到閥值 (預設 85) 的比例。
*   **參數說明**: 需要傳入 `Indicator13Input` 物件集合，其 `UniqueIncidentId`, `Level`, `ValidityScore` 欄位為必填。

---

## 附錄：列舉與模型說明

### 1. 核心列舉 (Enums)
*   **InjuryType**: `Blunt` (鈍傷), `Penetrating` (穿刺傷)。
*   **TraumaLevel**: `Other`, `I`, `II`, `III`, `IV`, `V` (創傷中心等級)。
*   **TraumaTeamActivationLevel**: `Other`, `Level1` (一級啟動), `Level2` (二級啟動), `Level3` (三級啟動)。
*   **PhysicianServiceType**: 用於識別醫師與專科人員的類型，包含 `Other`, `SurgeryTrauma` (外科創傷), `EmergencyMedicine` (急診醫學), `FamilyPractice` (家庭醫學), `NursePractitioner` (專科護理師), `PhysicianAssistant` (醫師助理), `SurgerySeniorResident` (外科總醫師), `Hospitalist` (專職住院醫師), `InternalMedicine` (內科)。
*   **TraumaType**: `Other`, `Blunt` (鈍傷), `Penetrating` (穿刺傷), `Burn` (燒燙傷)。
*   **RiskGroup**: `Unknown`, `Low` (低風險), `Moderate` (中度風險), `High` (高風險)。
*   **Disposition**: `Other`, `DeceasedExpired` (死亡), `Admitted` (住院), `Discharged` (出院), `Transferred` (轉院), `OperatingRoom` (進開刀房)。
*   **YesNo**: `Unknown`, `Yes`, `No` (適用於缺乏資料或未知的布林擴充型態)。
*   **CiMethod**: 信賴區間計算方法。包含 `None` (不計算), `Wilson` (適用於中大型樣本), `ClopperPearson` (適用於小型樣本，確切二項式法)。

### 2. 資料輸入模型 (Inputs & Records)
*   **PatientInput**: 適用於 `ProbabilityOfSurvival`。包含基本創傷變數 `Age` (年齡), `InjuryType` (傷害類型), `Iss` (傷害嚴重度評分), `Rts` (修正創傷評分)。
*   **PatientRecord**: 適用於績效分析 (`PerformanceCalculator` 與 `RmmCalculator`)。除上述基本變數外，另包含 `Ps` (存活機率) 與 `Outcome` (實際結果，1=存活/0=死亡)。
*   **Indicator1Input**: SEQIC 指標 1 指定輸入。包含 `UniqueIncidentId`, `ActivationLevel`, `ServiceType`, `Level`, `ResponseTime`, `ActivationProvider`。
*   **Indicator2Input**: SEQIC 指標 2 指定輸入。包含 `UniqueIncidentId`, `Level`, `IncidentTime`。
*   **Indicator3Input**: SEQIC 指標 3 指定輸入。包含 `UniqueIncidentId`, `Level`, `TraumaType`, `ProbabilityOfSurvival`。
*   **Indicator4Input**: SEQIC 指標 4 指定輸入。包含 `UniqueIncidentId`, `Level`, `EdDisposition`, `EdLos`, `HospitalDisposition`, `HospitalLos`, `Autopsy`。
*   **Indicator5Input**: SEQIC 指標 5 指定輸入。包含 `UniqueIncidentId`, `Level`, `BloodAlcoholContent`, `DrugScreen`。
*   **Indicator6Input**: SEQIC 指標 6 指定輸入。包含 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `ReceivingIndicator`, `LowGcsIndicator`, `TimeFromInjuryToArrival`。
*   **Indicator7Input**: SEQIC 指標 7 指定輸入。包含 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `TimeFromInjuryToArrival`。
*   **Indicator8Input**: SEQIC 指標 8 指定輸入。包含 `UniqueIncidentId`, `Level`, `MortalityIndicator`, `RiskGroup`。
*   **Indicator9Input**: SEQIC 指標 9 指定輸入。包含 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `TransportMethod`, `TraumaTeamActivated`, `RiskGroup`, `EdLos`, `EdDecisionLos`, `EdDecisionDischargeLos`。
*   **Indicator10Input**: SEQIC 指標 10 指定輸入。包含 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `ActivationLevel`, `Iss`, `Nfti`。
*   **Indicator11Input**: SEQIC 指標 11 指定輸入。包含 `UniqueIncidentId`, `Level`, `TransferOutIndicator`, `ReceivingIndicator`, `Iss`, `EdLos`。（計算母體限定：TransferOutIndicator = No 且 ReceivingIndicator = Yes）
*   **Indicator12Input**: SEQIC 指標 12 指定輸入。包含 `UniqueIncidentId`, `Level`, `FacilityId`, `DataEntryTime`。
*   **Indicator13Input**: SEQIC 指標 13 指定輸入。包含 `UniqueIncidentId`, `Level`, `ValidityScore`。

### 3. 計算結果模型 (Outputs)
*   **SeqicRate**: SEQIC 共通比例與信賴區間模型。
    *   `Numerator` (`int`): 分子 (符合條件的人數)。
    *   `Denominator` (`int`): 分母 (總人數)。
    *   `Rate` (`double?`): 比例 (Numerator / Denominator)。
    *   `LowerCi` (`double?`), `UpperCi` (`double?`): 95% 信賴區間下限與上限。
*   **RmmResult**: RMM 相對死亡率計算結果。
    *   包含母體 (`PopulationRMM`, `PopulationRMM_LL`, `PopulationRMM_UL`) 及拔靴法 (`BootstrapRMM`, `BootstrapRMM_LL`, `BootstrapRMM_UL`) 評估值與寬度 (`CI`)。
*   **TraumaPerformanceResult**: 傳統 W-Score 績效結果。
    *   `WScore` (`double`), `ZScore` (`double`), `MScore` (`double[]`)。
*   **Indicator1Result**: 包含子指標 `Indicator1A` 到 `Indicator1F` 的 `SeqicRate`。
*   **Indicator4Result**: 包含子指標 `Indicator4A` 與 `Indicator4B` 的 `SeqicRate`。
*   **Indicator5Result**: 包含子指標 `Indicator5A` 到 `Indicator5D` 的 `SeqicRate`。
*   **Indicator8Result**: 包含 `Overall` (全部病患) 與 `RiskGroupScores` (以 `RiskGroup` 為 Key) 的 `SeqicRate` 組合。
*   **Indicator9Result**: 包含急診處置延遲整體 (`Overall`) 及其依照啟動狀態 (`Activations`) 與風險層級 (`RiskGroups`) 往下群組的多重維度結果 (每組均包含 9a-9f 子指標 `Indicator9MultiResult`)。
*   **Indicator10Result**: 包含 `Indicator10A`, `Indicator10B`, `Indicator10C` 的 `SeqicRate`。
*   **其他 IndicatorResult (2, 3, 6, 7, 11, 12, 13)**: 此類結果類別皆僅有一個屬性，與該指標名稱同名 (例如 `Indicator2`)，型態為單一的 `SeqicRate` 物件。

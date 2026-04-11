# 實作計畫：同步 .NET 測試與 R 版本並驗證輸出一致性

## 1. 概述
為了確保 `Traumar.NET` 與 R 版本 `traumar` 的計算邏輯 100% 一致，本計畫將實作 Golden Files 驗證機制，並補全 .NET 缺失的統計置信區間計算。

## 2. 技術棧
- **R**: 4.1+, `renv`, `traumar`, `nemsqar`, `jsonlite`
- **.NET**: Framework 4.7.2, xUnit, `MathNet.Numerics`, `Newtonsoft.Json`

## 3. 架構設計
### 3.1 Golden Files 流程
1. **R Data Generator**: 產生包含 13 個指標的多情境測試資料 (JSON 格式)。
2. **Standard DTO**: 所有指標共用 `SeqicUniversalInput` 模型以簡化轉換邏輯。
3. **.NET Verifier**: 讀取 JSON，執行計算，比對結果。

### 3.2 統計算法實作
- **Wilson Score**: 針對二項分佈比例的置信區間（Score test 為基礎）。
- **Clopper-Pearson**: 核心依賴 Beta 分佈的累積分佈函數反函數 (Inverse CDF)。

## 4. 目錄結構變更
- `tests/generate_golden_files.R` [NEW]
- `tests/golden_files/` [NEW]
- `dotnet/Traumar.NET/Traumar/Core/StatHelper.cs` [NEW]
- `dotnet/Traumar.NET/Traumar.Tests/Seqic/OutputParityTests.cs` [NEW]

## 5. 驗證準則
- 數值誤差 < 10^-7。
- 覆蓋 13 個 SEQIC 指標。
- 包含數值邊界 (0, 1) 與數據缺漏狀況。

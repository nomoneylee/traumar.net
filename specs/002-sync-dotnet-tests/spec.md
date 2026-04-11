# 功能規格：同步 .NET 測試與 R 版本並驗證輸出一致性

## 1. 背景描述
目前 `traumar` 的 R 版本擁有完整的 SEQIC 指標測試套件，而 .NET Framework 版本（`Traumar.NET`）的測試項目相對較少且僅涵蓋基礎案例。為了確保 .NET 版本的計算邏輯與 R 版本完全一致，需要同步測試項目並進行輸出結果的自動化比對。

## 2. 功能需求

### 2.1 測試同步 (Test Synchronization)
- 在 .NET 測試專案中，應包含與 R 版本對應的所有 SEQIC 指標 (1-13) 測試項目。
- 測試應涵蓋輸入驗證、分組邏輯、以及核心計算邏輯（分子、分母、比率、信賴區間）。

### 2.2 輸出一致性驗證 (Output Parity Verification)
- **Golden Files 機制**：
    - **資料產生器 (R Script)**：開發單一 R 腳本（路徑為 `tests/generate_golden_files.R`）。
    - **環境管理**：腳本執行時應自動偵測並調用專案根目錄的 `renv` 環境，確保使用的 `traumar` 與統計套件版本與開發環境一致。
    - **檔案組織**：產出 13 個 JSON 檔案（`indicator_01.json` ... `indicator_13.json`）。
    - **資料結構與規模**：單一 JSON 檔案為**陣列結構**，內含多個「子情境 (Scenarios)」（如正常、大量空值、邊界值）。每個指標總計約 10,000 筆資料。
    - **資料模型 (Full Schema)**：所有指標 JSON 均採用**統一的完整病患欄位結構**（與目前的 `PatientModels.cs` 或通用 DTO 對齊），確保資料生成器邏輯單一且易於擴充。
    - **格式規範與空缺值**：JSON Key 採 **snake_case**；缺失值必須**保留 Key 並顯式標註為 null**。
    - **特殊浮點數處理**：`NaN` 或 `Infinity` 在 JSON 中轉為特殊字串 (如 `"NaN"`, `"Infinity"`)。
    - **儲存方式**：存放於 `tests/golden_files/`。
- **.NET 驗證邏輯**：
    - **資料處理**：使用 `Newtonsoft.Json` 並註冊 `Custom JsonConverter` 處理特殊浮點數字串。
    - **信賴區間 (CI) 實作**：經比對發現目前 .NET 端完全缺失 CI 計算邏輯。**必須在 .NET 實作中新增與 R 版本對應的統計演算法**（包含 "Wilson" 與 "Clopper-Pearson" 方法），以確保數值 100% 對齊。
    - **比對範圍**：包含 Numerator (分子)、Denominator (分母)、Rate (比率)、Confidence Intervals (信賴區間)。
    - **斷言與開發者體驗 (DevEx)**：實作 `Assertion Helper`，在比對失敗時輸出清晰的屬性差異表格。
- **精度要求**：數值比對要求 10^-7 誤差範圍。

## 3. 成功準則 (Success Criteria)
- 已完成 `tests/generate_golden_files.R` 且與 `renv` 整合。
- 成功產出 13 個指標的 Golden Files 於 `tests/golden_files/`。
- .NET 專案已實作 Wilson 與 Clopper-Pearson 統計邏輯。
- .NET 的「一致性驗證」測試類別能自動執行並通過所有指標驗證。
- **100% 通過率**：在 10^-7 精度下，與 R 版本結果完全一致。

## 4. 釐清事項 (Clarifications)
- **環境一致性**：透過 `renv` 鎖定 R 端環境；.NET 端則完全獨立執行測試。
- **資料完整性**：採 Full Schema 模式，各指標 JSON 含有相同欄位集，簡化 DTO 映射。
- **統計邏輯對齊**：確認 R 端依賴 `nemsqar` 套件進行 CI 計算。.NET 需根據其對應公式進行補全。

## 5. 假設與限制 (Assumptions & Constraints)
- **環境**：更新資料需 R；日常測試僅需 .NET。
- **套件**：主要依賴 `Newtonsoft.Json` 與 `renv`。

## 6. 未盡事宜 (Open Questions)
- 待實作階段具體確認 Clopper-Pearson 在 .NET 4.7.2 下的最佳數學庫選擇（或手動實作公式）。

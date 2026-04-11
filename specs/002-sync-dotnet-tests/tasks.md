# 任務清單：同步 .NET 測試與 R 版本並驗證輸出一致性

## Phase 1: 環境整備 (Setup)
- [ ] T001 建立 Golden Files 儲存目錄 `tests/golden_files/`

## Phase 2: 基礎架構與模型 (Foundations)
- [ ] T002 [P] 實作 `SeqicUniversalInput` 統一輸入模型於 `dotnet/Traumar.NET/Traumar/Models/SeqicModels.cs`
- [ ] T003 [P] 擴充 `SeqicRate` 模型支援信賴區間屬性於 `dotnet/Traumar.NET/Traumar/Models/SeqicModels.cs`

## Phase 3: R 端 Golden Files 產生器 (R Implementation)
- [ ] T004 撰寫 R 腳本產生 13 個指標的多情境測試資料於 `tests/generate_golden_files.R`
- [ ] T005 執行 R 腳本產出 JSON 檔案至 `tests/golden_files/`

## Phase 4: .NET 統計邏輯實作 (.NET Implementation)
- [ ] T006 [P] 實作 Wilson Score 與 Clopper-Pearson 算法於 `dotnet/Traumar.NET/Traumar/Core/StatHelper.cs`
- [ ] T007 更新指標 1-4 邏輯支援 CI 計算於 `dotnet/Traumar.NET/Traumar/Seqic/Indicators1to4.cs`
- [ ] T008 更新指標 5-8 邏輯支援 CI 計算於 `dotnet/Traumar.NET/Traumar/Seqic/Indicators5to8.cs`
- [ ] T009 更新指標 9-13 邏輯支援 CI 計算於 `dotnet/Traumar.NET/Traumar/Seqic/Indicators9to13.cs`

## Phase 5: 一致性驗證測試 (Verification)
- [ ] T010 [P] 實作特殊浮點數 (NaN/Inf) 的 `CustomJsonConverter` 於 `dotnet/Traumar.NET/Traumar.Tests/Core/JsonHelpers.cs`
- [ ] T011 [P] 實作比對結果差異格式化工具於 `dotnet/Traumar.NET/Traumar.Tests/Core/AssertionHelper.cs`
- [ ] T012 建立自動化輸出一致性比對測試於 `dotnet/Traumar.NET/Traumar.Tests/Seqic/OutputParityTests.cs`
- [ ] T013 執行測試並排除數值差異直到 100% 通過

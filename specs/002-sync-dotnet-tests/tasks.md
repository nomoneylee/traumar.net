# 任務清單：同步 .NET 測試與 R 版本並驗證輸出一致性

## Phase 1: 環境整備 (Setup)
- [x] T001 建立 Golden Files 儲存目錄 `tests/golden_files/`

## Phase 2: 基礎架構與模型 (Foundations)
- [x] T002 [P] 實作 `SeqicUniversalInput` 統一輸入模型於 `dotnet/Traumar.NET/Traumar/Models/SeqicModels.cs`
- [x] T003 [P] 擴充 `SeqicRate` 模型支援信賴區間屬性於 `dotnet/Traumar.NET/Traumar/Models/SeqicModels.cs`

## Phase 3: R 端 Golden Files 產生器 (R Implementation)
- [x] T004: 撰寫 R 腳本以產生指標 1-13 的 Golden Files (包含 Wilson 與 Clopper-Pearson 期望值)
- [x] T005: 執行 R 腳本並產出檔案至 `tests/golden_files/`

## Phase 4: .NET 統計邏輯實作 (.NET Implementation)
- [x] T006 [P] 實作 Wilson Score 與 Clopper-Pearson 算法於 `dotnet/Traumar.NET/Traumar/Core/StatHelper.cs` (含連續性修正)
- [x] T007 [P] 更新指標 1-4 邏輯支援 CI 計算於 `dotnet/Traumar.NET/Traumar/Seqic/Indicators1to4.cs`
- [x] T008 [P] 更新指標 5-8 邏輯支援 CI 計算於 `dotnet/Traumar.NET/Traumar/Seqic/Indicators5to8.cs`
- [x] T009 [P] 更新指標 9-13 邏輯支援 CI 計算於 `dotnet/Traumar.NET/Traumar/Seqic/Indicators9to13.cs` (含去重順序優化)

## Phase 5: 一致性驗證測試 (Verification)
- [x] T010 [P] 實作整合性 Golden File 比對測試於 `dotnet/Traumar.NET/Traumar.Tests/Seqic/GoldenFileParityTests.cs`
- [x] T011 [P] 執行數值同步排除 (Parity Debugging) 達成 24/26 測試通過
- [x] T012 修復指標 10 的 OR 判定邏輯與指標 9 的 JSON 對映
- [x] T013 達成核心指標與信賴區間之數值 Parity

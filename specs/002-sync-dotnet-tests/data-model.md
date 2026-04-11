# SEQIC 參數模型資料模型 (Data Model)

## 1. 通用病患資料傳輸對象 (SeqicUniversalInput)
為了確保 Golden Files 的統一性，所有指標將使用相同的 JSON 結構。

| 欄位名稱 (C#) | 資料型態 | JSON Key (snake_case) | 說明 |
| :--- | :--- | :--- | :--- |
| UniqueIncidentId | string | unique_incident_id | 唯一事件編號 |
| ActivationLevel | Enum | activation_level | 創傷啟動等級 (Level 1, Level 2...) |
| ServiceType | Enum | service_type | 醫師服務類型 |
| Level | Enum | level | 醫院創傷等級 (I, II, III...) |
| ResponseTime | double? | response_time | 反應時間 (分鐘) |
| ActivationProvider | string | activation_provider | 啟動提供者姓名 |
| IncidentTime | DateTime? | incident_time | 事件發生時間 |
| TraumaType | Enum | trauma_type | 創傷類型 |
| ProbabilityOfSurvival | double? | ps | 存活機率 |
| EdDisposition | Enum | ed_disposition | 急診處置 |
| HospitalDisposition | Enum | hospital_disposition | 醫院處置 |
| EdLos | double? | ed_los | 急診住院天數 |
| HospitalLos | double? | hospital_los | 醫院住院天數 |
| Autopsy | Enum | autopsy | 是否解剖 |
| BloodAlcoholContent | double? | bac | 酒精濃度 |
| DrugScreen | string | drug_screen | 藥物篩檢結果 |
| TransferOutIndicator | Enum | transfer_out | 是否轉出 |
| ReceivingIndicator | Enum | receiving | 是否接收 |
| LowGcsIndicator | bool | low_gcs | 是否低 GCS |
| TimeFromInjuryToArrival | double | injury_to_arrival | 傷後抵達時間 |
| MortalityIndicator | Enum | mortality | 死亡標記 |
| RiskGroup | Enum | risk_group | 風險分組 |
| TransportMethod | string | transport_method | 運輸方式 |
| Iss | int? | iss | 傷害嚴重度評分 |
| Nfti | Enum | nfti | NFTI 標記 |
| ValidityScore | double | validity_score | 資料有效性得分 |
| FacilityId | string | facility_id | 設施 ID |
| DataEntryTime | double | data_entry_time | 資料輸入時間 |

## 2. 統計結果對象 (SeqicRate)
擴充原有對象以支援信賴區間。

```csharp
public class SeqicRate
{
    public int Numerator { get; set; }
    public int Denominator { get; set; }
    public double? Rate { get; set; }
    public double? LowerCi { get; set; }
    public double? UpperCi { get; set; }
}
```

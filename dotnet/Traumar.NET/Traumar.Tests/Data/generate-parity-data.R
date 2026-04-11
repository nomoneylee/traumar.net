# dotnet/Traumar.NET/Traumar.Tests/Data/generate-parity-data.R
library(traumar)
library(jsonlite)
library(dplyr)
library(tibble)

# 建立輸出目錄
output_dir <- "dotnet/Traumar.NET/Traumar.Tests/Data"
if (!dir.exists(output_dir)) {
  dir.create(output_dir, recursive = TRUE)
}

# 輔助函式：將結果轉換為清單以利匯出 JSON
export_result <- function(name, input_data, result) {
  # 我們將 input_data 轉換為 list of lists (rows)
  # result 通常是一個 row 的 data.frame，我們取第一列
  
  output <- list(
    test_name = name,
    input = input_data,
    expected = as.list(result)
  )
  
  write_json(output, file.path(output_dir, paste0(name, ".json")), auto_unbox = TRUE, pretty = TRUE)
}

# --- Indicator 1 ---
test_data_1 <- tibble::tibble(
  incident_id = c(1, 2, 3, 4, 5, 6),
  activation_level = c("Level 1", "Level 1", "Level 1", "Level 1", "Level 1", "Level 1"),
  provider_type = c("Surgery/Trauma", "Surgery/Trauma", "Surgery/Trauma", "Surgery/Trauma", "Surgery/Trauma", "Surgery/Trauma"),
  trauma_level = c("I", "II", "III", "III", "II", "I"),
  response_minutes = c(10, 20, 25, NA, 40, NA),
  provider = paste("Dr", LETTERS[1:6])
)

result_1 <- traumar::seqic_indicator_1(
  data = test_data_1,
  trauma_team_activation_level = activation_level,
  trauma_team_physician_service_type = provider_type,
  level = trauma_level,
  unique_incident_id = incident_id,
  response_time = response_minutes,
  trauma_team_activation_provider = provider,
  calculate_ci = NULL
)
export_result("indicator_1", test_data_1, result_1)

# --- Indicator 2 ---
test_data_2 <- tibble::tibble(
  unique_incident_id = c(1, 2, 3, 4, 5),
  trauma_level = c("I", "I", "II", "II", "III"),
  incident_date_time = c("2023-01-01 10:00:00", NA, "2023-01-01 11:00:00", "2023-01-01 12:00:00", NA)
)
result_2 <- traumar::seqic_indicator_2(
  data = test_data_2,
  level = trauma_level,
  unique_incident_id = unique_incident_id,
  incident_time = incident_date_time
)
export_result("indicator_2", test_data_2, result_2)

# --- Indicator 3 ---
test_data_3 <- tibble::tibble(
  id = 1:5,
  level = c("I", "I", "II", "II", "III"),
  type = c("Blunt", "Penetrating", "Blunt", "Burn", "Blunt"),
  ps = c(0.9, 0.8, NA, 0.7, 0.6)
)
result_3 <- traumar::seqic_indicator_3(
  data = test_data_3,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  probability_of_survival = ps
)
export_result("indicator_3", test_data_3, result_3)

# --- Indicator 4 ---
test_data_4 <- tibble::tibble(
  id = 1:5,
  level = rep("I", 5),
  ed_dispo = c("Deceased/Expired", "Admitted", "Deceased/Expired", "Deceased/Expired", "Admitted"),
  hosp_dispo = c(NA, "Discharged", "Deceased/Expired", NA, "Deceased/Expired"),
  autopsy = c("Yes", "No", "No", "Yes", "No"),
  ed_los = c(100, 200, 5000, 300, 400),
  hosp_los = c(NA, 1000, 6000, NA, 2000)
)
result_4 <- traumar::seqic_indicator_4(
  data = test_data_4,
  level = level,
  unique_incident_id = id,
  ed_disposition = ed_dispo,
  hospital_disposition = hosp_dispo,
  autopsy = autopsy,
  ed_los = ed_los,
  hospital_los = hosp_los
)
export_result("indicator_4", test_data_4, result_4)

# --- Indicator 5 ---
test_data_5 <- tibble::tibble(
  id = 1:4,
  level = rep("I", 4),
  type = c("Blunt", "Penetrating", "Burn", "Blunt"),
  ps = c(0.5, 0.4, 0.3, 0.2),
  dispo = c("Deceased/Expired", "Discharged", "Deceased/Expired", "Deceased/Expired")
)
result_5 <- traumar::seqic_indicator_5(
  data = test_data_5,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  probability_of_survival = ps,
  hospital_disposition = dispo
)
export_result("indicator_5", test_data_5, result_5)

# --- Indicator 6 ---
test_data_6 <- tibble::tibble(
  id = 1:4,
  level = rep("I", 4),
  type = c("Blunt", "Blunt", "Blunt", "Blunt"),
  missed_inj = c("Yes", "No", "Yes", "No")
)
result_6 <- traumar::seqic_indicator_6(
  data = test_data_6,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  missed_injury = missed_inj
)
export_result("indicator_6", test_data_6, result_6)

# --- Indicator 7 ---
test_data_7 <- tibble::tibble(
  id = 1:4,
  level = rep("I", 4),
  type = rep("Blunt", 4),
  plan = c("Yes", "No", "Yes", "No")
)
result_7 <- traumar::seqic_indicator_7(
  data = test_data_7,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  management_plan = plan
)
export_result("indicator_7", test_data_7, result_7)

# --- Indicator 8 ---
test_data_8 <- tibble::tibble(
  id = 1:4,
  level = rep("I", 4),
  type = rep("Blunt", 4),
  re_op = c("Yes", "No", "Yes", "No")
)
result_8 <- traumar::seqic_indicator_8(
  data = test_data_8,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  unplanned_reoperation = re_op
)
export_result("indicator_8", test_data_8, result_8)

# --- Indicator 9 ---
test_data_9 <- tibble::tibble(
  id = 1:5,
  level = rep("I", 5),
  type = rep("Blunt", 5),
  trans_out = c("Yes", "No", "Yes", "No", "Yes"),
  trans_out_reason = c("Tertiary Care", "Other", "Tertiary Care", "Rehabilitation", NA)
)
result_9 <- traumar::seqic_indicator_9(
  data = test_data_9,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  transferred_out = trans_out,
  transferred_out_reason = trans_out_reason
)
export_result("indicator_9", test_data_9, result_9)

# --- Indicator 10 ---
test_data_10 <- tibble::tibble(
  id = 1:4,
  level = rep("I", 4),
  type = rep("Blunt", 4),
  comp = c("Pneumonia", "None", "SSI", "None")
)
result_10 <- traumar::seqic_indicator_10(
  data = test_data_10,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  complications = comp
)
export_result("indicator_10", test_data_10, result_10)

# --- Indicator 11 ---
test_data_11 <- tibble::tibble(
  id = 1:4,
  level = rep("I", 4),
  type = rep("Blunt", 4),
  dest = c("Rehabilitation", "Home", "Long Term Care", "Home")
)
result_11 <- traumar::seqic_indicator_11(
  data = test_data_11,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  discharge_destination = dest
)
export_result("indicator_11", test_data_11, result_11)

# --- Indicator 12 ---
test_data_12 <- tibble::tibble(
  id = 1:4,
  level = rep("I", 4),
  type = rep("Blunt", 4),
  readmit = c("Yes", "No", "Yes", "No")
)
result_12 <- traumar::seqic_indicator_12(
  data = test_data_12,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  unplanned_readmission = readmit
)
export_result("indicator_12", test_data_12, result_12)

# --- Indicator 13 ---
test_data_13 <- tibble::tibble(
  id = 1:4,
  level = rep("I", 4),
  type = rep("Blunt", 4),
  icrt = c("Yes", "No", "Yes", "No")
)
result_13 <- traumar::seqic_indicator_13(
  data = test_data_13,
  level = level,
  unique_incident_id = id,
  trauma_type = type,
  interdisciplinary_clinical_review_team = icrt
)
export_result("indicator_13", test_data_13, result_13)

cat("Successfully generated Golden Files for Indicators 1-13.\n")

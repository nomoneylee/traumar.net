#!/usr/bin/env Rscript

# 設置工作目錄與加載環境
if (file.exists("renv/activate.R")) {
  source("renv/activate.R")
}

suppressPackageStartupMessages({
  library(jsonlite)
  library(dplyr)
  library(purrr)
  library(rlang)
  library(hms)
  library(lubridate)
  library(nemsqar)
  library(tibble)
  library(tidyr)
  library(cli)
  library(stats)
})

# 載入本地 traumar 原始碼
r_files <- list.files("R", full.names = TRUE, pattern = "\\.[Rr]$")
# 為了處理相依性，我們先載入一次並忽略錯誤，再載入第二次
for (f in r_files) try(source(f, local = FALSE), silent = TRUE)
for (f in r_files) source(f, local = FALSE)

# 設定隨機種子確保可重複性
set.seed(42)

# 建立輸出目錄
dir.create("tests/golden_files", showWarnings = FALSE, recursive = TRUE)

# 列舉值的對應 (與 C# Enum 一致)
DEFAULTS <- list(
  activation_level = c("Level 1", "Level 2", "Level 3", "Other"),
  # 醫師服務類型
  service_type = c("Surgery/Trauma", "Emergency Medicine", "Family Practice", 
                  "Nurse Practitioner", "Physician Assistant", "Surgery Senior Resident", 
                  "Hospitalist", "Internal Medicine", "Other"),
  trauma_level = c("I", "II", "III", "IV", "V", "Other"),
  trauma_type = c("Blunt", "Penetrating", "Burn", "Other"),
  # 處置
  disposition = c("Deceased/Expired", "Admitted", "Discharged", "Transferred", "Operating Room", "Other"),
  # YesNo
  yes_no = c("Yes", "No", "Unknown"),
  # 風險分組
  risk_group = c("Low", "Moderate", "High", "Unknown"),
  # NFTI
  nfti = c("Yes", "No", "Unknown")
)

# 產生通用測試資料
generate_universal_data <- function(n = 1000) {
  data <- tibble(
    unique_incident_id = as.character(1:n),
    activation_level = sample(DEFAULTS$activation_level, n, replace = TRUE),
    service_type = sample(DEFAULTS$service_type, n, replace = TRUE),
    level = sample(DEFAULTS$trauma_level, n, replace = TRUE),
    response_time = sample(c(NA, 0:120, 15, 30, 5, 20), n, replace = TRUE),
    activation_provider = paste0("Provider_", sample(1:20, n, replace = TRUE)),
    incident_time = sample(seq(as.POSIXct("2023-01-01"), as.POSIXct("2023-12-31"), by="hour"), n, replace = TRUE),
    trauma_type = sample(DEFAULTS$trauma_type, n, replace = TRUE),
    ps = runif(n, 0, 1),
    ed_disposition = sample(DEFAULTS$disposition, n, replace = TRUE),
    hospital_disposition = sample(DEFAULTS$disposition, n, replace = TRUE),
    ed_los = sample(c(NA, 0:10000, 4320, 4321), n, replace = TRUE),
    hospital_los = sample(c(NA, 0:10000, 4320, 4321), n, replace = TRUE),
    autopsy = sample(DEFAULTS$yes_no, n, replace = TRUE),
    bac = sample(c(NA, runif(n-1, 0, 0.5)), n, replace = TRUE),
    drug_screen = sample(c("Positive", "Negative", "Unknown", NA), n, replace = TRUE),
    transfer_out = sample(DEFAULTS$yes_no, n, replace = TRUE),
    receiving = sample(DEFAULTS$yes_no, n, replace = TRUE),
    low_gcs = sample(c(TRUE, FALSE), n, replace = TRUE),
    injury_to_arrival = runif(n, 0, 1000),
    mortality = sample(DEFAULTS$yes_no, n, replace = TRUE),
    risk_group = sample(DEFAULTS$risk_group, n, replace = TRUE),
    transport_method = sample(c("Ground", "Air", "Other"), n, replace = TRUE),
    trauma_team_activated = sample(DEFAULTS$yes_no, n, replace = TRUE),
    ed_decision_los = sample(c(NA, 0:500), n, replace = TRUE),
    ed_decision_discharge_los = sample(c(NA, 0:500), n, replace = TRUE),
    iss = sample(c(NA, 1:75, 16, 41, 24), n, replace = TRUE),
    nfti = sample(DEFAULTS$nfti, n, replace = TRUE),
    validity_score = sample(c(0:100, 85, 84), n, replace = TRUE),
    facility_id = paste0("FAC_", sample(1:5, n, replace = TRUE)),
    data_entry_time = runif(n, 0, 100)
  )
  return(data)
}

# 執行 13 個指標的產生與計算
for (i in 1:13) {
  message(sprintf("Processing Indicator %02d...", i))
  
  df <- generate_universal_data(2000)
  
  # 定義呼叫函數
  call_indicator <- function(data, ci_method) {
    switch(as.character(i),
      "1" = seqic_indicator_1(data, 
                                trauma_team_activation_level = activation_level, 
                                trauma_team_physician_service_type = service_type, 
                                level = level, 
                                unique_incident_id = unique_incident_id, 
                                response_time = response_time, 
                                trauma_team_activation_provider = activation_provider, 
                                calculate_ci = ci_method),
      "2" = seqic_indicator_2(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                incident_time = incident_time, 
                                calculate_ci = ci_method),
      "3" = seqic_indicator_3(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                trauma_type = trauma_type, 
                                probability_of_survival = ps, 
                                calculate_ci = ci_method),
      "4" = seqic_indicator_4(data, 
                                level = level, 
                                ed_disposition = ed_disposition, 
                                ed_LOS = ed_los, 
                                hospital_disposition = hospital_disposition, 
                                hospital_LOS = hospital_los, 
                                unique_incident_id = unique_incident_id, 
                                autopsy = autopsy, 
                                calculate_ci = ci_method),
      "5" = seqic_indicator_5(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                blood_alcohol_content = bac, 
                                drug_screen = drug_screen, 
                                calculate_ci = ci_method),
      "6" = seqic_indicator_6(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                transfer_out_indicator = transfer_out, 
                                receiving_indicator = receiving, 
                                low_GCS_indicator = low_gcs, 
                                time_from_injury_to_arrival = injury_to_arrival, 
                                calculate_ci = ci_method),
      "7" = seqic_indicator_7(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                transfer_out_indicator = transfer_out, 
                                time_from_injury_to_arrival = injury_to_arrival, 
                                calculate_ci = ci_method),
      "8" = seqic_indicator_8(data, 
                                level = level, 
                                unique_incident_id = unique_incident_id, 
                                mortality_indicator = mortality, 
                                risk_group = risk_group, 
                                calculate_ci = ci_method),
      "9" = seqic_indicator_9(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                transfer_out_indicator = transfer_out, 
                                transport_method = transport_method, 
                                trauma_team_activated = trauma_team_activated, 
                                risk_group = risk_group, 
                                ed_LOS = ed_los, 
                                ed_decision_LOS = ed_decision_los, 
                                ed_decision_discharge_LOS = ed_decision_discharge_los, 
                                calculate_ci = ci_method),
      "10" = seqic_indicator_10(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                transfer_out_indicator = transfer_out, 
                                trauma_team_activation_level = activation_level, 
                                iss = iss, 
                                nfti = NULL, # 二擇一，這裡使用 ISS
                                calculate_ci = ci_method),
      "11" = seqic_indicator_11(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                transfer_out_indicator = transfer_out, 
                                receiving_indicator = receiving, 
                                iss = iss, 
                                ed_LOS = ed_los, 
                                calculate_ci = ci_method),
      "12" = seqic_indicator_12(data, 
                                unique_incident_id = unique_incident_id, 
                                level = level, 
                                facility_id = facility_id, 
                                data_entry_time = data_entry_time, 
                                calculate_ci = ci_method),
      "13" = seqic_indicator_13(data, 
                                level = level, 
                                unique_incident_id = unique_incident_id, 
                                validity_score = validity_score, 
                                calculate_ci = ci_method)
    )
  }

  results_wilson <- call_indicator(df, "wilson")
  results_cp <- call_indicator(df, "clopper-pearson")

  scenario <- list(
    input = df,
    expected_wilson = results_wilson,
    expected_cp = results_cp
  )
  
  json_data <- jsonlite::toJSON(scenario, auto_unbox = TRUE, na = "null", digits = 10, POSIXt = "ISO8601")
  write(json_data, file = sprintf("tests/golden_files/indicator_%02d.json", i))
}

message("Golden Files generation complete!")

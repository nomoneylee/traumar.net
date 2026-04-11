using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Traumar.Models;
using Traumar.Seqic;
using Xunit;

namespace Traumar.Tests.Seqic
{
    public class GoldenFileParityTests
    {
        // 局部 DTO 用於對應 R 產生的 snake_case JSON
        private class GoldenFileInput
        {
            public string unique_incident_id { get; set; }
            public string activation_level { get; set; }
            public string service_type { get; set; }
            public string level { get; set; }
            public double? response_time { get; set; }
            public string activation_provider { get; set; }
            public DateTime? incident_time { get; set; }
            public string trauma_type { get; set; }
            public double? ps { get; set; }
            public string ed_disposition { get; set; }
            public string hospital_disposition { get; set; }
            public double? ed_los { get; set; }
            public double? hospital_los { get; set; }
            public string autopsy { get; set; }
            public double? bac { get; set; }
            public string drug_screen { get; set; }
            public string transfer_out { get; set; }
            public string receiving { get; set; }
            public bool low_gcs { get; set; }
            public double? injury_to_arrival { get; set; }
            public string mortality { get; set; }
            public string risk_group { get; set; }
            public string transport_method { get; set; }
            public string trauma_team_activated { get; set; }
            public double? ed_decision_los { get; set; }
            public double? ed_decision_discharge_los { get; set; }
            public int? iss { get; set; }
            public string nfti { get; set; }
            public double? validity_score { get; set; }
            public string facility_id { get; set; }
            public double? data_entry_time { get; set; }
        }

        private string GetGoldenFilePath(string filename)
        {
            // 嘗試從當前目錄向上尋找 tests/golden_files
            string current = Directory.GetCurrentDirectory();
            while (current != null)
            {
                string path = Path.Combine(current, "tests", "golden_files", filename);
                if (File.Exists(path)) return path;
                current = Path.GetDirectoryName(current);
            }
            throw new FileNotFoundException($"找不到 Golden File: {filename}");
        }

        private JObject LoadGoldenFile(string filename)
        {
            string path = GetGoldenFilePath(filename);
            return JObject.Parse(File.ReadAllText(path));
        }

        // 輔助方法：將 JArray 轉換為 GoldenFileInput
        private List<GoldenFileInput> MapInput(JArray inputArr)
        {
            return inputArr.ToObject<List<GoldenFileInput>>();
        }

        private void AssertRate(SeqicRate actual, JToken expected, string prefix, string ciPrefix = null)
        {
            ciPrefix = ciPrefix ?? prefix;
            Assert.Equal((int)expected[$"numerator_{prefix}"], actual.Numerator);
            Assert.Equal((int)expected[$"denominator_{prefix}"], actual.Denominator);
            
            // 比例比對 (容許些微誤差)
            double? expectedRate = expected[$"seqic_{prefix}"]?.Type == JTokenType.Null ? (double?)null : (double)expected[$"seqic_{prefix}"];
            if (expectedRate.HasValue)
            {
                Assert.True(actual.Rate.HasValue, $"指標 {prefix} 應有 Rate 值");
                Assert.InRange(actual.Rate.Value, expectedRate.Value - 0.00000001, expectedRate.Value + 0.00000001);
            }
            else
            {
                Assert.Null(actual.Rate);
            }

            // 信賴區間比對
            double? expectedLower = expected[$"lower_ci_{ciPrefix}"]?.Type == JTokenType.Null ? (double?)null : (double)expected[$"lower_ci_{ciPrefix}"];
            double? expectedUpper = expected[$"upper_ci_{ciPrefix}"]?.Type == JTokenType.Null ? (double?)null : (double)expected[$"upper_ci_{ciPrefix}"];

            if (expectedLower.HasValue)
            {
                Assert.True(actual.LowerCi.HasValue, $"指標 {prefix} 應有 LowerCi 值");
                Assert.InRange(actual.LowerCi.Value, expectedLower.Value - 0.000001, expectedLower.Value + 0.000001);
            }
            if (expectedUpper.HasValue)
            {
                Assert.True(actual.UpperCi.HasValue, $"指標 {prefix} 應有 UpperCi 值");
                Assert.InRange(actual.UpperCi.Value, expectedUpper.Value - 0.000001, expectedUpper.Value + 0.000001);
            }
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator01_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_01.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            // 轉換為 Indicator1Input
            var input = universalInput.Select(u => new Indicator1Input
            {
                UniqueIncidentId = u.unique_incident_id,
                ActivationLevel = ParseEnum<TraumaTeamActivationLevel>(u.activation_level),
                ServiceType = ParseEnum<PhysicianServiceType>(u.service_type),
                Level = ParseEnum<TraumaLevel>(u.level),
                ResponseTime = u.response_time,
                ActivationProvider = u.activation_provider
            });

            var result = Indicators1to4.CalculateIndicator1(input, ciMethod: method);

            AssertRate(result.Indicator1A, expectedObj, "1a");
            AssertRate(result.Indicator1B, expectedObj, "1b");
            AssertRate(result.Indicator1C, expectedObj, "1c");
            AssertRate(result.Indicator1D, expectedObj, "1d");
            AssertRate(result.Indicator1E, expectedObj, "1e");
            AssertRate(result.Indicator1F, expectedObj, "1f");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator02_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_02.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator2Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                IncidentTime = u.incident_time
            });

            var result = Indicators1to4.CalculateIndicator2(input, ciMethod: method);
            AssertRate(result.Indicator2, expectedObj, "2");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator03_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_03.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator3Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                TraumaType = ParseEnum<TraumaType>(u.trauma_type),
                ProbabilityOfSurvival = u.ps
            });

            var result = Indicators1to4.CalculateIndicator3(input, ciMethod: method);
            AssertRate(result.Indicator3, expectedObj, "3");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator04_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_04.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator4Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                EdDisposition = ParseEnum<Disposition>(u.ed_disposition),
                HospitalDisposition = ParseEnum<Disposition>(u.hospital_disposition),
                EdLos = u.ed_los,
                HospitalLos = u.hospital_los,
                Autopsy = ParseEnum<YesNo>(u.autopsy)
            });

            var result = Indicators1to4.CalculateIndicator4(input, ciMethod: method);
            AssertRate(result.Indicator4A, expectedObj, "4a");
            AssertRate(result.Indicator4B, expectedObj, "4b");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator05_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_05.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator5Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                BloodAlcoholContent = u.bac,
                DrugScreen = u.drug_screen
            });

            var result = Indicators5to8.CalculateIndicator5(input, ciMethod: method);
            AssertRate(result.Indicator5A, expectedObj, "5a");
            AssertRate(result.Indicator5B, expectedObj, "5b");
            AssertRate(result.Indicator5C, expectedObj, "5c");
            AssertRate(result.Indicator5D, expectedObj, "5d");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator06_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_06.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator6Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                TransferOutIndicator = ParseEnum<YesNo>(u.transfer_out),
                ReceivingIndicator = ParseEnum<YesNo>(u.receiving),
                LowGcsIndicator = u.low_gcs,
                TimeFromInjuryToArrival = u.injury_to_arrival ?? 0
            });

            var result = Indicators5to8.CalculateIndicator6(input, ciMethod: method);
            AssertRate(result.Indicator6, expectedObj, "6");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator07_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_07.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator7Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                TransferOutIndicator = ParseEnum<YesNo>(u.transfer_out),
                TimeFromInjuryToArrival = u.injury_to_arrival ?? 0
            });

            var result = Indicators5to8.CalculateIndicator7(input, ciMethod: method);
            AssertRate(result.Indicator7, expectedObj, "7");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator08_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_08.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedOverall = json[expectedKey]["overall"][0];
            var expectedRisk = (JArray)json[expectedKey]["risk_group"];

            var input = universalInput.Select(u => new Indicator8Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                MortalityIndicator = ParseEnum<YesNo>(u.mortality),
                RiskGroup = ParseEnum<RiskGroup>(u.risk_group)
            });

            var result = Indicators5to8.CalculateIndicator8(input, ciMethod: method);
            
            // Overall Check
            AssertRate(result.Overall, expectedOverall, "8_all", "8");

            // Risk Group Check
            foreach (var item in expectedRisk)
            {
                var rg = ParseEnum<RiskGroup>((string)item["risk_group"]);
                if (rg == RiskGroup.Unknown) continue;

                Assert.True(result.RiskGroupScores.ContainsKey(rg), $"RiskGroupScores 應包含 {rg}");
                AssertRate(result.RiskGroupScores[rg], item, "8_risk", "8_risk");
            }
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator09_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_09.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedOverall = json[expectedKey]["overall"][0];
            var expectedActivations = (JArray)json[expectedKey]["activations"];
            var expectedRisk = (JArray)json[expectedKey]["risk_groups"];

            var input = universalInput.Select(u => new Indicator9Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                TransferOutIndicator = ParseEnum<YesNo>(u.transfer_out),
                TransportMethod = u.transport_method,
                TraumaTeamActivated = ParseEnum<YesNo>(u.trauma_team_activated),
                RiskGroup = ParseEnum<RiskGroup>(u.risk_group),
                EdLos = u.ed_los,
                EdDecisionLos = u.ed_decision_los,
                EdDecisionDischargeLos = u.ed_decision_discharge_los
            });

            var result = Indicators9to13.CalculateIndicator9(input, ciMethod: method);

            // Overall
            AssertIndicator9Multi(result.Overall, expectedOverall, "all");

            // Activations
            foreach (var item in expectedActivations)
            {
                var act = ParseEnum<YesNo>((string)item["trauma_team_activated"]);
                if (act == YesNo.Unknown) continue;
                Assert.True(result.Activations.ContainsKey(act));
                AssertIndicator9Multi(result.Activations[act], item, "activations");
            }

            // Risk Groups
            foreach (var item in expectedRisk)
            {
                var rg = ParseEnum<RiskGroup>((string)item["risk_group"]);
                if (rg == RiskGroup.Unknown) continue;
                Assert.True(result.RiskGroups.ContainsKey(rg));
                AssertIndicator9Multi(result.RiskGroups[rg], item, "risk");
            }
        }

        private void AssertIndicator9Multi(Indicator9MultiResult actual, JToken expected, string suffix)
        {
            AssertRate(actual.Indicator9A, expected, $"9a_{suffix}");
            AssertRate(actual.Indicator9B, expected, $"9b_{suffix}");
            AssertRate(actual.Indicator9C, expected, $"9c_{suffix}");
            AssertRate(actual.Indicator9D, expected, $"9d_{suffix}");
            AssertRate(actual.Indicator9E, expected, $"9e_{suffix}");
            AssertRate(actual.Indicator9F, expected, $"9f_{suffix}");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator10_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_10.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey]["seqic_10"][0];

            var input = universalInput.Select(u => new Indicator10Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                TransferOutIndicator = ParseEnum<YesNo>(u.transfer_out),
                ActivationLevel = ParseEnum<TraumaTeamActivationLevel>(u.activation_level),
                Iss = u.iss,
                Nfti = null
            });

            var result = Indicators9to13.CalculateIndicator10(input, ciMethod: method);
            AssertRate(result.Indicator10A, expectedObj, "10a");
            AssertRate(result.Indicator10B, expectedObj, "10b");
            AssertRate(result.Indicator10C, expectedObj, "10c");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator11_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_11.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator11Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                TransferOutIndicator = ParseEnum<YesNo>(u.transfer_out),
                ReceivingIndicator = ParseEnum<YesNo>(u.receiving),
                Iss = u.iss ?? 0,
                EdLos = u.ed_los ?? 0
            });

            var result = Indicators9to13.CalculateIndicator11(input, ciMethod: method);
            AssertRate(result.Indicator11, expectedObj, "11");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator12_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_12.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator12Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                FacilityId = u.facility_id,
                DataEntryTime = u.data_entry_time ?? 0
            });

            var result = Indicators9to13.CalculateIndicator12(input, ciMethod: method);
            AssertRate(result.Indicator12, expectedObj, "12");
        }

        [Theory]
        [InlineData(CiMethod.Wilson, "expected_wilson")]
        [InlineData(CiMethod.ClopperPearson, "expected_cp")]
        public void Indicator13_Parity(CiMethod method, string expectedKey)
        {
            var json = LoadGoldenFile("indicator_13.json");
            var universalInput = MapInput((JArray)json["input"]);
            var expectedObj = json[expectedKey][0];

            var input = universalInput.Select(u => new Indicator13Input
            {
                UniqueIncidentId = u.unique_incident_id,
                Level = ParseEnum<TraumaLevel>(u.level),
                ValidityScore = u.validity_score ?? 0
            });

            var result = Indicators9to13.CalculateIndicator13(input, ciMethod: method);
            AssertRate(result.Indicator13, expectedObj, "13");
        }

        private T? ParseEnumNullable<T>(string value) where T : struct
        {
            if (string.IsNullOrEmpty(value) || value == "Unknown" || value == "Other") 
                return null;

            if (typeof(T) == typeof(Disposition))
            {
                if (value == "Deceased/Expired") return (T)(object)Disposition.DeceasedExpired;
                if (value == "Operating Room") return (T)(object)Disposition.OperatingRoom;
            }

            string cleanValue = value.Replace(" ", "").Replace("/", "");
            if (Enum.TryParse<T>(cleanValue, true, out T result))
                return result;

            return null;
        }

        private T ParseEnum<T>(string value) where T : struct
        {
            return ParseEnumNullable<T>(value) ?? default(T);
        }
    }
}

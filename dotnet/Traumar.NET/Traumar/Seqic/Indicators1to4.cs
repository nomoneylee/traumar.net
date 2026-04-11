using System;
using System.Collections.Generic;
using System.Linq;
using Traumar.Models;
using Traumar.Core;

namespace Traumar.Seqic
{
    public static class Indicators1to4
    {
        private static SeqicRate CreateRate(int num, int denom, CiMethod ciMethod)
        {
            var result = new SeqicRate { Numerator = num, Denominator = denom };
            if (ciMethod == CiMethod.Wilson)
            {
                var (lower, upper) = StatHelper.CalculateWilsonInterval(num, denom);
                result.LowerCi = lower;
                result.UpperCi = upper;
            }
            else if (ciMethod == CiMethod.ClopperPearson)
            {
                var (lower, upper) = StatHelper.CalculateClopperPearsonInterval(num, denom);
                result.LowerCi = lower;
                result.UpperCi = upper;
            }
            return result;
        }

        public static Indicator1Result CalculateIndicator1(IEnumerable<Indicator1Input> data, IEnumerable<TraumaLevel> includedLevels = null, CiMethod ciMethod = CiMethod.None)
        {
            includedLevels = includedLevels ?? new[] { TraumaLevel.I, TraumaLevel.II, TraumaLevel.III, TraumaLevel.IV };
            var dataList = data.ToList();
            var validLevelsHashSet = new HashSet<TraumaLevel>(includedLevels);

            // 1A
            var group1a = dataList
                .Where(x => x.ActivationLevel == TraumaTeamActivationLevel.Level1 &&
                            x.ServiceType == PhysicianServiceType.SurgeryTrauma &&
                            (x.Level == TraumaLevel.I || x.Level == TraumaLevel.II))
                .GroupBy(x => x.UniqueIncidentId)
                .Select(g => g.OrderBy(x => x.ResponseTime ?? double.MaxValue).First())
                .ToList();

            var aDenom = group1a.Count(x => x.ResponseTime.HasValue);
            var aNum = group1a.Count(x => x.ResponseTime.HasValue && x.ResponseTime.Value <= 15.0);

            // 1B
            var group1b = dataList
                .Where(x => x.ActivationLevel == TraumaTeamActivationLevel.Level1 &&
                            x.ServiceType == PhysicianServiceType.SurgeryTrauma &&
                            (x.Level == TraumaLevel.I || x.Level == TraumaLevel.II || x.Level == TraumaLevel.III) &&
                            x.ResponseTime.HasValue)
                .GroupBy(x => x.UniqueIncidentId)
                .Select(g => g.OrderBy(x => x.ResponseTime.Value).First())
                .ToList();
            var bDenom = group1b.Count;
            var bNum = group1b.Count(x => x.ResponseTime.Value <= 30.0);

            // 1C
            var group1c = dataList
                .Where(x => x.ActivationLevel == TraumaTeamActivationLevel.Level1 &&
                            x.ServiceType == PhysicianServiceType.SurgeryTrauma &&
                            (x.Level == TraumaLevel.I || x.Level == TraumaLevel.II || x.Level == TraumaLevel.III))
                .GroupBy(x => new { x.UniqueIncidentId, x.ActivationProvider })
                .Select(g => g.First())
                .ToList();
            var cDenom = group1c.Count;
            var cNum = group1c.Count(x => !x.ResponseTime.HasValue);

            // Provider group for 1D, 1E, 1F
            var providerGroup1de = new HashSet<PhysicianServiceType>
            {
                PhysicianServiceType.SurgeryTrauma,
                PhysicianServiceType.EmergencyMedicine,
                PhysicianServiceType.FamilyPractice,
                PhysicianServiceType.NursePractitioner,
                PhysicianServiceType.PhysicianAssistant,
                PhysicianServiceType.SurgerySeniorResident,
                PhysicianServiceType.Hospitalist,
                PhysicianServiceType.InternalMedicine
            };

            var group1de = dataList
                .Where(x => (x.ActivationLevel == TraumaTeamActivationLevel.Level1 || x.ActivationLevel == TraumaTeamActivationLevel.Level2) &&
                            providerGroup1de.Contains(x.ServiceType) &&
                            validLevelsHashSet.Contains(x.Level))
                .GroupBy(x => x.UniqueIncidentId)
                .Select(g => g.OrderBy(x => x.ResponseTime ?? double.MaxValue).First())
                .ToList();

            var deDenom = group1de.Count(x => x.ResponseTime.HasValue);
            var dNum = group1de.Count(x => x.ResponseTime.HasValue && x.ResponseTime.Value <= 5.0);
            var eNum = group1de.Count(x => x.ResponseTime.HasValue && x.ResponseTime.Value <= 20.0);

            var group1f = dataList
                .Where(x => (x.ActivationLevel == TraumaTeamActivationLevel.Level1 || x.ActivationLevel == TraumaTeamActivationLevel.Level2) &&
                            providerGroup1de.Contains(x.ServiceType) &&
                            validLevelsHashSet.Contains(x.Level))
                .GroupBy(x => new { x.UniqueIncidentId, x.ActivationProvider })
                .Select(g => g.First())
                .ToList();

            var fDenom = group1f.Count;
            var fNum = group1f.Count(x => !x.ResponseTime.HasValue);

            return new Indicator1Result
            {
                Indicator1A = CreateRate(aNum, aDenom, ciMethod),
                Indicator1B = CreateRate(bNum, bDenom, ciMethod),
                Indicator1C = CreateRate(cNum, cDenom, ciMethod),
                Indicator1D = CreateRate(dNum, deDenom, ciMethod),
                Indicator1E = CreateRate(eNum, deDenom, ciMethod),
                Indicator1F = CreateRate(fNum, fDenom, ciMethod)
            };
        }

        public static Indicator2Result CalculateIndicator2(IEnumerable<Indicator2Input> data, IEnumerable<TraumaLevel> includedLevels = null, CiMethod ciMethod = CiMethod.None)
        {
            includedLevels = includedLevels ?? new[] { TraumaLevel.I, TraumaLevel.II, TraumaLevel.III, TraumaLevel.IV };
            var validLevelsHashSet = new HashSet<TraumaLevel>(includedLevels);

            var distinctIncidents = data
                .Where(x => validLevelsHashSet.Contains(x.Level))
                .GroupBy(x => x.UniqueIncidentId)
                .Select(g => g.First())
                .ToList();

            return new Indicator2Result
            {
                Indicator2 = CreateRate(distinctIncidents.Count(x => !x.IncidentTime.HasValue), distinctIncidents.Count, ciMethod)
            };
        }

        public static Indicator3Result CalculateIndicator3(IEnumerable<Indicator3Input> data, IEnumerable<TraumaLevel> includedLevels = null, CiMethod ciMethod = CiMethod.None)
        {
            includedLevels = includedLevels ?? new[] { TraumaLevel.I, TraumaLevel.II, TraumaLevel.III, TraumaLevel.IV };
            var validLevelsHashSet = new HashSet<TraumaLevel>(includedLevels);

            var distinctIncidents = data
                .Where(x => validLevelsHashSet.Contains(x.Level) && x.TraumaType != TraumaType.Burn)
                .GroupBy(x => x.UniqueIncidentId)
                .Select(g => g.First())
                .ToList();

            return new Indicator3Result
            {
                Indicator3 = CreateRate(distinctIncidents.Count(x => x.ProbabilityOfSurvival.HasValue), distinctIncidents.Count, ciMethod)
            };
        }

        public static Indicator4Result CalculateIndicator4(IEnumerable<Indicator4Input> data, IEnumerable<TraumaLevel> includedLevels = null, CiMethod ciMethod = CiMethod.None)
        {
            includedLevels = includedLevels ?? new[] { TraumaLevel.I, TraumaLevel.II, TraumaLevel.III, TraumaLevel.IV };
            var validLevelsHashSet = new HashSet<TraumaLevel>(includedLevels);

            var filter4a = data
                .Where(x => validLevelsHashSet.Contains(x.Level) &&
                            (x.EdDisposition == Disposition.DeceasedExpired || x.HospitalDisposition == Disposition.DeceasedExpired))
                .GroupBy(x => x.UniqueIncidentId)
                .Select(g => g.First())
                .ToList();

            var filter4b = data
                .Where(x => validLevelsHashSet.Contains(x.Level) &&
                            (x.EdDisposition == Disposition.DeceasedExpired || x.HospitalDisposition == Disposition.DeceasedExpired) &&
                            ((x.EdLos.HasValue && x.EdLos.Value > 4320) || (x.HospitalLos.HasValue && x.HospitalLos.Value > 4320)))
                .GroupBy(x => x.UniqueIncidentId)
                .Select(g => g.First())
                .ToList();

            return new Indicator4Result
            {
                Indicator4A = CreateRate(filter4a.Count(x => x.Autopsy == YesNo.Yes), filter4a.Count, ciMethod),
                Indicator4B = CreateRate(filter4b.Count(x => x.Autopsy != YesNo.Yes), filter4b.Count, ciMethod)
            };
        }
    }
}

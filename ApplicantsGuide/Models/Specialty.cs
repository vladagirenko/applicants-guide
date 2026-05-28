using System;

namespace ApplicantsGuide.Models
{
    /// <summary>
    /// Спеціальність ВНЗ з балами для денної та заочної форм, бюджет і контракт окремо.
    /// </summary>
    public class Specialty
    {
        
        public string Name { get; set; }

        
        public double FullTimeBudgetScore { get; set; }

        
        public double FullTimeContractScore { get; set; }

        
        public double PartTimeBudgetScore { get; set; }

        
        public double PartTimeContractScore { get; set; }

        
        public decimal ContractCost { get; set; }

       
        public Specialty()
        {
            Name = string.Empty;
        }

        
        public Specialty(
            string name,
            double fullTimeBudgetScore,
            double fullTimeContractScore,
            double partTimeBudgetScore,
            double partTimeContractScore,
            decimal contractCost)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва спеціальності не може бути порожньою.", nameof(name));
            if (fullTimeBudgetScore    < 0) throw new ArgumentOutOfRangeException(nameof(fullTimeBudgetScore));
            if (fullTimeContractScore  < 0) throw new ArgumentOutOfRangeException(nameof(fullTimeContractScore));
            if (partTimeBudgetScore    < 0) throw new ArgumentOutOfRangeException(nameof(partTimeBudgetScore));
            if (partTimeContractScore  < 0) throw new ArgumentOutOfRangeException(nameof(partTimeContractScore));
            if (contractCost           < 0) throw new ArgumentOutOfRangeException(nameof(contractCost));

            Name                  = name;
            FullTimeBudgetScore   = fullTimeBudgetScore;
            FullTimeContractScore = fullTimeContractScore;
            PartTimeBudgetScore   = partTimeBudgetScore;
            PartTimeContractScore = partTimeContractScore;
            ContractCost          = contractCost;
        }
    }
}
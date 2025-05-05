using System;

namespace WisVestAPI.Services
{
    public class InvestmentAmountService
    {
        public double CalculateInvestmentAmount(double percentageSplit, double targetAmount, double annualReturn, string investmentHorizon)
{
    // Parse the investment horizon (e.g., "5 years" -> 5)
    int years = investmentHorizon switch
    {
        "Short" => 2,
        "Moderate" => 5,
        "Long" => 8,
        _ => throw new ArgumentException("Invalid investment horizon value") // Handle invalid input
    };

    // Formula: Investment = (PercentageSplit * TargetAmount) / (1 + AnnualReturn/100)^Years
    double denominator = Math.Pow(1 + (annualReturn / 100), years);
    double investmentAmount = (percentageSplit/100) * targetAmount / denominator;

    return Math.Round(investmentAmount, 2); // Round to 2 decimal places
}
    }
}



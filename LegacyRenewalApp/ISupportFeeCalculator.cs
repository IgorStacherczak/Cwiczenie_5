namespace LegacyRenewalApp
{
    public interface ISupportFeeCalculator
    {
        decimal Calculate(bool includePremiumSupport, string normalizedPlanCode, ref string notes);
    }
}
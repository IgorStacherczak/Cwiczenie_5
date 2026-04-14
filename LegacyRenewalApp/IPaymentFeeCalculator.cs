namespace LegacyRenewalApp
{
    public interface IPaymentFeeCalculator
    {
        decimal Calculate(string normalizedPaymentMethod, decimal subtotalAfterDiscount, decimal supportFee, ref string notes);
    }
}
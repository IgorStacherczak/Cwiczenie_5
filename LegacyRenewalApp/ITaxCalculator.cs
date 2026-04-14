namespace LegacyRenewalApp
{
    public interface ITaxCalculator
    {
        decimal Calculate(Customer customer, decimal subtotalAfterDiscount, decimal supportFee, decimal paymentFee);
    }
}
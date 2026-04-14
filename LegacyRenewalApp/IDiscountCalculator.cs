namespace LegacyRenewalApp
{
    public interface IDiscountCalculator
    {
        DiscountResult Calculate(
            Customer customer,
            SubscriptionPlan plan,
            decimal baseAmount,
            int seatCount,
            bool useLoyaltyPoints);
    }
}
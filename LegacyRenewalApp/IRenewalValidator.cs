namespace LegacyRenewalApp
{
    public interface IRenewalValidator
    {
        void ValidateInput(int customerId, string planCode, int seatCount, string paymentMethod);
        void ValidateCustomer(Customer customer);
    }
}
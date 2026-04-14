namespace LegacyRenewalApp
{
    public class TaxCalculator: ITaxCalculator
    {
        public decimal Calculate(Customer customer, decimal subtotalAfterDiscount, decimal supportFee, decimal paymentFee)
        {
            decimal taxRate = 0.20m;

            if (customer.Country == "Poland")
            {
                taxRate = 0.23m;
            }
            else if (customer.Country == "Germany")
            {
                taxRate = 0.19m;
            }
            else if (customer.Country == "Czech Republic")
            {
                taxRate = 0.21m;
            }
            else if (customer.Country == "Norway")
            {
                taxRate = 0.25m;
            }

            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            return taxBase * taxRate;
        }
    }
}
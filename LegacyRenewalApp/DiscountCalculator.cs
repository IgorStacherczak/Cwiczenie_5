namespace LegacyRenewalApp
{
    public class DiscountCalculator: IDiscountCalculator
    {
        public DiscountResult Calculate(
            Customer customer,
            SubscriptionPlan plan,
            decimal baseAmount,
            int seatCount,
            bool useLoyaltyPoints)
        {
            decimal discountAmount = 0m;
            string notes = string.Empty;

            ApplySegmentDiscount(customer, plan, baseAmount, ref discountAmount, ref notes);
            ApplyYearsDiscount(customer, baseAmount, ref discountAmount, ref notes);
            ApplySeatDiscount(seatCount, baseAmount, ref discountAmount, ref notes);
            ApplyLoyaltyPointsDiscount(customer, useLoyaltyPoints, ref discountAmount, ref notes);

            return new DiscountResult
            {
                DiscountAmount = discountAmount,
                Notes = notes
            };
        }

        private void ApplySegmentDiscount(Customer customer, SubscriptionPlan plan, decimal baseAmount, ref decimal discountAmount, ref string notes)
        {
            if (customer.Segment == "Silver")
            {
                discountAmount += baseAmount * 0.05m;
                notes += "silver discount; ";
            }
            else if (customer.Segment == "Gold")
            {
                discountAmount += baseAmount * 0.10m;
                notes += "gold discount; ";
            }
            else if (customer.Segment == "Platinum")
            {
                discountAmount += baseAmount * 0.15m;
                notes += "platinum discount; ";
            }
            else if (customer.Segment == "Education" && plan.IsEducationEligible)
            {
                discountAmount += baseAmount * 0.20m;
                notes += "education discount; ";
            }
        }

        private void ApplyYearsDiscount(Customer customer, decimal baseAmount, ref decimal discountAmount, ref string notes)
        {
            if (customer.YearsWithCompany >= 5)
            {
                discountAmount += baseAmount * 0.07m;
                notes += "long-term loyalty discount; ";
            }
            else if (customer.YearsWithCompany >= 2)
            {
                discountAmount += baseAmount * 0.03m;
                notes += "basic loyalty discount; ";
            }
        }

        private void ApplySeatDiscount(int seatCount, decimal baseAmount, ref decimal discountAmount, ref string notes)
        {
            if (seatCount >= 50)
            {
                discountAmount += baseAmount * 0.12m;
                notes += "large team discount; ";
            }
            else if (seatCount >= 20)
            {
                discountAmount += baseAmount * 0.08m;
                notes += "medium team discount; ";
            }
            else if (seatCount >= 10)
            {
                discountAmount += baseAmount * 0.04m;
                notes += "small team discount; ";
            }
        }

        private void ApplyLoyaltyPointsDiscount(Customer customer, bool useLoyaltyPoints, ref decimal discountAmount, ref string notes)
        {
            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                discountAmount += pointsToUse;
                notes += $"loyalty points used: {pointsToUse}; ";
            }
        }
    }
}
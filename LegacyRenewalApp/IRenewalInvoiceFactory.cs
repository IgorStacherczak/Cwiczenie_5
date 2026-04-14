namespace LegacyRenewalApp
{
    public interface IRenewalInvoiceFactory
    {
        RenewalInvoice Create(
            Customer customer,
            int customerId,
            string normalizedPlanCode,
            string normalizedPaymentMethod,
            int seatCount,
            decimal baseAmount,
            decimal discountAmount,
            decimal supportFee,
            decimal paymentFee,
            decimal taxAmount,
            decimal finalAmount,
            string notes);
    }
}
using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository customerRepository;
        private readonly ISubscriptionPlanRepository planRepository;
        private readonly IDiscountCalculator discountCalculator;
        private readonly ISupportFeeCalculator supportFeeCalculator;
        private readonly IPaymentFeeCalculator paymentFeeCalculator;
        private readonly ITaxCalculator taxCalculator;
        private readonly IBillingGateway billingGateway;
        private readonly IRenewalInvoiceFactory renewalInvoiceFactory;
        private readonly IRenewalValidator renewalValidator;

        public SubscriptionRenewalService()
            : this(
                new CustomerRepository(),
                new SubscriptionPlanRepository(),
                new DiscountCalculator(),
                new SupportFeeCalculator(),
                new PaymentFeeCalculator(),
                new TaxCalculator(),
                new RenewalInvoiceFactory(),
                new RenewalValidator(),
                new LegacyBillingGatewayAdapter())
        {
        }

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            IDiscountCalculator discountCalculator,
            ISupportFeeCalculator supportFeeCalculator,
            IPaymentFeeCalculator paymentFeeCalculator,
            ITaxCalculator taxCalculator,
            IRenewalInvoiceFactory renewalInvoiceFactory,
            IRenewalValidator renewalValidator,
            IBillingGateway billingGateway)
        {
            this.customerRepository = customerRepository;
            this.planRepository = planRepository;
            this.discountCalculator = discountCalculator;
            this.supportFeeCalculator = supportFeeCalculator;
            this.paymentFeeCalculator = paymentFeeCalculator;
            this.taxCalculator = taxCalculator;
            this.renewalInvoiceFactory = renewalInvoiceFactory;
            this.renewalValidator = renewalValidator;
            this.billingGateway = billingGateway;
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            renewalValidator.ValidateInput(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = customerRepository.GetById(customerId);
            var plan = planRepository.GetByCode(normalizedPlanCode);

            renewalValidator.ValidateCustomer(customer);

            decimal baseAmount = CalculateBaseAmount(plan, seatCount);

            var discountResult = discountCalculator.Calculate(
                customer,
                plan,
                baseAmount,
                seatCount,
                useLoyaltyPoints);

            decimal discountAmount = discountResult.DiscountAmount;
            string notes = discountResult.Notes;

            decimal subtotalAfterDiscount = CalculateSubtotalAfterDiscount(baseAmount, discountAmount, ref notes);
            decimal supportFee = supportFeeCalculator.Calculate(includePremiumSupport, normalizedPlanCode, ref notes);
            decimal paymentFee = paymentFeeCalculator.Calculate(normalizedPaymentMethod, subtotalAfterDiscount, supportFee, ref notes);
            decimal taxAmount = taxCalculator.Calculate(customer, subtotalAfterDiscount, supportFee, paymentFee);
            decimal finalAmount = CalculateFinalAmount(subtotalAfterDiscount, supportFee, paymentFee, taxAmount, ref notes);

            var invoice = renewalInvoiceFactory.Create(
                customer,
                customerId,
                normalizedPlanCode,
                normalizedPaymentMethod,
                seatCount,
                baseAmount,
                discountAmount,
                supportFee,
                paymentFee,
                taxAmount,
                finalAmount,
                notes);

            billingGateway.SaveInvoice(invoice);
            SendInvoiceEmail(customer, normalizedPlanCode, invoice);

            return invoice;
        }

        private decimal CalculateBaseAmount(SubscriptionPlan plan, int seatCount)
        {
            return (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
        }

        private decimal CalculateSubtotalAfterDiscount(decimal baseAmount, decimal discountAmount, ref string notes)
        {
            decimal subtotalAfterDiscount = baseAmount - discountAmount;

            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            return subtotalAfterDiscount;
        }

        private decimal CalculateFinalAmount(decimal subtotalAfterDiscount, decimal supportFee, decimal paymentFee, decimal taxAmount, ref string notes)
        {
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            return finalAmount;
        }

        private void SendInvoiceEmail(Customer customer, string normalizedPlanCode, RenewalInvoice invoice)
        {
            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                return;
            }

            string subject = "Subscription renewal invoice";
            string body =
                $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

            billingGateway.SendEmail(customer.Email, subject, body);
        }
    }
}
namespace PaymentsAPI.Models
{
    public class Case
    {
        public int TotalPayments
        {
            get
            {
                return TotalPayments1();
            }
        }

        

        public int AmountDue
        {
            get
            {
                var _total = 0;
                foreach (var serviceRequest in ServiceRequests)
                {
                    _total += serviceRequest.AmountDue;
                }
                return _total;
            }
        }

        public int OverPayment
        {
            get
            {
                var _total = 0;
                foreach (var serviceRequest in ServiceRequests)
                {
                    _total += serviceRequest.OverPayment;
                }
                return _total;
            }
        }
        public int TotalRemission
        {
            get
            {
                return TotalRemissions1();
            }
        }


        public Case()
        {
            ServiceRequests = new List<ServiceRequest>();
        }
        public string CaseId { get; set; }

        public string Name { get; set; }

        public List<ServiceRequest> ServiceRequests { get; set; }

        private int TotalPayments1()
        {
            var total = 0;
            foreach (var serviceRequest in ServiceRequests)
            {
                foreach (var payment in serviceRequest.Payments)
                {
                    total += payment.Amount;
                }
            }
            return total;

        }

        private int TotalRemissions1()
        {
            var total = 0;
            foreach (var serviceRequest in ServiceRequests)
            {
                foreach (var fee in serviceRequest.Fees)
                {
                    if (fee.Remissiom.Discount > 0)
                    {
                        total += fee.Remissiom.Discount;
                    }
                   
                }
            }
            return total;

        }

        private int AmountDue1()
        {
            var total = 0;
            foreach (var serviceRequest in ServiceRequests)
            {
                total += serviceRequest.InvoiceAmount - serviceRequest.Payments.Sum(p => p.Amount);
            }
            return total;
        }
    }
}




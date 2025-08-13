using PaymentsAPI.Models;

namespace PaymentsAPI.DataLayer
{
    public class StaticData
    {
        public List<Case> CaseList;


        public StaticData()
        {
            Init();
        }

        public void Init()
        {
            CaseList = new List<Case>
            {
                new Case { 
                    CaseId = "1754-5791-4808-7001", 
                    Name = "Scenario 7: Retro remission with a calculated refund",
                    ServiceRequests = new List<ServiceRequest>
                    {
                        new ServiceRequest
                        {
                            Reference = "SR7",
                            Fees = new List<Fees>
                            {
                                new Fees { Code = "F701", GrossAmount = 300 },
                                new Fees { Code = "F702", GrossAmount = 50 }
                            }
                        }
                    }
                },

                // Scenario 8A: 1 Service Requests  there would be no amount due or overpayment
                new Case { 
                    CaseId = "1754-5791-4808-7002", 
                    Name = "Scenario 8-A: 1 Service Requests  there would be no amount due or overpayment",
                    ServiceRequests = new List<ServiceRequest>
                    {
                        new ServiceRequest
                        {
                            Reference = "SR8A1",
                            Fees = new List<Fees>
                            {
                                new Fees { Code = "F801", GrossAmount = 300 },
                                new Fees { Code = "F802", GrossAmount = 50 }
                            }
                        }
                    }
                },

                // Scenario 8B: 2 Service Requests there would be an amount due and overpayment
                new Case { 
                    CaseId = "1754-5791-4808-7003", 
                    Name = "Scenario 8-B: 2 Service Requests there would be an amount due and overpayment",
                    ServiceRequests = new List<ServiceRequest>
                    {
                        new ServiceRequest
                        {
                            Reference = "SR8B1",
                            Fees = new List<Fees>
                            {
                                new Fees { Code = "F811", GrossAmount = 300 }
                            }
                        },
                        new ServiceRequest
                        {
                            Reference = "SR8B2",
                            Fees = new List<Fees>
                            {
                                new Fees { Code = "F812", GrossAmount = 50 }
                            }
                        }
                    }
                },

                // Scenario 9: There would be an amount due and overpayment
                new Case { 
                    CaseId = "1754-5791-4808-7004", 
                    Name = "Scenario 9: There would be an amount due and overpayment",
                    ServiceRequests = new List<ServiceRequest>
                    {
                        new ServiceRequest
                        {
                            Reference = "SR9A",
                            Fees = new List<Fees>
                            {
                                new Fees { Code = "F911", GrossAmount = 612 },
                                new Fees { Code = "F912", GrossAmount = 45 }
                            }
                        },
                        new ServiceRequest
                        {
                            Reference = "SR9B",
                            Fees = new List<Fees>
                            {
                                new Fees { Code = "F922", GrossAmount = 300 }
                            }
                        },
                        new ServiceRequest
                        {
                            Reference = "SR9C",
                            Fees = new List<Fees>
                            {
                                new Fees { Code = "F931", GrossAmount = 50 }
                            }
                        }
                    }
                },

                // Scenario 10: 3 Retro remissions - the system will not allow more than 1 remission and therefore each retro remission will need to be added and refunded before adding the next 
                new Case { 
                    CaseId = "1754-5791-4808-7005", 
                    Name = "Scenario 10: 3 Retro remissions - the system will not allow more than 1 remission and therefore each retro remission will need to be added and refunded before adding the next",
                    ServiceRequests = new List<ServiceRequest>
                    {
                        new ServiceRequest
                        {
                            Reference = "SR10A",
                            Fees = new List<Fees>
                            {
                                new Fees { Code = "F1000", GrossAmount = 612 },
                                new Fees { Code = "F1000", GrossAmount = 50 },
                                new Fees { Code = "F1000", GrossAmount = 300 }
                            }
                        }
                    }
                }
            };
        }

        public void AddPayment(string caseId, string sr, int paymentAmount)
        {
            var case1 = CaseList.FirstOrDefault(c => c.CaseId == caseId);
            if (case1 != null)
            {
                var serviceRequest = case1.ServiceRequests.FirstOrDefault(s => s.Reference == sr);
                if (serviceRequest != null && serviceRequest.CanPay)
                {
                    var paymentInstruction = new PaymentInstruction
                    {
                        Reference = "PAY" + DateTime.Now.Ticks,
                        PaymentMethod = "Online",
                        Amount = paymentAmount,
                        Status = "Success"
                    };
                    serviceRequest.Payments.Add(paymentInstruction);
                    CreateApportionment(paymentAmount, serviceRequest, paymentInstruction);
                }
            }

        }

        private void CreateApportionment(int paymentAmount, ServiceRequest serviceRequest, PaymentInstruction paymentInstruction)
        {
            foreach (var fee in serviceRequest.Fees)
            {
                if (paymentAmount <= 0)
                    break;
                if (fee.AmountApportioned == fee.GrossAmount)
                    continue;

                var balance = fee.GrossAmount - fee.AmountApportioned;

               if( fee == serviceRequest.Fees.Last())
                {
                    fee.ApportionPayment(paymentInstruction, paymentAmount);
                    continue;
                }
               if (paymentAmount >= balance)
               {
                    fee.ApportionPayment(paymentInstruction, balance);
                    paymentAmount -= balance;
               }
                else
                {
                    fee.ApportionPayment(paymentInstruction, paymentAmount);
                    paymentAmount = 0;
                }
            }
        }

        private static void CreateApportionment2(int paymentAmount, ServiceRequest serviceRequest, PaymentInstruction paymentInstruction)
        {
            var apportionAmount = paymentAmount;

            if (serviceRequest.Fees.Count == 1)
            {
                serviceRequest.Fees[0].ApportionPayment(paymentInstruction, paymentAmount);
            }
            else
            {
                foreach (var fee in serviceRequest.Fees)
                {
                    
                    if (fee.GrossAmount < apportionAmount)
                    {
                        fee.ApportionPayment(paymentInstruction, fee.GrossAmount);
                        
                    }
                    else
                    {
                        fee.ApportionPayment(paymentInstruction, apportionAmount);

                    }
                    apportionAmount = apportionAmount - fee.GrossAmount;

                }
            }
        }

        public void AddDiscount(string caseId, string sr, string feeCode, int discountAmount)
        {
            var case1 = CaseList.FirstOrDefault(c => c.CaseId == caseId);
            if (case1 != null)
            {
                var serviceRequest = case1.ServiceRequests.FirstOrDefault(s => s.Reference == sr);
                if (serviceRequest != null)
                {
                    var fee = serviceRequest.Fees.FirstOrDefault(f => f.Code == feeCode);
                    if (fee != null && discountAmount > 0 && discountAmount <= fee.GrossAmount)
                    {

                        fee.Remissiom = new HelpWithFees
                        {
                            Reference = "HWF-" + DateTime.Now.Ticks,
                            Discount = discountAmount,
                        };
                        
                    }
                }
            }

        }

        public void AddServiceRequest(string caseId, FeeItem feeItem)
        {
            var case1 = CaseList.FirstOrDefault(c => c.CaseId == caseId);
            if (case1 != null)
            {
                var newServiceRequest = new ServiceRequest
                {
                    Reference = "SR-" + DateTime.Now.Ticks,
                    Fees = new List<Fees>
                    {
                        new Fees { Code = feeItem.Code, GrossAmount = feeItem.Amount }
                    }
                };
                case1.ServiceRequests.Add(newServiceRequest);
            }
        }

        public void AddServiceRequest(string caseId, List<FeeItem> selectedFees)
        {
            var case1 = CaseList.FirstOrDefault(c => c.CaseId == caseId);
            if (case1 != null)
            {
                var newServiceRequest = new ServiceRequest
                {
                    Reference = "SR-" + DateTime.Now.Ticks,
                    Fees = selectedFees.Select(f => new Fees { Code = f.Code, GrossAmount = f.Amount }).ToList()
                };
                case1.ServiceRequests.Add(newServiceRequest);
            }
        }

        
    }
}

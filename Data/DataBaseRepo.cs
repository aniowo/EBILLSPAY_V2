using EBILLSPAY_V2.Controllers;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Hosting;
using static EBILLSPAY_V2.Models.ApiModels;

namespace EBILLSPAY_V2.Data
{
    public class DataBaseRepo
    {
        private readonly Data.ApplicationDbContext _db;
        private readonly ILogger<DataBaseRepo> _logger;

        public DataBaseRepo(Data.ApplicationDbContext db, ILogger<DataBaseRepo> logger)
        {
            _db = db;
            _logger = logger;

        }

        public bool Approve(long trandId, string status, string sessionId, string approval) 
        {
            bool update = false;
            try
            {
                
                var dbUpdate = _db.EbillsPay.Find(trandId);
                if (dbUpdate != null)
                {
                    dbUpdate.CurrApprStatus = "APPROVED";
                    dbUpdate.TransStatus = status;
                    dbUpdate.SessionId = sessionId;
                    dbUpdate.ApprovedBy = approval;
                    _db.SaveChanges();
                    update = true;
                    _logger.LogInformation("Table has been Updated Successfully");
                }

            }
            catch (Exception ex)
            {
                update = false;
                _logger.LogError(ex, "Table was not Updated Successfully");
            }


            return update;
        }

        public bool Approve(long trandId, string approval)
        {
            bool update = false;
            try
            {

                var dbUpdate = _db.EbillsPay.Find(trandId);
                if (dbUpdate != null)
                {
                    dbUpdate.CurrApprStatus = "APPROVED";
                    dbUpdate.TransStatus = "FAILED";
                    dbUpdate.ApprovedBy = approval;
                    _db.SaveChanges();
                    update = true;
                    _logger.LogInformation("Table has been Updated Successfully");
                }

            }
            catch (Exception ex)
            {
                update = false;
                _logger.LogError(ex, "Table was not Updated Successfully");
            }


            return update;
        }

        public bool Reject(long trandId, string approval)
        {
            bool update = false;
            try
            {

                var dbUpdate = _db.EbillsPay.Find(trandId);
                if (dbUpdate != null)
                {
                    dbUpdate.CurrApprStatus = "REJECTED";
                    dbUpdate.TransStatus = "FAILED";
                    dbUpdate.ApprovedBy = approval;
                    _db.SaveChanges();
                    update = true;
                    _logger.LogInformation("Table has been Updated Successfully");
                }

            }
            catch (Exception ex)
            {
                update = false;
                _logger.LogError(ex, "Table was not Updated Successfully");
            }


            return update;
        }

        public bool AddPostTransaction(string debitAcct, int tellerId, string branchCode, string initiator, string transStatus, string sessionId, string productId, string productName, decimal amount, decimal fee, string collAcct, string bankName, string billerId, string billerName, string fieldList)
        {
            bool update = false;
            try
            {
                Data.EbillsPay ebills = new Data.EbillsPay();
                ebills.AccountToDebit = debitAcct;
                ebills.TellerID = tellerId;
                ebills.BranchCode = branchCode;
                ebills.InitiatedBy = initiator;
                ebills.TransStatus = transStatus;
                ebills.TransDate = DateTime.Now.ToString();
                ebills.SessionId = sessionId;
                ebills.ProductId = productId;
                ebills.ProductName = productName;
                ebills.Amount = amount;
                ebills.Fee = fee;
                ebills.CollectionAccNumber = collAcct;
                ebills.BankName = bankName;
                ebills.BillerId = billerId;
                ebills.BillerName = billerName;
                ebills.FieldList = fieldList;

                var dbUpdate = _db.EbillsPay.Add(ebills);
               
                if (dbUpdate != null)
                {
                   
                    _db.SaveChanges();
                    update = true;
                    _logger.LogInformation("Table has been Updated Successfully");
                }

            }
            catch (Exception ex)
            {
                update = false;
                _logger.LogError(ex, "Table was not Updated Successfully");
            }


            return update;
        }

        public bool AddPostTransaction(string debitAcct, int tellerId, string branchCode, string initiator, string transStatus, string productId, string productName, decimal amount, decimal fee, string collAcct, string bankName, string billerId, string billerName, string fieldList)
        {
            bool update = false;
            try
            {
                Data.EbillsPay ebills = new Data.EbillsPay();
                ebills.AccountToDebit = debitAcct;
                ebills.TellerID = tellerId;
                ebills.BranchCode = branchCode;
                ebills.InitiatedBy = initiator;
                ebills.TransStatus = transStatus;
                ebills.TransDate = DateTime.Now.ToString();
                ebills.ProductId = productId;
                ebills.ProductName = productName;
                ebills.Amount = amount;
                ebills.Fee = fee;
                ebills.CollectionAccNumber = collAcct;
                ebills.BankName = bankName;
                ebills.BillerId = billerId;
                ebills.BillerName = billerName;
                ebills.FieldList = fieldList;

                var dbUpdate = _db.EbillsPay.Add(ebills);

                if (dbUpdate != null)
                {

                    _db.SaveChanges();
                    update = true;
                    _logger.LogInformation("Table has been Updated Successfully");
                }

            }
            catch (Exception ex)
            {
                update = false;
                _logger.LogError(ex, "Table was not Updated Successfully");
            }


            return update;
        }


        public bool AddPendingTransaction(string debitAcct, int tellerId, string branchCode, string initiator, string transStatus, string CurrApprStatus, string productId, string productName, decimal amount, decimal fee, string collAcct, string bankName, string billerId, string billerName, string fieldList)
        {
            bool update = false;
            try
            {
                Data.EbillsPay ebills = new Data.EbillsPay();
                ebills.AccountToDebit = debitAcct;
                ebills.TellerID = tellerId;
                ebills.BranchCode = branchCode;
                ebills.InitiatedBy = initiator;
                ebills.TransStatus = transStatus;
                ebills.TransDate = DateTime.Now.ToString();
                ebills.CurrApprStatus = CurrApprStatus;
                ebills.ProductId = productId;
                ebills.ProductName = productName;
                ebills.Amount = amount;
                ebills.Fee = fee;
                ebills.CollectionAccNumber = collAcct;
                ebills.BankName = bankName;
                ebills.BillerId = billerId;
                ebills.BillerName = billerName;
                ebills.FieldList = fieldList;

                var dbUpdate = _db.EbillsPay.Add(ebills);

                if (dbUpdate != null)
                {
                    _db.SaveChanges();
                    update = true;
                    _logger.LogInformation("Table has been Updated Successfully");
                }

            }
            catch (Exception ex)
            {
                update = false;
                _logger.LogError(ex, "Table was not Updated Successfully");
            }


            return update;
        }
    }
    
}

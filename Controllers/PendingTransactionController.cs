//using EBILLSPAY_V2.Data;
using EBILLSPAY_V2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using static EBILLSPAY_V2.Models.ApiModels;
using JsonSerializer = System.Text.Json.JsonSerializer;
using EBILLSPAY_V2.Data;

namespace EBILLSPAY_V2.Controllers
{
    public class PendingTransactionController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EbillsPay _ebills;
        private readonly IConfiguration _configuration;
        private readonly Data.ApplicationDbContext _db;
        private readonly Data.DataBaseRepo _repo;


        public PendingTransactionController(ILogger<HomeController> logger, EbillsPay ebills, IConfiguration configuration, Data.ApplicationDbContext db, DataBaseRepo repo)
        {
            _logger = logger;
            _ebills = ebills;
            _configuration = configuration;
            _db = db;
            _repo = repo;
        }

        public IActionResult Index()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RetrieveTransaction get)
        {
            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("HeadBranchCode", get.BranchCode);
                if (get.BranchCode.Length != 3)
                {
                    ModelState.AddModelError("BranchCode", "Branch Code can not be more/less than three(3)");
                }
                else
                {
                    return RedirectToAction("Pending");
                }
            }

            string roleId = HttpContext.Session.GetString("RoleId");
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            return View(get);

        }
        public IActionResult Pending()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            var branchCode = HttpContext.Session.GetString("HeadBranchCode");
            var objPendingList = _db.EbillsPay
                                        .Where(p => p.BranchCode == branchCode && p.TransStatus == "PENDING")
                                        .ToList();


            byte[] byteArray2 = JsonSerializer.SerializeToUtf8Bytes(objPendingList);
            HttpContext.Session.Set("PendingList", byteArray2);


            bool hasRecords = objPendingList.Count > 0;

            List<PendingTransaction> objectList = new List<PendingTransaction>();

            foreach (var pending in objPendingList)
            {
                PendingTransaction transaction = new PendingTransaction();
                transaction.billerProductId = pending.ProductId;
                transaction.productName = pending.ProductName;
                transaction.billerId = pending.BillerId;
                transaction.billerName = pending.BillerName;
                transaction.amount = pending.Amount;
                transaction.TransId = pending.TransId;
                transaction.debitAccountNumber = pending.AccountToDebit;

                objectList.Add(transaction);
            }

            ViewBag.HasRecords = hasRecords;

            IEnumerable<PendingTransaction> obj = objectList.ToList();

            return View(obj);
        }

        public IActionResult Approve(long? tranId)
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            if (tranId == null)
            {
                TempData["error"] = "Unable to Approve Transaction. Please Try Again Later";
            }

            byte[] storedByteArray = HttpContext.Session.Get("PendingList");
            List<Data.EbillsPay> retrievedList = JsonSerializer.Deserialize<List<Data.EbillsPay>>(storedByteArray);
  
            PendingTransaction approve = new PendingTransaction();

            foreach (var trans in retrievedList)
            {
                if (tranId == trans.TransId)
                {
                   approve.TransId = trans.TransId;
                   approve.debitAccountNumber = trans.AccountToDebit;
                   approve.billerId = trans.BillerId;
                   approve.billerName = trans.BillerName;
                   approve.billerProductId = trans.ProductId;
                   approve.productName = trans.ProductName;
                   approve.amount = trans.Amount;
                   HttpContext.Session.SetString("Fee", trans.Fee.ToString());
                    break;
                }
                else
                {
                    continue;
                }
            }

           



            return View(approve);

        }

        [HttpPost]
        public IActionResult Approve(PendingTransaction post)
        {
            try
            {
                string email = "";
                TransactionRequest payReq = new TransactionRequest();
                if (ModelState.IsValid)
                {
                    payReq.authenticated_InstitutionId = _configuration.GetConnectionString("auth_InstitutionId");
                    payReq.authenticated_UserEmail = _configuration.GetConnectionString("Email");
                    payReq.authenticated_UserId = _configuration.GetConnectionString("auth_UID");
                    payReq.branchId = _configuration.GetConnectionString("ebills_BranchID");
                    payReq.billerProductId = post.billerProductId;
                    payReq.billerId = post.billerId;
                    payReq.transactionChannel = "Web";
                    payReq.channel = _configuration.GetConnectionString("Channel");
                    payReq.billerName = post.billerName;
                    payReq.amount = Convert.ToDouble(post.amount);
                    payReq.fee = Convert.ToDouble(HttpContext.Session.GetString("Fee"));
                    payReq.debitAccountNumber = post.debitAccountNumber;
                    payReq.productName = post.productName;

                    HttpContext.Session.SetString("AccountNumber", post.debitAccountNumber);
                    HttpContext.Session.SetString("BillerName", post.billerName);
                    HttpContext.Session.SetString("ProductName", post.productName);
                    HttpContext.Session.SetString("TransactionAmount", post.amount.ToString());


                    byte[] storedByteArray2 = HttpContext.Session.Get("PendingList");
                    List<Data.EbillsPay> retrievedList = JsonSerializer.Deserialize<List<Data.EbillsPay>>(storedByteArray2);

                    PendingTransaction approve = new PendingTransaction();

                    foreach (var trans in retrievedList)
                    {
                        if (post.TransId == trans.TransId)
                        {
                            payReq.productfields = JsonSerializer.Deserialize<List<Productfield>>(trans.FieldList); ;
                        }
                    }

                    string request = JsonConvert.SerializeObject(payReq);

                    TransactionResponse TransResp = new TransactionResponse();
                    TransResp = _ebills.MakePayment(payReq);

                    if (TransResp.code == -1 && TransResp.hasErrors)
                    {
                        bool dbUpdate = _repo.Approve(post.TransId, HttpContext.Session.GetString("muserid"));
                        if (dbUpdate)
                        {
                            TempData["error"] = "Transaction Failed. Please Try Again Later";
                            return RedirectToAction("Failed");
                        }
                        else
                        {
                            TempData["error"] = "Error. Please Try Again Later";
                        }

                    }
                    else if (TransResp.description.ToUpper() == "SUCCESS" && !TransResp.hasErrors)
                    {
                        HttpContext.Session.SetString("SessionId", TransResp.payload);
                        bool dbUpdate = _repo.Approve(post.TransId, TransResp.description.ToUpper(), TransResp.payload, HttpContext.Session.GetString("muserid"));
                        if (dbUpdate)
                        {
                            TempData["success"] = "Transaction Successful";
                            return RedirectToAction("Success");
                        }
                        else
                        {
                            TempData["error"] = "Error Occurred. Please Try Again Later";
                        }
                    }
                    else
                    {
                        bool dbUpdate = _repo.Approve(post.TransId, HttpContext.Session.GetString("muserid"));
                        if (dbUpdate)
                        {
                            TempData["error"] = "Unable to Complete Transactions. Please Try Again Later";
                            return RedirectToAction("Failed");
                        }
                        else
                        {
                            TempData["error"] = "Error Occurred. Please Try Again Later";
                        }
                    }
         
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occured while Approving Transaction");
                TempData["error"] = "An Error Occured while Approving Transaction";
                return View(post);
            }
           
            return View(post);

        }

        public IActionResult Reject(long? tranId)
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            if (tranId == null)
            {
                TempData["error"] = "Unable to Reject Transaction. Please Try Again Later";
            }

            byte[] storedByteArray = HttpContext.Session.Get("PendingList");
            List<Data.EbillsPay> retrievedList = JsonSerializer.Deserialize<List<Data.EbillsPay>>(storedByteArray);

            PendingTransaction reject = new PendingTransaction();

            foreach (var trans in retrievedList)
            {
                if (tranId == trans.TransId)
                {
                    reject.TransId = trans.TransId;
                    reject.debitAccountNumber = trans.AccountToDebit;
                    reject.billerId = trans.BillerId;
                    reject.billerName = trans.BillerName;
                    reject.billerProductId = trans.ProductId;
                    reject.productName = trans.ProductName;
                    reject.amount = trans.Amount;
                    break;
                }
                else
                {
                    continue;
                }
            }

            return View(reject);

        }

        [HttpPost]
        public IActionResult Reject(PendingTransaction post)
        {

            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("AccountNumber", post.debitAccountNumber);
                HttpContext.Session.SetString("BillerName", post.billerName);
                HttpContext.Session.SetString("ProductName", post.productName);
                HttpContext.Session.SetString("TransactionAmount", post.amount.ToString());

                bool dbUpdate = _repo.Reject(post.TransId, HttpContext.Session.GetString("muserid"));
                if (dbUpdate)
                {
                    TempData["error"] = "Transaction has been Rejected";
                    return RedirectToAction("Failed");
                }
                else
                {
                    TempData["error"] = "Error Occurred. Please Try Again Later";
                }
            }

            return View(post);

        }

        public IActionResult Success()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            TransactionStatus status = new TransactionStatus();
            status.Nuban = HttpContext.Session.GetString("AccountNumber");
            status.BillerName = HttpContext.Session.GetString("BillerName");
            status.ProductName = HttpContext.Session.GetString("ProductName");
            status.Amount = HttpContext.Session.GetString("TransactionAmount");

            return View(status);
        }

        public IActionResult Failed()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            TransactionStatus status = new TransactionStatus();
            status.Nuban = HttpContext.Session.GetString("AccountNumber");
            status.BillerName = HttpContext.Session.GetString("BillerName");
            status.ProductName = HttpContext.Session.GetString("ProductName");
            status.Amount = HttpContext.Session.GetString("TransactionAmount");

            return View(status);
        }


        [HttpPost]
        public IActionResult Print()
        {
            ReceiptResponse receipt = new ReceiptResponse();
            receipt = _ebills.GenerateReceipt(HttpContext.Session.GetString("SessionId"));
            if (receipt == null)
            {
                TempData["error"] = "Error Occured while Printing Receipt";
                return View("Index");
            }

            if (receipt.code == 0)
            {
                TempData["error"] = "Print Receipt is Unavailable, Please Try Again Later";
                return View("Index");
            }

            if (!string.IsNullOrEmpty(receipt.description))
            {
                if (receipt.description != "Pdf Generation Successful." || receipt.code != 1)
                {
                    TempData["error"] = receipt.description;
                    return View("Index");
                }

            }

            if (string.IsNullOrEmpty(receipt.payload.base64String))
            {
                TempData["error"] = "Print Receipt is Unavailable, Please Try Again Later";
                return View("Index");
            }

            else
            {

                byte[] receiptBytes = Convert.FromBase64String(receipt.payload.base64String);
                string fileName = receipt.payload.name;
                string contentType = "application/octet-stream";

                Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
                Response.Headers.Add("Content-Type", contentType);
                TempData["success"] = "Receipt Printed Successfully";
                return File(receiptBytes, contentType);
            }
        }


    }
}
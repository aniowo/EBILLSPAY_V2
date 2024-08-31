using EBILLSPAY_V2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static EBILLSPAY_V2.Models.ApiModels;

namespace EBILLSPAY_V2.Controllers
{
    public class TransactionHistoryController : Controller
    {
        private readonly EbillsPay _ebills;
        private readonly ILogger<TransactionHistoryController> _logger;
        private readonly IConfiguration _configuration;


        public TransactionHistoryController(EbillsPay ebills, ILogger<TransactionHistoryController> logger, IConfiguration configuration)
        {
            _ebills = ebills;
            _logger = logger;
            _configuration = configuration;
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
        public IActionResult Index(GetTransaction get)
        {
            
                try
                {
                    if (ModelState.IsValid)
                    {
                        int nuban = 0;
                        if (get.Nuban.Length != 10)
                        {
                            ModelState.AddModelError("Nuban", "Account Number(NUBAN) is Invalid");
                        }
                        if (Convert.ToDateTime(get.StartDate) > DateTime.Today)
                        {
                            ModelState.AddModelError("StartDate", "Start Date must not be greater than the current date.");

                        }
                        if (Convert.ToDateTime(get.EndDate) > DateTime.Today)
                        {
                            ModelState.AddModelError("EndDate", "End Date must not be greater than the current date.");

                        }
                        if (Convert.ToDateTime(get.StartDate) > Convert.ToDateTime(get.EndDate))
                        {
                            ModelState.AddModelError("StartDate", "Start Date cannot be greater than End Date.");

                        }

                        if (!int.TryParse(get.Nuban, out nuban))
                        {

                            ModelState.AddModelError("Nuban", "Incorrect String Format");
                        }

                        var history = _ebills.GetTransHistory(get.Nuban, Convert.ToDateTime(get.StartDate), Convert.ToDateTime(get.EndDate));

                        if (history != null)
                        {
                            if (history.responseCode == "00")
                            {
                                HttpContext.Session.SetString("ApiData", JsonConvert.SerializeObject(history));
                                TempData["success"] = "Transaction History Retrieved Successfully";
                                return RedirectToAction("Retrieve");
                            }
                            else
                            {
                                TempData["error"] = "An Error Occured while Retrieving Transaction History";
                                return View(get);
                            }

                        }
                    }
   
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "An Error Occured while Retrieving Transaction History");
                    TempData["error"] = "An Error Occured while Retrieving Transaction History";
                    return View(get);
                }
          
               return View(get);
        }

        public IActionResult Retrieve()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            var apiDataString = HttpContext.Session.GetString("ApiData");
            var history = JsonConvert.DeserializeObject<HistoryResponse>(apiDataString);
            List<TransactionHistory> objectList = new List<TransactionHistory>();
            foreach (var transactionHistory in history.transactionHistory)
            {
                TransactionHistory transaction = new TransactionHistory();
                transaction.SessionId = transactionHistory.requestid;
                transaction.Narration = transactionHistory.narration;
                transaction.Originator = transactionHistory.originatorAccountNumber;
                transaction.Amount = transactionHistory.amount;
                transaction.Beneficiary = transactionHistory.beneficiaryAccountNumber;
                transaction.Description = transactionHistory.transDescription;
                transaction.DateInitiated = transactionHistory.dateInitiated;

                objectList.Add(transaction);
            }
            IEnumerable<TransactionHistory> obj = objectList.ToList();

            return View(obj);

        }


        public IActionResult Print(string? sessionId)
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            if (sessionId == null)
            {
                TempData["error"] = "Unable to Print Receipt";
            }
            var apiDataString = HttpContext.Session.GetString("ApiData");
            var history = JsonConvert.DeserializeObject<HistoryResponse>(apiDataString);

            PrintReceipt print = new PrintReceipt();

            foreach (var transactionHistory in history.transactionHistory)
            {
                if (sessionId.ToString() == transactionHistory.requestid)
                {

                    print.SessionId = transactionHistory.requestid;
                    print.Narration = transactionHistory.narration;
                    print.Amount = transactionHistory.amount;
                    print.DateInitiated = transactionHistory.dateInitiated.ToString();
                    break;
                }
                else
                {
                    continue;
                }             
            }

            return View(print);

        }





        [HttpPost]
        public IActionResult Print(PrintReceipt print)
        {

            ReceiptResponse receipt = new ReceiptResponse();
            receipt = _ebills.GenerateReceipt(print.SessionId);
            if (receipt == null)
            {
                TempData["error"] = "Error Occured while Printing Receipt";
                return View("Print");
            }

            if(receipt.code == 0)
            {
                TempData["error"] = "Print Receipt is Unavailable, Please Try Again Later";
                return View("Print");
            }

            if (!string.IsNullOrEmpty(receipt.description))
            {
                if (receipt.description != "Pdf Generation Successful." || receipt.code != 1)
                {
                    TempData["error"] = receipt.description;
                    return View("Print");
                }

            }

            if (string.IsNullOrEmpty(receipt.payload.base64String))
            {
                TempData["error"] = "Print Receipt is Unavailable, Please Try Again Later";
                return View("Print");
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

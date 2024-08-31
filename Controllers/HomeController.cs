using EBILLSPAY_V2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using static EBILLSPAY_V2.Models.ApiModels;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Drawing;
using EBILLSPAY_V2.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace EBILLSPAY_V2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EbillsPay _ebills;
        private readonly Authentication _auth;
        private readonly Utilities _utils;
        private readonly IConfiguration _configuration;
        private readonly Data.ApplicationDbContext _db;
        private readonly Data.DataBaseRepo _repo;
        private Customformfield[] productList;

        public HomeController(ILogger<HomeController> logger, EbillsPay ebills, Utilities utilities, IConfiguration configuration, Authentication authentication, Data.ApplicationDbContext db, DataBaseRepo repo)
        {
            _logger = logger;
            _ebills = ebills;
            _utils = utilities;
            _configuration = configuration;
            _auth = authentication;
            _db = db;
            _repo = repo;
        }

        public async Task<ActionResult> Index()
        {
            string uid, pass, ucode = string.Empty;
            try
            {
                if (Request.Form.Count > 0)
                {
                    _logger.LogInformation("HTTP Request Count is " + Request.Form.Count);
                    uid = Request.Form["uid"];
                    pass = Request.Form["upass"];
                    ucode = Request.Form["ucode"];

                    UserInfo sValue = new UserInfo();

                    sValue = await _auth.Authenticate(sValue, uid, pass, true, ucode);
                    if (sValue != null)
                    {
                        HttpContext.Session.SetString("UserName", sValue.userId);
                        HttpContext.Session.SetString("LastLoginDate", sValue.lastLoginDate);
                        HttpContext.Session.SetString("museremail", sValue.muserEmail);
                        HttpContext.Session.SetString("muserid", sValue.muserName);
                        HttpContext.Session.SetString("RoleId", sValue.mroleId);
                        HttpContext.Session.SetString("BranchCode", sValue.mbranchCode);
                        ViewBag.RoleId = sValue.mroleId;
                        ViewBag.UserName = sValue.muserName;
                        _logger.LogInformation("Login Successfully Completed");

                        string mroleId = sValue.mroleId;
                        if (mroleId != _configuration.GetConnectionString("RoleId"))
                        {
                            return RedirectToAction("ValidateAccount");
                        }
                        else
                        {
                            return RedirectToAction("Index", "PendingTransaction");
                        }
                        
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error While Accessing Application");
                return View();
            }
            return View();
        }

        public IActionResult ValidateAccount()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if (string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ValidateAccount(ValidateAccount validate)
        {
            string roleId = string.Empty;
           
            try
            {
                if (ModelState.IsValid)
                {
                    HttpContext.Session.SetString("AccountNumber", validate.Nuban);
                    int nuban = 0;
                    if (validate.Nuban.Length != 10)
                    {
                        ModelState.AddModelError("Nuban", "Account Number(NUBAN) is Invalid");
                    }

                    if (!int.TryParse(validate.Nuban, out nuban))
                    {

                        ModelState.AddModelError("Nuban", "Incorrect String Format");
                    }
                    string userid = _utils.GetUserId(validate.Nuban);
                    if (userid == "-2")
                    {
                        TempData["error"] = "Account Number is Invalid or not a GTBank Account";
                        roleId = HttpContext.Session.GetString("RoleId");
                        ViewBag.RoleId = roleId;
                        ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
                        return View();
                    }
                    else if (userid == "-1")
                    {
                        TempData["error"] = "An Error Ocurred. Please try again Later";
                        roleId = HttpContext.Session.GetString("RoleId");
                        ViewBag.RoleId = roleId;
                        ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
                        return View();
                    }
                    else
                    {

                        string customerName = _utils.GetCustNameUserId(userid);
                        if (customerName != "")
                        {
                            HttpContext.Session.SetString("CustomerName", customerName);
                        }
                        else
                        {
                            TempData["error"] = "Account Number is Invalid or not a GTBank Account";
                            roleId = HttpContext.Session.GetString("RoleId");
                            ViewBag.RoleId = roleId;
                            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
                            return View();
                        }
                    }


                    var getBillers = _ebills.GetBillers();

                    if (getBillers != null)
                    {

                        if (getBillers.code == 1)
                        {
                            HttpContext.Session.SetString("GetBillers", JsonConvert.SerializeObject(getBillers));
                            TempData["success"] = "Account Validation Successful/Billers Retrieved Succesfully";
                            return RedirectToAction("transaction");
                        }
                        else
                        {
                            TempData["error"] = "Error, Please Try Again";
                            roleId = HttpContext.Session.GetString("RoleId");
                            ViewBag.RoleId = roleId;
                            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
                            return View();
                        }
                    }
                    else
                    {
                        TempData["error"] = "An Error Occured While Retrieving Billers";
                        roleId = HttpContext.Session.GetString("RoleId");
                        ViewBag.RoleId = roleId;
                        ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
                        return View();
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An Error Occured While Validating Account";
                _logger.LogError(ex, "Error Occured While Validating Account");
                roleId = HttpContext.Session.GetString("RoleId");
                ViewBag.RoleId = roleId;
                ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
                return View();
            }


            return View();

        }

        [HttpGet]
        [Route("Home/GetBillerProducts")]
        public IActionResult GetBillerProducts(string billerId)
        {
            List<GetBillerProducts> objectList = new List<GetBillerProducts>();
            if (string.IsNullOrEmpty(billerId))
            {
                GetBillerProducts getbiller = new GetBillerProducts();
                getbiller.Id = "0";
                getbiller.Name = "No Products to Display";
            }
            else
            {
                var billerProducts = _ebills.GetBillersProd(billerId);

                HttpContext.Session.SetString("GetProduct", JsonConvert.SerializeObject(billerProducts));

                if (billerProducts != null)
                {
                    if (billerProducts.code == 1)
                    {
                        foreach (var biller in billerProducts.payload)
                        {
                            if (biller.isActive == true)
                            {
                                GetBillerProducts getbiller = new GetBillerProducts();
                                getbiller.Id = biller.id;
                                getbiller.Name = biller.name;
                                objectList.Add(getbiller);
                            }

                        }
                    }
                    else
                    {
                        GetBillerProducts getbiller = new GetBillerProducts();
                        getbiller.Id = "0";
                        getbiller.Name = "No Products Found";

                    }
                }
            }


            return Json(objectList);
        }


        [HttpGet]
        [Route("Home/GetCustomFields")]
        public IActionResult GetCustomFields(string productId)
        {

            List<GetFieldName> objectList = new List<GetFieldName>();
            
            var getAmount = HttpContext.Session.GetString("GetProduct");
            if (!string.IsNullOrEmpty(getAmount))
            {
                var amount = JsonConvert.DeserializeObject<ProductResponse>(getAmount);
                foreach (var item in amount.payload)
                {
                    if (productId == item.id)
                    {
                        HttpContext.Session.SetString("Amount", item.amount.ToString());
                        HttpContext.Session.SetString("Fee", item.fee.ToString());
                        productList = item.customFormFields;
                    }
                }

                foreach (var field in productList)
                {                      
                        GetFieldName getFields = new GetFieldName();
                        getFields.Name = field.label;
                        objectList.Add(getFields);                    
                }


            }

            return Json(objectList);
        }

        [HttpGet]
        [Route("Home/GetAmountandFee")]
        public IActionResult GetAmountandFee()
        {
            var amount = HttpContext.Session.GetString("Amount");
            var fee = HttpContext.Session.GetString("Fee");

            if (amount == "0")
            {
                amount = "";
            }

            var customFields = new { Amount = amount, Fee = fee };

            return Json(customFields);
        }

        public IActionResult Transaction()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");

            PostTransaction transaction = new PostTransaction();
            transaction.Nuban = HttpContext.Session.GetString("AccountNumber");
            transaction.AccountName = HttpContext.Session.GetString("CustomerName");

            var apiDataString = HttpContext.Session.GetString("GetBillers");
            var billers = JsonConvert.DeserializeObject<BillerResponse>(apiDataString);

            List<GetBiller> objectList = new List<GetBiller>();
            foreach (var biller in billers.payload)
            {
                GetBiller getbiller = new GetBiller();
                getbiller.Id = biller.id;
                getbiller.Name = biller.name;
                objectList.Add(getbiller);
            }

            transaction.Biller = objectList;

            return View(transaction);
        }

        [HttpPost]
        public IActionResult Transaction(PostTransaction post)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string email = "";
                    TransactionRequest payReq = new TransactionRequest();
                    ValidateRequest valReq = new ValidateRequest();
                    var billerName = JsonConvert.DeserializeObject<GetBiller>(post.SelectedBiller);
                    var productName = JsonConvert.DeserializeObject<GetBillerProducts>(post.SelectedProduct);

                    HttpContext.Session.SetString("BillerName", billerName.Name);
                    HttpContext.Session.SetString("ProductName", productName.Name);
                    HttpContext.Session.SetString("TransactionAmount", post.Amount);
                    List<Productfield> productFields = new List<Productfield>();
                    foreach (var item2 in post.DynamicFields)
                    {
                        Productfield productfield = new Productfield();
                        productfield.field = item2.Key;
                        productfield.data = item2.Value;

                        bool isValid = _utils.hasSpecialChar(productfield.data);
                        if(isValid)
                        {
                            TempData["error"] = "Incorrect String Format";
                            return RedirectToAction("ValidateAccount");
                        }
                        productFields.Add(productfield);
                    }

                    var validateProduct = HttpContext.Session.GetString("GetProduct");
                    if (!string.IsNullOrEmpty(validateProduct))
                    {
                        var validate = JsonConvert.DeserializeObject<ProductResponse>(validateProduct);
                        foreach (var item in validate.payload)
                        {
                            if (post.ProductId == item.id)
                            {
                                if (item.isValidationRequired == true)
                                {
                                    valReq.sourceBankCode = Convert.ToInt32(_configuration.GetConnectionString("sourceBankCode"));
                                    valReq.sourceBankName = _configuration.GetConnectionString("sourceBankName");
                                    valReq.institutionCode = _configuration.GetConnectionString("institutionCode");
                                    valReq.channelCode = _configuration.GetConnectionString("auth_InstitutionId");
                                    valReq.billerId = post.BillerId;
                                    valReq.billerName = billerName.Name;
                                    valReq.productID = post.ProductId;
                                    valReq.productName = productName.Name;
                                    valReq.amount = post.Amount;
                                    valReq.Params = post.DynamicFields;

                                    ValidateResponse ValResp = new ValidateResponse();
                                    ValResp = _ebills.ValidateCustomFields(valReq);
                                    if (ValResp.description.ToUpper() != "SUCCESS" && ValResp.hasErrors.Equals(true))
                                    {

                                        TempData["error"] = "Validation Failed. Please Try Again Later";
                                        return RedirectToAction("ValidateAccount");

                                    }
                                }

                                if (Convert.ToDecimal(post.Amount.ToString()) > Convert.ToDecimal(_configuration.GetConnectionString("TellerLimit")))
                                {
                                    bool addPendingTrans = _repo.AddPendingTransaction(post.Nuban, int.Parse(HttpContext.Session.GetString("UserName")), HttpContext.Session.GetString("BranchCode"), HttpContext.Session.GetString("muserid"), "PENDING", "AWAITING APPROVAL", post.ProductId, productName.Name, Convert.ToDecimal(post.Amount), Convert.ToDecimal(post.Fee), item.collectionAccountNumber, item.bankName, post.BillerId, billerName.Name, JsonSerializer.Serialize(productFields));
                                   
                                    if (addPendingTrans)
                                    {
                                        TempData["success"] = "Transaction has been Sent for Approval";
                                        return RedirectToAction("Pending");
                                    }

                                    else
                                    {
                                        TempData["success"] = "An Error Occurred. Please Try Again Later";
                                        return RedirectToAction("ValidateAccount");
                                    }

                                }


                                payReq.authenticated_InstitutionId = _configuration.GetConnectionString("auth_InstitutionId");
                                payReq.authenticated_UserEmail = _configuration.GetConnectionString("Email");
                                payReq.authenticated_UserId = _configuration.GetConnectionString("auth_UID");
                                payReq.branchId = _configuration.GetConnectionString("ebills_BranchID");
                                payReq.billerProductId = post.ProductId;
                                payReq.billerId = post.BillerId;
                                payReq.transactionChannel = "Web";
                                payReq.channel = _configuration.GetConnectionString("Channel");
                                payReq.billerName = billerName.Name;
                                payReq.amount = Convert.ToDouble(post.Amount);
                                payReq.fee = Convert.ToDouble(post.Fee);
                                payReq.debitAccountNumber = post.Nuban;
                                payReq.productName = productName.Name;
                                payReq.productfields = productFields;

                                string request = JsonConvert.SerializeObject(payReq);
                                _logger.LogInformation("Ebills request to the api for Transaction is : " + request);
                                TransactionResponse TransResp = new TransactionResponse();
                                TransResp = _ebills.MakePayment(payReq);

                                if (String.IsNullOrEmpty(TransResp.description) && string.IsNullOrEmpty(TransResp.payload))
                                {
                                    TransResp.description = "FAILED";
                                    bool addPostTrans = _repo.AddPostTransaction(post.Nuban, int.Parse(HttpContext.Session.GetString("UserName")), HttpContext.Session.GetString("BranchCode"), HttpContext.Session.GetString("muserid"), TransResp.description, post.ProductId, productName.Name, Convert.ToDecimal(post.Amount), Convert.ToDecimal(post.Fee), item.collectionAccountNumber, item.bankName, post.BillerId, billerName.Name, JsonSerializer.Serialize(productFields));
                                  
                                }
                                else
                                {
                                    bool addPostTrans = _repo.AddPostTransaction(post.Nuban, int.Parse(HttpContext.Session.GetString("UserName")), HttpContext.Session.GetString("BranchCode"), HttpContext.Session.GetString("muserid"), TransResp.description.ToUpper(), TransResp.payload, post.ProductId, productName.Name, Convert.ToDecimal(post.Amount), Convert.ToDecimal(post.Fee), item.collectionAccountNumber, item.bankName, post.BillerId, billerName.Name, JsonSerializer.Serialize(productFields));

                                }

                               
                                if (TransResp.code == -1 && TransResp.hasErrors)
                                {
                                    TempData["error"] = "Transaction Failed. Please Try Again Later";
                                    return RedirectToAction("Failed");
                                }
                                else if (TransResp.description.ToUpper() == "SUCCESS" && !TransResp.hasErrors)
                                {
                                    HttpContext.Session.SetString("SessionId", TransResp.payload);
                                    TempData["success"] = "Transaction Successful";
                                    return RedirectToAction("Success");
                                }
                                else
                                {
                                    TempData["error"] = "Unable to Complete Transactions. Please Try Again Later";
                                    return RedirectToAction("Failed");
                                }
                            }
                        }


                    }


                }

                TempData["error"] = "Kindly Ensure all Fields are Inputted Correctly";
                return RedirectToAction("ValidateAccount");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Validat Account Error");
                TempData["error"] = "Unable to Complete Transactions. Please Try Again Later";
                return RedirectToAction("ValidateAccount");
            }
            

          



        }

        public IActionResult Success()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            TransactionStatus status = new TransactionStatus();
            status.Nuban = HttpContext.Session.GetString("AccountNumber");
            status.AccountName = HttpContext.Session.GetString("CustomerName");
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
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            TransactionStatus status = new TransactionStatus();
            status.Nuban = HttpContext.Session.GetString("AccountNumber");
            status.AccountName = HttpContext.Session.GetString("CustomerName");
            status.BillerName = HttpContext.Session.GetString("BillerName");
            status.ProductName = HttpContext.Session.GetString("ProductName");
            status.Amount = HttpContext.Session.GetString("TransactionAmount");

            return View(status);
        }

        public IActionResult Pending()
        {
            string roleId = HttpContext.Session.GetString("RoleId");
            if(string.IsNullOrEmpty(roleId))
            {
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = roleId;
            ViewBag.ConfigRoleId = _configuration.GetConnectionString("RoleId");
            TransactionStatus status = new TransactionStatus();
            status.Nuban = HttpContext.Session.GetString("AccountNumber");
            status.AccountName = HttpContext.Session.GetString("CustomerName");
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
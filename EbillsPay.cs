using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Net.Security;
using static EBILLSPAY_V2.Models.ApiModels;

namespace EBILLSPAY_V2
{
    public class EbillsPay
    {
        string channel = string.Empty;
        AccessResponse authTokenCode = new AccessResponse();
        
        private readonly ILogger<EbillsPay> _logger;
        private readonly IConfiguration _configuration;
        private readonly Utilities _Utilities;

        public EbillsPay(ILogger<EbillsPay> logger, IConfiguration configuration, Utilities utilities)
        {
            _logger = logger;
            _configuration = configuration;
            _Utilities = utilities;
        }

        public HistoryResponse GetTransHistory(string userID, DateTime startDate, DateTime endDate)
        {
            HistoryResponse objRt = new HistoryResponse();
            channel = _configuration.GetConnectionString("Channel");

            try
            {
                var url = _configuration.GetConnectionString("newEbillsUrl");
                var client = new RestClient(url);
                authTokenCode = GetAccessToken();

                Random rnd = new Random();
                int reqID = rnd.Next(10000000, 99999999);
                string sessID = Guid.NewGuid().ToString();

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var request = new RestRequest(_configuration.GetConnectionString("HistoryEndpoint"), Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader("Channel", channel.ToUpper());
                request.AddHeader("RequestId", reqID);
                request.AddHeader("Channel", channel.ToUpper());
                request.AddParameter("Nuban", userID);
                request.AddParameter("startDate", startDate.ToString("dd/MM/yyyy"));
                request.AddParameter("endDate", endDate.ToString("dd/MM/yyyy"));
                request.AddHeader("Authorization", "Bearer " + authTokenCode.tokenResponse.token);

                string jsonTxt = JsonConvert.SerializeObject(request);
                _logger.LogError("Ebills request to the api for Get Transaction History is : " + jsonTxt);
                var response = client.Execute(request);

                var resp = JsonConvert.DeserializeObject(response.Content);
                objRt = JsonConvert.DeserializeObject<HistoryResponse>(resp.ToString());
                _logger.LogError("Done calling API returned : ");
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
                    _logger.LogError("Response code EbillsPay Request GetTransactionHistory method: " + response.StatusCode);
                    _logger.LogError("EbillsPay GenerateReceipt  Response formatted : " + resp.ToString());
                    _logger.LogError("Response code GenerateReceipt Details method: " + response.StatusCode);
                    if (objRt.responseCode.Equals("00"))
                    {
                        return objRt;
                    }
                }
                else
                {
                    return objRt;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred calling EbillsPay TransactionHistory API: " + ex.Message + ex.Source + ex.StackTrace);
            }
            return objRt;
        }

        public AccessResponse GetAccessToken()
        {
            AccessResponse objs = new AccessResponse();
            channel = _configuration.GetConnectionString("Channel");
            try
            {
                _logger.LogInformation("Entered the GetAccessToken Method");
                string clientPassword = _configuration.GetConnectionString("clientPassword");

                string publicEncryptionKey = _Utilities.GetXMLEncryptionKey(channel.ToUpper());
                var TokenRequest = new AccessRequest()
                {
                    password = _Utilities.Encryption(clientPassword, publicEncryptionKey),
                    username = _Utilities.Encryption(channel.ToUpper(), publicEncryptionKey)
                };
                string url =_configuration.GetConnectionString("newEbillsUrl");

                var client = new RestClient(url);

                var request = new RestRequest(_configuration.GetConnectionString("AccessTokenEndpoint"), Method.Post)
                {
                    RequestFormat = DataFormat.Json
                };

                ServicePointManager.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };


                request.AddHeader("Channel", channel.ToUpper());
                request.AddBody(TokenRequest);
                string requestLog = JsonConvert.SerializeObject(request);
                _logger.LogInformation("About to Call Ebills's REST API for Request: " + requestLog);
                var response = client.Execute(request);
                 _logger.LogInformation(JsonConvert.SerializeObject(response));
                 objs = JsonConvert.DeserializeObject<AccessResponse>(response.Content);
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
               
                    if (objs.responseCode.Equals("00"))
                    {
                        _logger.LogInformation("Call to EBills GetTokenAPI Successful");
                    }
                    else
                    {
                        objs.responseCode = "01";
                        objs.responseDescription = "An error occured while retrieving token. Please try again later";
                    }
                }
                else
                {
                    objs.responseCode = "01";
                    objs.responseDescription = "An error occured while retrieving token. Please try again later";
                    _logger.LogError("Response code for EbillsPay Request GetAccessToken method: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred calling EbillsPay GetAccessToken API: " + ex.Message + ex.Source + ex.StackTrace);
            }
            return objs;
        }


        public ReceiptResponse GenerateReceipt(string sessionid)
        {
            ReceiptResponse objRt = new ReceiptResponse();

            try
            {
                var url = _configuration.GetConnectionString("newEbillsUrl");
                channel = _configuration.GetConnectionString("Channel");
                var client = new RestClient(url);
                authTokenCode = GetAccessToken();
                Random rnd = new Random();
                int reqID = rnd.Next(100000, 999999);
                string sessID = Guid.NewGuid().ToString();
                var request = new RestRequest(_configuration.GetConnectionString("ReceiptEndpoint"), Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddParameter("SessionID", sessionid);
                request.AddHeader("RequestId", reqID);
                request.AddHeader("Authorization", "Bearer " + authTokenCode.tokenResponse.token);
                request.AddHeader("Channel", channel.ToUpper());

                string jsonTxt = JsonConvert.SerializeObject(request);
                _logger.LogInformation("Ebills request to the api for GenerateReceipt is : " + jsonTxt);
                var response = client.Execute(request);

                _logger.LogInformation("Done calling API returned : ");

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
                    _logger.LogInformation("Response code EbillsPay Request GenerateReceipt method: " + response.StatusCode);
                    var resp = JsonConvert.DeserializeObject(response.Content);
                    _logger.LogInformation("EbillsPay GenerateReceipt  Response formatted : " + resp.ToString());
                    objRt = JsonConvert.DeserializeObject<ReceiptResponse>(resp.ToString());

                    _logger.LogInformation("Response code GenerateReceipt Details method: " + response.StatusCode);

                    if (objRt.code.Equals("1") && objRt.description == "Pdf Generation Successful.")
                    {
                        return objRt;
                    }
                }
                else
                {
                    _logger.LogError("Response code EbillsPay Request GenerateReceipt method: " + response.StatusCode);
                }
                   
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred calling EbillsPay GenerateReceipt API: ", ex.Message + ex.Source + ex.StackTrace);
            }
            return objRt;
        }

        public BillerResponse GetBillers()
        {
            BillerResponse objRt = new BillerResponse();
            try
            {
                var url = _configuration.GetConnectionString("newEbillsUrl");
                channel = _configuration.GetConnectionString("Channel");
                var client = new RestClient(url);
                var bearerToken = GetAccessToken();

                Random rnd = new Random();
                int reqID = rnd.Next(100000000, 999999999);
                string sessID = Guid.NewGuid().ToString();

                ServicePointManager.ServerCertificateValidationCallback = delegate
                {
                    return true;
                };

                var request = new RestRequest((_configuration.GetConnectionString("BillerEndpoint")), Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader("Requestid", Convert.ToString(reqID));              
                request.AddHeader("Authorization", "Bearer " + bearerToken.tokenResponse.token);
                request.AddHeader("Channel", channel.ToUpper());
                var response = client.Execute(request);
                _logger.LogInformation("Response code EbillsPay Request GetBillers method: " + response.StatusCode);
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
                    var resp = JsonConvert.DeserializeObject(response.Content);
                    objRt = JsonConvert.DeserializeObject<BillerResponse>(resp.ToString());
                    if (!objRt.hasErrors == false & objRt.totalCount > 0)
                    {
                        return objRt;
                    }
                }
            }
            catch (Exception ex)
            { 
                _logger.LogError("An error occurred calling EbillsPay GetBillers API: " + ex.Message + ex.Source + ex.StackTrace);
            }
            return objRt;
        }

        public ProductResponse GetBillersProd(string billerId)
        {
            ProductResponse resp = new ProductResponse();
            try
            {
                var url = _configuration.GetConnectionString("newEbillsUrl");
                var client = new RestClient(url);
                var bearerToken = GetAccessToken();

                Random rnd = new Random();
                int reqID = rnd.Next(100000000, 999999999);
                string sessID = Guid.NewGuid().ToString();

                var request = new RestRequest(_configuration.GetConnectionString("ProductEndpoint"), Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader("Channel", channel.ToUpper());
                request.AddHeader("Authorization", "Bearer " + bearerToken.tokenResponse.token);
                request.AddHeader("Requestid", Convert.ToString(reqID));
                request.AddParameter("BillerId", billerId);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                string jsonTxt = JsonConvert.SerializeObject(request);
                var response = client.Execute(request);

                _logger.LogInformation($"Done calling GetBillerProducts endpoint returned : {response.StatusCode} for billerid {billerId}");
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
                    resp = JsonConvert.DeserializeObject<ProductResponse>(response.Content);

                    if (resp.hasErrors.Equals(false) && resp.totalCount > 0)
                    {
                        return resp;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred calling EbillsPay BillerProduct API: " + ex.Message + ex.Source + ex.StackTrace);
            }
            return resp;
        }

        public CustomResponse GetCustomFields(string productId)
        {
            CustomResponse resp = new CustomResponse();
            try
            {
                var url = _configuration.GetConnectionString("newEbillsUrl");               
                var client = new RestClient(url);
                var bearerToken = GetAccessToken();

                Random rnd = new Random();
                int reqID = rnd.Next(100000000, 999999999);
                string sessID = Guid.NewGuid().ToString();

                var request = new RestRequest(_configuration.GetConnectionString("CustomEndpoint"), Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader("Channel", channel.ToUpper());
                request.AddHeader("Authorization", "Bearer " + bearerToken.tokenResponse.token);
                request.AddHeader("RequestId", Convert.ToString(reqID));
                request.AddParameter("ProductId", productId);

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                string jsonTxt = JsonConvert.SerializeObject(request);
                _logger.LogInformation("Ebills request to the api for GetCustomFields is : " + jsonTxt);
                var response = client.Execute(request);

                _logger.LogInformation("Done calling API returned : " + response.Content);
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
                    _logger.LogInformation("Response code EbillsPay Request Custom method: " + response.StatusCode);
                    resp = JsonConvert.DeserializeObject<CustomResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
               _logger.LogError("An error occurred calling EbillsPay CustomFields API: " + ex.Message + ex.Source + ex.StackTrace);
            }
            return resp;
        }

        public ValidateResponse ValidateCustomFields(ValidateRequest valReq)
        {

            ValidateResponse objRt = new ValidateResponse();
            try
            {
                var url = _configuration.GetConnectionString("newEbillsUrl");

                var client = new RestClient(url);
                Random rnd = new Random();
                int reqID = rnd.Next(10000000, 99999999);
                var bearerToken = GetAccessToken();

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var request = new RestRequest(_configuration.GetConnectionString("ValidateEndpoint"), Method.Post)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader("Channel", channel.ToUpper());
                request.AddHeader("Authorization", "Bearer " + bearerToken.tokenResponse.token);
                request.AddHeader("Requestid", Convert.ToString(reqID));
                request.AddBody(valReq);
                string jsonTxt = JsonConvert.SerializeObject(request);
                _logger.LogInformation("Ebills request to the api for Transaction is : " + jsonTxt);
                var response = client.Execute(request);

                _logger.LogInformation("Done calling API returned : " + response);
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
                    _logger.LogInformation("Response code EbillsPay Request ValidateCustomFields method: " + response.StatusCode);
                    var resp = JsonConvert.DeserializeObject(response.Content);
                    _logger.LogInformation("EbillsPay BillerProduct  Response formatted : " + resp.ToString());
                    objRt = JsonConvert.DeserializeObject<ValidateResponse>(resp.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred calling EbillsPay ValidateCustomFields API: ", ex.Message + ex.Source + ex.StackTrace);
            }
            return objRt;
        }
        public TransactionResponse MakePayment(TransactionRequest payReq)
        {
            TransactionResponse objRt = new TransactionResponse();
            try
            {
                var url = _configuration.GetConnectionString("newEbillsUrl");

                var client = new RestClient(url);
                Random rnd = new Random();
                int reqID = rnd.Next(10000000, 99999999);
                var bearerToken = GetAccessToken();

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var request = new RestRequest(_configuration.GetConnectionString("PayEndpoint"), Method.Post)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader("Channel", channel.ToUpper());
                request.AddHeader("Authorization", "Bearer " + bearerToken.tokenResponse.token);
                request.AddHeader("Requestid", Convert.ToString(reqID));
                request.AddBody(payReq);
                string jsonTxt = JsonConvert.SerializeObject(request);
                _logger.LogInformation("Ebills request to the api for Transaction is : " + jsonTxt);
                var response = client.Execute(request);

                _logger.LogInformation("Done calling API returned : " + response.Content);
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
                    _logger.LogInformation("Response code EbillsPay Request CreateTransaction method: " + response.StatusCode);
                    var resp = JsonConvert.DeserializeObject(response.Content);
                    _logger.LogInformation("EbillsPay BillerProduct  Response formatted : " + resp.ToString());
                    objRt = JsonConvert.DeserializeObject<TransactionResponse>(resp.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred calling EbillsPay CreateTransaction API: ", ex.Message + ex.Source + ex.StackTrace);
            }
            return objRt;
        }
    }
}

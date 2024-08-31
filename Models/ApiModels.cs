namespace EBILLSPAY_V2.Models
{
    public class ApiModels
    {

        public class HistoryResponse
        {
            public Transactionhistory[] transactionHistory { get; set; }
            public string responseCode { get; set; }
            public string responseDesc { get; set; }
        }

        public class Transactionhistory
        {
            public string sessionid { get; set; }
            public DateTime dateInitiated { get; set; }
            public string narration { get; set; }
            public string transStatus { get; set; }
            public string originatorAccountNumber { get; set; }
            public string beneficiaryAccountNumber { get; set; }
            public string requestid { get; set; }
            public string amount { get; set; }
            public string transDescription { get; set; }
        }

        public class AccessRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class AccessResponse
        {
            public Tokenresponse tokenResponse { get; set; }
            public object logEntry { get; set; }
            public string responseCode { get; set; }
            public string responseDescription { get; set; }
        }

        public class Tokenresponse
        {
            public string tokenType { get; set; }
            public string expiresIn { get; set; }
            public DateTime expiryDate { get; set; }
            public string token { get; set; }
        }

        public class ReceiptResponse
        {
            public Payload3 payload { get; set; }
            public int totalCount { get; set; }
            public object[] errors { get; set; }
            public bool hasErrors { get; set; }
            public int code { get; set; }
            public string description { get; set; }
        }

        public class Payload3
        {
            public string base64String { get; set; }
            public string name { get; set; }
        }

        public class BillerResponse
        {
            public Payload[] payload { get; set; }
            public int totalCount { get; set; }
            public object[] errors { get; set; }
            public bool hasErrors { get; set; }
            public int code { get; set; }
            public object description { get; set; }
        }

        public class Payload
        {
            public string id { get; set; }
            public string name { get; set; }
            public string acronym { get; set; }
            public string tin { get; set; }
            public string rcNumber { get; set; }
            public string institutionId { get; set; }
            public string institution { get; set; }
            public string industry { get; set; }
            public string industryId { get; set; }
            public string address1 { get; set; }
            public object address2 { get; set; }
            public string brandName { get; set; }
            public string subBrand { get; set; }
            public string email { get; set; }
            public string approvalStatus { get; set; }
            public string approvalActionBy { get; set; }
            public DateTime? approvalActionDate { get; set; }
            public string logoFileId { get; set; }
            public string logo { get; set; }
            public DateTime createdOn { get; set; }
            public string createdBy { get; set; }
            public object dateRejected { get; set; }
            public string rejectedBy { get; set; }
            public object rejectionReason { get; set; }
            public int billerType { get; set; }
            public bool isActive { get; set; }
            public string localGovtAreaId { get; set; }
            public string localGovtArea { get; set; }
            public string stateId { get; set; }
            public string state { get; set; }
            public string ivKey { get; set; }
            public string secret { get; set; }
        }

        public class ProductResponse
        {
            public Payload2[] payload { get; set; }
            public int totalCount { get; set; }
            public object[] errors { get; set; }
            public bool hasErrors { get; set; }
            public int code { get; set; }
            public object description { get; set; }
        }

        public class Payload2
        {
            public string id { get; set; }
            public string name { get; set; }
            public string subBrand { get; set; }
            public string collectionAccountNumber { get; set; }
            public string bankName { get; set; }
            public double amount { get; set; }
            public double fee { get; set; }
            public string approvalStatus { get; set; }
            public string institution { get; set; }
            public DateTime createdOn { get; set; }
            public string createdBy { get; set; }
            public bool isActive { get; set; }
            public bool isFixedFee { get; set; }
            public bool isValidationRequired { get; set; }
            public string billerId { get; set; }
            public Customformfield[] customFormFields { get; set; }
        }


        public class Customformfield
        {
            public string label { get; set; }
            public bool isRequired { get; set; }
            public string fieldType { get; set; }
        }

        public class CustomResponse
        {
            public CustomResponsePayload[] payload { get; set; }
            public int totalCount { get; set; }
            public object[] errors { get; set; }
            public bool hasErrors { get; set; }
            public int code { get; set; }
            public object description { get; set; }
        }
        public class CustomResponsePayload
        {
            public string label { get; set; }
            public bool isRequired { get; set; }
            public string fieldType { get; set; }
            public int minimumCharacterSize { get; set; }
            public int maximumCharacterSize { get; set; }
            public double minimumValue { get; set; }
            public double maximumValue { get; set; }
            public int sequenceId { get; set; }
            public string validationPattern { get; set; }
            public string multiSelectId { get; set; }
            public string listValue { get; set; }
            public string endpoint { get; set; }
            public DateTime createdOn { get; set; }
            public string billerProductId { get; set; }
            public string customFieldId { get; set; }
        }

        public class TransactionRequest
        {
            public string authenticated_InstitutionId { get; set; }
            public string authenticated_UserEmail { get; set; }
            public string authenticated_UserId { get; set; }
            public string branchId { get; set; }
            public string billerProductId { get; set; }
            public string billerId { get; set; }
            public string transactionChannel { get; set; }
            public string channel { get; set; }
            public string billerName { get; set; }
            public double amount { get; set; }
            public double fee { get; set; }
            public string debitAccountNumber { get; set; }
            public string productName { get; set; }
            public List<Productfield> productfields { get; set; } = new List<Productfield>();

            //public Productfield[] productfields { get; set; } = new Productfield[] { };
        }

        public class Productfield
        {
            public string field { get; set; }
            public string data { get; set; }
        }

        public class ValidateRequest
        {
            public int sourceBankCode { get; set; }
            public string sourceBankName { get; set; }
            public string institutionCode { get; set; }
            public string channelCode { get; set; }
            public string billerId { get; set; }
            public string billerName { get; set; }
            public string productID { get; set; }
            public string productName { get; set; }
            public string amount { get; set; }
            public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
        }

        public class TransactionResponse
        {
            public string payload { get; set; }
            public int totalCount { get; set; }
            public object[] errors { get; set; }
            public bool hasErrors { get; set; }
            public int code { get; set; }
            public string description { get; set; }
        }

        public class ValidateResponse
        {
            public string payload { get; set; }
            public int totalCount { get; set; }
            public object[] errors { get; set; }
            public bool hasErrors { get; set; }
            public int code { get; set; }
            public string description { get; set; }
        }

    }
}

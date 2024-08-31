//using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EBILLSPAY_V2.Data;

public partial class EbillsPay
{
    [Key]
    public long TransId { get; set; }
    public int TellerID { get; set; }
    public string BranchCode { get; set; } = null!;
    public string InitiatedBy { get; set; } = null!;
    public string AccountToDebit { get; set; } = null!;

    public string CollectionAccNumber { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string BillerName { get; set; } = null!;

    public string BillerId { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal Fee { get; set; }

    public string TransStatus { get; set; } = null!;

    public string TransDate { get; set; } = null!;

    public string? FieldList { get; set; }

    public string? SessionId { get; set; }
    public string? CurrApprStatus { get; set; }
    public string? ApprovedBy { get; set; }


}

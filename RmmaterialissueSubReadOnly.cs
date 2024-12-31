namespace ERP_API.Moduls
{
    public class RmmaterialissueSubReadOnly
    {
        public int matIssueId { get; set; }


        public int? matIssueSubId { get; set; }
       

        public int? SlNo { get; set; }

        public int? ItemId { get; set; }


        public float? IssQty { get; set; }

        public float? StockQty { get; set; }


        public string? BatchNo { get; set; }

        public int? TotalBags { get; set; }


        public double? BagsIssued { get; set; }
    }
}

namespace ERP_API.Moduls
{
    public class RmmaterialissueCreateOnly
    {
        public int matIssueId { get; set; }


        public string? IssNo { get; set; }

        public int? StoreId { get; set; }

        public DateTime? DateOfIssue { get; set; }

        public int? FromStore { get; set; }

        public int? ToStore { get; set; }

        public List<RmmaterialissueSubReadOnly> RmmaterialissueSubs{ get; set; }
    }
}

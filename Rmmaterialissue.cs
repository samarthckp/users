using System;
using System.Collections.Generic;

namespace ERP_API.Data;

public partial class Rmmaterialissue
{
    public int matIssueId { get; set; }
  

    public string? IssNo { get; set; }

    public int? StoreId { get; set; }

    public DateTime? DateOfIssue { get; set; }

    public int? FromStore { get; set; }

    public int? ToStore { get; set; }



    public ICollection<RmmaterialissueSub> RmmaterialissueSubs{ get; set; } = new List<RmmaterialissueSub>();
}

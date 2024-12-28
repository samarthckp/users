using System;
using System.Collections.Generic;

namespace ERP_API.Data;

public partial class Location
{
    public int? LocationId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }
    public int? CompanyId { get; set; }
    public string locationcode { get; set; }
    public ICollection<User> Users { get; set; } // Navigation property




}

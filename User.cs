using System;
using System.Collections.Generic;

namespace ERP_API.Data;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserPassword { get; set; }

    public string? XlPassword { get; set; }

    public string? CompanyId { get; set; }

    public string? ConfirmPassword { get; set; }
    public int? LocationId { get; set; }//forgien
  

    public Location Location { get; set; }

}

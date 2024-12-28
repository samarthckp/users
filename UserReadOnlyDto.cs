namespace ERP_API.Moduls
{
    public class UserReadOnlyDto
    {

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserPassword { get; set; }

        public string? XlPassword { get; set; }

        public string? CompanyId { get; set; }

        public string? ConfirmPassword { get; set; }
        public int? LocationId { get; set; }



    }
}

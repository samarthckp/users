namespace ERP_API.Moduls
{
    public class LocationCreateDto
    {
        public int? LocationId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
        public int? CompanyId { get; set; }
        public string locationcode { get; set; }

    }
}

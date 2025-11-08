namespace SchoolKeeper.DTO;

public class ReptDto
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public int GeneratedBy { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string? Summary { get; set; }
    public DateTime GeneratedOn { get; set; }
}

public class ReptCreateDto
{
    public int SchoolId { get; set; }
    public int GeneratedBy { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string? Summary { get; set; }
    public DateTime? GeneratedOn { get; set; }
}

public class ReptUpdateDto
{
    public int? SchoolId { get; set; }
    public int? GeneratedBy { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public string? Summary { get; set; }
    public DateTime? GeneratedOn { get; set; }
}


namespace SchoolKeeper.DTO;

public class SchoolDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public string? Region { get; set; }
    public string? ContactNumber { get; set; }
}

public class SchoolCreateDto
{
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public string? Region { get; set; }
    public string? ContactNumber { get; set; }
}

public class SchoolUpdateDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Region { get; set; }
    public string? ContactNumber { get; set; }
}


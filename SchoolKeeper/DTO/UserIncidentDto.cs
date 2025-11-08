namespace SchoolKeeper.DTO;

public class UserIncidentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int IncidentId { get; set; }
}

public class UserIncidentCreateDto
{
    public int UserId { get; set; }
    public int IncidentId { get; set; }
}


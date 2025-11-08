namespace SchoolKeeper.DTO;

public class ReptIncidentDto
{
    public int Id { get; set; }
    public int ReptId { get; set; }
    public int IncidentId { get; set; }
}

public class ReptIncidentCreateDto
{
    public int ReptId { get; set; }
    public int IncidentId { get; set; }
}


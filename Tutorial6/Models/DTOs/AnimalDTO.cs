namespace Tutorial6.Models.DTOs;

public class AnimalDTO
{
    public int ID { get; set; }
    public string Name { get; set; }
    public DateTime AdmissionDate { get; set; }
    public OwnerDTO Owner { get; set; }
    public ClassDTO Class { get; set; }
}

public record OwnerDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public record ClassDTO
{
    public string Name { get; set; }
}
namespace Gateway.Data.Entities.Common;

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime DateCreated { get; set; } = DateTime.Now;
    
    public DateTime DateUpdated { get; set; } = DateTime.Now;
}
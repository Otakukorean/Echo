namespace Shared.DDD;

public class Entity<T> : IEntity<T>
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public required T Id { get; set; }
}
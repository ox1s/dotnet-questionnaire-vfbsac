namespace SharedKernel;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    protected Entity(Guid id) => Id = id;

    public bool IsDeleted { get; protected set; }

    protected Entity() { }

    public void SetIdForSeeding(Guid id)
    {
        Id = id;
    }
    public void MarkAsDeleted()
    {
        IsDeleted = true;
    }
}

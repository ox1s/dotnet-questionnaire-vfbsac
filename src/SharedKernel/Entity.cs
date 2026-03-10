namespace SharedKernel;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    protected Entity(Guid id) => Id = id;

    protected Entity() { }

    public void SetIdForSeeding(Guid id)
    {
        Id = id;
    }
}

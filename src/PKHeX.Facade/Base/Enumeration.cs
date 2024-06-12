namespace PKHeX.Facade.Base;

public abstract class Enumeration : IComparable
{
    public string Name { get; private set; }

    public int Id { get; private set; }

    protected Enumeration(int id, string name) => (Id, Name) = (id, name);

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public int CompareTo(object? other)
    {
        if (other is null) throw new ArgumentException(nameof(other));
        return Id.CompareTo(((Enumeration)other).Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
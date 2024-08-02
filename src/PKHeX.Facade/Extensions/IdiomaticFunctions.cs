namespace PKHeX.Facade.Extensions;

public static class IdiomaticFunctions
{
    public static IList<T> ListOfNotNull<T>(params T?[] items) => items.Where(i => i != null).ToList()!;
}
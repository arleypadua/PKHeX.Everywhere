using PKHeX.Facade;

namespace PKHeX.Web.BackendApi.Representation;

public record GenderRepresentation(
    int Id,
    string Name,
    string Symbol)
{
    public Gender ToGender() => Id switch
    {
        0 => Gender.Male,
        1 => Gender.Female,
        2 => Gender.Genderless,
        _ => throw new ArgumentOutOfRangeException(nameof(Id), $"Value {Id} not supported")
    };
};
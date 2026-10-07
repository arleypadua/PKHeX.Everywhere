using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class CatalogHandlers
{
    [Query("catalog.names")]
    public static CatalogNames Names(CatalogNamesRequest request)
    {
        var game = request.FormatId is null ? null : EmptyOf(request.FormatId);
        Func<ushort, SpeciesDefinition?> species = game is null ? SpeciesRepository.Find : game.SpeciesRepository.FindKnown;
        Func<ushort, ItemDefinition> item = game is null ? ItemRepository.GetItem : game.ItemRepository.GetGameItem;

        return new(
            request.SpeciesIds.Select(species).OfType<SpeciesDefinition>().Select(definition => definition.ToCatalogName()).ToArray(),
            request.ItemIds.Select(id => item(id).ToCatalogName()).ToArray(),
            (request.AbilityIds ?? []).Select(id => AbilityRepository.GetAbility(id).ToCatalogName()).ToArray(),
            (request.NatureIds ?? []).Select(id => NatureRepository.GetNature(id).ToCatalogName()).ToArray());
    }

    private static Game EmptyOf(string formatId) => Game.EmptyOf(GameHandlers.FindFormat(formatId))
        ?? throw new EngineException(ErrorCodes.NotSupported, $"The save format '{formatId}' can't name its species and items without a save.");
}

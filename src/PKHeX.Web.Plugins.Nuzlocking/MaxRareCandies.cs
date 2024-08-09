using PKHeX.Facade.Repositories;

namespace PKHeX.Web.Plugins.Nuzlocking;

public class MaxRareCandies(IGameProvider gameProvider) : IQuickAction
{
    public string Description => "Adds a button on the home page to give max rare candies";
    public string Label => "Max Rare Candies";
    
    private static ItemDefinition? RareCandyDefinition => 
        ItemRepository.GetItemByName("Rare Candy");
    
    public Task<Outcome> OnActionRequested()
    {
        if (gameProvider.Game is null) 
            return Outcome.Notify("No game loaded").Completed();
        
        var rareCandyDefinition = RareCandyDefinition;
        if (rareCandyDefinition is null)
            return Outcome.Notify("No Rare Candy definition found in this game").Completed();

        var inventorySupportingRareCandies = gameProvider.LoadedGame.Trainer
            .Inventories.InventoryItems
            .FirstOrDefault(i => i.Value.Supports(rareCandyDefinition))
            .Value;
        
        if (inventorySupportingRareCandies is null)
            return Outcome.Notify("No inventory supports rare candies for this game").Completed();
        
        inventorySupportingRareCandies.Set(
            rareCandyDefinition.Id, 
            (uint)inventorySupportingRareCandies.MaxItemCountAllowed);

        return Outcome.Notify(
            $"{inventorySupportingRareCandies.MaxItemCountAllowed} Rare candies set")
            .Completed();
    }
}
// Assets/Scripts/Managers/GameStateManager.cs
public static class GameStateManager
{
    public static bool IsGamePaused =>
        PauseMenu.IsPaused ||
        WeaponCraftingSystem.IsCraftingOpen ||
        NPCInteraction.IsTradeOpen;
}

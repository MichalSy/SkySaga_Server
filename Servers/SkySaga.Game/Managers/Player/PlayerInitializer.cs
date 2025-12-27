namespace SkySaga.Game.Managers.Player;

/// <summary>
/// Service responsible for initializing player entities with default components and inventory.
/// </summary>
public class PlayerInitializer(IMapEntityManager entityManager)
{
    private readonly IMapEntityManager _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));

    /// <summary>
    /// Initializes a player entity with default components and inventory.
    /// </summary>
    public void InitializePlayer(Entity player)
    {
        ArgumentNullException.ThrowIfNull(player);

        // TODO: Component property defaults and database
        if (player.TryGetComponent<ClientPlayernameComponent>(out var clientPlayernameComponent))
            clientPlayernameComponent.Playername = $"Player {player.Id}";

        if (player.TryGetComponent<ClientHealthComponent>(out var clientHealthComponent))
        {
            clientHealthComponent.InitialHP = 10;
            clientHealthComponent.WholeHearts = 20;
        }


        if (player.TryGetComponent<SmoothedTransformComponent>(out var smoothedTransformComponent))
            smoothedTransformComponent.Position = new Vector3(35.45f, 13f, 19.82f);

        if (player.TryGetComponent<ClientFeatureUnlockComponent>(out var clientFeatureUnlockComponent))
            clientFeatureUnlockComponent.FeatureIsLockedStatusList.Add(true);

        if (player.TryGetComponent<ClientCraftingDropSlotsComponent>(out var clientCraftingDropSlotsComponent))
            clientCraftingDropSlotsComponent.CraftingDropSlots = [0, 0];

        if (player.TryGetComponent<ClientPlayerAspectsComponent>(out var clientPlayerAspectsComponent))
        {
            clientPlayerAspectsComponent.CanEditMap = true;
            clientPlayerAspectsComponent.CanDamageEntities = true;
            clientPlayerAspectsComponent.CanDamagePlayers = true;
            clientPlayerAspectsComponent.CanCreateDevices = true;
            clientPlayerAspectsComponent.CanDamageDevices = true;
            clientPlayerAspectsComponent.IsDebugPlayer = true;
            clientPlayerAspectsComponent.AccountLevel = 2;
        }

        if (player.TryGetComponent<ClientWalletComponent>(out var clientWalletComponent))
        {
            clientWalletComponent.Currency.CurrencyList.Add(new WalletData.CurrencyData
            {
                NameHash = Util.ComputeCrc32("Life_Ticket"),
                Value = 69
            });

            clientWalletComponent.Currency.CurrencyList.Add(new WalletData.CurrencyData
            {
                NameHash = Util.ComputeCrc32("Portal_Ticket"),
                Value = 420
            });
        }

        if (player.TryGetComponent<ClientInventoryComponent>(out var clientInventoryComponent))
        {
            clientInventoryComponent.MaxInventorySlots = 45;

            for (int i = 0; i < clientInventoryComponent.MaxInventorySlots; i++)
                clientInventoryComponent.InventoryEntityList.Add(0);

            // Armor pieces
            //CreateAndEquipInventoryItem(clientInventoryComponent, 2, "ExplorerArmourHead");
            //CreateAndEquipInventoryItem(clientInventoryComponent, 3, "ExplorerArmourTorso");
            //CreateAndEquipInventoryItem(clientInventoryComponent, 4, "ExplorerArmourArms");
            //CreateAndEquipInventoryItem(clientInventoryComponent, 5, "ExplorerArmourLegs");

            // Inventory items
            CreateAndEquipInventoryItem(clientInventoryComponent, 9, "Dirt", 10);
            CreateAndEquipInventoryItem(clientInventoryComponent, 10, "Torch", 10);
            CreateAndEquipInventoryItem(clientInventoryComponent, 11, "Anvil", 1);
            CreateAndEquipInventoryItem(clientInventoryComponent, 13, "Camp_Fire", 1);
            CreateAndEquipInventoryItem(clientInventoryComponent, 14, "Metal_Dagger", 1);
            CreateAndEquipInventoryItem(clientInventoryComponent, 15, "Metal_Sword", 1);
            CreateAndEquipInventoryItem(clientInventoryComponent, 16, "Metal_Pickaxe", 1);

            CreateAndEquipInventoryItem(clientInventoryComponent, 17, "Stone", 10);
            CreateAndEquipInventoryItem(clientInventoryComponent, 18, "Clay", 10);

            CreateAndEquipInventoryItem(clientInventoryComponent, 19, "Sharp_Rock", 10);

            //
        }
    }

    /// <summary>
    /// Helper method to create and equip an inventory item.
    /// </summary>
    private void CreateAndEquipInventoryItem(ClientInventoryComponent inventory, int slot, string itemName, int count = 1)
    {
        if (_entityManager.TryCreateEntity("BasicInventoryItem", out var item))
        {
            if (item.TryGetComponent<InventoryItemComponent>(out var inventoryItemComponent))
            {
                inventoryItemComponent.InventorySlotData.Name = Util.ComputeCrc32(itemName);
                inventoryItemComponent.InventorySlotData.Count = count;
                inventoryItemComponent.InventorySlotData.ItemUUID = Util.NewGuid();
            }

            inventory.InventoryEntityList[slot] = item.Id;
        }
    }
}

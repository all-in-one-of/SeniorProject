﻿using UnityEngine;
using System.Collections;

public static class GuiInstanceManager
{
	/// <summary>
	/// The recipe page instance.
	/// The instance is set in RecipePageBehavior.cs's SetUpRecipePage function.
	/// </summary>
	public static RecipePageBehavior RecipePageInstance;

	/// <summary>
	/// The inventory user interface instance.
	/// The instance is set in InventoryUI.cs's Awake function. 
	/// And the TargetInventory property is set in that InventoryUI.cs's Start function.
	/// </summary>
	public static InventoryUI InventoryUiInstance;

	/// <summary>
	/// The interface used to transfer items between containers and player inventory.
	/// </summary>
	public static InventoryTransferUI InventoryTransferInstance;

	/// <summary>
	/// The item amount panel instance. 
	/// The instance is set in ChooseItemAmountPanelBehavior.cs's Awake function.
	/// </summary>
	public static ChooseItemAmountPanelBehavior ItemAmountPanelInstance;

	/// <summary>
	/// The item stack detail panel instance.
	/// The instance is set in ItemStackDetailPanelBehavior.cs's Awake function.
	/// </summary>
	public static ItemStackDetailPanelBehavior ItemStackDetailPanelInstance;

	/// <summary>
	/// The panel that pops up when the user needs to select an item to execute a specific action, as prompted during item interaction.
	/// </summary>
	public static WorldSelectionGUIDirector WorldSelectionGuiInstance;

	/// <summary>
	/// The GUI handler that shows the equiped item.
	/// </summary>
	public static EquippedItemDropdown EquippedItemGuiInstance;

	/// <summary>
	/// The GUI that pops up to notify the player.
	/// </summary>
	public static PlayerNotification PlayerNotificationInstance;
}

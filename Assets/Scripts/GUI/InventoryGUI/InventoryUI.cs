﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour 
{
	[SerializeField]
	private string inventoryFile;
	[SerializeField]
	private string inventoryName;
	[SerializeField]
	private ItemStackUI baseItemUiTemplate;
	[SerializeField]
	private GameObject inventoryParentUI;

	[SerializeField]
	private GridLayoutManager inventoryLayoutManager;

	[SerializeField]
	[Tooltip("Filepath to the atlas containing sprites for items")]
	private string atlasFilepath;

	public bool isPlayerInventoryUIInstance;

	private List<ItemStack> inventory = new List<ItemStack>();
	private List<ItemStack> slots = new List<ItemStack> ();
	private List<ItemStackUI> itemStackUIList = new List<ItemStackUI> ();

	/// <summary>
	/// The target inventory that will be shown by the UI.
	/// </summary>
	public Inventory AssociatedInventory;

	/// <summary>
	/// Gets or sets the items to discard.
	/// </summary>
	/// <value>The items to discard.</value>
	public List<ItemStack> ItemsToDiscard
	{
		get;
		set;
	}

	/// <summary>
	/// Gets the item sprite manager.
	/// </summary>
	/// <value>The item sprite manager.</value>
	public SpriteManager ItemSpriteManager
	{
		get;
		private set;
	}

	/// <summary>
	/// Awake this instance of InventoryUi.
	/// </summary>
	void Awake()
	{
		if(isPlayerInventoryUIInstance)
		{
			GuiInstanceManager.InventoryUiInstance = this;
		}

		ItemSpriteManager = new SpriteManager(atlasFilepath);
	}

	/// <summary>
	/// Start this instance of Inventory Wrapper.
	/// </summary>
	void Start () 
	{
		ItemsToDiscard = new List<ItemStack>();

		if(isPlayerInventoryUIInstance)
		{
			GuiInstanceManager.InventoryUiInstance.AssociatedInventory = Game.Instance.PlayerInstance.Inventory;

			if(itemStackUIList.Count == 0)
			{
				LoadNewInventory(AssociatedInventory);
			}

			DisplayInventory();
		}
	}

	/// <summary>
	/// Displays the inventory.
	/// </summary>
	public void DisplayInventory()
	{
		for (int i = 0; i < AssociatedInventory.InventorySize; ++i) 
		{
			slots [i] = inventory [i];
			if (slots [i].Item != null) 
			{
				// fill in data for UI element from stack - add item sprite once they're available
				itemStackUIList [i].SetUpInventoryItem(inventory[i]);
			}
		}

		inventoryLayoutManager.CheckGridSize();
	}

	/// <summary>
	/// Refreshes the inventory display and updates the slots on the panel.
	/// </summary>
	public void RefreshInventoryPanel()
	{
		ItemStack[] newContents = AssociatedInventory.GetInventory ();
		inventoryLayoutManager.CheckGridSize();

		for (int i = 0; i < newContents.Length; ++i) 
		{
			// if item has been removed from inventory...
			if(newContents[i] == null && itemStackUIList[i] != null && slots[i] != null)
			{
				UpdateRemovedStackInUI (i);
			}
			// if item has been added to inventory...
			else if(newContents[i] != null && slots[i].Item == null)
			{
				UpdatedAddedStackInUI (newContents [i], i);
			}
			// if item has been updated...
			else if(newContents[i] != null && itemStackUIList[i] != null && (newContents[i].Id != itemStackUIList[i].GetStack().Id 
				|| !newContents[i].Item.ItemName.Equals(itemStackUIList[i].GetStack().Item.ItemName) 
				|| newContents[i].Amount != itemStackUIList[i].GetStack().Amount))
			{
				UpdateStackInformationInUI (newContents [i], i);
			}
		}
	}

	/// <summary>
	/// Updates the removed stack in UI.
	/// </summary>
	/// <param name="currentInventoryIndex">Current inventory index.</param>
	private void UpdateRemovedStackInUI(int currentInventoryIndex)
	{
		// get current item's sibling index in UI and then destroy it
		int slotIndex = itemStackUIList[currentInventoryIndex].gameObject.transform.GetSiblingIndex();

		if (inventory[currentInventoryIndex] != null && inventory [currentInventoryIndex].Item != null) 
		{
			// unsubscribes from events to avoid being triggered later
			itemStackUIList [currentInventoryIndex].Unsubscribe(inventory[currentInventoryIndex]);
			inventory[currentInventoryIndex] = null;
		}

		GameObject.Destroy(itemStackUIList[currentInventoryIndex].gameObject);

		// add empty slot item at that index in ui list
		ItemStackUI newItemUI = GameObject.Instantiate (baseItemUiTemplate);
		itemStackUIList [currentInventoryIndex] = newItemUI;
		slots [currentInventoryIndex] = new ItemStack ();

		// place empty ui slot back in position in grid layout
		newItemUI.gameObject.SetActive (true);
		newItemUI.transform.SetParent (inventoryParentUI.transform, false);
		newItemUI.transform.SetSiblingIndex(slotIndex);
	}

	/// <summary>
	/// Loads a new inventory into the ui panel.
	/// </summary>
	/// <param name="newInventory">New inventory.</param>
	public void LoadNewInventory(Inventory newInventory)
	{
		ItemStack[] contents = newInventory.GetInventory ();

		if(itemStackUIList.Count > 0)
		{
			for (int i = 0; i < itemStackUIList.Count; ++i) 
			{
				if (inventory[i] != null && inventory [i].Item != null) 
				{
					// unsubscribes from events to avoid being triggered later
					itemStackUIList [i].Unsubscribe(inventory[i]);
				}

				GameObject.Destroy(itemStackUIList[i].gameObject);
			}
		}

		// create empty slots
		slots.Clear();
		inventory.Clear();
		itemStackUIList.Clear();

		for (int i = 0; i < newInventory.InventorySize; ++i) 
		{
			slots.Add (new ItemStack());
			inventory.Add (new ItemStack ());
			ItemStackUI newItemUI = GameObject.Instantiate (baseItemUiTemplate);
			itemStackUIList.Add(newItemUI);

			// place empty ui slots in grid layout
			newItemUI.gameObject.SetActive (true);
			newItemUI.transform.SetParent (inventoryParentUI.transform, false);

			if (contents [i] != null)
			{
				inventory[i] = contents[i];
			}
		}

		AssociatedInventory = newInventory;
		DisplayInventory();
	}

	/// <summary>
	/// Updateds the added stack in UI.
	/// </summary>
	/// <param name="newStackFromInventory">New stack from inventory.</param>
	/// <param name="currentInventoryIndex">Current inventory index.</param>
	private void UpdatedAddedStackInUI(ItemStack newStackFromInventory, int currentInventoryIndex)
	{
		// add slot item at that index in ui list
		itemStackUIList [currentInventoryIndex].SetUpInventoryItem (newStackFromInventory);
		slots [currentInventoryIndex] = newStackFromInventory;

		if(inventory.Count > currentInventoryIndex)
		{
			inventory[currentInventoryIndex] = newStackFromInventory;
		}
		else
		{
			inventory.Add(newStackFromInventory);
		}
	}

	/// <summary>
	/// Updates the stack information in UI.
	/// </summary>
	/// <param name="updatedStackFromInventory">Updated stack from inventory.</param>
	/// <param name="currentInventoryIndex">Current inventory index.</param>
	private void UpdateStackInformationInUI(ItemStack updatedStackFromInventory, int currentInventoryIndex)
	{
		itemStackUIList[currentInventoryIndex].RefreshInventoryItem(updatedStackFromInventory);
		slots [currentInventoryIndex] = updatedStackFromInventory;

		if(inventory.Count > currentInventoryIndex)
		{
			inventory[currentInventoryIndex] = updatedStackFromInventory;
		}
		else
		{
			inventory.Add(updatedStackFromInventory);
		}
	}

	/// <summary>
	/// Gets the stack UI component given a stack id.
	/// </summary>
	/// <returns>The stack U.</returns>
	/// <param name="id">Identifier.</param>
	public ItemStackUI GetStackUI(string id)
	{
		for(int i = 0; i < itemStackUIList.Count; ++i)
		{
			if(itemStackUIList[i].GetStack().Id.Equals(id))
			{
				return itemStackUIList[i];
			}
		}

		return null;
	}
}

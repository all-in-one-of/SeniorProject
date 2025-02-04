﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Item amount affect panel behavior.
/// </summary>
public class ChooseItemAmountPanelBehavior : MonoBehaviour 
{
	[Tooltip("number displayed that shows how many units will be affected by default it is 1")]
	[SerializeField]
	private Text NumDisplay;

	[Tooltip("button that subtracts from the number of units affected when clicked")]
	[SerializeField]
	private Button Minus;

	[Tooltip("button that adds to the number of units affected when clicked")]
	[SerializeField]
	private Button Plus;

	[SerializeField]
	private Button Ok;

	[SerializeField]
	private Button Cancel;

	[SerializeField]
	private Button Close;

	[Tooltip("text that shows item's name")]
	public Text ItemNameDisplay;

	[SerializeField]
	private GameObject itemStackDetailPanel;

	[SerializeField]
	private GameObject itemAmountAffectPanel;

	[SerializeField]
	private GameObject attributesAndActionPanel;

    // the current number of units selected
    public int CurrentAmount
    {
        get;
        set;
    }

    //the maximum number of units that can be selected for the current item
    private int maxAmount;

	// the current selected item from the inventory
	public GameObject SelectedItem
	{
		get;
		set;
	}

	private ItemActionButtonUI selectedActionButton;

	/// <summary>
	/// Awake this instance of ChooseItemAmountPanelBehavior.
	/// </summary>
	void Awake()
	{
		GuiInstanceManager.ItemAmountPanelInstance = this;
	}

	/// <summary>
	/// Change the number of units to affect. Updates the number displayed and the number saved in the backend.
	/// </summary>
	/// <param name="amt">The amount to change. Typically either 1 or -1.</param>
	public void Change(int amt)
	{
		CurrentAmount += amt;
		//update text on the panel
		NumDisplay.text = CurrentAmount.ToString();
		// if there is no more to subtract, then disable the minus button
		if (CurrentAmount < 1) 
		{
			Minus.gameObject.SetActive (false);
		} 
		else if (CurrentAmount >= maxAmount) 
		{
			// if there is no more to add, disable the plus button
			Plus.gameObject.SetActive (false);
		} 
		else if (!Minus.gameObject.activeInHierarchy) 
		{
			// if the minus button is disabled but should not be then enable it
			Minus.gameObject.SetActive (true);
		} 
		else if (!Plus.gameObject.activeInHierarchy) 
		{
			// if the plus button is disabled but should not be then enable it
			Plus.gameObject.SetActive (true);
		}

		if (CurrentAmount == 0) 
		{
			Ok.gameObject.SetActive (false);
		} 
		else 
		{
			Ok.gameObject.SetActive (true);
		}
	}

	/// <summary>
	/// Opens the panel that allows users to select the desired number of items to affect with the action. 
	/// Clears the information left behind from the last time it was open. Sets
	/// the max amount to be the number of units available from the selected item.
	/// </summary>
	public void OpenItemAmountPanel()
	{
		ItemNameDisplay.text = SelectedItem.GetComponent<ItemStackUI>().ItemName.text;
		maxAmount = (int)SelectedItem.GetComponent<ItemStackUI>().GetMaxAmount();

		// if there is more than 1 of an item, then ask user to choose how many to affect
		if(maxAmount > 1)
		{
			CurrentAmount = 0;
			NumDisplay.text = CurrentAmount.ToString();

			Close.gameObject.SetActive (false);

			itemAmountAffectPanel.SetActive (true);

			Minus.gameObject.SetActive (true);
			Plus.gameObject.SetActive (true);

			// checks to see if the buttons should be activated or not
			// by default they are activated since 1 is the default number of
			// units, however if there is only 1 unit of that item, then 
			// the plus sign should not be displayed
			Change (0);
		}
		else
		{
			// if there's only one, automatically fire off the action
			CurrentAmount = 1;
			FinalizeAction();
		}
	}

	/// <summary>
	/// Close the panel.
	/// </summary>
	public void CancelChosenAmount()
	{
		itemAmountAffectPanel.SetActive (false);
		attributesAndActionPanel.SetActive (true);
		Close.gameObject.SetActive (true);
	}

	/// <summary>
	/// Closes the entire item info panel.
	/// </summary>
	public void CloseEntirePanel()
	{
		itemStackDetailPanel.SetActive (false);
	}

	/// <summary>
	/// Opens the item detail panel.
	/// </summary>
	/// <param name="selected">Selected.</param>
	public void OpenItemDetailPanel(GameObject selected)
	{
		ItemStackUI selectedItemUI = selected.GetComponent<ItemStackUI> ();

		if(selectedItemUI.GetStack() != null)
		{
			SelectedItem = selected;
			itemStackDetailPanel.SetActive (true);
			attributesAndActionPanel.SetActive (true);
			itemAmountAffectPanel.SetActive (false);

			ItemNameDisplay.text = selectedItemUI.ItemName.text;
			CurrentAmount = 0;
			NumDisplay.text = CurrentAmount.ToString();

			// checks to see if the buttons should be activated or not
			// by default they are activated since 1 is the default number of
			// units, however if there is only 1 unit of that item, then 
			// the plus sign should not be displayed
			Change (0);

			GuiInstanceManager.ItemStackDetailPanelInstance.SetSelectedItem (SelectedItem);
			GuiInstanceManager.ItemStackDetailPanelInstance.ClearAttributesAndActions();
			GuiInstanceManager.ItemStackDetailPanelInstance.ClearSubActionPanel();
			GuiInstanceManager.ItemStackDetailPanelInstance.SetItemData(SelectedItem);
			attributesAndActionPanel.SetActive (true);
		}
	}

	/// <summary>
	/// Chooses the number of items to affect.
	/// </summary>
	public void ChooseNumOfItemsToAffect(ItemActionButtonUI actionButton)
	{
		selectedActionButton = actionButton;
		OpenItemAmountPanel();
	}

	/// <summary>
	/// Executes the action.
	/// </summary>
	public void FinalizeAction()
	{
		GuiInstanceManager.ItemStackDetailPanelInstance.UpdateSelectedAmount(CurrentAmount);
        selectedActionButton.PerformAction();
		Close.gameObject.SetActive (true);
		itemAmountAffectPanel.SetActive (false);
	}
}

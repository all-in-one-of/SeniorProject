﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IngredientButtonBehavior : MonoBehaviour 
{
	[SerializeField]
	private Text itemNameText;

	[SerializeField]
	private Text itemAmountText;

	[Tooltip("Color the button should be when it can no longer be selected")]
	[SerializeField]
	private Color DisabledColor;

	[SerializeField]
	private GameObject selectedItemTemplate;

	[SerializeField]
	private GameObject selectedIngredientsPanel;

	private int itemAmount;
	private Color originalColor;
	private Image buttonImage;
	private Button button;
	private string associatedStackId;

	/// <summary>
	/// Delegate function that takes in a baseItem
	/// </summary>
	public delegate void UpdateIngredientEvent (); 

	/// <summary>
	/// Sets up ingredient.
	/// </summary>
	/// <param name="name">Name of item.</param>
	/// <param name="amount">Amount of item.</param>
	/// <param name="stackId">Stack identifier of the stack this represents.</param>
	public void SetUpIngredient(string name, int amount, string stackId)
	{
		itemNameText.text = name;
		itemAmountText.text = amount.ToString();
		itemAmount = amount;
		gameObject.name = name;

		buttonImage = GetComponent<Image>();
		button = GetComponent<Button>();

		originalColor = buttonImage.color;
		associatedStackId = stackId;
	}

	/// <summary>
	/// Selects the ingredient.
	/// </summary>
	public void SelectIngredient()
	{
		--itemAmount;

		if(itemAmount <= 0)
		{
			buttonImage.color = DisabledColor;
			button.interactable = false;
		}

		itemAmountText.text = itemAmount.ToString();

		if (UpdateIngredientSelection != null) 
		{
			UpdateIngredientSelection ();
		}
		else
		{
			SelectedIngredientButton selectedButton = GameObject.Instantiate(selectedItemTemplate).GetComponent<SelectedIngredientButton>();
			selectedButton.gameObject.SetActive(true);
			selectedButton.SetUpSelection(this, itemNameText.text);
			selectedButton.transform.SetParent(selectedIngredientsPanel.transform, false);
			selectedButton.AssociatedStackId = associatedStackId;
		}
	}

	/// <summary>
	/// Deselects the ingredient.
	/// </summary>
	public void DeselectIngredient()
	{
		if(itemAmount < 1)
		{
			button.interactable = true;
			buttonImage.color = originalColor;
		}

		++itemAmount;
		itemAmountText.text = itemAmount.ToString();
	}

	/// <summary>
	/// Event that can be subscribed to by functions of UpdateItemEvent format
	/// </summary>
	public event UpdateIngredientEvent UpdateIngredientSelection;
}

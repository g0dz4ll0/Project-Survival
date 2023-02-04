using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // --- Este item é descartável? --- //
    public bool isTrashable;

    // --- Interface de Informações Sobre o Item --- //
    private GameObject itemInfoUI;

    private Text itemInfoUI_itemName;
    private Text itemInfoUI_itemDescription;
    private Text itemInfoUI_itemFunctionality;

    public string thisName, thisDescription, thisFunctionality;

    // --- Consumo --- //
    private GameObject itemPendingConsumption;
    public bool isConsumable;

    public float healthEffect;
    public float caloriesEffect;
    public float hydrationEffect;

    // --- Equipar --- //
    public bool isEquippable;
    private GameObject itemPendingEquipping;
    public bool isInsideQuickSlot;

    public bool isSelected;

    // --- Fundição --- //
    public bool isFuel;
    public bool isOre;
    public bool isIngot;

    public bool isInsideFuelSlot;
    public bool isInsideOreSlot;
    public bool isInsideIngotSlot;

    public bool isUsable;

    private void Awake()
    {
        itemInfoUI = InventorySystem.Instance.ItemInfoUI;
        itemInfoUI_itemName = itemInfoUI.transform.Find("itemName").GetComponent<Text>();
        itemInfoUI_itemDescription = itemInfoUI.transform.Find("itemDescription").GetComponent<Text>();
        itemInfoUI_itemFunctionality = itemInfoUI.transform.Find("itemFunctionality").GetComponent<Text>();
    }

    void Update()
    {
        if (isSelected)
        {
            gameObject.GetComponent<DragDrop>().enabled = false;
        }
        else
        {
            gameObject.GetComponent<DragDrop>().enabled = true;
        }
    }

// Ativado quando o cursor passa em cima da área do item que contém este script
public void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.ShowTooltip_Static(thisName, thisDescription, thisFunctionality);
    }

    // Ativado quando o cursor sai da área do item que contém este script
    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.HideTooltip_Static();
    }

    // Ativado quando se carrega no rato em cima do item que contém este script
    public void OnPointerDown(PointerEventData eventData)
    {
        //Se carregar com o botão direito do rato
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isConsumable)
            {
                // Definir este gameobject específico no item que queremos destruir mais tarde
                itemPendingConsumption = gameObject;

                consumingFunction(healthEffect, caloriesEffect, hydrationEffect);
            }

            if (isEquippable && EquipSystem.Instance.CheckIfFull() == false)
            {
                if (!isInsideQuickSlot)
                {
                    EquipSystem.Instance.AddToQuickSlots(gameObject);
                    isInsideQuickSlot = true;
                }
                else
                {
                    InventorySystem.Instance.AddToSlot(gameObject);
                    isInsideQuickSlot = false;
                }
            }

            if (SmeltSystem.Instance.isOpen)
            {
                if (isFuel && !SmeltSystem.Instance.isFuelSlotFull)
                {
                    SmeltSystem.Instance.AddToFuelSlot(gameObject);
                    isInsideFuelSlot = true;
                }

                if (isOre && !SmeltSystem.Instance.isOreSlotFull)
                {
                    SmeltSystem.Instance.AddToOreSlot(gameObject);
                    isInsideOreSlot = true;
                }

                if (isIngot && SmeltSystem.Instance.isIngotSlotFull)
                {
                    InventorySystem.Instance.AddToSlot(gameObject);
                    isInsideIngotSlot = false;
                    SmeltSystem.Instance.isIngotSlotFull = false;
                }
            }

            if (isUsable)
            {
                ConstructionManager.Instance.itemToBeDestroyed = gameObject;
                gameObject.SetActive(false);
                UseItem();
            }
        }
    }

    private void UseItem()
    {
        itemInfoUI.SetActive(false);

        InventorySystem.Instance.isOpen = false;
        InventorySystem.Instance.inventoryScreenUI.SetActive(false);

        CraftingSystem.Instance.isOpen = false;
        CraftingSystem.Instance.craftingScreenUI.SetActive(false);
        CraftingSystem.Instance.toolsScreenUI.SetActive(false);
        CraftingSystem.Instance.survivalScreenUI.SetActive(false);
        CraftingSystem.Instance.refineScreenUI.SetActive(false);
        CraftingSystem.Instance.constructionScreenUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SelectionManager.Instance.EnableSelection();
        SelectionManager.Instance.enabled = true;

        switch (gameObject.name)
        {
            case "Foundation(Clone)":
                ConstructionManager.Instance.ActivateConstructionPlacement("FoundationModel");
                break;
            case "Foundation":
                ConstructionManager.Instance.ActivateConstructionPlacement("FoundationModel"); //Para testar
                break;
            case "Wall(Clone)":
                ConstructionManager.Instance.ActivateConstructionPlacement("WallModel");
                break;
            case "Wall":
                ConstructionManager.Instance.ActivateConstructionPlacement("WallModel"); //Para testar
                break;
            case "DoorFrame(Clone)":
                ConstructionManager.Instance.ActivateConstructionPlacement("DoorFrameModel");
                break;
            case "DoorFrame":
                ConstructionManager.Instance.ActivateConstructionPlacement("DoorFrameModel"); //Para testar
                break;
            default:
                // não fazer nada
                break;
        }

    }

    // Ativado quando se larga o rato em cima do item que contém este script
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isConsumable && itemPendingConsumption == gameObject)
            {
                DestroyImmediate(gameObject);
                InventorySystem.Instance.ReCalculateList();
                CraftingSystem.Instance.RefreshNeededItems();
            }
        }
    }

    private void consumingFunction(float healthEffect, float caloriesEffect, float hydrationEffect)
    {
        Tooltip.HideTooltip_Static();

        healthEffectCalculation(healthEffect);

        caloriesEffectCalculation(caloriesEffect);

        hydrationEffectCalculation(hydrationEffect);
    }

    private static void healthEffectCalculation(float healthEffect)
    {
        // -- Vida -- //

        float healthBeforeConsumption = PlayerState.Instance.currentHealth;
        float maxHealth = PlayerState.Instance.maxHealth;

        if (healthEffect != 0)
        {
            if ((healthBeforeConsumption + healthEffect) > maxHealth)
            {
                PlayerState.Instance.setHealth(maxHealth);
            }
            else
            {
                PlayerState.Instance.setHealth(healthBeforeConsumption + healthEffect);
            }
        }
    }

    private static void caloriesEffectCalculation(float caloriesEffect)
    {
        // -- Calorias -- //

        float caloriesBeforeConsumption = PlayerState.Instance.currentCalories;
        float maxCalories = PlayerState.Instance.maxCalories;

        if (caloriesEffect != 0)
        {
            if ((caloriesBeforeConsumption + caloriesEffect) > maxCalories)
            {
                PlayerState.Instance.setCalories(maxCalories);
            }
            else
            {
                PlayerState.Instance.setCalories(caloriesBeforeConsumption + caloriesEffect);
            }
        }
    }

    private static void hydrationEffectCalculation(float hydrationEffect)
    {
        // -- Hidratação -- //

        float hydrationBeforeConsumption = PlayerState.Instance.currentHydrationPercent;
        float maxHydration = PlayerState.Instance.maxHydrationPercent;

        if (hydrationEffect != 0)
        {
            if ((hydrationBeforeConsumption + hydrationEffect) > maxHydration)
            {
                PlayerState.Instance.setHydration(maxHydration);
            }
            else
            {
                PlayerState.Instance.setHydration(hydrationBeforeConsumption + hydrationEffect);
            }
        }
    }
}

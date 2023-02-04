using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmeltSystem : MonoBehaviour
{
    public static SmeltSystem Instance { get; set; }

    // -- UI -- //
    public GameObject smeltUIPanel;

    public GameObject fuelSlot;
    public GameObject oreSlot;
    public GameObject ingotSlot;

    public bool isFuelSlotFull;
    public bool isOreSlotFull;
    public bool isIngotSlotFull;

    string cleanOreName;
    string cleanFuelName;
    string cleanIngotName;

    GameObject fuelItemToRemove;
    GameObject oreItemToRemove;

    private GameObject ingotItem;

    public bool isOpen;

    public float timeNeededToSmeltIron = 10.0f;
    public float remainingTime;

    public void AddToFuelSlot(GameObject fuelItem)
    {
        // Definir o local do nosso objeto
        fuelItem.transform.SetParent(fuelSlot.transform, false);
        // Guardar o item para remover mais tarde
        fuelItemToRemove = fuelItem; 
        // Arranjar um nome limpo
        cleanFuelName = fuelItem.name.Replace("(Clone)", "");

        isFuelSlotFull = true;

        InventorySystem.Instance.ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItems();

        VerifySmelt();
    }

    public void AddToOreSlot(GameObject oreItem)
    {
        // Definir o local do nosso objeto
        oreItem.transform.SetParent(oreSlot.transform, false);
        // Guardar o item para remover mais tarde
        oreItemToRemove = oreItem;
        // Arranjar um nome limpo
        cleanOreName = oreItem.name.Replace("(Clone)", "");

        isOreSlotFull = true;

        VerifySmelt();
    }

    public void AddToIngotSlot()
    {
        ingotItem = Instantiate(Resources.Load<GameObject>("Iron_Ingot"), ingotSlot.transform.position, ingotSlot.transform.rotation);
        ingotItem.transform.SetParent(ingotSlot.transform);
        cleanIngotName = ingotItem.name.Replace("(Clone)", "");

        ingotItem.GetComponent<InventoryItem>().isInsideIngotSlot = true;
        isIngotSlotFull = true;
    }

    public void VerifySmelt()
    {
        if (isOreSlotFull && isFuelSlotFull && !isIngotSlotFull)
        {
            if (cleanOreName == "Iron_Ore")
            {
                StartCoroutine(StartSmelting(cleanOreName));
            }
        }
    }

    IEnumerator StartSmelting(string oreName)
    {
        if (oreName == "Iron_Ore")
        {
            float duration = timeNeededToSmeltIron;

            remainingTime = duration;
            while(remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            DestroyImmediate(fuelItemToRemove);
            DestroyImmediate(oreItemToRemove);

            isFuelSlotFull = false;
            isOreSlotFull = false;

            remainingTime = 10;

            AddToIngotSlot();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            smeltUIPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SelectionManager.Instance.DisableSelection();
            SelectionManager.Instance.GetComponent<SelectionManager>().enabled = false;

            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.E) && isOpen)
        {
            smeltUIPanel.SetActive(false);

            if (!InventorySystem.Instance.isOpen && !CraftingSystem.Instance.isOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                SelectionManager.Instance.EnableSelection();
                SelectionManager.Instance.GetComponent<SelectionManager>().enabled = true;
            }

            isOpen = false;
        }
    }
}

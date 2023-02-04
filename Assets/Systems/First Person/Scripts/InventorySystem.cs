using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveLoadSystem;

[RequireComponent(typeof(SaveableEntity))]
public class InventorySystem : MonoBehaviour, ISaveable
{
    public GameObject ItemInfoUI;

    public static InventorySystem Instance { get; set; }

    public GameObject inventoryScreenUI;

    public List<GameObject> slotList = new List<GameObject>();

    [SerializeField] public List<string> itemList = new List<string>();

    private GameObject itemToAdd;

    private GameObject whatSlotToEquip;

    public bool isOpen;

    //public bool isFull;

    //Pickup Popup
    public GameObject pickupAlert;
    public Text pickupName;
    public Image pickupImage;

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

    void Start()
    {
        isOpen = false;

        PopulateSlotList();

        Cursor.visible = false;
    }

    private void PopulateSlotList()
    {
        foreach (Transform child in inventoryScreenUI.transform)
        {
            if (child.CompareTag("Slot"))
            {
                slotList.Add(child.gameObject);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isOpen && !ConstructionManager.Instance.inConstructionMode)
        {
            inventoryScreenUI.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SelectionManager.Instance.DisableSelection();
            SelectionManager.Instance.GetComponent<SelectionManager>().enabled = false;

            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.I) && isOpen)
        {
            inventoryScreenUI.SetActive(false);

            if (!CraftingSystem.Instance.isOpen && !SmeltSystem.Instance.isOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                SelectionManager.Instance.EnableSelection();
                SelectionManager.Instance.GetComponent<SelectionManager>().enabled = true;
            }

            isOpen = false;
        }
    }

    public void AddToInventory(string itemName)
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.pickupItemSound);

        whatSlotToEquip = FindNextEmptySlot();

        itemToAdd = Instantiate(Resources.Load<GameObject>(itemName), whatSlotToEquip.transform.position, whatSlotToEquip.transform.rotation);
        itemToAdd.transform.SetParent(whatSlotToEquip.transform);

        itemList.Add(itemName);

        TriggerPickupPopUp(itemName, itemToAdd.GetComponent<Image>().sprite);

        ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItems();
    }

    public void AddToSlot(GameObject itemToAdd)
    {
        string cleanName;

        whatSlotToEquip = FindNextEmptySlot();

        itemToAdd.transform.SetParent(whatSlotToEquip.transform, false);

        cleanName = itemToAdd.name.Replace("(Clone)", "");

        itemList.Add(itemToAdd.name);

        ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItems();
    }

    void TriggerPickupPopUp(string itemName, Sprite itemSprite)
    {
        pickupAlert.SetActive(true);

        pickupName.text = itemName;
        pickupImage.sprite = itemSprite;

        StartCoroutine(DeactivatePopUp());
    }

    IEnumerator DeactivatePopUp()
    {
        yield return new WaitForSeconds(2);
        pickupAlert.SetActive(false);
    }

    private GameObject FindNextEmptySlot()
    {
        foreach(GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }

        return new GameObject();
    }

    public bool CheckSlotsAvailable(int emptyNeeded)
    {
        int emptySlot = 0;

        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount <= 0)
            {
                emptySlot += 1;
            }
        }

        if (emptySlot >= emptyNeeded)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveItem(string nameToRemove, int amountToRemove)
    {
        int counter = amountToRemove;

        for (var i = slotList.Count - 1; i >= 0; i--)
        {
            if (slotList[i].transform.childCount > 0)
            {
                if (slotList[i].transform.GetChild(0).name == nameToRemove + "(Clone)" && counter != 0)
                {
                    Destroy(slotList[i].transform.GetChild(0).gameObject);

                    counter -= 1;
                }
            }
        }

        ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItems();
    }

    public void ReCalculateList()
    {
        itemList.Clear();

        foreach (GameObject slot in slotList)
        {
            {
                if (slot.transform.childCount > 0)
                {
                    string name = slot.transform.GetChild(0).name; //Stone (Clone)
                    string str2 = "(Clone)";
                    string result = name.Replace(str2, "");

                    itemList.Add(result);
                }
            }
        }
    }

    [System.Serializable]
    struct InventoryData
    {
        public List<string> itemList;
    }

    public bool NeedsToBeSaved()
    {
        return true;
    }

    public bool NeedsReinstantiation()
    {
        return false;
    }

    public object SaveState()
    {
        return new InventoryData()
        {
            itemList = itemList
        };
    }

    public void LoadState(object state)
    {
        InventoryData data = (InventoryData)state;

        this.itemList = data.itemList;
    }

    public void PostInstantiation(object state)
    {
        InventoryData data = (InventoryData)state;
    }

    public void GotAddedAsChild(GameObject obj, GameObject hisParent)
    {
        
    }
}
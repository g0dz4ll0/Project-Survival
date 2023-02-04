using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    public GameObject Item
    {
        get
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }

            return null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");

        //se ainda não houver nenhum item então definir o item
        if (!Item && !DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isInsideFuelSlot && !DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isInsideOreSlot)
        {
            if (transform.CompareTag("Slot"))
            {
                if (DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isEquippable && DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isInsideQuickSlot)
                {
                    DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isInsideQuickSlot = false;
                }

                if (DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isIngot)
                {
                    SmeltSystem.Instance.isIngotSlotFull = false;
                }

                SoundManager.Instance.PlaySound(SoundManager.Instance.dropItemSound);

                DragDrop.itemBeingDragged.transform.SetParent(transform);
                DragDrop.itemBeingDragged.transform.localPosition = new Vector2(0, 0);

                InventorySystem.Instance.ReCalculateList();
            }

            if (transform.CompareTag("QuickSlot") && DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isEquippable)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.dropItemSound);

                DragDrop.itemBeingDragged.transform.SetParent(transform);
                DragDrop.itemBeingDragged.transform.localPosition = new Vector2(0, 0);

                DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isInsideQuickSlot = true;
                InventorySystem.Instance.ReCalculateList();
            }

            if (transform.CompareTag("FuelSlot") && DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isFuel)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.dropItemSound);

                SmeltSystem.Instance.AddToFuelSlot(DragDrop.itemBeingDragged);

                DragDrop.itemBeingDragged.transform.SetParent(transform);
                DragDrop.itemBeingDragged.transform.localPosition = new Vector2(0, 0);

                DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isInsideFuelSlot = true;
                InventorySystem.Instance.ReCalculateList();
            }

            if (transform.CompareTag("OreSlot") && DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isOre)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.dropItemSound);

                SmeltSystem.Instance.AddToOreSlot(DragDrop.itemBeingDragged);

                DragDrop.itemBeingDragged.transform.SetParent(transform);
                DragDrop.itemBeingDragged.transform.localPosition = new Vector2(0, 0);

                DragDrop.itemBeingDragged.GetComponent<InventoryItem>().isInsideOreSlot = true;
                InventorySystem.Instance.ReCalculateList();
            }
        }
    }
}

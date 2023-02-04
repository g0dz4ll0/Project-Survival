using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance { get; set; }

    public GameObject itemToBeConstructed;
    public bool inConstructionMode = false;
    public GameObject constructionHoldingSpot;

    public bool isValidPlacement;

    public bool selectingAGhost;
    public GameObject selectedGhost;

    // Materiais guardados como referência para os fantasmas
    public Material ghostSelectMat;
    public Material ghostSemiTransparentMat; // Para testes
    public Material ghostFullTransparentMat;

    // Mantemos uma referência de todos os fantasmas no nosso mundo,
    // para que o gestor possa geri-los para várias operações
    public List<GameObject> allGhostsInExistence = new List<GameObject>();

    public GameObject itemToBeDestroyed;

    public GameObject player;

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

    public void ActivateConstructionPlacement(string itemToConstruct)
    {
        GameObject item = Instantiate(Resources.Load<GameObject>(itemToConstruct));

        //mudar o nome do objeto para que não tenha o "(Clone)"
        item.name = itemToConstruct;

        item.transform.SetParent(constructionHoldingSpot.transform, false);
        itemToBeConstructed = item;
        itemToBeConstructed.gameObject.tag = "activeConstructable";

        // Desativar a colisão para que o nosso rato consiga fazer cast a um ray
        itemToBeConstructed.GetComponent<Constructable>().solidCollider.enabled = false;

        // Ativar o modo de construção
        inConstructionMode = true;
    }

    private void GetAllGhosts(GameObject itemToBeConstructed)
    {
        List<GameObject> ghostList = itemToBeConstructed.gameObject.GetComponent<Constructable>().ghostList;

        foreach (GameObject ghost in ghostList)
        {
            Debug.Log(ghost);
            allGhostsInExistence.Add(ghost);
        }
    }

    private void PerformGhostDeletionScan()
    {
        foreach (GameObject ghost in allGhostsInExistence)
        {
            if (ghost != null)
            {
                if (ghost.GetComponent<GhostItem>().hasSamePosition == false) // se ainda não adicionamos uma flag
                {
                    foreach (GameObject ghostX in allGhostsInExistence)
                    {
                        // Primeiro verificamos se não é o mesmo objeto
                        if (ghost.gameObject != ghostX.gameObject)
                        {
                            // Se não for o mesmo objeto mas tiverem a mesma posição
                            if (XPositionToAccurateFloat(ghost) == XPositionToAccurateFloat(ghostX) && ZPositionToAccurateFloat(ghost) == ZPositionToAccurateFloat(ghostX))
                            {
                                if (ghost != null && ghostX != null)
                                {
                                    // definir a flag
                                    ghostX.GetComponent<GhostItem>().hasSamePosition = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (GameObject ghost in allGhostsInExistence)
        {
            if (ghost != null)
            {
                if (ghost.GetComponent<GhostItem>().hasSamePosition)
                {
                    DestroyImmediate(ghost);
                }
            }
        }
    }

    private float XPositionToAccurateFloat(GameObject ghost)
    {
        if (ghost != null)
        {
            // Tornar a posição para uma número decimal arredondado às duas décimas
            Vector3 targetPosition = ghost.gameObject.transform.position;
            float pos = targetPosition.x;
            float xFloat = Mathf.Round(pos * 100f) / 100f;
            return xFloat;
        }
        return 0;
    }

    private float ZPositionToAccurateFloat(GameObject ghost)
    {
        if (ghost != null)
        {
            // Tornar a posição para uma número decimal arredondado às duas décimas
            Vector3 targetPosition = ghost.gameObject.transform.position;
            float pos = targetPosition.z;
            float zFloat = Mathf.Round(pos * 100f) / 100f;
            return zFloat;
        }
        return 0;
    }

    private void Update()
    {
        if (itemToBeConstructed != null && inConstructionMode)
        {
            if (itemToBeConstructed.name == "FoundationModel")
            {
                if (CheckValidConstructionPosition())
                {
                    isValidPlacement = true;
                    itemToBeConstructed.GetComponent<Constructable>().SetValidColor();
                }
                else
                {
                    isValidPlacement = false;
                    itemToBeConstructed.GetComponent<Constructable>().SetInvalidColor();
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var selectionTransform = hit.transform;
                if (selectionTransform.gameObject.CompareTag("ghost") && itemToBeConstructed.name == "FoundationModel")
                {
                    itemToBeConstructed.SetActive(false);
                    selectingAGhost = true;
                    selectedGhost = selectionTransform.gameObject;
                }
                else if (selectionTransform.gameObject.CompareTag("wallGhost") && (itemToBeConstructed.name == "WallModel" || itemToBeConstructed.name == "DoorFrameModel"))
                {
                    itemToBeConstructed.SetActive(false);
                    selectingAGhost = true;
                    selectedGhost = selectionTransform.gameObject;
                }
                else
                {
                    itemToBeConstructed.SetActive(true);
                    selectedGhost = null;
                    selectingAGhost = false;
                }
            }
        }

        // Botão esquerdo do rato para colocar um item
        if (Input.GetMouseButtonDown(0) && inConstructionMode)
        {
            if (isValidPlacement && selectedGhost == false && itemToBeConstructed.name == "FoundationModel") // Não queremos o freestyle ativado quando selecionamos um fantasma.
            {
                PlaceItemFreeStyle();
                DestroyItem(itemToBeDestroyed);
            }

            if (selectingAGhost)
            {
                PlaceItemInGhostPosition(selectedGhost);
                DestroyItem(itemToBeDestroyed);
            }
        }
        // Botão direito do rato para cancelar                   //TODO - não destruir o UI do item até que o coloquemos.
        if (Input.GetKeyDown(KeyCode.X))
        {
            itemToBeDestroyed.SetActive(true);
            itemToBeDestroyed = null;
            DestroyItem(itemToBeConstructed);
            itemToBeConstructed = null;
            inConstructionMode = false;
        }
    }

    private void PlaceItemInGhostPosition(GameObject copyOfGhost)
    {
        Vector3 ghostPosition = copyOfGhost.transform.position;
        Quaternion ghostRotation = copyOfGhost.transform.rotation;

        selectedGhost.gameObject.SetActive(false);

        // Ativar o item de novo (depois de o desativarmos no ray cast)
        itemToBeConstructed.gameObject.SetActive(true);
        // Definir o parente para que este seja a raiz da nossa cena
        itemToBeConstructed.transform.SetParent(transform.parent.parent.parent, true); 

        itemToBeConstructed.transform.rotation = ghostRotation;

        // Ativar de volta a colisão sólida que desativamos anteriormente
        itemToBeConstructed.GetComponent<Constructable>().solidCollider.enabled = true;

        // Definir para a cor/material padrão
        itemToBeConstructed.GetComponent<Constructable>().SetDefaultColor();

        if (itemToBeConstructed.name == "FoundationModel")
        {
            itemToBeConstructed.transform.position = ghostPosition;

            // Fazer os filhos do fantasma não serem mais filhos deste item
            itemToBeConstructed.GetComponent<Constructable>().ExtractGhostMembers();
            itemToBeConstructed.tag = "placedFoundation";
            //Adicionar todos os fantasmas deste item ao banco do gestor
            GetAllGhosts(itemToBeConstructed);
            PerformGhostDeletionScan();
        }
        else if (itemToBeConstructed.name == "WallModel" || itemToBeConstructed.name == "DoorFrameModel")
        {
            itemToBeConstructed.transform.position = ghostPosition - new Vector3(0, 0.4f, 0);

            itemToBeConstructed.tag = "placedWall";
            DestroyItem(selectedGhost); //Removemos este fantasma da parede porque o gestor não o fará
        }

        itemToBeConstructed = null;

        inConstructionMode = false;
    }

    private void PlaceItemFreeStyle()
    {
        // Definir o parente para que seja a raiz da nossa cena
        itemToBeConstructed.transform.SetParent(transform.parent.parent.parent, true);

        // Fazer os filhos do fantasma não serem mais filhos deste item
        itemToBeConstructed.GetComponent<Constructable>().ExtractGhostMembers();
        // Definir para a cor/material padrão
        itemToBeConstructed.GetComponent<Constructable>().SetDefaultColor();
        itemToBeConstructed.tag = "placedFoundation";
        itemToBeConstructed.GetComponent<Constructable>().enabled = false;
        // Ativar de volta a colisão sólida que desativamos anteriormente
        itemToBeConstructed.GetComponent<Constructable>().solidCollider.enabled = true;

        //Adicionar todos os fantasmas deste item ao banco do gestor
        GetAllGhosts(itemToBeConstructed);
        PerformGhostDeletionScan();

        itemToBeConstructed = null;

        inConstructionMode = false;
    }

    void DestroyItem(GameObject item)
    {
        DestroyImmediate(item);
        InventorySystem.Instance.ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItems();
    }

    private bool CheckValidConstructionPosition()
    {
        if (itemToBeConstructed != null)
        {
            return itemToBeConstructed.GetComponent<Constructable>().isValidToBeBuilt;
        }

        return false;
    }
}

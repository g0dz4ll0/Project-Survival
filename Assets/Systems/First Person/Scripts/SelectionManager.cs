using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; set; }

    public bool onTarget;

    public GameObject selectedObject;

    public GameObject interaction_Info_UI;
    Text interaction_text;

    public Image centerDotImage;
    public Image pickIcon;

    public bool pickIsVisible;

    public GameObject selectedNode;
    public GameObject gathererHolder;
    public Text gatherable_text;

    private void Start()
    {
        onTarget = false;
        interaction_text = interaction_Info_UI.GetComponent<Text>();
    }

    private void Awake()
    {
        interaction_Info_UI = GameObject.Find("interaction_info_UI");
        interaction_Info_UI.SetActive(false);
        centerDotImage = GameObject.Find("Middle Reticle").GetComponent<Image>();
        pickIcon = GameObject.Find("PickIcon").GetComponent<Image>();
        pickIcon.gameObject.SetActive(false);

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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;

            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            GatherableNode gatherableNode = selectionTransform.GetComponent<GatherableNode>();

            if (gatherableNode && gatherableNode.playerInRange)
            {
                gatherableNode.canBeGathered = true;
                selectedNode = gatherableNode.gameObject;
                gatherable_text.text = gatherableNode.GetNodeName();
                gathererHolder.gameObject.SetActive(true);
            }
            else
            {
                if(selectedNode != null)
                {
                    selectedNode.gameObject.GetComponent<GatherableNode>().canBeGathered = false;
                    selectedNode = null;
                    gathererHolder.gameObject.SetActive(false);
                }
            }

            if (interactable && interactable.playerInRange)
            {
                onTarget = true;
                selectedObject = interactable.gameObject;
                interaction_text.text = interactable.GetItemName();
                interaction_Info_UI.SetActive(true);

                if (interactable.CompareTag("pickable"))
                {
                    centerDotImage.gameObject.SetActive(false);
                    pickIcon.gameObject.SetActive(true);

                    pickIsVisible = true;
                }
                else
                {
                    pickIcon.gameObject.SetActive(false);
                    centerDotImage.gameObject.SetActive(true);

                    pickIsVisible = false;
                }
            }
            else //se houver uma colisão, mas sem ter o script InteractableObject
            {
                onTarget = false;
                interaction_Info_UI.SetActive(false);
                pickIcon.gameObject.SetActive(false);
                centerDotImage.gameObject.SetActive(true);

                pickIsVisible = false;
            }
        }
        else //se não houver nenhuma colisão
        {
            onTarget = false;
            interaction_Info_UI.SetActive(false);
            pickIcon.gameObject.SetActive(false);
            centerDotImage.gameObject.SetActive(true);

            pickIsVisible = false;
        }
    }

    public void DisableSelection()
    {
        pickIcon.enabled = false;
        centerDotImage.enabled = false;
        interaction_Info_UI.SetActive(false);

        selectedObject = null;
    }

    public void EnableSelection()
    {
        pickIcon.enabled = true;
        centerDotImage.enabled = true;
        interaction_Info_UI.SetActive(true);
    }
}

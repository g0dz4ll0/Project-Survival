using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingScreenUI, toolsScreenUI, survivalScreenUI, refineScreenUI, constructionScreenUI;

    public List<string> inventoryItemList = new List<string>();

    //Butões de Categoria
    Button toolsBTN, survivalBTN, refineBTN, constructionBTN;

    //Butões de Crafting
    Button craftAxeBTN, craftPickAxeBTN, craftPlankBTN, craftFoundationBTN, craftWallBTN, craftWindowBTN, craftRoofBTN;

    //Texto de Requesitos
    Text AxeReq1, AxeReq2, PickAxeReq1, PickAxeReq2, PlankReq1, FoundationReq1, WallReq1, WindowReq1, RoofReq1;

    public bool isOpen;

    //Todos os Blueprints
    public Blueprint AxeBLP;
    public Blueprint PickAxeBLP;
    public Blueprint PlankBLP;
    public Blueprint FoundationBLP;
    public Blueprint WallBLP;
    public Blueprint WindowBLP;
    public Blueprint RoofBLP;

    public static CraftingSystem Instance { get; set; }

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

    //Start is called before the first frame update
    void Start()
    {
        isOpen = false;

        toolsBTN = craftingScreenUI.transform.Find("ToolsButton").GetComponent<Button>();
        toolsBTN.onClick.AddListener(delegate { OpenToolsCategory(); });

        survivalBTN = craftingScreenUI.transform.Find("SurvivalButton").GetComponent<Button>();
        survivalBTN.onClick.AddListener(delegate { OpenSurvivalCategory(); });

        refineBTN = craftingScreenUI.transform.Find("RefineButton").GetComponent<Button>();
        refineBTN.onClick.AddListener(delegate { OpenRefineCategory(); });

        constructionBTN = craftingScreenUI.transform.Find("ConstructionButton").GetComponent<Button>();
        constructionBTN.onClick.AddListener(delegate { OpenConstructionCategory(); });

        //Machado
        AxeReq1 = toolsScreenUI.transform.Find("Axe").transform.Find("req1").GetComponent<Text>();
        AxeReq2 = toolsScreenUI.transform.Find("Axe").transform.Find("req2").GetComponent<Text>();

        craftAxeBTN = toolsScreenUI.transform.Find("Axe").transform.Find("Button").GetComponent<Button>();
        craftAxeBTN.onClick.AddListener(delegate { CraftAnyItem(AxeBLP); });

        //Picareta
        PickAxeReq1 = toolsScreenUI.transform.Find("PickAxe").transform.Find("req1").GetComponent<Text>();
        PickAxeReq2 = toolsScreenUI.transform.Find("PickAxe").transform.Find("req2").GetComponent<Text>();

        craftPickAxeBTN = toolsScreenUI.transform.Find("PickAxe").transform.Find("Button").GetComponent<Button>();
        craftPickAxeBTN.onClick.AddListener(delegate { CraftAnyItem(PickAxeBLP); });

        //Tábua
        PlankReq1 = refineScreenUI.transform.Find("Plank").transform.Find("req1").GetComponent<Text>();

        craftPlankBTN = refineScreenUI.transform.Find("Plank").transform.Find("Button").GetComponent<Button>();
        craftPlankBTN.onClick.AddListener(delegate { CraftAnyItem(PlankBLP); });

        //Fundação
        FoundationReq1 = constructionScreenUI.transform.Find("Foundation").transform.Find("req1").GetComponent<Text>();

        craftFoundationBTN = constructionScreenUI.transform.Find("Foundation").transform.Find("Button").GetComponent<Button>();
        craftFoundationBTN.onClick.AddListener(delegate { CraftAnyItem(FoundationBLP); });

        //Parede
        WallReq1 = constructionScreenUI.transform.Find("Wall").transform.Find("req1").GetComponent<Text>();

        craftWallBTN = constructionScreenUI.transform.Find("Wall").transform.Find("Button").GetComponent<Button>();
        craftWallBTN.onClick.AddListener(delegate { CraftAnyItem(WallBLP); });

        //Janela
        WindowReq1 = constructionScreenUI.transform.Find("Window").transform.Find("req1").GetComponent<Text>();

        craftWindowBTN = constructionScreenUI.transform.Find("Window").transform.Find("Button").GetComponent<Button>();
        craftWindowBTN.onClick.AddListener(delegate { CraftAnyItem(WindowBLP); });

        //Teto
        RoofReq1 = constructionScreenUI.transform.Find("Roof").transform.Find("req1").GetComponent<Text>();

        craftRoofBTN = constructionScreenUI.transform.Find("Roof").transform.Find("Button").GetComponent<Button>();
        craftRoofBTN.onClick.AddListener(delegate { CraftAnyItem(RoofBLP); });
    }

    void OpenConstructionCategory()
    {
        craftingScreenUI.SetActive(false);
        refineScreenUI.SetActive(false);
        survivalScreenUI.SetActive(false);
        toolsScreenUI.SetActive(false);

        constructionScreenUI.SetActive(true);
    }

    void OpenToolsCategory()
    {
        craftingScreenUI.SetActive(false);
        refineScreenUI.SetActive(false);
        survivalScreenUI.SetActive(false);
        constructionScreenUI.SetActive(false);

        toolsScreenUI.SetActive(true);
    }

    private void OpenSurvivalCategory()
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(false);
        refineScreenUI.SetActive(false);
        constructionScreenUI.SetActive(false);

        survivalScreenUI.SetActive(true);
    }

    private void OpenRefineCategory()
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(false);
        survivalScreenUI.SetActive(false);
        constructionScreenUI.SetActive(false);

        refineScreenUI.SetActive(true);
    }

    void CraftAnyItem(Blueprint blueprintToCraft)
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.craftingSound);

        // Produzir a quantidade de items de acordo com o Blueprint
        for (var i = 0; i < blueprintToCraft.numberOfItemsToProduce; i++)
        {
            InventorySystem.Instance.AddToInventory(blueprintToCraft.itemName);
        }

        if (blueprintToCraft.numOfRequirements == 1)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1amount);
        }
        else if (blueprintToCraft.numOfRequirements == 2)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1amount);
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req2, blueprintToCraft.Req2amount);
        }

        StartCoroutine(calculate());
    }

    public IEnumerator calculate()
    {
        yield return 0; //Para não haver delay
        InventorySystem.Instance.ReCalculateList();
        RefreshNeededItems();
    }

    //Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isOpen && !ConstructionManager.Instance.inConstructionMode)
        {
            craftingScreenUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SelectionManager.Instance.DisableSelection();
            SelectionManager.Instance.GetComponent<SelectionManager>().enabled = false;

            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.C) && isOpen)
        {
            craftingScreenUI.SetActive(false);
            toolsScreenUI.SetActive(false);
            survivalScreenUI.SetActive(false);
            refineScreenUI.SetActive(false);
            constructionScreenUI.SetActive(false);

            if (!InventorySystem.Instance.isOpen && !SmeltSystem.Instance.isOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                SelectionManager.Instance.EnableSelection();
                SelectionManager.Instance.GetComponent<SelectionManager>().enabled = true;
            }

            isOpen = false;
        }
    }

    public void RefreshNeededItems()
    {
        int stone_count = 0;
        int stick_count = 0;
        int log_count = 0;
        int plank_count = 0;

        inventoryItemList = InventorySystem.Instance.itemList;

        foreach (string itemName in inventoryItemList)
        {
            switch (itemName)
            {
                case "Stone":
                    stone_count += 1;
                    break;
                case "Stick":
                    stick_count += 1;
                    break;
                case "Log":
                    log_count += 1;
                    break;
                case "Plank":
                    plank_count += 1;
                    break;
            }
        }

        // ---- MACHADO ---- //

        AxeReq1.text = "3 Stone [" + stone_count + "]";
        AxeReq2.text = "3 Stick [" + stick_count + "]";

        if (stone_count >= 3 && stick_count >= 3 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftAxeBTN.gameObject.SetActive(true);
        }
        else
        {
            craftAxeBTN.gameObject.SetActive(false);
        }

        // ---- PICARETA ---- //

        PickAxeReq1.text = "3 Stone [" + stone_count + "]";
        PickAxeReq2.text = "3 Stick [" + stick_count + "]";

        if (stone_count >= 3 && stick_count >= 3 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftPickAxeBTN.gameObject.SetActive(true);
        }
        else
        {
            craftPickAxeBTN.gameObject.SetActive(false);
        }

        // ---- TÁBUA X 2 ---- //

        PlankReq1.text = "1 Log [" + log_count + "]";

        if (log_count >= 1 && InventorySystem.Instance.CheckSlotsAvailable(2))
        {
            craftPlankBTN.gameObject.SetActive(true);
        }
        else
        {
            craftPlankBTN.gameObject.SetActive(false);
        }

        // ---- Fundação ---- //

        FoundationReq1.text = "4 Plank [" + plank_count + "]";

        if (plank_count >= 4 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftFoundationBTN.gameObject.SetActive(true);
        }
        else
        {
            craftFoundationBTN.gameObject.SetActive(false);
        }

        // ---- Parede ---- //

        WallReq1.text = "2 Plank [" + plank_count + "]";

        if (plank_count >= 2 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftWallBTN.gameObject.SetActive(true);
        }
        else
        {
            craftWallBTN.gameObject.SetActive(false);
        }

        // ---- Janela ---- //

        WindowReq1.text = "1 Plank [" + plank_count + "]";

        if (plank_count >= 1 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftWindowBTN.gameObject.SetActive(true);
        }
        else
        {
            craftWindowBTN.gameObject.SetActive(false);
        }

        // ---- Teto ---- //

        RoofReq1.text = "2 Plank [" + plank_count + "]";

        if (plank_count >= 2 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftRoofBTN.gameObject.SetActive(true);
        }
        else
        {
            craftRoofBTN.gameObject.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private static Tooltip instance;
    
    [SerializeField]
    private Camera uiCamera;

    private Text tooltipItemName;
    private Text tooltipItemDescription;
    private Text tooltipItemFunctionality;
    private RectTransform backgroundRectTransform;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
        backgroundRectTransform = transform.Find("background").GetComponent<RectTransform>();
        tooltipItemName = transform.Find("itemName").GetComponent<Text>();
        tooltipItemDescription = transform.Find("itemDescription").GetComponent<Text>();
        tooltipItemFunctionality = transform.Find("itemFunctionality").GetComponent<Text>();
    }

    private void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
        transform.localPosition = localPoint;
    }

    private void ShowTooltip(string tooltipStringName, string tooltipStringDescription, string tooltipStringFunctionality)
    {
        gameObject.SetActive(true);

        tooltipItemName.text = tooltipStringName;
        tooltipItemDescription.text = tooltipStringDescription;
        tooltipItemFunctionality.text = tooltipStringFunctionality;
        float textPaddingSizeHeight = 40f;
        float textPaddingSizeWidth = 25f;
        Vector2 backgroundSize = new Vector2(tooltipItemDescription.preferredWidth + textPaddingSizeWidth * 2f, tooltipItemName.preferredHeight + tooltipItemDescription.preferredHeight + tooltipItemFunctionality.preferredHeight + textPaddingSizeHeight * 2f);
        backgroundRectTransform.sizeDelta = backgroundSize;
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip_Static(string tooltipStringName, string tooltipStringDescription, string tooltipStringFunctionality)
    {
        instance.ShowTooltip(tooltipStringName, tooltipStringDescription, tooltipStringFunctionality);
    }

    public static void HideTooltip_Static()
    {
        instance.HideTooltip();
    }
}

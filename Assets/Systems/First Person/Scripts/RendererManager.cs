using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererManager : MonoBehaviour
{
    private GameObject itemActivatorObject;
    private ItemActivator activationScript;

    void Start()
    {
        itemActivatorObject = GameObject.Find("Player(Clone)");
        activationScript = itemActivatorObject.GetComponent<ItemActivator>();

        StartCoroutine("AddToList");
    }

    IEnumerator AddToList()
    {
        yield return new WaitForSeconds(0.1f);

        activationScript.addList.Add(new ActivatorItem { item = this.gameObject });
    }
}

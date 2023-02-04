using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EquipableItem : MonoBehaviour
{
    public Animator animator;
    public bool isAxe;
    public bool isPickAxe;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) &&  // Botão esquerdo do rato
            InventorySystem.Instance.isOpen == false &&
            CraftingSystem.Instance.isOpen == false &&
            SelectionManager.Instance.pickIsVisible == false &&
            !ConstructionManager.Instance.inConstructionMode
            )
        {
            StartCoroutine(SwingSoundDelay());

            animator.SetTrigger("hit");
        }
    }

    public void GetHit()
    {
        GameObject selectedNode = SelectionManager.Instance.selectedNode;

        if (selectedNode != null && selectedNode.CompareTag("Tree"))
        {
            if (isAxe)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.chopSound);

                selectedNode.GetComponent<GatherableNode>().GetHit();
            }
        }
        else if (selectedNode != null && selectedNode.CompareTag("Iron"))
        {
            if (isPickAxe)
            {
                selectedNode.GetComponent<GatherableNode>().GetHit();
            }
        }
    }

    IEnumerator SwingSoundDelay()
    {
        yield return new WaitForSeconds(0.2f);
        SoundManager.Instance.PlaySound(SoundManager.Instance.toolSwingSound);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherableNode : MonoBehaviour
{
    public bool playerInRange;
    public bool canBeGathered;

    public float nodeMaxHealth;
    public float nodeHealth;

    public string destroiedNodePrefab;

    public Animator animator;

    public float caloriesSpentGathering = 20;

    public string nodeName;

    private void Start()
    {
        nodeHealth = nodeMaxHealth;
        animator = transform.parent.transform.parent.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void GetHit()
    {
        animator.SetTrigger("shake");

        nodeHealth -= 1;

        PlayerState.Instance.currentCalories -= caloriesSpentGathering;

        if (nodeHealth <= 0)
        {
            NodeIsDestroyed();
        }
    }

    void NodeIsDestroyed()
    {
        Vector3 treePosition = transform.position;

        Destroy(transform.parent.transform.parent.gameObject);
        canBeGathered = false;
        SelectionManager.Instance.selectedNode = null;
        SelectionManager.Instance.gathererHolder.gameObject.SetActive(false);

        GameObject brokenTree = Instantiate(Resources.Load<GameObject>(destroiedNodePrefab),
            treePosition, Quaternion.Euler(0, 0, 0));
    }

    private void Update()
    {
        if (canBeGathered)
        {
            GlobalState.Instance.resourceHealth = nodeHealth;
            GlobalState.Instance.resourceMaxHealth = nodeMaxHealth;
        }
    }

    public string GetNodeName()
    {
        return nodeName;
    }
}

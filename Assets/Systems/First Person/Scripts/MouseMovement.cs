using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoadSystem;

[RequireComponent(typeof(SaveableEntity))]
public class MouseMovement : MonoBehaviour, ISaveable
{
    [SerializeField] public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;
    float yRotation = 0f;

    [System.Serializable]
    struct PlayerData
    {
        public float mouseSensitivity;
    }

    public void GotAddedAsChild(GameObject obj, GameObject hisParent)
    {
        
    }

    public void LoadState(object state)
    {
        PlayerData data = (PlayerData)state;

        this.mouseSensitivity = data.mouseSensitivity;
    }

    public bool NeedsReinstantiation()
    {
        return false;
    }

    public bool NeedsToBeSaved()
    {
        return true;
    }

    public void PostInstantiation(object state)
    {
        PlayerData data = (PlayerData)state;
    }

    public object SaveState()
    {
        return new PlayerData()
        {
            mouseSensitivity = mouseSensitivity
        };
    }

    void Start()
    {
        //Prender o cursor no meio da tela e torná-lo invisível
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!InventorySystem.Instance.isOpen && !CraftingSystem.Instance.isOpen && !SmeltSystem.Instance.isOpen)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            //controlar a rotação em torno do eixo de x (Olhar para cima e para baixo)
            xRotation -= mouseY;

            //limitamos a rotação para que não seja possível fazer rotações exageradas (Como na vida real)
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            //aplicar ambas as rotações
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}

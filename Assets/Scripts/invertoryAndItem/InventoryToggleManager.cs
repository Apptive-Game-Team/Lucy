using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryToggleManager : MonoBehaviour, IKeyInputListener
{
    [Header("Events")]
    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;

    public GameObject inventoryWindow;

    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unEquipButton;

    private InputManager inputManager;

    private void Start()
    {
        inputManager = InputManager.Instance;
        inputManager.SetKeyListener(this);
    }

    public void ToggleInventory()
    {
        bool isActive = inventoryWindow.activeSelf;

        if (isActive)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        inventoryWindow.SetActive(true);
        onOpenInventory?.Invoke();
    }

    public void CloseInventory()
    {
        Inventory.instance.ClearSelectItemWindow();
        inventoryWindow.SetActive(false);
        onCloseInventory?.Invoke();
        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
    }

    void IKeyInputListener.OnKeyDown(ActionCode action)
    {
        if (action == ActionCode.OpenInventory)
        {
            ToggleInventory();
        }
    }
}

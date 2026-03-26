using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to the AlchemyPanel Canvas object.
/// Assign slotImages[0..1] as the two input slots and resultImage as the result slot.
/// Drag all AlchemyRecipe ScriptableObjects into the recipes list.
///
/// Supports:
///   - Click slot1 → click slot2 → press Combine button
///   - Drag item from slot1 onto slot2 (or vice versa) to auto-combine
/// </summary>
public class AlchemyUI : MonoBehaviour
{
    [Header("Slot UI")]
    public Image[] slotImages = new Image[2];    // input A and B
    public Image resultImage;                     // output preview
    public Button combineButton;
    public GameObject panel;                      // the whole alchemy panel

    [Header("Recipes")]
    public List<AlchemyRecipe> recipes;

    // Which inventory slot index feeds into each alchemy slot (-1 = empty)
    private int[] inventorySlotSource = new int[] { -1, -1 };
    private int selectedInventorySlot = -1;  // currently clicked inventory slot

    // Drag state
    private int draggingFromInventorySlot = -1;
    private int draggingAlchemySlot = -1;

    private AlchemyRecipe currentMatch;

    void Start()
    {
        combineButton.onClick.AddListener(OnCombineClicked);
        combineButton.interactable = false;
        Inventory.Instance.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= RefreshUI;
    }

    void Update()
    {
        // Toggle panel with Tab
        if (Input.GetKeyDown(KeyCode.Tab))
            panel.SetActive(!panel.activeSelf);
    }

    // ─── Called by the inventory slot UI buttons ───────────────────────────

    /// <summary>Called when the player clicks an inventory slot button.</summary>
    public void OnInventorySlotClicked(int invSlotIndex)
    {
        var inv = Inventory.Instance;
        if (inv.slots[invSlotIndex].IsEmpty) return;

        // Fill alchemy slot A first, then B
        if (inventorySlotSource[0] == -1)
        {
            inventorySlotSource[0] = invSlotIndex;
        }
        else if (inventorySlotSource[1] == -1 && invSlotIndex != inventorySlotSource[0])
        {
            inventorySlotSource[1] = invSlotIndex;
        }
        else
        {
            // Clicking again deselects
            if (inventorySlotSource[0] == invSlotIndex)
                inventorySlotSource[0] = -1;
            else if (inventorySlotSource[1] == invSlotIndex)
                inventorySlotSource[1] = -1;
        }

        RefreshUI();
    }

    // ─── Drag and drop ─────────────────────────────────────────────────────

    public void OnInventorySlotBeginDrag(int invSlotIndex)
    {
        draggingFromInventorySlot = invSlotIndex;
    }

    public void OnAlchemySlotDrop(int alchemySlot)
    {
        if (draggingFromInventorySlot < 0) return;
        inventorySlotSource[alchemySlot] = draggingFromInventorySlot;
        draggingFromInventorySlot = -1;
        RefreshUI();
    }

    // ─── Combine ───────────────────────────────────────────────────────────

    void OnCombineClicked()
    {
        if (currentMatch == null) return;

        Inventory.Instance.RemoveAt(inventorySlotSource[0]);
        Inventory.Instance.RemoveAt(inventorySlotSource[1]);
        Inventory.Instance.AddItem(currentMatch.output, currentMatch.outputAmount);

        inventorySlotSource[0] = -1;
        inventorySlotSource[1] = -1;
        currentMatch = null;
        RefreshUI();
    }

    // ─── UI refresh ────────────────────────────────────────────────────────

    void RefreshUI()
    {
        var inv = Inventory.Instance;

        for (int i = 0; i < 2; i++)
        {
            int src = inventorySlotSource[i];
            if (src >= 0 && !inv.slots[src].IsEmpty)
            {
                slotImages[i].sprite  = inv.slots[src].item.icon;
                slotImages[i].color   = Color.white;
            }
            else
            {
                slotImages[i].sprite  = null;
                slotImages[i].color   = new Color(1, 1, 1, 0.15f);
                inventorySlotSource[i] = -1;
            }
        }

        // Check for recipe match
        currentMatch = null;
        if (inventorySlotSource[0] >= 0 && inventorySlotSource[1] >= 0)
        {
            InventoryItem a = inv.slots[inventorySlotSource[0]].item;
            InventoryItem b = inv.slots[inventorySlotSource[1]].item;

            foreach (var r in recipes)
            {
                if (r.Matches(a, b))
                {
                    currentMatch = r;
                    break;
                }
            }
        }

        if (currentMatch != null)
        {
            resultImage.sprite      = currentMatch.output.icon;
            resultImage.color       = Color.white;
            combineButton.interactable = true;
        }
        else
        {
            resultImage.sprite      = null;
            resultImage.color       = new Color(1, 1, 1, 0.15f);
            combineButton.interactable = false;
        }
    }
}
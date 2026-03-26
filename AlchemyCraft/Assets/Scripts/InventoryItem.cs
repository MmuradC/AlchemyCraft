using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "AlchemyCraft/Inventory Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isBlock;           // true = can be placed back into world
    public BlockData blockData;    // only if isBlock == true
}
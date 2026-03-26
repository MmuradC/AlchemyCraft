using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewBlock", menuName = "AlchemyCraft/Block Data")]
public class BlockData : ScriptableObject
{
    public string blockName;
    public TileBase tile;              // drag your pixel art tile here
    public float miningTime = 1.5f;   // seconds to mine
    public InventoryItem dropItem;    // item dropped on break
    public int dropAmount = 1;
}
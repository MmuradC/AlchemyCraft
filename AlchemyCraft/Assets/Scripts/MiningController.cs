using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// Attach to the Player GameObject.
/// Hold LMB on a tile to mine it (progress bar fills).
/// RMB to place the selected block from inventory.
/// </summary>
public class MiningController : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public BlockData[] blockDataList;    // drag all 4 BlockData assets here
    public Image miningProgressBar;      // UI Image (fill type = Filled)
    public int selectedInventorySlot = 0;

    [Header("Settings")]
    public float miningRange = 4f;

    private Vector3Int currentTarget = new Vector3Int(int.MinValue, 0, 0);
    private float miningProgress;
    private float currentMiningTime;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (miningProgressBar != null)
            miningProgressBar.fillAmount = 0f;
    }

    void Update()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector3Int tilePos = tilemap.WorldToCell(mouseWorld);

        // Range check
        float dist = Vector2.Distance(transform.position, tilemap.GetCellCenterWorld(tilePos));
        if (dist > miningRange)
        {
            ResetMining();
            return;
        }

        // LMB held = mine
        if (Input.GetMouseButton(0))
        {
            TileBase tile = tilemap.GetTile(tilePos);
            if (tile == null) { ResetMining(); return; }

            // Changed target — reset progress
            if (tilePos != currentTarget)
            {
                currentTarget      = tilePos;
                miningProgress     = 0f;
                currentMiningTime  = GetMiningTime(tile);
            }

            miningProgress += Time.deltaTime;
            float fill = Mathf.Clamp01(miningProgress / currentMiningTime);

            if (miningProgressBar != null)
                miningProgressBar.fillAmount = fill;

            if (miningProgress >= currentMiningTime)
                BreakBlock(tilePos, tile);
        }
        else
        {
            ResetMining();
        }

        // RMB = place block
        if (Input.GetMouseButtonDown(1))
            PlaceBlock(tilePos);
    }

    void BreakBlock(Vector3Int pos, TileBase tile)
    {
        BlockData data = GetBlockData(tile);
        if (data != null && data.dropItem != null)
            Inventory.Instance.AddItem(data.dropItem, data.dropAmount);

        tilemap.SetTile(pos, null);
        ResetMining();
    }

    void PlaceBlock(Vector3Int pos)
    {
        if (tilemap.GetTile(pos) != null) return;   // already occupied

        var inv  = Inventory.Instance;
        var slot = inv.slots[selectedInventorySlot];
        if (slot.IsEmpty || !slot.item.isBlock) return;

        BlockData bd = slot.item.blockData;
        if (bd == null || bd.tile == null) return;

        tilemap.SetTile(pos, bd.tile);
        inv.RemoveAt(selectedInventorySlot);
    }

    float GetMiningTime(TileBase tile)
    {
        BlockData bd = GetBlockData(tile);
        return bd != null ? bd.miningTime : 1.5f;
    }

    BlockData GetBlockData(TileBase tile)
    {
        foreach (BlockData bd in blockDataList)
            if (bd.tile == tile) return bd;
        return null;
    }

    void ResetMining()
    {
        currentTarget  = new Vector3Int(int.MinValue, 0, 0);
        miningProgress = 0f;
        if (miningProgressBar != null)
            miningProgressBar.fillAmount = 0f;
    }
}
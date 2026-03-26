using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Generates a Terraria-style world on Start.
/// Attach to a GameObject in the scene and fill in the references.
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap tilemap;

    [Header("Block Tiles — drag from BlockData assets")]
    public TileBase grassTile;
    public TileBase soilTile;
    public TileBase stoneTile;
    public TileBase woodTile;

    [Header("World Size")]
    public int worldWidth  = 120;
    public int worldDepth  = 60;
    public int surfaceY    = 20;    // y-level of the grass surface (centre of map)

    [Header("Terrain Settings")]
    [Range(0f, 1f)] public float noiseScale    = 0.07f;
    public int surfaceVariation = 5;    // max height fluctuation above surfaceY
    public int soilDepth        = 4;    // grass → soil layers before stone
    public int stoneDepth       = 30;   // stone starts this many tiles below surface

    [Header("Tree Settings")]
    public int treeSpacing     = 6;
    public int treeHeightMin   = 3;
    public int treeHeightMax   = 6;

    void Start() => Generate();

    public void Generate()
    {
        tilemap.ClearAllTiles();

        float offsetX = Random.Range(0f, 9999f);

        for (int x = 0; x < worldWidth; x++)
        {
            // Perlin noise for surface height
            float noise   = Mathf.PerlinNoise(x * noiseScale + offsetX, 0f);
            int   height  = surfaceY + Mathf.RoundToInt((noise - 0.5f) * 2f * surfaceVariation);

            for (int y = 0; y <= height; y++)
            {
                TileBase tile;

                if (y == height)
                    tile = grassTile;
                else if (y >= height - soilDepth)
                    tile = soilTile;
                else if (y < height - stoneDepth)
                    tile = stoneTile;
                else
                    tile = soilTile;

                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }

            // Plant trees
            if (x % treeSpacing == 0)
                PlantTree(x, height + 1);
        }
    }

    void PlantTree(int x, int baseY)
    {
        int treeHeight = Random.Range(treeHeightMin, treeHeightMax + 1);
        for (int y = baseY; y < baseY + treeHeight; y++)
            tilemap.SetTile(new Vector3Int(x, y, 0), woodTile);
    }
}
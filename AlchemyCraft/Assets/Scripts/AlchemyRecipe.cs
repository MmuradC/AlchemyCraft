using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "AlchemyCraft/Alchemy Recipe")]
public class AlchemyRecipe : ScriptableObject
{
    public InventoryItem inputA;
    public InventoryItem inputB;
    public InventoryItem output;
    public int outputAmount = 1;

    /// <summary>Returns true if the two given items match this recipe (order independent).</summary>
    public bool Matches(InventoryItem a, InventoryItem b)
    {
        return (inputA == a && inputB == b) ||
               (inputA == b && inputB == a);
    }
}
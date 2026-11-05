using UnityEngine;

public class FoodManager : MonoBehaviour
{
    private int _foodCount;

    public int FoodCount
    {
        get { return _foodCount; }
    }

    public void AddFood(int amount)
    {
        _foodCount += amount;
        GameEventsManager.OnFoodCollected?.Invoke(_foodCount);

    }
}

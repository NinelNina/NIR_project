using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [SerializeField]
    private int _value = 1;

    public int GetValue()
    {
        return _value;
    }
}

using UnityEngine;

public class FoodCollector : MonoBehaviour
{
    [SerializeField]
    private FoodManager _foodManager;
    private LayerMask _layerMask = 9;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == _layerMask)
        {
            _foodManager.AddFood(collision.gameObject.GetComponent<FoodItem>().GetValue());
            Destroy(collision.gameObject);
        }
    }

}
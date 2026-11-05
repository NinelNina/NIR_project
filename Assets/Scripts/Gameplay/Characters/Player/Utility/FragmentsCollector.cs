using UnityEngine;

public class FragmentsCollector : MonoBehaviour
{
    [SerializeField]
    private FragmentsManager _fragmentManager;
    private LayerMask _layerMask = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == _layerMask)
        {
            _fragmentManager.AddFragment(collision.gameObject.GetComponent<FragmentItem>().GetFragmentIndex());
            Destroy(collision.gameObject);
        }
    }
}

using UnityEngine;

public class FragmentItem : MonoBehaviour
{
    [SerializeField, Range(0, 2)]
    private int _fragmentIndex = 0;

    public int GetFragmentIndex() 
    {  
        return _fragmentIndex; 
    }
}

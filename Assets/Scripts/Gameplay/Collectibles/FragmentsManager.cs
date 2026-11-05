using UnityEngine;

public class FragmentsManager : MonoBehaviour
{
    private bool[] _collectedFragments = new bool[] { false, false, false };

    public void AddFragment(int fragmentIndex)
    {
        _collectedFragments[fragmentIndex] = true;
        GameEventsManager.OnFragmentCollected?.Invoke(fragmentIndex);

        if (CheckCollectedFragments())
        {
            GameEventsManager.OnStoryRecieved?.Invoke();
        }
    }

    // проверка, все ли фрагменты собраны
    private bool CheckCollectedFragments()
    {
        foreach (bool fragment in _collectedFragments)
        {
            if (!fragment)
            {
                return false;
            }
        }
        return true;
    }
}

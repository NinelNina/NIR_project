using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EntityType = GameEventsManager.EntityType;

public class HUDManager : MonoBehaviour
{
    [SerializeField] 
    private TMP_Text _foodText;

    [SerializeField]
    private Image[] _livesImages;

    [SerializeField]
    private Sprite _fullLife;

    [SerializeField]
    private Sprite _emptyLife;

    [SerializeField]
    private Image[] _fragmentsImages;

    [SerializeField]
    private Sprite[] _activeFragments;

    private int _foodCount;
    private bool[] _collectedFragments = new bool[] { false, false, false };
    private int _currentHP;


    [Header("BossHUD")]
    [SerializeField] GameObject _BossHUD;
    [SerializeField] TMP_Text _bossNameText;
    [SerializeField] Transform _bossHpPanel;
    [SerializeField] GameObject _lifePrefab;
    private List<Image> _bossLivesImages = new List<Image>();
    private int _currentBossHP;

    private void Awake()
    {
        GameEventsManager.OnFoodCollected += UpdateFoodCount;
        GameEventsManager.OnHealthChanged += UpdateHP;
        GameEventsManager.OnFragmentCollected += UpdateCollectedFragments;

        GameEventsManager.OnBossSetActive += BossHudSetActive;
        GameEventsManager.OnBossHealthChanged += UpdateBossHP;

        GameEventsManager.OnEntityDeath += BossHudSetInactive;
    }

    private void Start()
    {
        UpdateFoodUI();
        UpdateHealthUI();
        UpdateFragmentsUI();
    }
    
    private void UpdateFoodCount(int foodCount)
    {
        _foodCount = foodCount;
        UpdateFoodUI();
    }
    
    private void UpdateCollectedFragments(int fragmentIndex)
    {
        _collectedFragments[fragmentIndex] = true;
        UpdateFragmentsUI();
    }

    private void UpdateHP(int hp) {  
        _currentHP = hp; 
        UpdateHealthUI();
    }

    private void UpdateBossHP(int hp)
    {
        _currentBossHP = hp;
        UpdateBossHealthUI();
    }

    private void UpdateHealthUI()
    {
        for (int i = 0; i < _livesImages.Length; i++)
        {
            if (i < _currentHP)
            {
                _livesImages[i].sprite = _fullLife;
            }
            else
            {
                _livesImages[i].sprite = _emptyLife;
            }
        }
    }

    private void UpdateBossHealthUI()
    {
        for (int i = 0; i < _bossLivesImages.Count; i++)
        {
            if (i < _currentBossHP)
            {
                _bossLivesImages[i].sprite = _fullLife;
            }
            else
            {
                _bossLivesImages[i].sprite = _emptyLife;
            }
        }
    }

    private void BossHudSetActive(string bossName, int bossMaxHp)
    {
        _BossHUD.SetActive(true);

        _bossNameText.text = bossName;

        _bossLivesImages.Clear();
        foreach (Image life in _bossHpPanel)
        {
            Destroy(life.gameObject);
        }

        for (int i = 0; i < bossMaxHp; i++)
        {
            var newLife = Instantiate(_lifePrefab, _bossHpPanel).GetComponent<Image>();
            _bossLivesImages.Add(newLife);
        }
    }

    private void BossHudSetInactive(EntityType entityType)
    {
        if (entityType == EntityType.Boss)
        {
           _BossHUD.SetActive(false);
        }
    }

    private void UpdateFoodUI()
    {
        _foodText.text = _foodCount.ToString();
    }
    
    private void UpdateFragmentsUI()
    {
        for (int i = 0; i < _collectedFragments.Length; i++)
        {
            if (_collectedFragments[i])
            {
                _fragmentsImages[i].sprite = _activeFragments[i];
            }
        }
    }
    
    private void OnDestroy()
    {
        GameEventsManager.OnFoodCollected -= UpdateFoodCount;
        GameEventsManager.OnHealthChanged -= UpdateHP;
        GameEventsManager.OnFragmentCollected -= UpdateCollectedFragments;

        GameEventsManager.OnBossHealthChanged -= UpdateBossHP;
        GameEventsManager.OnBossSetActive -= BossHudSetActive;
        GameEventsManager.OnEntityDeath -= BossHudSetInactive;
    }
}

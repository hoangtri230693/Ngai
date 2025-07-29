using System;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private CharacterSO[] characterData;

    [Header("UI Elements")]
    [SerializeField] private Text textNumFood;
    [SerializeField] private Text textNumPray;
    [SerializeField] private Image barXP;
    [SerializeField] private Text textLevel;
    [SerializeField] private int xpPerFood = 5;
    [SerializeField] private GameObject buttonRestart;

    public event Action OnLevelUp;
    public event Action<Sprite, Sprite, Sprite> OnEvolutionStart;
    public event Action OnEnding;

    private void Awake()
    {
        //Test Mode
        // GameData.playerLevel = 6;
        // GameData.playerXP = characterData[GameData.playerLevel].maxXP - 9;
        // GameData.playerPray = 0;
        // GameData.PlayerFoodCount = 999;
        //End test mode

        if (GameData.playerPray == -1) GameData.playerPray = characterData[GameData.playerLevel].maxPray;
        UpdateUI();
        buttonRestart.SetActive(GameData.unlockedEnding1 || GameData.unlockedEnding2);
    }

    public void GainXP()
    {
        GameData.PlayerFoodCount--;
        GameData.playerXP += xpPerFood;
        UpdateUI();
        if (CheckLevelUp())
        {
            LevelUp();
        }
    }

    public void GainPray()
    {
        // GameData.playerPray++;
        // GameData.playerPray = Mathf.Min(GameData.playerPray, characterData[GameData.playerLevel].maxPray);
        GameData.playerPray--;
        GameData.playerPray = Mathf.Max(GameData.playerPray, 0);
        UpdateUI();
        if (CheckLevelUp())
        {
            LevelUp();
        }
    }

    private bool CheckLevelUp()
    {
        return GameData.playerXP >= characterData[GameData.playerLevel].maxXP &&
               GameData.playerPray == 0;
        //    GameData.playerPray == characterData[GameData.playerLevel].maxPray;
    }

    private void LevelUp()
    {
        // Kiểm tra đạt max level hay chưa
        if (GameData.playerLevel == characterData.Length - 1)
        {
            if (!GameData.watchedEnding)
            {
                OnLevelUp?.Invoke();
                OnEnding?.Invoke(); // Chỉ xem 1 lần ending
            }
            return;
        }

        // Tăng cấp độ người chơi
        int oldLevel = GameData.playerLevel;
        GameData.playerXP -= characterData[oldLevel].maxXP;
        GameData.playerLevel++;
        GameData.playerPray = characterData[GameData.playerLevel].maxPray;
        // GameData.playerPray = 0;
        UpdateUI();
        OnLevelUp?.Invoke();
        OnEvolutionStart?.Invoke(
            characterData[oldLevel].sprite,
            characterData[GameData.playerLevel].sprite,
            characterData[GameData.playerLevel].characterName);
    }

    private void UpdateUI()
    {
        textLevel.text = $"LV {GameData.playerLevel + 1}";
        barXP.fillAmount = Mathf.Min(GameData.playerXP / characterData[GameData.playerLevel].maxXP, 1);
        textNumFood.text = GameData.PlayerFoodCount == 0 ? "" : GameData.PlayerFoodCount.ToString();
        if (GameData.playerPray <= 0)
            textNumPray.text = "";
        else
            textNumPray.text = GameData.playerPray.ToString();
        // if (GameData.playerPray == characterData[GameData.playerLevel].maxPray)
        //     textNumPray.text = "max";
        // else if (GameData.playerPray == 0)
        //     textNumPray.text = "";
        // else
        //     textNumPray.text = GameData.playerPray.ToString();
    }
}

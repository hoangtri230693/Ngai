using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    // Key cho PlayerPrefs
    public const string SETTING_BGMVOLUME = "BGMVolume";
    public const string SETTING_SFXVOLUME = "SFXVolume";
    public const string PLAYER_LEVEL = "PlayerLevel";
    public const string PLAYER_XP = "PlayerXP";
    public const string PLAYER_FOOD_COUNT = "PlayerFoodCount";
    public const string PLAYER_PRAY = "PlayerPray";
    public const string LAST_VISIT_HARVEST = "LastVisitHarvest";
    public const string FOOD = "Food";
    public const string FOOD_COUNT = "Food_Count";
    public const string WATCHED_INTRO = "WatchedIntro";
    public const string WATCHED_ENDING = "WatchedEnding";
    public const string UNLOCKED_ENDING_1 = "UnlockedEnding1";
    public const string UNLOCKED_ENDING_2 = "UnlockedEnding2";
    public const string UNLOCKED_ENDING_SECRET = "UnlockedEndingSecret";

    // Biến lưu trữ dữ liệu
    public static float bgmVolume;
    public static float sfxVolume;
    public static int playerLevel;
    public static float playerXP;
    public static int PlayerFoodCount;
    public static int playerPray;
    public static int unlockedVariants => playerLevel;
    public static bool watchedIntro;
    public static bool watchedEnding;
    public static bool unlockedEnding1;
    public static bool unlockedEnding2;
    public static bool unlockedEndingSecret;

    static GameData()
    {
        Application.targetFrameRate = 60;
        //Load data từ PlayerPrefs
        bgmVolume = PlayerPrefs.GetFloat(SETTING_BGMVOLUME, 1.0f);
        sfxVolume = PlayerPrefs.GetFloat(SETTING_SFXVOLUME, 1.0f);
        playerLevel = PlayerPrefs.GetInt(PLAYER_LEVEL, 0);
        playerXP = PlayerPrefs.GetFloat(PLAYER_XP, 0.0f);
        PlayerFoodCount = PlayerPrefs.GetInt(PLAYER_FOOD_COUNT, 0);
        // playerPray = PlayerPrefs.GetInt(PLAYER_PRAY, 0);
        playerPray = PlayerPrefs.GetInt(PLAYER_PRAY, -1);
        watchedIntro = PlayerPrefs.GetInt(WATCHED_INTRO, 0) == 1;
        watchedEnding = PlayerPrefs.GetInt(WATCHED_ENDING, 0) == 1;
        unlockedEnding1 = PlayerPrefs.GetInt(UNLOCKED_ENDING_1, 0) == 1;
        unlockedEnding2 = PlayerPrefs.GetInt(UNLOCKED_ENDING_2, 0) == 1;
        unlockedEndingSecret = PlayerPrefs.GetInt(UNLOCKED_ENDING_SECRET, 0) == 1;
    }

    public static void ResetGameData()
    {
        PlayerPrefs.DeleteAll();
        bgmVolume = 1.0f;
        sfxVolume = 1.0f;
        playerLevel = 0;
        playerXP = 0.0f;
        PlayerFoodCount = 0;
        PlayerPrefs.SetInt(FOOD_COUNT, 0);
        // playerPray = 0;
        playerPray = -1;
        watchedIntro = false;
        watchedEnding = false;
        unlockedEnding1 = false;
        unlockedEnding2 = false;
        unlockedEndingSecret = false;
        SaveGameData();
    }

    public static void SaveGameData()
    {
        // Lưu dữ liệu vào PlayerPrefs
        PlayerPrefs.SetFloat(SETTING_BGMVOLUME, bgmVolume);
        PlayerPrefs.SetFloat(SETTING_SFXVOLUME, sfxVolume);
        PlayerPrefs.SetInt(PLAYER_LEVEL, playerLevel);
        PlayerPrefs.SetFloat(PLAYER_XP, playerXP);
        PlayerPrefs.SetInt(PLAYER_FOOD_COUNT, PlayerFoodCount);
        PlayerPrefs.SetInt(PLAYER_PRAY, playerPray);
        PlayerPrefs.SetInt(WATCHED_INTRO, watchedIntro ? 1 : 0);
        PlayerPrefs.SetInt(WATCHED_ENDING, watchedEnding ? 1 : 0);
        PlayerPrefs.SetInt(UNLOCKED_ENDING_1, unlockedEnding1 ? 1 : 0);
        PlayerPrefs.SetInt(UNLOCKED_ENDING_2, unlockedEnding2 ? 1 : 0);
        PlayerPrefs.SetInt(UNLOCKED_ENDING_SECRET, unlockedEndingSecret ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SaveHarvestData(List<GameObject> listInactiveFood)
    {
        // Lưu thời gian thu hoạch cuối cùng, Luôn dùng UTC cho tính toán
        PlayerPrefs.SetString(LAST_VISIT_HARVEST, DateTime.UtcNow.Ticks.ToString());
        // Lưu danh sách Food đã thu hoạch vào PlayerPrefs
        PlayerPrefs.SetInt(FOOD_COUNT, listInactiveFood.Count);
        for (int i = 0; i < listInactiveFood.Count; i++)
            if (listInactiveFood[i] != null)
                PlayerPrefs.SetString($"{FOOD}{i}", listInactiveFood[i].name);
        // Debug.Log("SaveHarvestData: Food count = " + PlayerPrefs.GetInt(FOOD_COUNT));
        PlayerPrefs.Save();
    }
    public static List<string> LoadHarvestData()
    {
        // Lấy thời gian thu hoạch cuối cùng
        string savedTicks = PlayerPrefs.GetString(LAST_VISIT_HARVEST, "0");
        int differenceSeconds = 0;
        int maxFoodCount = 350; // Giới hạn tối đa là 350 giây (5 phút)
        if (savedTicks != "0")
        {
            // Quan trọng: specify Kind as Utc
            DateTime lastVisit = new DateTime(long.Parse(savedTicks), DateTimeKind.Utc);
            // Tính toán thời gian đã qua kể từ lần truy cập cuối
            differenceSeconds = Mathf.RoundToInt((float)(DateTime.UtcNow - lastVisit).TotalSeconds);
            differenceSeconds = Mathf.Min(differenceSeconds, maxFoodCount);
        }

        // Lấy danh sách Food đã thu hoạch từ PlayerPrefs
        List<string> listFood = new List<string>();
        int foodCount = PlayerPrefs.GetInt(FOOD_COUNT, 0);
        foodCount -= differenceSeconds / 10;
        // Debug.Log($"LoadHarvestData: Food count = {foodCount}; differenceSeconds = {differenceSeconds}");
        if (foodCount <= 0) return listFood; // Không load lại Food (đầy food)
        for (int i = 0; i < foodCount; i++)
        {
            string foodName = PlayerPrefs.GetString($"{FOOD}{i}", "");
            if (string.IsNullOrEmpty(foodName)) continue; // Bỏ qua nếu tên Food rỗng                                                         
            listFood.Add(foodName);
            // Debug.Log("LoadHarvestData: " + foodName);
        }
        return listFood;
    }

    public static void SaveEndingData()
    {
        // Reset lại dữ liệu người chơi
        playerLevel = 0;
        playerXP = 0.0f;
        PlayerFoodCount = 0;
        PlayerPrefs.SetInt(FOOD_COUNT, 0);
        // playerPray = 0;
        playerPray = -1;
        watchedIntro = false;
        watchedEnding = false;
        SaveGameData();
    }
}

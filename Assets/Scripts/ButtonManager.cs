using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ButtonType
{
    // Tính năng cơ bản có button type đánh số từ 1 đến 20
    Quit = 1, Back = 2, Setting = 3, ShowWindow = 4, CloseWindow = 5, ResetData = 6,
    // Nằm trong Scene MainMenu đánh số từ 21 đến 40
    PlayGame = 21, WatchedIntro = 22,
    // Nằm trong Scene PlayGame đánh số từ 41 đến 60
    GoHarvest = 41, GoDiary = 42, GiveFood = 43, StopGiveFood = 44, Pray = 45,
    WatchEnding1 = 46, WatchEnding2 = 47, 
    // Nằm trong Scene Harvest đánh số từ 61 đến 80
    // Nằm trong Scene Diary đánh số từ 81 đến 100
    ShowCharacter = 83, CloseCharacter = 84, WatchEnding1Diary = 85, WatchEnding2Diary = 86,
    // Nằm trong Scene Intro/Ending đánh số từ 101 đến 120
    NextStoryImage = 101, WatchedEnding = 102, EndingConfirmYes = 103, EndingConfirmNo = 104
}

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance;

    private Stack<string> sceneStack = new Stack<string>(); // lưu trữ lịch sử các scene đã truy cập

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnApplicationQuit() => GameData.SaveGameData(); // Lưu dữ liệu khi ứng dụng thoát
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) GameData.SaveGameData(); // Lưu dữ liệu khi ứng dụng tạm dừng
    }

    public void HandleButton(ButtonType type, GameObject targetObject = null)
    {
        switch (type)
        {
            case ButtonType.PlayGame:
                PlayGame();
                break;
            case ButtonType.Quit:
                Application.Quit();
                break;
            case ButtonType.Back:
                GoBack();
                break;
            case ButtonType.Setting:
                Setting(targetObject);
                break;
            case ButtonType.ShowWindow:
            case ButtonType.ShowCharacter:
                WindowsOn(targetObject);
                break;
            case ButtonType.CloseWindow:
            case ButtonType.CloseCharacter:
                targetObject?.SetActive(false);
                break;
            case ButtonType.GoHarvest:
                LoadScene("Harvest");
                break;
            case ButtonType.GoDiary:
                DiaryOn();
                break;
            case ButtonType.GiveFood:
                GiveFood(targetObject);
                break;
            case ButtonType.StopGiveFood:
                StopGiveFood(targetObject);
                break;
            case ButtonType.Pray:
                Pray(targetObject);
                break;
            case ButtonType.WatchedIntro:
                WatchedIntro();
                break;
            case ButtonType.ResetData:
                GameData.ResetGameData();
                break;
            case ButtonType.NextStoryImage:
                NextStoryImage(targetObject);
                break;
            case ButtonType.WatchedEnding:
                WatchedEnding(targetObject);
                break;
            case ButtonType.EndingConfirmYes:
                EndingConfirmYes();
                break;
            case ButtonType.EndingConfirmNo:
                EndingConfirmNo();
                break;
            case ButtonType.WatchEnding1:
                WatchEnding1();
                break;
            case ButtonType.WatchEnding2:
                WatchEnding2();
                break;
            case ButtonType.WatchEnding1Diary:
                WatchEnding1Diary();
                break;
            case ButtonType.WatchEnding2Diary:
                WatchEnding2Diary();
                break;
            default:
                Debug.LogWarning("Unhandled button type: " + type);
                break;
        }
    }

    private void LoadScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        sceneStack.Push(currentScene);
        SceneManager.LoadScene(sceneName);
    }

    private void GoBack()
    {
        if (sceneStack.Count > 0)
        {
            string previousScene = sceneStack.Pop();
            if(previousScene == "CutScene") previousScene = sceneStack.Pop();
            SceneManager.LoadScene(previousScene);
        }
        else
        {
            Debug.LogWarning("No previous scene in history.");
        }
    }

    private void PlayGame()
    {
        if (!GameData.watchedIntro)
        {
            CutSceneManager.AlbumToPlay = CutSceneManager.Album.Intro;
            LoadScene("CutScene");
            return;
        }
        LoadScene("PlayGame");
    }

    private void Setting(GameObject targetObject)
    {
        if (targetObject.activeSelf)
        {
            targetObject?.SetActive(false);
            return;
        }
        targetObject?.SetActive(true);

        var sliderBGM = targetObject?.transform.Find("SliderBGM").GetComponent<Slider>();
        sliderBGM.onValueChanged.RemoveAllListeners();
        sliderBGM.value = GameData.bgmVolume;
        sliderBGM.onValueChanged.AddListener(value => AudioManager.Instance.SetVolumeBGM(value));

        var sliderSFX = targetObject?.transform.Find("SliderSFX").GetComponent<Slider>();
        sliderSFX.onValueChanged.RemoveAllListeners();
        sliderSFX.value = GameData.sfxVolume;
        sliderSFX.onValueChanged.AddListener(value => AudioManager.Instance.SetVolumeSFX(value));
    }

    private void WindowsOn(GameObject targetObject)
    {
        targetObject?.SetActive(true);
    }

    private void GiveFood(GameObject targetObject)
    {
        targetObject?.GetComponent<CharacterAnimation>().StartEating();
    }
    private void StopGiveFood(GameObject targetObject)
    {
        targetObject?.GetComponent<CharacterAnimation>().StopEating();
    }
    private void Pray(GameObject targetObject)
    {
        targetObject?.GetComponent<CharacterAnimation>().StartPray();
    }

    private void WatchedIntro()
    {
        GameData.watchedIntro = true;
        AudioManager.Instance?.PlayCurrentBGM();
        LoadScene("PlayGame");
    }

    private void NextStoryImage(GameObject targetObject)
    {
        targetObject?.GetComponent<CutSceneManager>().OnNextButtonClicked();
    }

    private void WatchedEnding(GameObject targetObject)
    {
        switch (CutSceneManager.AlbumToPlay)
        {
            case CutSceneManager.Album.Ending1WithReset:
                GameData.unlockedEnding1 = true;
                GameData.watchedEnding = true;
                targetObject?.SetActive(true);
                break;
            case CutSceneManager.Album.Ending2WithReset:
                GameData.unlockedEnding2 = true;
                GameData.watchedEnding = true;
                targetObject?.SetActive(true);
                break;
            case CutSceneManager.Album.Ending1NoReset:
            case CutSceneManager.Album.Ending2NoReset:
                DiaryManager.isVariants = false;
                AudioManager.Instance?.PlayCurrentBGM();
                GoBack();
                break;
        }
    }

    private void EndingConfirmYes()
    {
        GameData.SaveEndingData();
        AudioManager.Instance?.PlayCurrentBGM();
        LoadScene("MainMenu");
    }

    private void EndingConfirmNo()
    {
        AudioManager.Instance?.PlayCurrentBGM();
        GoBack();
    }

    private void WatchEnding1()
    {
        CutSceneManager.AlbumToPlay = CutSceneManager.Album.Ending1WithReset;
        LoadScene("CutScene");
    }

    private void WatchEnding2()
    {
        CutSceneManager.AlbumToPlay = CutSceneManager.Album.Ending2WithReset;
        LoadScene("CutScene");
    }

    private void WatchEnding1Diary()
    {
        CutSceneManager.AlbumToPlay = CutSceneManager.Album.Ending1NoReset;
        LoadScene("CutScene");
    }

    private void WatchEnding2Diary()
    {
        CutSceneManager.AlbumToPlay = CutSceneManager.Album.Ending2NoReset;
        LoadScene("CutScene");
    }

    private void DiaryOn()
    {
        DiaryManager.isVariants = true;
        DiaryManager.isEndings = true;
        LoadScene("Diary");
    }
}

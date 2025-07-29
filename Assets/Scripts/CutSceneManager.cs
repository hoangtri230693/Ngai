using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CutSceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image imageDisplay;//Image component để hiển thị hình ảnh.
    [SerializeField] private GameObject buttonNext;//Button để người dùng click chuyển sang ảnh tiếp theo.
    [SerializeField] private Image imageTransition; // Image component sử dụng cho hiệu ứng mờ dần.
    [SerializeField] private GameObject buttonCloseIntro;//Button để người dùng đóng intro.
    [SerializeField] private GameObject buttonCloseEnding;//Button để người dùng đóng ending.
    [SerializeField] private GameObject panelConfirm;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 1f;//Thời gian cho mỗi lần mờ dần (vào hoặc ra).

    [Header("Album")]
    [SerializeField] private List<Sprite> imagesIntro;
    [SerializeField] private List<Sprite> imagesEnding1;
    [SerializeField] private List<Sprite> imagesEnding2;

    public enum Album { Intro, Ending1WithReset, Ending2WithReset, Ending1NoReset, Ending2NoReset };
    public static Album AlbumToPlay;

    private List<Sprite> currentAlbum; // Album đang được chạy
    private int currentImageIndex = 0; // Chỉ số của ảnh hiện tại trong album

    void Start()
    {
        // Chọn album để chạy
        switch (AlbumToPlay)
        {
            case Album.Intro:
                currentAlbum = imagesIntro;
                break;
            case Album.Ending1WithReset:
            case Album.Ending1NoReset:
                currentAlbum = imagesEnding1;
                break;
            case Album.Ending2WithReset:
            case Album.Ending2NoReset:
                currentAlbum = imagesEnding2;
                break;
        }

        // Các thiết lập ban đầu
        AudioManager.Instance?.PlayBGM(AudioManager.KEY.BGM_Character);
        AudioManager.Instance.BGM.loop = true;
        imageDisplay.sprite = currentAlbum[currentImageIndex];
        panelConfirm.SetActive(false);
    }

    public void OnNextButtonClicked()
    {
        // Tăng chỉ số ảnh
        currentImageIndex++;
        if (currentImageIndex < currentAlbum.Count)
        {
            // Bắt đầu coroutine để hiển thị ảnh tiếp theo
            StartCoroutine(ShowNextImage());
        }
    }

    private IEnumerator ShowNextImage()
    {
        // 0. Chuẩn bị
        imageTransition.gameObject.SetActive(true);
        Color color = imageTransition.color;

        // 1. Fade out ảnh cũ (nếu có)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            color.a = Mathf.Lerp(0, 1, timer / fadeDuration);
            imageTransition.color = color;
            timer += Time.deltaTime;
            yield return null;
        }
        color.a = 1;
        imageTransition.color = color;

        // Gán ảnh mới vào, lúc này màn hình đang đen
        imageDisplay.sprite = currentAlbum[currentImageIndex];
        // Kiểm tra nếu là Image cuối cùng trong danh sách
        CheckLastImage();

        // 2. Fade in ảnh mới
        timer = 0f;
        while (timer < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, timer / fadeDuration);
            imageTransition.color = color;
            timer += Time.deltaTime;
            yield return null;
        }
        color.a = 0;
        imageTransition.color = color;

        // 3. Chuyển cảnh xong, dọn dẹp
        imageTransition.gameObject.SetActive(false);
    }

    private void CheckLastImage()
    {
        // 1) Ending2NoReset tại ảnh thứ 4 và chưa unlock secret → dừng luôn
        if (AlbumToPlay == Album.Ending2NoReset
            && currentImageIndex == 4
            && !GameData.unlockedEndingSecret)
        {
            buttonNext.SetActive(false);
            buttonCloseEnding.SetActive(true);
            return;
        }

        // 2) Ending2WithReset tại ảnh thứ 4
        if (AlbumToPlay == Album.Ending2WithReset
            && currentImageIndex == 4)
        {
            if (GameData.unlockedEnding1 || GameData.unlockedEnding2)
            {
                // Đã có ending khác → unlock secret và tiếp tục flow
                GameData.unlockedEndingSecret = true;
                return;
            }
            else
            {
                // Chưa có ending khác → dừng luôn
                buttonNext.SetActive(false);
                buttonCloseEnding.SetActive(true);
                return;
            }
        }

        // 3) Nếu không phải 2 case trên, thì chỉ cần check ảnh cuối chung cho mọi album
        if (currentImageIndex == currentAlbum.Count - 1)
        {
            buttonNext.SetActive(false);
            if (AlbumToPlay == Album.Intro)
                buttonCloseIntro.SetActive(true);
            else
                buttonCloseEnding.SetActive(true);
        }
    }
}
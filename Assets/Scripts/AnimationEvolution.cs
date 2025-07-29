using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationEvolution : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationDuration = 3.0f; // Tổng thời gian animation tiến hóa
    public float shortTransitionDuration = 0.05f; // Thời gian chuyển SpriteNew về SpriteOld (rất ngắn)
    public float moveDuration = 1.0f; // Thời gian để di chuyển về trung tâm

    [Header("Flash Effect Settings")]
    public SpriteRenderer flashEffectRenderer; // Kéo GameObject 'Flash' vào đây
    public float flashScaleTarget = 12f; // Kích thước cuối cùng của hiệu ứng Flash
    public float flashFadeInDuration = 0.3f; // Thời gian Flash sáng rõ dần
    public float flashFadeOutDuration = 0.5f; // Thời gian Flash mờ dần

    [Header("References")]
    public Image backgroundImage;
    public GameObject panel; // Kéo GameObject 'Panel' (UI Panel) vào đây
    public Image nameImage; // Tên hiện trong panel
    public ParticleSystem panelEffect; // Kéo GameObject 'ParticleEvolution' vào đây
    public Character character;

    private SpriteRenderer characterSpriteRenderer;
    private Animator animator; // Tham chiếu đến Animator (nếu có)
    private Vector3 startPosition;

    private void Awake()
    {
        characterSpriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        panel.GetComponent<Button>().onClick.AddListener(EndEvolution);
        panel.SetActive(false);
        character.OnEvolutionStart += StartEvolution;
    }
    private void OnDestroy()
    {
        character.OnEvolutionStart -= StartEvolution;
    }

    public void StartEvolution(Sprite spriteOld, Sprite spriteNew, Sprite spriteNewName)
    {
        StartCoroutine(EvolutionProcessRoutine(spriteOld, spriteNew, spriteNewName));
    }

    private IEnumerator EvolutionProcessRoutine(Sprite spriteOld, Sprite spriteNew, Sprite spriteNewName)
    {
        // --- 0. Chuẩn bị tiến hóa ---
        animator.enabled = false;
        panelEffect.gameObject.SetActive(true);
        backgroundImage.gameObject.SetActive(true);
        AudioManager.Instance?.PlayBGM(AudioManager.KEY.BGM_Evolution); // Phát nhạc nền tiến hóa

        // --- 1. Di chuyển SpriteOld về trung tâm màn hình ---
        // Đồng thời Fade In background
        startPosition = characterSpriteRenderer.transform.position;
        Vector3 targetPosition = Vector3.zero; // Trung tâm màn hình (World Space)
        float elapsedTime = 0f;
        Color color = backgroundImage.color;
        color.a = 0f;
        backgroundImage.color = color;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            characterSpriteRenderer.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            // Alpha (sáng dần)
            color.a = Mathf.Lerp(0f, 1f, t);
            backgroundImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null; // Đợi một frame
        }
        // Đảm bảo đạt giá trị cuối cùng
        characterSpriteRenderer.transform.position = targetPosition;
        color.a = 1f;
        backgroundImage.color = color;

        yield return new WaitForSeconds(0.2f); // Đợi một chút trước khi bắt đầu tiến hóa

        // --- 2. Thực hiện Animation Tiến hóa (chớp nhoáng) ---
        // Chuỗi: Old -> New -> Old -> New -> Old -> New -> Old -> New -> Old -> New -> Old -> New -> Old -> New -> Old
        // Tổng cộng 7 lần chuyển Old->New và 7 lần chuyển New->Old

        float totalTimeForOldToNew = animationDuration - (7 * shortTransitionDuration); // 3s - (7 * 0.05s) = 2.65s
        float totalWeight = 28f; // 7+6+5+4+3+2+1
        float timePerWeightUnit = totalTimeForOldToNew / totalWeight; // 2.65 / 28 = ~0.09464s
        int[] weights = { 7, 6, 5, 4, 3, 2, 1 }; // Danh sách các trọng số cho các khoảng Old -> New

        AudioManager.Instance?.PlaySFX(AudioManager.KEY.SFX_LevelUp); // Phát âm thanh tiến hóa
        for (int i = 0; i < 7; i++) // 7 lần SpriteNew xuất hiện
        {
            // Chuyển đổi Old -> New (nhanh dần)
            float currentOldToNewDuration = weights[i] * timePerWeightUnit;
            characterSpriteRenderer.sprite = spriteOld; // Đảm bảo là Old trước khi chờ
            yield return new WaitForSeconds(currentOldToNewDuration); // Chờ khoảng thời gian Old->New

            // Chuyển sang SpriteNew
            characterSpriteRenderer.sprite = spriteNew;
            yield return new WaitForSeconds(shortTransitionDuration); // Chờ khoảng thời gian New->Old (rất ngắn)

            // Sau khi chờ xong 0.05s, sprite sẽ tự động chuyển về Old ở vòng lặp tiếp theo
        }

        // Đảm bảo kết thúc ở SpriteOld sau vòng lặp chớp nhoáng
        characterSpriteRenderer.sprite = spriteOld;
        yield return new WaitForSeconds(0.2f); // Đợi một chút trước hiệu ứng Flash

        // --- 3. Thực hiện hiệu ứng Flash, kích hoạt Panel và Particle System ---
        flashEffectRenderer.gameObject.SetActive(true);
        flashEffectRenderer.transform.localScale = Vector3.zero; // Bắt đầu từ kích thước 0
        color = flashEffectRenderer.color;
        color.a = 0f; // Alpha ban đầu là 0
        flashEffectRenderer.color = color;

        // Flash sáng rõ dần và scale to
        elapsedTime = 0f;
        while (elapsedTime < flashFadeInDuration)
        {
            float t = elapsedTime / flashFadeInDuration;
            // Scale
            flashEffectRenderer.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * flashScaleTarget, t);
            // Alpha (sáng dần)
            color.a = Mathf.Lerp(0f, 1f, t);
            flashEffectRenderer.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Đảm bảo đạt giá trị cuối cùng
        flashEffectRenderer.transform.localScale = Vector3.one * flashScaleTarget;
        color.a = 1f;
        flashEffectRenderer.color = color;

        // Khi Flash lên tới đỉnh alpha = 1
        backgroundImage.gameObject.SetActive(false);
        panel.SetActive(true); // Kích hoạt Panel
        characterSpriteRenderer.sprite = spriteNew; // Đổi sang SpriteNew
        nameImage.sprite = spriteNewName; // Đổi sprite Name
        panelEffect.Play(); // Chạy Particle System

        // Flash mờ dần về 0
        elapsedTime = 0f;
        while (elapsedTime < flashFadeOutDuration)
        {
            float t = elapsedTime / flashFadeOutDuration;
            color.a = Mathf.Lerp(1f, 0f, t); // Alpha mờ dần
            flashEffectRenderer.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Đảm bảo Flash biến mất
        color.a = 0f;
        flashEffectRenderer.color = color;
        flashEffectRenderer.gameObject.SetActive(false); // Tắt GameObject Flash

        // --- 4. Kết thúc và dọn dẹp ---
    }

    public void EndEvolution()
    {
        // Kết thúc quá trình tiến hóa, có thể thêm logic khác nếu cần
        animator.Rebind();
        animator.enabled = true;
        GetComponent<CharacterAnimation>()?.SetCharacterLevel(GameData.playerLevel + 1);
        characterSpriteRenderer.transform.position = startPosition; // Trả Sprite về vị trí ban đầu
        panel.SetActive(false);
        panelEffect.Stop();
        panelEffect.gameObject.SetActive(false);
    }
}

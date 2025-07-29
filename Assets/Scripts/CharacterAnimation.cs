using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterAnimation : MonoBehaviour
{
    [Header("For Animation Idle")]
    [SerializeField] private int characterLevel = 0;

    [Header("For Animation Eating")]
    [SerializeField] private GameObject bgEating;
    [SerializeField] private GameObject foodObject;
    private bool isEating = false;
    private const float EATING_INTERVAL = 0.09f;

    [Header("For Animation Pray")]
    [SerializeField] private GameObject bgPray;
    [SerializeField] private ParticleSystem prayEffect;
    private const float PRAY_DURATION = 3f;

    [Header("For Animation Ending")]
    [SerializeField] private Image bgEnding;
    [SerializeField] private GameObject panelSelectEnding;
    private float moveDuration = 1.0f; // Thời gian để di chuyển về trung tâm


    [Header("References")]
    [SerializeField] private Character character;
    [SerializeField] private Button buttonFood;
    // [SerializeField] private Button buttonPray;
    [SerializeField] private Button buttonHarvest;
    [SerializeField] private Button buttonDiary;
    private SpriteRenderer spriteCharacter;
    private Animator animator;

    private void Awake()
    {
        spriteCharacter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (characterLevel == 0) characterLevel = GameData.playerLevel + 1;
        SetCharacterLevel(characterLevel);
        if (character != null)
        {
            character.OnLevelUp += StopEating;
            character.OnEnding += StartEnding;
        }
    }
    private void OnDestroy()
    {
        if (character != null)
        {
            character.OnLevelUp -= StopEating;
            character.OnEnding -= StartEnding;
        }
    }

    public void SetCharacterLevel(int level) => animator.Play($"LV{level}_Idle");

    #region Eating & Pray
    private bool CanEat() => GameData.PlayerFoodCount > 0 && buttonFood.interactable;
    public void StartEating()
    {
        if (!CanEat()) return;
        isEating = true;
        bgEating.SetActive(true);
        foodObject.SetActive(true);
        SetCharacterAlpha(0f);
        StartCoroutine(EatingFoodRoutine());
    }
    public void StopEating() => isEating = false;
    private IEnumerator EatingFoodRoutine()
    {
        while (isEating)
        {
            AudioManager.Instance?.PlaySFX(AudioManager.KEY.SFX_ClickFood);
            yield return new WaitForSeconds(EATING_INTERVAL);
            if (CanEat()) character.GainXP();
        }
        // Kết thúc ăn
        bgEating.SetActive(false);
        foodObject.SetActive(false);
        SetCharacterAlpha(1f);
    }

    public void StartPray()
    {
        if (prayEffect.isPlaying) return;
        prayEffect.Play();
        bgPray.SetActive(true);
        StartCoroutine(PrayRoutine());
        // Tắt button trong lúc animation
        buttonFood.interactable = false;
        buttonHarvest.interactable = false;
        buttonDiary.interactable = false;
    }
    private IEnumerator PrayRoutine()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.KEY.SFX_Pray);
        yield return new WaitForSeconds(PRAY_DURATION);
        bgPray.SetActive(false);
        character.GainPray();
        // Mở button sau khi animation
        buttonFood.interactable = true;
        buttonHarvest.interactable = true;
        buttonDiary.interactable = true;
    }

    private void SetCharacterAlpha(float alpha)
    {
        Color color = spriteCharacter.color;
        color.a = alpha;
        spriteCharacter.color = color;
    }
    #endregion

    #region Ending
    public void StartEnding() => StartCoroutine(EndingRoutine());
    private IEnumerator EndingRoutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = Vector3.zero; // Trung tâm màn hình (World Space)
        bgEnding.gameObject.SetActive(true);
        Color color = bgEnding.color;
        color.a = 0f;
        bgEnding.color = color;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            // Alpha (sáng dần)
            color.a = Mathf.Lerp(0f, 1f, t);
            bgEnding.color = color;
            elapsedTime += Time.deltaTime;
            yield return null; // Đợi một frame
        }
        // Đảm bảo đạt giá trị cuối cùng
        transform.position = targetPosition;
        color.a = 1f;
        bgEnding.color = color;

        yield return new WaitForSeconds(0.2f); // Đợi một chút trước khi bắt đầu
        panelSelectEnding.SetActive(true);
    }
    #endregion
}

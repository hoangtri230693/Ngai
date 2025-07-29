using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private enum ButtonEvent { Click, Hold };
    [SerializeField] private ButtonEvent buttonEvent = ButtonEvent.Click;
    [SerializeField] private ButtonType buttonType;
    [SerializeField] private GameObject targetWindow; // Gameobject, Panel cần tác động
    [SerializeField] private bool playSoundEffect = true; // Bật tắt âm thanh nút

    void Start()
    {
        if (buttonEvent == ButtonEvent.Click)
            GetComponent<Button>().onClick.AddListener(ButtonHandler);
    }

    private void ButtonHandler()
    {
        if (playSoundEffect) PlaySoundButton();
        ButtonManager.Instance.HandleButton(buttonType, targetWindow);
    }

    private void PlaySoundButton()
    {
        switch (buttonType)
        {
            case ButtonType.PlayGame:
                AudioManager.Instance?.PlaySFX(AudioManager.KEY.SFX_ClickPlayGame);
                break;
            case ButtonType.Back:
                AudioManager.Instance?.PlaySFX(AudioManager.KEY.SFX_ClickBack);
                break;
            case ButtonType.ShowCharacter:
                AudioManager.Instance?.PlayBGM(AudioManager.KEY.BGM_Character);
                AudioManager.Instance.BGM.loop = true;
                break;
            case ButtonType.CloseCharacter:
                AudioManager.Instance?.BGM.Stop();
                AudioManager.Instance.BGM.loop = false;
                break;
            // case ButtonType.GiveFood:
            //     // Không play sfx
            //     break;
            default:
                AudioManager.Instance?.PlaySFX(AudioManager.KEY.SFX_ClickDefault);
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buttonEvent != ButtonEvent.Hold) return;
        ButtonHandler();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (buttonEvent != ButtonEvent.Hold) return;
        switch (buttonType)
        {
            case ButtonType.GiveFood:
                ButtonManager.Instance.HandleButton(ButtonType.StopGiveFood, targetWindow);
                break;
        }
    }
}

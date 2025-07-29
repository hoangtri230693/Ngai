using UnityEngine;
using UnityEngine.UI;

public class DiaryManager : MonoBehaviour
{
    [SerializeField] private Button[] buttonVariants = new Button[7];
    [SerializeField] private Button[] buttonEndings = new Button[2];
    [SerializeField] private Image[] imageVariants = new Image[7];
    [SerializeField] private GameObject character;
    [SerializeField] private GameObject Variations;
    [SerializeField] private GameObject Endings;

    public static bool isVariants;
    public static bool isEndings;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddListener();       
        UnlockedVariants();
        UnlockedEndings();
        CheckState();
    }

    public void UnlockedVariants()
    {
        for (int i = 0; i < buttonVariants.Length; i++)
        {
            imageVariants[i].enabled = false;
        }

        for (int i = 0; i < buttonVariants.Length; i++)
        {
            if (i <= GameData.playerLevel)
            {
                imageVariants[i].enabled = true;
            }
            else
            {
                buttonVariants[i].interactable = false;
            }
        }
    }
    
    public void UnlockedEndings()
    {
        if (GameData.unlockedEnding1 == true)
        {
            buttonEndings[0].interactable = true;
        }
        else
        {
            buttonEndings[0].interactable = false;
        }
        //Debug.Log("UnlockedEnding1 = " + GameData.unlockedEnding1);

        if (GameData.unlockedEnding2 == true)
        {
            buttonEndings[1].interactable = true;
        }
        else
        {
            buttonEndings[1].interactable = false;
        }
        //Debug.Log("UnlockedEnding2 = " + GameData.unlockedEnding2);
    }

    public void OpeningCharacter(int index)
    {
        character.GetComponent<CharacterAnimation>().SetCharacterLevel(index + 1);
    }

    private void CheckState()
    {
        if (!isVariants) Variations.SetActive(false);
        if (!isEndings) Endings.SetActive(false);
    }

    private void AddListener()
    {
        for (int i = 0; i < buttonVariants.Length; i++)
        {
            int index = i; // tránh lỗi biến loop
            buttonVariants[i].onClick.AddListener(() => OpeningCharacter(index));
        }
    }
}

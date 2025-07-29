using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FoodManager : MonoBehaviour
{
    [Header("Input system")]
    [SerializeField] private InputActionReference pointAction; // Input action cho vị trí (chuột/chạm)

    [Header("Food Field")]
    [SerializeField] private List<GameObject> foodField;
    private List<GameObject> listInactiveFood;
    private bool startSpawnFood;

    private void OnEnable()
    {
        pointAction.action.performed += OnPickFoodAction;
        LoadFoodData();
    }

    private void OnDisable()
    {
        pointAction.action.performed -= OnPickFoodAction;
        SaveFoodData();
    }

    #region Save and Load
    private void SaveFoodData()
    {
        GameData.SaveHarvestData(listInactiveFood);
    }

    private void LoadFoodData()
    {
        // Lấy danh sách Food đã thu hoạch
        var listFood = GameData.LoadHarvestData();
        listInactiveFood = new List<GameObject>();
        foreach (var foodName in listFood)
        {
            GameObject food = foodField.Find(food => food.name == foodName);
            if (food != null)
            {
                listInactiveFood.Add(food);
                food.SetActive(false); // Ẩn Food đã thu hoạch
            }
        }

        // Bắt đầu spawn food nếu chưa đầy
        if (listInactiveFood.Count > 0)
        {
            startSpawnFood = true;
            //Invoke(nameof(SpawnFood), UnityEngine.Random.Range(5f, 10f));
            StartCoroutine(SpawnFoodRoutine());
        }
    }
    #endregion

    #region Pick Food
    private void OnPickFoodAction(InputAction.CallbackContext context)
    {
        Vector2 screenPos = context.ReadValue<Vector2>();

        // Dùng raycast để tìm UI Image
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("Food")) // Image có tag "Food"
            {
                PickFood(result.gameObject);
                return;
            }
        }
    }
    private void PickFood(GameObject food)
    {
        food.SetActive(false); // Ẩn Food
        AudioManager.Instance?.PlaySFX(AudioManager.KEY.SFX_ClickHarvest);
        listInactiveFood.Add(food); // Thêm vào danh sách inactive

        // Add score
        GameData.PlayerFoodCount++;
        // Bắt đầu spawn food nếu chưa bắt đầu
        if (!startSpawnFood)
        {
            startSpawnFood = true;
            // Invoke(nameof(SpawnFood), UnityEngine.Random.Range(5f, 10f));
            StartCoroutine(SpawnFoodRoutine());
        }
    }
    #endregion

    #region Spawn Food
    // private void SpawnFood()
    // {
    //     // Lấy một Food ngẫu nhiên từ danh sách inactive
    //     int index = UnityEngine.Random.Range(0, listInactiveFood.Count);
    //     GameObject foodToSpawn = listInactiveFood[index];
    //     listInactiveFood.RemoveAt(index);
    //     foodToSpawn.SetActive(true); // Kích hoạt Food

    //     if (listInactiveFood.Count == 0)
    //     {
    //         // Nếu không còn Food nào inactive, dừng spawn
    //         startSpawnFood = false;
    //     }
    //     else
    //     {
    //         // Tiếp tục spawn Food sau một khoảng thời gian ngẫu nhiên
    //         Invoke(nameof(SpawnFood), UnityEngine.Random.Range(5f, 10f));
    //     }
    // }
    private IEnumerator SpawnFoodRoutine()
    {
        // Mỗi lần lặp, spawn 1 food và delay unscaled
        while (startSpawnFood && listInactiveFood.Count > 0)
        {
            // 1. Delay unscaled
            // Tính thời điểm kết thúc delay
            float delay = UnityEngine.Random.Range(5f, 10f);
            System.DateTime start = System.DateTime.UtcNow;
            System.DateTime end = start.AddSeconds(delay);
            // Chờ đến khi thời gian thực đã đủ
            while (System.DateTime.UtcNow < end)
                yield return null;

            // 2. Spawn
            int index = UnityEngine.Random.Range(0, listInactiveFood.Count);
            GameObject foodToSpawn = listInactiveFood[index];
            listInactiveFood.RemoveAt(index);
            foodToSpawn.SetActive(true);

            // 3. Nếu hết thì dừng luôn
            if (listInactiveFood.Count == 0)
            {
                startSpawnFood = false;
                yield break;
            }
        }
    }
    #endregion
}
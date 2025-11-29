using UnityEngine;
using HappyHarvest; // 引用 namespace

public class Animal : MonoBehaviour
{
    [Header("數值設定")]
    [SerializeField] private float hunger = 100f;
    [SerializeField] private float hungerRate = 5f;

    [Header("產出設定")]
    [SerializeField] private Item produceItem;
    [SerializeField] private float produceCooldown = 10f; // 牛奶產出的冷卻時間 (秒)
    private float nextProduceTime = 0f; // 下一次可以產出的時間點

    [Header("餵食設定")]
    [SerializeField] private Item feedItem; // 請拖入 Hay

    public PlayerController playerController;

    [Header("顯示設定 (圖示化)")]
    [SerializeField] private SpriteRenderer statusIconRenderer; // 用來顯示圖示的 SpriteRenderer
    [SerializeField] private Sprite milkSprite; // 牛奶圖示 (可收成時顯示)
    [SerializeField] private Sprite haySprite;  // 稻草圖示 (飢餓時顯示)

    private void Start()
    {
        playerController = GameManager.Instance.Player;

        // 初始設定：如果沒有指定 Icon Renderer，嘗試抓取自己身上的 (建議在子物件放一個專門顯示 Icon 的)
        if (statusIconRenderer == null)
            statusIconRenderer = GetComponentInChildren<SpriteRenderer>();

        UpdateStatusIcon();
    }

    void Update()
    {
        // 飢餓度隨時間增加
        hunger += hungerRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0, 100);

        // 更新圖示顯示狀態
        UpdateStatusIcon();
    }

    private void OnMouseDown()
    {
        OnClickAnimal();
    }

    public void OnClickAnimal()
    {
        // 優先順序：如果餓了就餵食，如果沒餓且準備好產出就收成
        if (IsHungry())
        {
            Feed();
        }
        else if (IsReadyToProduce())
        {
            Collect();
        }
        // 如果既不餓，也在冷卻中，點擊不做任何事
    }

    // 判斷是否飢餓
    private bool IsHungry()
    {
        return hunger >= 50;
    }

    // 判斷是否可以產出 (不餓 + 冷卻時間已到)
    private bool IsReadyToProduce()
    {
        return !IsHungry() && Time.time >= nextProduceTime;
    }

    private void UpdateStatusIcon()
    {
        if (statusIconRenderer == null) return;

        if (IsHungry())
        {
            // 狀態 1: 飢餓 -> 顯示稻草
            statusIconRenderer.sprite = haySprite;
            statusIconRenderer.enabled = true;
        }
        else if (IsReadyToProduce())
        {
            // 狀態 2: 可收成 -> 顯示牛奶
            statusIconRenderer.sprite = milkSprite;
            statusIconRenderer.enabled = true;
        }
        else
        {
            // 狀態 3: 冷卻中或剛吃飽 -> 不顯示
            statusIconRenderer.enabled = false;
        }
    }

    public void Feed()
    {
        if (feedItem == null)
        {
            Debug.LogError("錯誤：請在 Inspector 設定 Feed Item！");
            return;
        }

        if (playerController != null)
        {
            var inventory = playerController.Inventory;

            // 檢查手上是否拿著飼料
            if (inventory.EquippedItem == feedItem)
            {
                int foundIndex = -1;
                for (int i = 0; i < inventory.Entries.Length; i++)
                {
                    if (inventory.Entries[i].Item == feedItem)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex != -1)
                {
                    inventory.Remove(foundIndex, 1);
                    hunger = 0; // 餵飽歸零
                    Debug.Log($"餵食成功！消耗了手上的 {feedItem.name}");
                    // 餵食後會自動在 UpdateStatusIcon 進入冷卻判斷
                }
            }
            else
            {
                Debug.Log("餵食失敗：請將飼料 (Hay) 拿在手上！");
            }
        }
    }

    public void Collect()
    {
        // 只有在準備好時才能收成
        if (!IsReadyToProduce()) return;

        if (playerController != null && produceItem != null)
        {
            bool success = playerController.AddItem(produceItem);
            if (success)
            {
                Debug.Log("收成成功");
                // ★ 設定下一次產出的時間 (現在時間 + 冷卻秒數)
                nextProduceTime = Time.time + produceCooldown;

                // 收成後立即刷新圖示 (會變成隱藏，因為進入冷卻了)
                UpdateStatusIcon();
            }
        }
    }
}
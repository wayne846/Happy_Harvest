using UnityEngine;
using TMPro;
using HappyHarvest; // 引用 namespace

public class Animal : MonoBehaviour
{
    [Header("數值設定")]
    [SerializeField] private float hunger = 100f;
    [SerializeField] private float hungerRate = 5f;

    [Header("產出設定")]
    [SerializeField] private Item produceItem;

    [Header("餵食設定")]
    [SerializeField] private Item feedItem; // 請拖入 Hay

    public PlayerController playerController;

    [Header("顯示設定")]
    [SerializeField] private TextMeshProUGUI hungerText;

    private void Start()
    {
        playerController = GameManager.Instance.Player;
    }

    void Update()
    {
        hunger += hungerRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0, 100);
        UpdateHungerText();
    }

    private void OnMouseDown()
    {
        OnClickAnimal();
    }

    public void OnClickAnimal()
    {
        if (hunger >= 50) Feed();
        else Collect();
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

            // ★ 關鍵修改：檢查「手上拿的裝備」是不是飼料
            if (inventory.EquippedItem == feedItem)
            {
                // 手上確實拿著飼料，開始執行刪除邏輯

                // 1. 為了安全刪除，我們還是需要找到這個物品在背包的哪個索引(Index)
                int foundIndex = -1;
                for (int i = 0; i < inventory.Entries.Length; i++)
                {
                    // 找到第一個符合飼料的格子
                    if (inventory.Entries[i].Item == feedItem)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                // 2. 執行刪除
                if (foundIndex != -1)
                {
                    inventory.Remove(foundIndex, 1); // 刪除一個
                    hunger = 0;
                    Debug.Log($"餵食成功！消耗了手上的 {feedItem.name}");
                }
            }
            else
            {
                // 手上拿錯東西，或是空手
                if (inventory.EquippedItem == null)
                    Debug.Log("餵食失敗：請將飼料 (Hay) 拿在手上！(目前空手)");
                else
                    Debug.Log($"餵食失敗：請將飼料拿在手上！(目前拿著 {inventory.EquippedItem.name})");
            }
        }
    }

    public void Collect()
    {
        if (playerController != null && produceItem != null)
        {
            bool success = playerController.AddItem(produceItem);
            if (success) Debug.Log("收成成功");
        }
    }

    private void UpdateHungerText()
    {
        if (hungerText != null)
        {
            int hungerInt = Mathf.RoundToInt(hunger);
            hungerText.text = hungerInt.ToString();
            if (hunger > 80) hungerText.color = Color.red;
            else hungerText.color = Color.white;
        }
    }
}
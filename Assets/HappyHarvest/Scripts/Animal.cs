using UnityEngine;
using TMPro;
// ---------------------------------------------------------
// ★ 關鍵修改：這裡要加上您剛才查到的 namespace
// 如果 PlayerController 裡寫的是 namespace HappyHarvest; 
// 那就加上下面這行：
using HappyHarvest;

// 如果 Item 在另一個 namespace (例如 HappyHarvest.Items)，也要加上：
// using HappyHarvest.Items;
// ---------------------------------------------------------

public class Animal : MonoBehaviour
{
    [Header("數值設定")]
    [SerializeField] private float hunger = 100f;
    [SerializeField] private float hungerRate = 5f;

    [Header("產出設定")]
    // ★ 關鍵修改：如果套件裡的物品腳本叫 "ItemData" 而不是 "Item"
    // 請把下面的 'Item' 改成 'ItemData'
    [SerializeField] private Item produceItem;

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
        hunger = 0;
        Debug.Log("動物已餵食");
    }

    public void Collect()
    {
        if (playerController != null && produceItem != null)
        {
            // ★ 如果這裡報錯，請確認 AddItem 括號內需要的參數
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
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; // 新增：引用文字系統 (TextMeshPro)

public class SimpleMiner : MonoBehaviour
{
    [Header("必要設置")]
    public Tilemap groundTilemap;

    [Header("金礦設置")]
    public TileBase goldTileAsset; // 告訴程式「金礦」長什麼樣子
    public TextMeshProUGUI goldText; // 拖入剛剛做的 UI 文字

    [Header("油條設置")]
    public Transform fuelFillSprite;

    [Header("參數調整")]
    public float moveSpeed = 5f;
    public float drillTime = 0.5f;

    [Header("燃料設定")]
    public float maxFuel = 100f;
    public float fuelConsumptionMove = 1f;
    public float fuelConsumptionDrill = 3f;

    // 私有變數
    private float currentFuel;
    private int currentGold = 0; // 新增：目前的金礦數量
    private bool isBusy = false;
    private Vector3 originalScale;

    void Start()
    {
        if (fuelFillSprite != null) originalScale = fuelFillSprite.localScale;
        currentFuel = maxFuel;

        UpdateFuelBar();
        UpdateGoldUI(); // 一開始先更新一次文字
    }

    void Update()
    {
        if (isBusy || currentFuel <= 0) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x != 0) CheckDestination(new Vector2Int((int)x, 0));
        else if (y != 0) CheckDestination(new Vector2Int(0, (int)y));
    }

    void CheckDestination(Vector2Int direction)
    {
        Vector3 targetWorldPos = transform.position + new Vector3(direction.x, direction.y, 0);
        Vector3Int gridPos = groundTilemap.WorldToCell(targetWorldPos);
        TileBase tile = groundTilemap.GetTile(gridPos);

        if (tile != null)
        {
            // 傳入 tile 資訊給挖掘函式，讓它知道挖到了什麼
            StartCoroutine(DrillRoutine(targetWorldPos, gridPos, tile));
        }
        else
        {
            StartCoroutine(MoveRoutine(targetWorldPos));
        }
    }

    // 修改：多接收一個 tile 參數，用來判斷挖到的是不是金礦
    IEnumerator DrillRoutine(Vector3 targetPos, Vector3Int gridPos, TileBase targetTile)
    {
        isBusy = true;
        yield return new WaitForSeconds(drillTime);

        // 1. 檢查挖到的是不是金礦
        if (targetTile == goldTileAsset)
        {
            currentGold++; // 金礦 +1
            UpdateGoldUI(); // 更新介面
            Debug.Log("挖到金礦了！目前數量：" + currentGold);
        }

        // 2. 消除方塊
        groundTilemap.SetTile(gridPos, null);

        ConsumeFuel(fuelConsumptionDrill);

        yield return StartCoroutine(MoveRoutine(targetPos, false));
    }

    IEnumerator MoveRoutine(Vector3 targetPos, bool consumeFuel = true)
    {
        isBusy = true;
        if (consumeFuel) ConsumeFuel(fuelConsumptionMove);

        while (Vector3.Distance(transform.position, targetPos) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isBusy = false;
    }

    void ConsumeFuel(float amount)
    {
        currentFuel -= amount;
        if (currentFuel < 0) currentFuel = 0;
        UpdateFuelBar();
    }

    void UpdateFuelBar()
    {
        if (fuelFillSprite != null)
        {
            float ratio = currentFuel / maxFuel;
            fuelFillSprite.localScale = new Vector3(originalScale.x * ratio, originalScale.y, originalScale.z);
        }
    }

    // 新增：更新金礦文字
    void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + currentGold;
        }
    }
}
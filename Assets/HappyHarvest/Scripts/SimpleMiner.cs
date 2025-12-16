using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class SimpleMiner : MonoBehaviour
{
    [Header("必要設置")]
    public Tilemap groundTilemap;

    [Header("視覺設置 (新功能)")]
    public Transform bodyTransform; // 請拖入剛剛建立的 Body 物件
    public bool spriteDrawnFacingDown = false; // 如果你的圖片原本就是畫成朝下的，請打勾

    [Header("金礦與UI")]
    public TileBase goldTileAsset;
    public TextMeshProUGUI goldText;
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
    private int currentGold = 0;
    private bool isBusy = false;
    private Vector3 originalScale;

    void Start()
    {
        if (fuelFillSprite != null) originalScale = fuelFillSprite.localScale;
        currentFuel = maxFuel;
        UpdateFuelBar();
        UpdateGoldUI();

        // --- 遊戲開始時，強制設定為朝下 ---
        RotatePlayer(Vector2.down);
    }

    void Update()
    {
        if (isBusy || currentFuel <= 0) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x != 0)
        {
            Vector2Int dir = new Vector2Int((int)x, 0);
            RotatePlayer(dir);
            CheckDestination(dir);
        }
        else if (y != 0)
        {
            Vector2Int dir = new Vector2Int(0, (int)y);
            RotatePlayer(dir);
            CheckDestination(dir);
        }
    }

    void RotatePlayer(Vector2 direction)
    {
        if (bodyTransform == null) return;

        // 判斷基準向量：如果你的圖原本是朝右(標準)，基準就是 Right；如果原本朝下，基準就是 Down
        Vector2 baseDir = spriteDrawnFacingDown ? Vector2.down : Vector2.right;

        // 計算角度
        float angle = Vector2.SignedAngle(baseDir, direction);

        // 只旋轉 Body，不動 Player 本體
        bodyTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // --- 以下邏輯不變 ---
    void CheckDestination(Vector2Int direction)
    {
        Vector3 targetWorldPos = transform.position + new Vector3(direction.x, direction.y, 0);
        Vector3Int gridPos = groundTilemap.WorldToCell(targetWorldPos);
        TileBase tile = groundTilemap.GetTile(gridPos);

        if (tile != null) StartCoroutine(DrillRoutine(targetWorldPos, gridPos, tile));
        else StartCoroutine(MoveRoutine(targetWorldPos));
    }

    IEnumerator DrillRoutine(Vector3 targetPos, Vector3Int gridPos, TileBase targetTile)
    {
        isBusy = true;
        yield return new WaitForSeconds(drillTime);

        if (targetTile == goldTileAsset)
        {
            currentGold++;
            UpdateGoldUI();
        }

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

    void UpdateGoldUI()
    {
        if (goldText != null) goldText.text = "Gold: " + currentGold;
    }
}
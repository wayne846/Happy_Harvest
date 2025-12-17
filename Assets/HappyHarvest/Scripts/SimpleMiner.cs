using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.SceneManagement;
using HappyHarvest; // 新增：必須引用這個才能切換場景！

public class SimpleMiner : MonoBehaviour
{
    public PlayerController playerController;
    public Item produceItem;
    [Header("必要設置")]
    public Tilemap groundTilemap;

    [Header("音效設置")]
    public AudioSource moveAudio;
    public AudioSource drillAudio;

    [Header("視覺設置")]
    public Transform bodyTransform;
    public bool spriteDrawnFacingDown = false;

    [Header("金礦與UI")]
    public TileBase goldTileAsset;
    public TextMeshProUGUI goldText;
    public Transform fuelFillSprite;

    [Header("場景切換設置 (新功能)")]
    public string sceneToLoad = "MainMenu"; // 沒油後要載入的場景名稱
    public float delayBeforeLoad = 2.0f;    // 沒油後等待幾秒才切換

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
    private bool isGameOver = false; // 新增：防止 Game Over 重複觸發
    private Vector3 originalScale;

    void Start()
    {
        if (fuelFillSprite != null) originalScale = fuelFillSprite.localScale;
        currentFuel = maxFuel;
        UpdateFuelBar();
        RotatePlayer(Vector2.down);
    }

    void Update()
    {
        // 如果忙碌、沒油或已經結束遊戲，就停止操作
        if (isBusy || currentFuel <= 0 || isGameOver) return;

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
        Vector2 baseDir = spriteDrawnFacingDown ? Vector2.down : Vector2.right;
        float angle = Vector2.SignedAngle(baseDir, direction);
        bodyTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

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
        if (drillAudio != null) drillAudio.Play();

        yield return new WaitForSeconds(drillTime);

        if (drillAudio != null) drillAudio.Stop();

        if (targetTile == goldTileAsset)
        {
            currentGold++;
            playerController = GameManager.Instance.Player;
            playerController.AddItem(produceItem);
        }

        groundTilemap.SetTile(gridPos, null);
        ConsumeFuel(fuelConsumptionDrill);
        yield return StartCoroutine(MoveRoutine(targetPos, false));
    }

    IEnumerator MoveRoutine(Vector3 targetPos, bool consumeFuel = true)
    {
        isBusy = true;
        if (consumeFuel) ConsumeFuel(fuelConsumptionMove);

        if (moveAudio != null && !moveAudio.isPlaying) moveAudio.Play();

        while (Vector3.Distance(transform.position, targetPos) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        if (moveAudio != null) moveAudio.Stop();

        isBusy = false;
    }

    void ConsumeFuel(float amount)
    {
        currentFuel -= amount;
        if (currentFuel <= 0)
        {
            currentFuel = 0;
            // 觸發遊戲結束流程
            if (!isGameOver)
            {
                StartCoroutine(GameOverRoutine());
            }
        }
        UpdateFuelBar();
    }

    // 新增：處理遊戲結束與切換場景的協程
    IEnumerator GameOverRoutine()
    {
        isGameOver = true;
        Debug.Log("沒油了！準備返回主選單...");

        // 可以在這裡播放一個沒油的音效，或是讓角色閃爍紅光

        // 等待幾秒，讓玩家反應過來
        yield return new WaitForSeconds(delayBeforeLoad);

        // 載入指定場景
        //SceneManager.LoadScene(sceneToLoad);
        GameManager.Instance.MoveTo(2, 0);
    }

    void UpdateFuelBar()
    {
        if (fuelFillSprite != null)
        {
            float ratio = currentFuel / maxFuel;
            fuelFillSprite.localScale = new Vector3(originalScale.x * ratio, originalScale.y, originalScale.z);
        }
    }


}
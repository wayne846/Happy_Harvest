using UnityEngine;
using HappyHarvest;
using UnityEngine.VFX; // 引用 VFX

public class Animal : MonoBehaviour
{
    [Header("數值設定")]
    [SerializeField] private float hunger = 100f;
    [SerializeField] private float hungerRate = 5f;

    [Header("產出設定")]
    [SerializeField] private Item produceItem;
    [SerializeField] private float produceCooldown = 10f;
    private float nextProduceTime = 0f;

    [Header("餵食設定")]
    [SerializeField] private Item feedItem;

    [Header("特效設定")]
    [SerializeField] private VisualEffect feedVFX; // 拖入 VFX_Eat
    [SerializeField] private string vfxEventName = "OnPlay";
    [SerializeField] private AudioClip feedSound;

    public PlayerController playerController;

    [Header("顯示設定")]
    [SerializeField] private SpriteRenderer statusIconRenderer;
    [SerializeField] private Sprite milkSprite;
    [SerializeField] private Sprite haySprite;

    private AudioSource audioSource;

    private void Start()
    {
        playerController = GameManager.Instance.Player;

        if (statusIconRenderer == null)
            statusIconRenderer = GetComponentInChildren<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // ★ 初始化：確保遊戲開始時特效是隱藏的，以免一出來就噴特效
        if (feedVFX != null)
        {
            feedVFX.gameObject.SetActive(false);
        }

        UpdateStatusIcon();
    }

    void Update()
    {
        hunger += hungerRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0, 100);
        UpdateStatusIcon();
    }

    private void OnMouseDown()
    {
        OnClickAnimal();
    }

    public void OnClickAnimal()
    {
        if (IsHungry()) Feed();
        else if (IsReadyToProduce()) Collect();
    }

    private bool IsHungry() => hunger >= 50;
    private bool IsReadyToProduce() => !IsHungry() && Time.time >= nextProduceTime;

    private void UpdateStatusIcon()
    {
        if (statusIconRenderer == null) return;
        if (IsHungry()) { statusIconRenderer.sprite = haySprite; statusIconRenderer.enabled = true; }
        else if (IsReadyToProduce()) { statusIconRenderer.sprite = milkSprite; statusIconRenderer.enabled = true; }
        else statusIconRenderer.enabled = false;
    }

    public void Feed()
    {
        if (feedItem == null) { Debug.LogError("請設定 Feed Item"); return; }

        if (playerController != null)
        {
            var inventory = playerController.Inventory;
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
                    hunger = 0;
                    Debug.Log($"餵食成功！");

                    // 播放特效
                    PlayFeedEffects();
                    UpdateStatusIcon();
                }
            }
            else
            {
                Debug.Log("請將飼料拿在手上！");
            }
        }
    }

    // ★ 修改後的特效播放邏輯
    private void PlayFeedEffects()
    {
        // 1. 播放 VFX
        if (feedVFX != null)
        {
            // ★ 關鍵修正：先強制把 GameObject 打開 (SetActive true)
            feedVFX.gameObject.SetActive(true);

            // 然後發送播放訊號
            feedVFX.SendEvent(vfxEventName);
        }

        // 2. 播放聲音
        if (feedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(feedSound);
        }
    }

    public void Collect()
    {
        if (!IsReadyToProduce()) return;
        if (playerController != null && produceItem != null)
        {
            if (playerController.AddItem(produceItem))
            {
                Debug.Log("收成成功");
                nextProduceTime = Time.time + produceCooldown;
                UpdateStatusIcon();
            }
        }
    }
}
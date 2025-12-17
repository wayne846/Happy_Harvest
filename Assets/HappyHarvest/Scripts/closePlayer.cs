using HappyHarvest;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneObjectController : MonoBehaviour
{
    [Header("需要控制的物件")]
    [SerializeField] private GameObject playerObject;      // 拖入你的 Character
    [SerializeField] private GameObject gameManagerUI; // 拖入你的 GameManager

    [Header("設定")]
    [SerializeField] private int menuSceneIndex = 4; // 主選單的 Index

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // 確保這個控制器自己不會死掉
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 判斷是否在主選單
        bool isMainMenu = (scene.buildIndex == 4 || scene.buildIndex == 5);

        // 控制主角開關
        if (playerObject != null)
        {
            // 如果是主選單 -> 關掉(false)；如果是遊戲 -> 打開(true)
            // 驚嘆號 (!) 代表「反轉」的意思
            playerObject.SetActive(!isMainMenu);
        }

        // 控制 GameManager 開關
        if (GameManager.Instance != null)
        {
            // 取得 GameManager 掛載的那個 GameObject 並開關
            Transform ui = GameManager.Instance.transform.Find("UI");
            //ui.gameObject.SetActive(!isMainMenu);
        }

        Debug.Log($"場景載入完成 (Index: {scene.buildIndex})。是否為選單: {isMainMenu}");
    }
}
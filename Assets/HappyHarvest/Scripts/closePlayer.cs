using UnityEngine;
using UnityEngine.SceneManagement;

public class closePlayer: MonoBehaviour
{
    [Header("把你的主角物件拖進這裡")]
    [SerializeField] private GameObject playerObject;

    // 設定要隱藏主角的場景 Index (例如你的主選單是 4)
    [SerializeField] private int menuSceneIndex = 4;

    private void Awake()
    {
        // 確保這個 Manager 換場景時不會消失
        DontDestroyOnLoad(gameObject);

        // 防呆：如果忘記拉主角，嘗試自動抓取 (僅限主角是開啟狀態時才抓得到)
        if (playerObject == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                playerObject = foundPlayer;
            }
            else
            {
                Debug.LogWarning("PlayerVisibilityManager: 警告！沒有指定主角物件，且找不到 Tag 為 Player 的物件。");
            }
        }
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
        // 如果沒有參考到主角，就無法做事
        if (playerObject == null) return;

        // 判斷邏輯
        if (scene.buildIndex == menuSceneIndex)
        {
            // 在主選單 -> 關掉主角
            // 因為是 Manager 在執行，即使主角被關掉，Manager 還是活著的，下次還能把他打開
            playerObject.SetActive(false);
            Debug.Log($"進入場景 {scene.buildIndex} (主選單)，已隱藏主角。");
        }
        else
        {
            // 在其他關卡 -> 打開主角
            // 這裡要注意：如果主角本來就是開的，這行也不會有副作用
            playerObject.SetActive(true);
            Debug.Log($"進入場景 {scene.buildIndex} (遊戲關卡)，已顯示主角。");

            // ★ 進階提示：如果你需要在這裡重設座標，可以在這裡呼叫 Player 身上的腳本
            // playerObject.transform.position = new Vector3(0, 0, 0);
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 記得引用 TextMeshPro
using System;

namespace HappyHarvest
{
    public class GamblingUI : MonoBehaviour
    {
        [Header("系統連結")]
        public Gambling GamblingSystem; // 連結回 Gambling 邏輯腳本

        [Header("UI 元件")]
        public GameObject panelRoot;
        public TMP_InputField bet; // 輸入賭注的地方
        public Button spin;         // 開始轉動的按鈕
        public Button return_to_farm;
        //public TextMeshProUGUI resultText;// 顯示結果 (+500) 的文字

        [Header("顯示物件")]
        public GameObject resultImageObject; // 中獎/結果顯示圖
        public TMP_Text reusult_text;

        [Header("錯誤提示圖")]
        public GameObject noMoneyImage;      // 情況 A：錢不夠時顯示
        public GameObject inputErrorImage;   // ★ 情況 B：輸入錯誤(0或負數)時顯示

        [Header("轉盤物件")]
        public Transform Wheel_Operator;
        public Transform Wheel_Number;

        [Header("音效設定")]
        public AudioSource audioSource;
        public AudioClip roll;     // 轉動時的聲音 (會重複播放)
        public AudioClip sad;
        public AudioClip win;

        [Header("設定")]
        public int numberOfOperatorSlots = 8;
        public int numberOfNumberSlots = 8;
        public float spinDuration = 3.0f;
        public int minFullSpins = 5;
        public int opIndex = 0;
        public int numIndex = 0;
        public int wager = 0;
        public AnimationCurve spinCurve;

        private Action onSpinCompleteCallback;
        private bool isSpinning = false; // 防止重複點擊
        public Gambling.IndexPair index = new Gambling.IndexPair();
        public Gambling.ResultPair resultPair = new Gambling.ResultPair();

        private void Start()
        {
            // 綁定按鈕事件
            if (spin != null)
            {
                spin.onClick.AddListener(OnSpinButtonClicked);
            }

            if (return_to_farm != null)
            {
                return_to_farm.onClick.AddListener(OnReturnButtonClicked);
            }

            // 初始化 UI 狀態
            //if (resultText != null) resultText.text = "";
            resultImageObject.SetActive(false);
            noMoneyImage.SetActive(false);
            inputErrorImage.SetActive(false);
        }

        // 當玩家按下 "SPIN" 按鈕
        private void OnSpinButtonClicked()
        {
            Debug.Log("按鈕被按下了！"); // 檢查點 1：按鈕有沒有壞

            if (isSpinning)
            {
                Debug.Log("正在轉動中，忽略點擊");
                return;
            }

            wager = 0;
            if (bet != null && int.TryParse(bet.text, out int result))
            {
                wager = result;
            }
            Debug.Log($"讀取到的賭注是: {wager}"); // 檢查點 2：賭注讀取對不對

            // 情況 B：輸入錯誤 (0 或 負數)
            if (wager <= 0)
            {
                // ★ 顯示「輸入錯誤」的圖片
                StartCoroutine(ShowWarningRoutine(inputErrorImage));

                //if (resultText) resultText.text = "金額無效!";
                return; // 直接結束，不繼續執行
            }

            // 重置介面
            //if (resultText != null) resultText.text = "";
            if (resultImageObject != null) resultImageObject.SetActive(false);
            if (noMoneyImage != null) noMoneyImage.SetActive(false);
            if (inputErrorImage != null) inputErrorImage.SetActive(false);

            // 呼叫系統
            if (GamblingSystem != null)
            {
                index = GamblingSystem.StartGambling(wager);

                // 情況 A：系統回傳失敗 (-1) 代表錢不夠
                if (index.opIndex == 9 && index.numIndex == 9)
                {
                    // ★ 顯示「錢不夠」的圖片
                    StartCoroutine(ShowWarningRoutine(noMoneyImage));
                    return;
                }
            }
        }

        // 通用的顯示警告協程 (傳入哪張圖，就顯示哪張)
        private IEnumerator ShowWarningRoutine(GameObject imageToShow)
        {
            if (imageToShow == null) yield break;

            // 1. 顯示指定的圖片
            imageToShow.SetActive(true);

            // 2. 鎖住按鈕
            if (spin != null) spin.interactable = false;

            // 3. 等待 2 秒
            yield return new WaitForSeconds(2.0f);

            // 4. 隱藏圖片
            imageToShow.SetActive(false);

            // 5. 解鎖按鈕
            if (spin != null) spin.interactable = true;
            if (bet != null) bet.ActivateInputField();
        }

        private void OnReturnButtonClicked()
        {
            GameManager.Instance.MoveTo(2, 0);
        }

        // 被 Gambling.cs 呼叫，開始執行轉動動畫
        public void OpenAndSpin(Gambling.ResultPair result, int opind, int numind, Action onComplete)
        {
            resultPair = result;
            index.opIndex = opind;
            index.numIndex = numind;
            Debug.Log(opind);
            Debug.Log(numIndex);
            if (panelRoot != null) panelRoot.SetActive(true);

            onSpinCompleteCallback = onComplete;
            isSpinning = true;
            spin.interactable = false; // 轉動時鎖住按鈕
            if (bet != null) bet.interactable = false; // 鎖住輸入框

            // 啟動轉盤協程
            StartCoroutine(SpinProcess());
        }

        private IEnumerator SpinProcess()
        {
            // 同時啟動兩個轉盤，但這裡我們用一個協程來管理整體的等待
            Coroutine opSpin = StartCoroutine(SpinWheelRoutine(Wheel_Operator, numberOfOperatorSlots, index.opIndex));
            Coroutine numSpin = StartCoroutine(SpinWheelRoutine(Wheel_Number, numberOfNumberSlots, index.numIndex));

            // 等待兩個轉盤都停下來
            yield return opSpin;
            yield return numSpin;

            // --- 轉動結束，直接結算 ---
            int final = wager;

            switch (resultPair.op)
            {
                case Gambling.Operator.Add:
                    final += resultPair.number;
                    break;
                case Gambling.Operator.Subtract:
                    final -= resultPair.number;
                    break;
                case Gambling.Operator.Multiply:
                    final *= resultPair.number;
                    break;
            }

            // 防止結算價值小於 0 (如果不想讓賭注變成負債)
            if (final < 0) final = 0;

            showresult(wager.ToString() + " " + resultPair.op.ToString() + " " + resultPair.number.ToString() + "=" + final.ToString());

            // 1. 執行給錢邏輯
            onSpinCompleteCallback?.Invoke();

            // 3. 解鎖按鈕，讓玩家可以再次賭博
            isSpinning = false;
            spin.interactable = true;
            if (bet != null) bet.interactable = true;
        }

        // 單個轉盤的轉動邏輯
        private IEnumerator SpinWheelRoutine(Transform wheel, int slots, int targetIndex)
        {
            float anglePerSlot = -360f / slots;
            // 假設 0 在正上方，根據你的貼圖可能需要調整 offset
            // 如果數字對不準，請調整這個 offset 值 (例如 + 18f 或 -18f)
            float angleOffset = -22f;

            float targetAngle = (targetIndex * anglePerSlot) + angleOffset;
            float endAngle = -(360 * minFullSpins + targetAngle);
            float startAngle = wheel.localEulerAngles.z;

            float timer = 0f;
            while (timer < spinDuration)
            {
                if (roll != null)
                {
                    roll.LoadAudioData();
                }
                timer += Time.deltaTime;
                float progress = timer / spinDuration;
                float curveValue = spinCurve.Evaluate(progress);

                float currentAngle = Mathf.Lerp(startAngle, endAngle, curveValue);
                wheel.localEulerAngles = new Vector3(0, 0, currentAngle);
                yield return null;
            }
            wheel.localEulerAngles = new Vector3(0, 0, endAngle);
        }

        // 簡單的文字彈跳效果
        private void showresult(string text)
        {
            reusult_text.text = text;
            StartCoroutine(ShowWarningRoutine(resultImageObject));
            reusult_text.text = "";
        }
    }
}
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
        public TMP_Text bet; // 輸入賭注的地方
        public Button spin;         // 開始轉動的按鈕
        public Button return_to_farm;
        public Button plus_one;
        public Button minus_one;
        public Button plus_ten;
        public Button minus_ten;
        public Button ALL;

        [Header("顯示物件")]
        public GameObject resultImageObject; // 中獎/結果顯示圖
        public TMP_Text result_text;

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
            if (bet != null)
            {
                bet.text = "0";
            }

            // 綁定按鈕事件
            if (spin != null) spin.onClick.AddListener(OnSpinButtonClicked);

            if (plus_one != null) plus_one.onClick.AddListener(() => AdjustBet(1));
            if (minus_one != null) minus_one.onClick.AddListener(() => AdjustBet(-1));
            if (plus_ten != null) plus_ten.onClick.AddListener(() => AdjustBet(10));
            if (minus_ten != null) minus_ten.onClick.AddListener(() => AdjustBet(-10));
            if (ALL != null) ALL.onClick.AddListener(OnAllInClicked);

            if (return_to_farm != null)
            {
                return_to_farm.onClick.AddListener(OnReturnButtonClicked);
            }

            // 初始化 UI 狀態
            //if (resultText != null) resultText.text = "";
            resultImageObject.SetActive(false);
            noMoneyImage.SetActive(false);
            inputErrorImage.SetActive(false);
            result_text.text = "";
            SetButtonsInteractable(true);
        }

        private void AdjustBet(int amount)
        {
            if (isSpinning) return;

            wager += amount;

            if (wager < 0)
            {
                wager -= amount; // 不能小於 0
                StartCoroutine(ShowWarningRoutine(inputErrorImage));
            }

            UpdateBetDisplay();
        }

        private void OnAllInClicked()
        {
            if (isSpinning) return;

            // 呼叫系統
            if (GamblingSystem != null)
            {
                index = GamblingSystem.StartGambling(-2);

                // 情況 A：系統回傳失敗 (-1) 代表錢不夠
                if (index.opIndex == 9)
                {
                    wager = index.numIndex;
                }
            }

            UpdateBetDisplay();
        }

        private void UpdateBetDisplay()
        {
            // ★★★ 雖然語法一樣是用 .text，但現在它是改一般的 Text 元件 ★★★
            if (bet != null)
            {
                bet.text = wager.ToString();
            }
        }

        // 當玩家按下 "SPIN" 按鈕
        private void OnSpinButtonClicked()
        {
            Debug.Log("按鈕被按下了！"); // 檢查點 1：按鈕有沒有壞
            if (result_text.text != null)
            {
                result_text.text = "";
            }
            if (isSpinning)
            {
                Debug.Log("正在轉動中，忽略點擊");
                return;
            }

            // 重置介面
            if (resultImageObject != null) resultImageObject.SetActive(false);
            if (noMoneyImage != null) noMoneyImage.SetActive(false);
            if (inputErrorImage != null) inputErrorImage.SetActive(false);

            if (wager == 0)
            {
                StartCoroutine(ShowWarningRoutine(inputErrorImage));
                wager = 0;
                UpdateBetDisplay();
                return;
            }

            // 呼叫系統
            if (GamblingSystem != null)
            {
                index = GamblingSystem.StartGambling(wager);

                // 情況 A：系統回傳失敗 (-1) 代表錢不夠
                if (index.opIndex == 9 && index.numIndex == 9)
                {
                    // ★ 顯示「錢不夠」的圖片
                    StartCoroutine(ShowWarningRoutine(noMoneyImage));
                    wager = 0;
                    UpdateBetDisplay();
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
            SetButtonsInteractable(false);

            if (imageToShow == resultImageObject)
            {
                yield return new WaitForSeconds(4.0f);
            }
            else
            {
                // 3. 等待 2 秒
                yield return new WaitForSeconds(2.0f);
            }

            // 4. 隱藏圖片
            imageToShow.SetActive(false);

            // 5. 解鎖按鈕
            if (result_text.text != null)
            {
                result_text.text = "";
            }
            SetButtonsInteractable(true);
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
            if (panelRoot != null) panelRoot.SetActive(true);

            onSpinCompleteCallback = onComplete;
            isSpinning = true;
            SetButtonsInteractable(false);

            // 啟動轉盤協程
            StartCoroutine(SpinProcess());
        }

        private void SetButtonsInteractable(bool state)
        {
            if (spin != null) spin.interactable = state;
            if (return_to_farm != null) return_to_farm.interactable = state;

            // 下注按鈕在轉動時鎖住
            if (plus_one != null) plus_one.interactable = state;
            if (plus_ten != null) plus_ten.interactable = state;
            if (minus_one != null) minus_one.interactable = state;
            if (minus_ten != null) minus_ten.interactable = state;
            if (ALL != null) ALL.interactable = state;

            // ★ 注意：betDisplay 不需要設定，因為 Text 本來就不能點
        }

        private IEnumerator SpinProcess()
        {
            // 同時啟動兩個轉盤，但這裡我們用一個協程來管理整體的等待
            Coroutine opSpin = StartCoroutine(SpinWheelRoutine(Wheel_Operator, numberOfOperatorSlots, index.opIndex));
            Coroutine numSpin = StartCoroutine(SpinWheelRoutine(Wheel_Number, numberOfNumberSlots, index.numIndex));

            // 等待兩個轉盤都停下來
            yield return opSpin;
            yield return numSpin;

            yield return new WaitForSeconds(2.0f);

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

            if (resultPair.op == Gambling.Operator.Subtract || resultPair.number == 0)
            {
                if (audioSource != null && roll != null)
                {
                    audioSource.PlayOneShot(sad);
                }
            }
            else
            {
                if (audioSource != null && roll != null)
                {
                    audioSource.PlayOneShot(win);
                }
            }

            // 防止結算價值小於 0 (如果不想讓賭注變成負債)
            if (final < 0) final = 0;

            result_text.text = wager.ToString() + " " + resultPair.op.ToString() + " " + resultPair.number.ToString() + " = " + final.ToString();
            StartCoroutine(ShowWarningRoutine(resultImageObject));

            // 1. 執行給錢邏輯
            onSpinCompleteCallback?.Invoke();

            // 3. 解鎖按鈕，讓玩家可以再次賭博
            isSpinning = false;
            SetButtonsInteractable(true);
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
            if (audioSource != null && roll != null)
            {
                audioSource.clip = roll; // 設定要播什麼
                audioSource.loop = false;      // 錯誤音效不需要循環
                audioSource.Play();            // 開始播放
            }
            while (timer < spinDuration)
            {

                timer += Time.deltaTime;
                float progress = timer / spinDuration;
                float curveValue = spinCurve.Evaluate(progress);

                float currentAngle = Mathf.Lerp(startAngle, endAngle, curveValue);
                wheel.localEulerAngles = new Vector3(0, 0, currentAngle);
                yield return null;
            }
            if (audioSource != null)
            {
                audioSource.Stop(); // ★ 這裡就是限制時間的關鍵！
            }
            wheel.localEulerAngles = new Vector3(0, 0, endAngle);
        }
    }
}
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
        public TextMeshProUGUI resultText;// 顯示結果 (+500) 的文字

        [Header("轉盤物件")]
        public Transform Wheel_Operator;
        public Transform Wheel_Number;

        [Header("設定")]
        public int numberOfOperatorSlots = 8;
        public int numberOfNumberSlots = 8;
        public float spinDuration = 3.0f;
        public int minFullSpins = 5;
        public AnimationCurve spinCurve;

        private Action onSpinCompleteCallback;
        private bool isSpinning = false; // 防止重複點擊

        private void Start()
        {
            // 綁定按鈕事件
            if (spin != null)
            {
                spin.onClick.AddListener(OnSpinButtonClicked);
            }

            // 初始化 UI 狀態
            if (resultText != null) resultText.text = "";
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

            int wager = 0;
            if (bet != null && int.TryParse(bet.text, out int result))
            {
                wager = result;
            }
            Debug.Log($"讀取到的賭注是: {wager}"); // 檢查點 2：賭注讀取對不對

            if (wager <= 0)
            {
                Debug.LogWarning("賭注無效");
                if (resultText) resultText.text = "請輸入金額!";
                return;
            }

            if (resultText != null) resultText.text = "";

            // 檢查點 3：連線有沒有斷
            if (GamblingSystem != null)
            {
                Debug.Log("呼叫 GamblingSystem...");
                GamblingSystem.StartGambling(wager);
            }
            else
            {
                Debug.LogError("嚴重錯誤：GamblingSystem 沒有綁定！請檢查 Inspector！");
            }
        }

        // 被 Gambling.cs 呼叫，開始執行轉動動畫
        public void OpenAndSpin(Gambling.Operator targetOp, int targetNum, Action onComplete)
        {
            if (panelRoot != null) panelRoot.SetActive(true);

            onSpinCompleteCallback = onComplete;
            isSpinning = true;
            spin.interactable = false; // 轉動時鎖住按鈕
            if (bet != null) bet.interactable = false; // 鎖住輸入框

            // 啟動轉盤協程
            StartCoroutine(SpinProcess(targetOp, targetNum));
        }

        private IEnumerator SpinProcess(Gambling.Operator targetOp, int targetNum)
        {
            // 同時啟動兩個轉盤，但這裡我們用一個協程來管理整體的等待
            Coroutine opSpin = StartCoroutine(SpinWheelRoutine(Wheel_Operator, numberOfOperatorSlots, (int)targetOp));
            Coroutine numSpin = StartCoroutine(SpinWheelRoutine(Wheel_Number, numberOfNumberSlots, targetNum));

            // 等待兩個轉盤都停下來
            yield return opSpin;
            yield return numSpin;

            // --- 轉動結束，直接結算 ---

            // 1. 執行給錢邏輯
            onSpinCompleteCallback?.Invoke();

            // 2. 顯示結果文字 (延遲一點點顯示比較有感)
            // 這裡我們需要知道剛剛算出來是多少錢，或是簡單顯示 "完成"
            // 因為 logic 層已經算好錢了，這裡單純顯示運算式即可
            string opSymbol = "";
            switch (targetOp)
            {
                case Gambling.Operator.Add: opSymbol = "+"; break;
                case Gambling.Operator.Subtract: opSymbol = "-"; break;
                case Gambling.Operator.Multiply: opSymbol = "x"; break;
            }

            if (resultText != null)
            {
                resultText.text = $"{opSymbol} {targetNum}";
                // 可以加個簡單的放大動畫效果
                StartCoroutine(AnimateText(resultText));
            }

            // 3. 解鎖按鈕，讓玩家可以再次賭博
            isSpinning = false;
            spin.interactable = true;
            if (bet != null) bet.interactable = true;
        }

        // 單個轉盤的轉動邏輯
        private IEnumerator SpinWheelRoutine(Transform wheel, int slots, int targetIndex)
        {
            float anglePerSlot = 360f / slots;
            // 假設 0 在正上方，根據你的貼圖可能需要調整 offset
            // 如果數字對不準，請調整這個 offset 值 (例如 + 18f 或 -18f)
            float angleOffset = 0f;

            float targetAngle = (targetIndex * anglePerSlot) + angleOffset;
            float endAngle = -(360 * minFullSpins + targetAngle);
            float startAngle = wheel.localEulerAngles.z;

            float timer = 0f;
            while (timer < spinDuration)
            {
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
        private IEnumerator AnimateText(TextMeshProUGUI text)
        {
            float timer = 0;
            Vector3 originalScale = Vector3.one;
            while (timer < 0.2f)
            {
                timer += Time.deltaTime;
                text.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.5f, timer / 0.2f);
                yield return null;
            }
            timer = 0;
            while (timer < 0.1f)
            {
                timer += Time.deltaTime;
                text.transform.localScale = Vector3.Lerp(originalScale * 1.5f, originalScale, timer / 0.1f);
                yield return null;
            }
        }
    }
}
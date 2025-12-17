using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HappyHarvest
{
    /// <summary>
    /// 賭博系統
    /// 負責處理賭博小遊戲的邏輯：先扣除賭注 -> 生成隨機運算 -> 將運算後的金額還給玩家。
    /// </summary>
    public class Gambling : MonoBehaviour
    {
        // ----------------------
        // 資料結構定義
        // ----------------------

        public enum Operator { Add, Subtract, Multiply }

        public struct ResultPair
        {
            public Operator op;
            public int number;
        }

        public struct IndexPair
        {
            public int numIndex;
            public int opIndex;
        }

        // ----------------------
        // 屬性與變數
        // ----------------------

        [Header("設定")]
        [SerializeField]
        private List<Operator> operatorSlots = new List<Operator>
        {
            Operator.Subtract,       // 第 0 格
            Operator.Multiply,  // 第 1 格
            Operator.Add,  // 第 2 格
            Operator.Subtract,       // 第 3 格
            Operator.Add,  // 第 4 格
            Operator.Multiply,  // 第 5 格
            Operator.Subtract,       // 第 6 格
            Operator.Add   // 第 7 格
        };

        // 數字轉盤 (假設有 8 格，順時針)
        private List<int> numberSlots = new List<int>
        {
            0,   // 第 0 格
            1,   // 第 1 格
            0,   // 第 2 格
            3,  // 第 3 格
            0,   // 第 4 格
            2,   // 第 5 格
            2,   // 第 6 格
            1   // 第 7 格
        };

        private IndexPair index = new IndexPair();

        [Header("UI 連結")]
        [SerializeField] private GamblingUI gamblingUI;

        // ----------------------
        // Unity 生命週期
        // ----------------------

        private void Awake()
        {
            // 系統啟動時，將自己註冊到 GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GamblingSystem = this;
            }
        }

        private void OnDestroy()
        {
            // 物件銷毀時，取消註冊
            if (GameManager.Instance != null && GameManager.Instance.GamblingSystem == this)
            {
                GameManager.Instance.GamblingSystem = null;
            }
        }

        // ----------------------
        // 主要功能方法
        // ----------------------

        /// <summary>
        /// [公開方法] 啟動賭博流程
        /// </summary>
        /// <param name="wager">玩家下注的金額</param>
        public IndexPair StartGambling(int wager)
        {
            // 1. 取得 Player 實例
            PlayerController player = GameManager.Instance.Player;
            Debug.Log(player.Coins);

            // 方法 B：備案 (如果 GameManager 死掉或不在場，自己去場景找)
            if (player == null)
            {
                // 強制在場景中搜尋掛有 PlayerController 的物件
                player = FindObjectOfType<PlayerController>();
            }

            index.numIndex = -1;
            index.opIndex = -1;

            // --- 步驟 2：最後檢查 ---
            if (player == null)
            {
                Debug.LogError("【嚴重錯誤】場景裡找不到任何 PlayerController！請確認你有把主角放進場景裡！");
                return index; // 真的沒救了，停止執行
            }

            index.numIndex = 9;
            index.opIndex = 9;

            // 2. 檢查賭注是否超過玩家持有的總金錢
            if (wager > player.Coins)
            {
                Debug.LogWarning($"賭注 ({wager}) 超過持有金錢 ({player.Coins})，無法進行賭博！");
                return index;
            }

            // 3. 先扣除玩家的賭注
            player.Coins -= wager;

            // 2. 產生結果
            ResultPair result = GenerateResult();

            if (gamblingUI != null)
            {
                gamblingUI.OpenAndSpin(result, index.opIndex, index.numIndex, () =>
                {
                    // 這是當 UI 按下 "確定" 後會回頭執行的程式碼
                    ComputeAndPayBack(result, wager);
                });
            }
            else
            {
                // 如果沒有接 UI (除錯用)，直接結算
                Debug.LogError("未綁定 GamblingUI，直接結算");
                ComputeAndPayBack(result, wager);
            }

            return index;
        }

        /// <summary>
        /// [內部方法] 產生隨機的運算符號與數字
        /// </summary>
        private ResultPair GenerateResult()
        {
            ResultPair result = new ResultPair();

            // 隨機決定運算符號 (0=Add, 1=Subtract, 2=Multiply)
            index.opIndex = Random.Range(0, operatorSlots.Count);
            Debug.Log(index.opIndex);
            result.op = operatorSlots[index.opIndex];

            // 隨機決定數字 (假設範圍 1~10)
            index.numIndex = Random.Range(0, numberSlots.Count);
            Debug.Log(index.numIndex);
            result.number = numberSlots[index.numIndex];

            return result;
        }

        /// <summary>
        /// [內部方法] 計算運算後的金額並還給玩家
        /// </summary>
        private void ComputeAndPayBack(ResultPair result, int wager)
        {
            // 初始價值為原本的賭注
            int finalValue = wager;

            // 根據隨機出的運算符號對「賭注」進行運算
            switch (result.op)
            {
                case Operator.Add:
                    finalValue += result.number;
                    break;
                case Operator.Subtract:
                    finalValue -= result.number;
                    break;
                case Operator.Multiply:
                    finalValue *= result.number;
                    break;
            }

            // 防止結算價值小於 0 (如果不想讓賭注變成負債)
            if (finalValue < 0) finalValue = 0;

            // 取得 Player 實例
            PlayerController player = GameManager.Instance.Player;

            // 將運算後的最終金額「加回」給玩家
            player.Coins += finalValue;

            // 計算淨賺/賠 (用於 Log 或音效判斷)
            int netProfit = finalValue - wager;
            Debug.Log($"[賭博結果] 投入: {wager} | 運算: {result.op} {result.number} | 取回: {finalValue} | 淨變動: {netProfit}");
        }
    }
}
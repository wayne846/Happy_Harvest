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

        // ----------------------
        // 屬性與變數
        // ----------------------

        [Header("設定")]
        [SerializeField]
        private List<int> nameWeight;

        [SerializeField]
        private List<int> symbolWeight;

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

        private void Start()
        {
            if (nameWeight == null) nameWeight = new List<int>();
            if (symbolWeight == null) symbolWeight = new List<int>();
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
        public void StartGambling(int wager)
        {
            // 1. 取得 Player 實例
            PlayerController player = GameManager.Instance.Player;

            if (player == null)
            {
                Debug.LogError("錯誤：找不到 Player 實例！");
                return;
            }

            // 2. 檢查賭注是否超過玩家持有的總金錢
            if (wager > player.Coins)
            {
                Debug.LogWarning($"賭注 ({wager}) 超過持有金錢 ({player.Coins})，無法進行賭博！");
                return;
            }

            // 3. 先扣除玩家的賭注
            player.Coins -= wager;

            // 4. 產生隨機的操作符(+-*)與數字
            ResultPair result = GenerateResult();

            // 5. 計算結果並將錢還給玩家
            ComputeAndPayBack(result, wager);
        }

        /// <summary>
        /// [內部方法] 產生隨機的運算符號與數字
        /// </summary>
        private ResultPair GenerateResult()
        {
            ResultPair result = new ResultPair();

            // 隨機決定運算符號 (0=Add, 1=Subtract, 2=Multiply)
            int randomOpIndex = Random.Range(0, 3);
            result.op = (Operator)randomOpIndex;

            // 隨機決定數字 (假設範圍 1~10)
            int randomNumber = Random.Range(1, 10);
            result.number = randomNumber;

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

            // 如果有賺錢 (取回的比投入的多)，播放音效
            if (netProfit > 0)
            {
                UIHandler.PlayBuySellSound(player.transform.position);
            }
        }
    }
}
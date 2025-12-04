using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utility;

using Random = UnityEngine.Random;

namespace HappyHarvest
{
    public class FishingSystem : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("This decides how much reel progress drops per second.")]
        private float ReelDropRate;
        [SerializeField]
        [Min(1)]
        private int FishMovementRate = 1;

        private List<Pond> pondList;
        
        private float remainTime;
        private float captureProgress;
        private float reelPosition;
        private float fishPosition;

        //Loop Cooldown
        private int FishMovementCooldown;

        private float FishMoveDistance;

        private List<WeightedFunction> movementWF;

        public event Action OpenFishingGameUI;
        public event Action<float, float, float, float> UpdateUIGameInfo;

        private Coroutine fishingGame;

        private void Awake()
        {
            GameManager.Instance.FishingSystem = this;
            IntializeChanceTable();
        }

        private void IntializeChanceTable()
        {
            movementWF = new List<WeightedFunction> {
                new WeightedFunction { weight = 0.25f, action = FishMoveUp },
                new WeightedFunction { weight = 0.25f, action = FishMoveDown },
                new WeightedFunction { weight = 0.5f,  action = RandomWeightedFunction.DoNothing}
                };
        }


        public void StartFishing()
        {
            if (fishingGame != null)
            {
                StopCoroutine(fishingGame);
                fishingGame = null;
            }
            fishingGame = StartCoroutine(FishingGame());
        }

        public void StopFishing()
        {
            if (fishingGame != null)
            {
                StopCoroutine(fishingGame);
                fishingGame = null;
            }

            GameManager.Instance.Player.ToggleFish(false);
        }

        public void ReelIn()
        {
            
            reelPosition += 5.0f;
            reelPosition = Mathf.Min(reelPosition, 100);
        }

        private IEnumerator FishingGame()
        {
            OpenFishingGameUI?.Invoke();
            GameManager.Instance.Player.ToggleFish(true);
            FishMoveDistance = 0f;
            fishPosition = 0f;
            reelPosition = 0f;
            remainTime = 100f;

            float originalFishPosition = fishPosition;

            FishMovementCooldown = 0;
            while (remainTime > 0)
            {
                reelPosition -= ReelDropRate * Time.deltaTime;
                reelPosition = Mathf.Max(reelPosition, 0);

                fishPosition = originalFishPosition + Mathf.Lerp(0, FishMoveDistance, ((float)FishMovementCooldown / FishMovementRate));

                fishPosition = Mathf.Clamp(fishPosition, 0f, 100f);

                if (FishMovementCooldown == FishMovementRate)
                {
                    FishMoveDistance = 0f;
                    originalFishPosition = fishPosition;

                    RandomWeightedFunction.Pick(movementWF).Invoke();
                    FishMovementCooldown = 0;
                }
                else
                {
                    FishMovementCooldown++;
                }

                

                remainTime -= Time.deltaTime;

                UpdateUIGameInfo?.Invoke(remainTime, captureProgress, reelPosition, fishPosition);
                yield return null;
            }

            fishingGame = null;
        }

        private void FishMoveUp()
        {
            FishMoveDistance = Random.Range(1f, 4f);
        }

        private void FishMoveDown()
        {
            FishMoveDistance = -Random.Range(-1f, 4f);
        }
    }
}

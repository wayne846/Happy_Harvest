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
        [SerializeField]
        [Tooltip("This decides how much capture progress gains per second.")]
        [Min(1)]
        private int CaptureRate = 1;

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
        public event Action CloseFishingGameUI;
        public event Action<float, float, float> UpdateUIGameInfo;
        public event Action ReelAnimation;

        //Info
        public bool UnlockState = false;

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
            captureProgress = 0f;

            float originalFishPosition = fishPosition;

            FishMovementCooldown = 0;
            while (captureProgress < 100f)
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

                if (Math.Abs(reelPosition - fishPosition) <= 5f)
                {
                    captureProgress = Mathf.Min(captureProgress + CaptureRate * Time.deltaTime, 100f);
                }



                remainTime -= Time.deltaTime;

                UpdateUIGameInfo?.Invoke(captureProgress, reelPosition, fishPosition);
                yield return null;
            }
               
            yield return CaptureFish();

            CloseFishingGameUI?.Invoke();
        }

        private IEnumerator CaptureFish()
        {
            ReelAnimation?.Invoke();

            yield return new WaitForSeconds(3);

            GameManager instance = GameManager.Instance;
            instance.Player.AddItem(instance.ItemDatabase.GetFromID("fih_calling"));
        }

        private void FishMoveUp()
        {
            FishMoveDistance = Random.Range(10f, 30f);
        }

        private void FishMoveDown()
        {
            FishMoveDistance = -Random.Range(10f, 30f);
        }
    }
}

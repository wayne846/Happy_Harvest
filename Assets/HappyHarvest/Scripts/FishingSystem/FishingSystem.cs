using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HappyHarvest
{
    public class FishingSystem : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("This decides how much reel progress drops per second.")]
        private float ReelDropRate;

        private List<Pond> pondList;
        private float reelPosition;
        private float fishPosition;
        private float remainTime;
        private float captureProgress;

        private Coroutine fishingGame;

        private void Awake()
        {
            GameManager.Instance.FishingSystem = this;
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
            UIHandler.OpenFishingGame();
            GameManager.Instance.Player.ToggleFish(true);
            reelPosition = 0f;
            remainTime = 100f;
            while (remainTime > 0)
            {
                reelPosition -= ReelDropRate * Time.deltaTime;
                reelPosition = Mathf.Max(reelPosition, 0);

                remainTime -= Time.deltaTime;

                UIHandler.UpdateFishingGameUI(reelPosition);
                yield return null;
            }

            fishingGame = null;
        }
    }
}

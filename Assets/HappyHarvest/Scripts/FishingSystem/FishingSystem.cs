using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HappyHarvest
{
    public class FishingSystem : MonoBehaviour
    {
        [SerializeField]
        private GameObject fishingGameGO;
        private FishingGameUI fishingGameUI;

        private List<Pond> pondList;
        private float fishPosition;
        private float remainTime;
        private float captureProgress;

        private Coroutine fishingGame;

        private void Awake()
        {
            GameManager.Instance.FishingSystem = this;

            fishingGameUI = fishingGameGO.GetComponent<FishingGameUI>();
            fishingGameUI.Init(this);

            fishingGameGO.SetActive(false);
        }


        public void StartFishing()
        {
            StopFishing();

            fishingGame = StartCoroutine(FishingGame());
        }

        public void StopFishing()
        {
            if (fishingGame != null)
            {
                StopCoroutine(fishingGame);
                fishingGame = null;
            }
        }

        private IEnumerator FishingGame()
        {
            fishingGameUI.Show();

            while (remainTime > 0)
            {


                remainTime -= Time.deltaTime;
                yield return null;
            }

            fishingGame = null;
        }
    }
}

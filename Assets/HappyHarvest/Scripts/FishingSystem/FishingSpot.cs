using System.Collections;
using UnityEngine;
using TMPro;
namespace HappyHarvest
{
    public class FishingSpot : InteractiveObject
    {
        [SerializeField]
        private GameObject pondHint;
        private Coroutine showHint;

        public override void InteractedWith()
        {
            if (GameManager.Instance.FishingSystem.UnlockState)
            {
                GameManager.Instance.FishingSystem.StartFishing();
            }
            else
            {
                if (showHint != null)
                {
                    StopCoroutine(showHint);
                    showHint = null;
                }
                showHint = StartCoroutine(ShowHint());
            }
        }
        IEnumerator ShowHint()
        {
            pondHint.SetActive(true);

            yield return new WaitForSeconds(2.0f);

            pondHint.SetActive(false);
        }
    }
}

using System.Collections;
using UnityEngine;
using TMPro;
namespace HappyHarvest
{
    public class FishingPond : InteractiveObject
    {
        [SerializeField]
        private GameObject pondHint;
        public override void InteractedWith()
        {
            StartCoroutine(ShowHint());
        }

        IEnumerator ShowHint()
        {
            pondHint.SetActive(true);

            yield return new WaitForSeconds(2.0f);

            pondHint.SetActive(false);
        }
    }

}

using UnityEngine;
using UnityEngine.UI;

namespace HappyHarvest {
    public class FishingGameUIController : MonoBehaviour
    {
        [SerializeField]
        private Button CloseButton;

        [Header("ReelBar")]
        [SerializeField]
        private GameObject ReelBar;
        [SerializeField]
        private float HeightPerUnit;
        [SerializeField]
        private float ReelDefaultPosition;

        public void Awake()
        {
            CloseButton.onClick.AddListener(Close);
        }

        public void UpdateReelBar(float position)
        {
            float target = ReelDefaultPosition + position * HeightPerUnit;
            ReelBar.transform.localPosition = new Vector3(0f, target, 0f);
        }

        public void Close()
        {
            GameManager.Instance.FishingSystem.StopFishing();
            UIHandler.CloseFishingGame();
        }
    }
}
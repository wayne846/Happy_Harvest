using UnityEngine;
using UnityEngine.UI;

namespace HappyHarvest {
    public class FishingGameUI : MonoBehaviour
    {
        [SerializeField]
        private Button CloseButton;
        [SerializeField]
        private GameObject ReelBar;

        private FishingSystem fishingSystem;

        public void Awake()
        {
            CloseButton.onClick.AddListener(Close);
        }

        public void Init(FishingSystem _fishingSystem)
        {
            fishingSystem = _fishingSystem;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            fishingSystem.StopFishing();
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HappyHarvest {
    public class FishingGameUIController : MonoBehaviour
    {
        private FishingGameUI fishingGameUI;

        [SerializeField]
        private Button CloseButton;
        [SerializeField]
        private TextMeshProUGUI Timer;

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

        public void Init(FishingGameUI _fishingGameUI)
        {
            fishingGameUI = _fishingGameUI;
        }

        public void UpdateReelBar(float position)
        {
            float target = ReelDefaultPosition + position * HeightPerUnit;
            ReelBar.transform.localPosition = new Vector3(0f, target, 0f);
        }

        public void UpdateTimer(float time)
        {
            int minute = (int)(time / 60f);
            int second = (int)time % 60;

            Timer.text = string.Format("{0:D2}:{1:D2}", minute, second);
        }

        public void Close()
        {
            fishingGameUI.Close();
        }
    }
}
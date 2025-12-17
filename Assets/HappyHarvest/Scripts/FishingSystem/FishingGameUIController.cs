using System.Collections;
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
        private Image Capture;

        [Header("Settings")]
        [SerializeField]
        private float HeightPerUnit;
        [SerializeField]
        private float DefaultPosition;

        [Header("ReelBar")]
        [SerializeField]
        private GameObject ReelBar;
        

        [Header("Fish")]
        [SerializeField]
        private GameObject Fish;
        [SerializeField]
        private GameObject FishIcon;


        public void Awake()
        {
            CloseButton.onClick.AddListener(Close);
            DontDestroyOnLoad(this);
        }

        public void Init(FishingGameUI _fishingGameUI)
        {
            fishingGameUI = _fishingGameUI;
        }

        public void Reset()
        {
            if (FishIcon == null) Debug.Log("error");
            FishIcon.transform.localPosition = new Vector3(0, 0, 1);
        }

        public void UpdateCaptureProgress(float capture)
        {
            float newHeight = capture * 7.9f;
            Capture.rectTransform.sizeDelta = new Vector2(20, newHeight);
        }

        public void UpdateReelBar(float position)
        {
            float target = DefaultPosition + position * HeightPerUnit;
            ReelBar.transform.localPosition = new Vector3(0f, target, 0f);
        }

        public void UpdateFish(float position)
        {
            float target = DefaultPosition + position * HeightPerUnit;
            Fish.transform.localPosition = new Vector3(0f, target, 0f);
        }

        public void RunReelAnimation()
        {
            StartCoroutine(ReelAnimation());
        }

        IEnumerator ReelAnimation()
        {
            float animationTime = 0f;

            while(animationTime <= 2.5f)
            {
                Vector3 newPosition = new Vector3();
                newPosition.y = Mathf.Lerp(0, -550f, animationTime / 2.5f);

                FishIcon.transform.localPosition = newPosition;

                animationTime += Time.deltaTime;
                yield return null;
            }
        }

        public void Close()
        {
            fishingGameUI.Close();
        }
    }
}
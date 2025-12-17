using System.Collections;
using System.Collections.Generic;
using Template2DCommon;
using UnityEngine;
using UnityEngine.UIElements;

namespace HappyHarvest
{
    /// <summary>
    /// Handle the FishingSpotUI that handle fishing.
    /// </summary>
    public class FishingGameUI
    {
        private FishingSystem fishingSystem;

        private GameObject m_Instance;

        private FishingGameUIController fishingGameUIController;

        public FishingGameUI(GameObject UI_Instance)
        {
            m_Instance = UI_Instance;

            fishingGameUIController = m_Instance.GetComponent<FishingGameUIController>();
            fishingGameUIController.Init(this);

            FishingSystem.InstanceCreation += Init;

            m_Instance.SetActive(false);
        }

        void Init(FishingSystem _fishingSystem)
        {
            fishingSystem = _fishingSystem;

            fishingSystem.OpenFishingGameUI += Open;
            fishingSystem.CloseFishingGameUI += Close;
            fishingSystem.UpdateUIGameInfo += UpdateUI;
            fishingSystem.ReelAnimation += ReelAnimation;
        }

        public void Open()
        {
            fishingGameUIController.Reset();
            m_Instance.SetActive(true);
        }

        public void Close()
        {
            m_Instance.SetActive(false);
            fishingSystem.StopFishing();
        }

        public void UpdateUI(float captureProgress, float reelPosition, float fishPosition)
        {
            fishingGameUIController.UpdateCaptureProgress(captureProgress);
            fishingGameUIController.UpdateReelBar(reelPosition);
            fishingGameUIController.UpdateFish(fishPosition);
        }

        public void ReelAnimation()
        {
            fishingGameUIController.RunReelAnimation();
        }

    }
}

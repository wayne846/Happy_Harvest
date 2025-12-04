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

        public FishingGameUI(GameObject UI_Instance, FishingSystem _fishingSystem)
        {
            m_Instance = UI_Instance;

            fishingGameUIController = m_Instance.GetComponent<FishingGameUIController>();
            fishingGameUIController.Init(this);

            fishingSystem = _fishingSystem;

            fishingSystem.OpenFishingGameUI += Open;
            fishingSystem.UpdateUIGameInfo += UpdateUI;

            m_Instance.SetActive(false);
        }

        public void Open()
        {
            m_Instance.SetActive(true);
        }

        public void Close()
        {
            m_Instance.SetActive(false);
            fishingSystem.StopFishing();
        }

        public void UpdateUI(float remaingTime, float captureProgress, float reelPosition, float fishPosition)
        {
            fishingGameUIController.UpdateReelBar(reelPosition);
            fishingGameUIController.UpdateTimer(remaingTime);
            fishingGameUIController.UpdateFish(fishPosition);
        }
    }
}

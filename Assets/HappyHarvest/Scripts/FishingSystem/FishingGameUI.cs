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

        private GameObject m_Instance;

        private FishingGameUIController fishingGameUIController;

        public FishingGameUI(GameObject UI_Instance)
        {
            m_Instance = UI_Instance;

            fishingGameUIController = m_Instance.GetComponent<FishingGameUIController>();

            Close();
        }

        public void Open()
        {
            m_Instance.SetActive(true);
        }

        public void Close()
        {
            m_Instance.SetActive(false);
        }

        public void UpdateUI(float reelPosition)
        {
            fishingGameUIController.UpdateReelBar(reelPosition);
        }
    }
}

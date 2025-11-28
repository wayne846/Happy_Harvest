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
    public class FishingSpotUI
    {

        private VisualElement m_Root;

        private Button m_StartFishing;
        private Button m_FihCalling;

        private ScrollView m_Scrollview;

        public FishingSpotUI(VisualElement root)
        {
            m_Root = root;

            m_StartFishing = m_Root.Q<Button>("StartFishing");
            m_StartFishing.clicked += StartFishing;

            m_FihCalling = m_Root.Q<Button>("FihCalling");
            m_FihCalling.clicked += GetFihCalling;

            m_Root.Q<Button>("CloseButton").clicked += Close;

            //m_Scrollview = m_Root.Q<ScrollView>("ContentScrollView");
        }

        public void Open()
        {
            m_Root.visible = true;
            GameManager.Instance.Pause();
            SoundManager.Instance.PlayUISound();
        }

        public void Close()
        {
            m_Root.visible = false;
            GameManager.Instance.Resume();
        }

        void GetFihCalling()
        {
            GameManager instance = GameManager.Instance;
            instance.Player.AddItem(instance.ItemDatabase.GetFromID("fih_calling"));
        }

        void StartFishing()
        {
            Close();
            GameManager.Instance.FishingSystem.StartFishing();  
        }
    }
}

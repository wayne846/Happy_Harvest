using System;
using System.Collections.Generic;
using Template2DCommon;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;


namespace HappyHarvest
{
    /// <summary>
    /// Handle everything related to the main gameplay UI. Will retrieve all the UI Element and contains various static
    /// functions that updates/change the UI so they can be called from any other class interacting with the UI.
    /// </summary>
    public class UIHandler : MonoBehaviour
    {
        protected static UIHandler s_Instance;

        public enum CursorType
        {
            Normal,
            Interact,
            System
        }
        
        [Header("Cursor")]
        public Texture2D NormalCursor;
        public Texture2D InteractCursor;

        [Header("UI Document")]
        public VisualTreeAsset MarketEntryTemplate;
        
        [Header("Sounds")] 
        public AudioClip MarketSellSound;

        [Header("UI Prefab")]
        public GameObject FishingGameUIPrefab;

        protected UIDocument m_Document;
        
        // Inventory System
        protected List<VisualElement> m_InventorySlots;     // �ֱ����x�s��
        protected List<Label> m_ItemCountLabels;            // �ֱ��檫�~�ƶq
        protected List<VisualElement> m_FullInventorySlots; // �j�I�]�x�s��
        protected List<Label> m_FullItemCountLabels;        // �j�I�]���~�ƶq
        protected VisualElement m_InventoryPopup; // �j�I�]����
        public static bool IsInventoryOpen => s_Instance.m_InventoryPopup.style.display == DisplayStyle.Flex;

        VisualElement m_GhostIcon;      // ���H�ƹ����ʪ��z���ϥ�
        bool m_IsDragging;              // �O�_���b�즲��
        int m_DragSourceIndex = -1;     // �ӷ���l�� Index
        int m_HoveredSlotIndex = -1;    // �ثe�ƹ����쪺��lIndex

        protected Label m_CointCounter;

        protected VisualElement m_MarketPopup;
        protected VisualElement m_MarketContentScrollview;

        protected Label m_TimerLabel;

        protected Button m_BuyButton;
        protected Button m_SellButton;

        protected bool m_HaveFocus = true;
        protected CursorType m_CurrentCursorType;

        protected SettingMenu m_SettingMenu;
        protected WarehouseUI m_WarehouseUI;
        protected FishingSpotUI m_FishingSpotUI;
        protected FishingGameUI m_FishingGameUI;

        // Fade to balck helper
        protected VisualElement m_Blocker;
        protected System.Action m_FadeFinishClbk;
        
        private Label m_SunLabel;
        private Label m_RainLabel;
        private Label m_ThunderLabel;

        

        void Awake()
        {
            s_Instance = this;

            m_Document = GetComponent<UIDocument>();

            m_InventoryPopup = m_Document.rootVisualElement.Q<VisualElement>("InventoryPopup");
            m_InventoryPopup.style.display = DisplayStyle.None;

            m_InventorySlots = m_Document.rootVisualElement.Q<VisualElement>("Inventory").Query<VisualElement>("InventoryEntry").ToList();
            m_ItemCountLabels = m_Document.rootVisualElement.Q<VisualElement>("Inventory").Query<Label>("ItemCount").ToList();
            m_FullInventorySlots = m_Document.rootVisualElement.Q<VisualElement>("InventoryPopup").Query<VisualElement>("InventoryEntry").ToList();
            m_FullItemCountLabels = m_Document.rootVisualElement.Q<VisualElement>("InventoryPopup").Query<Label>("ItemCount").ToList();

            for (int i = 0; i < m_InventorySlots.Count; ++i)
            {
                var i1 = i;
                m_InventorySlots[i].AddManipulator(new Clickable(() =>
                {
                    GameManager.Instance.Player.ChangeEquipItem(i1);
                }));
            }

            Debug.Assert(m_InventorySlots.Count == InventorySystem.HotBarSize,
                "Not enough items slots in the UI for inventory");

            Debug.Assert(m_FullInventorySlots.Count == InventorySystem.InventorySize,
                "Not enough items slots in the UI for full inventory");

            m_GhostIcon = new VisualElement();
            m_GhostIcon.style.position = Position.Absolute;
            m_GhostIcon.style.width = 80;  // �]�w�ϥܤj�p�A��ĳ�P��l�j�p�@�P
            m_GhostIcon.style.height = 80;
            m_GhostIcon.style.visibility = Visibility.Hidden;
            m_GhostIcon.pickingMode = PickingMode.Ignore; // ����I���ƹ��g�u��z���A�o�ˤ~�఻���쩳�U����l
            m_Document.rootVisualElement.Add(m_GhostIcon);

            // ���U�b root �W�H�T�O�즲���l�~�]�఻�����ʩΩ�}
            m_Document.rootVisualElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            m_Document.rootVisualElement.RegisterCallback<PointerUpEvent>(OnPointerUp);

            // --- �s�W�G���C�Ӥj�I�]��l���U�ƥ� ---
            for (int i = 0; i < m_FullInventorySlots.Count; ++i)
            {
                int index = i; // Closure capture
                var slot = m_FullInventorySlots[i];

                // 1. �ƹ��i�J�G���� Index
                slot.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    m_HoveredSlotIndex = index;
                });

                // 2. �ƹ����}�G�M�� Index
                slot.RegisterCallback<PointerLeaveEvent>(evt =>
                {
                    // �u�������}���u���O�ثe��������l�~�M�� (�קK�ֳt���ʮɪ� race condition)
                    if (m_HoveredSlotIndex == index)
                        m_HoveredSlotIndex = -1;
                });

                // 3. �ƹ��I���G�}�l�즲
                slot.RegisterCallback<PointerDownEvent>(evt => OnSlotDown(evt, index));
            }

            m_CointCounter = m_Document.rootVisualElement.Q<Label>("CoinAmount");

            m_MarketPopup = m_Document.rootVisualElement.Q<VisualElement>("MarketPopup");
            m_MarketPopup.Q<Button>("CloseButton").clicked += CloseMarket;
            m_MarketPopup.visible = false;

            m_BuyButton = m_MarketPopup.Q<Button>("BuyButton");
            m_BuyButton.clicked += ToggleToBuy;
            m_SellButton = m_MarketPopup.Q<Button>("SellButton");
            m_SellButton.clicked += ToggleToSell;

            m_MarketContentScrollview = m_MarketPopup.Q<ScrollView>("ContentScrollView");

            m_TimerLabel = m_Document.rootVisualElement.Q<Label>("Timer");

            m_SettingMenu = new SettingMenu(m_Document.rootVisualElement);
            m_SettingMenu.OnOpen += () => { GameManager.Instance.Pause(); };
            m_SettingMenu.OnClose += () => { GameManager.Instance.Resume(); };

            m_WarehouseUI = new WarehouseUI(m_Document.rootVisualElement.Q<VisualElement>("WarehousePopup"), MarketEntryTemplate);

            m_FishingSpotUI = new FishingSpotUI(m_Document.rootVisualElement.Q<VisualElement>("FishingSpotPopup"));

            m_FishingGameUI = new FishingGameUI(Instantiate(FishingGameUIPrefab));

            m_Blocker = m_Document.rootVisualElement.Q<VisualElement>("Blocker");
            
            m_Blocker.style.opacity = 1.0f;
            m_Blocker.schedule.Execute(() => { FadeFromBlack(() => { }); }).ExecuteLater(500);

            m_Blocker.RegisterCallback<TransitionEndEvent>(evt =>
            {
                m_FadeFinishClbk?.Invoke();
            });

            m_SunLabel = m_Document.rootVisualElement.Q<Label>("SunLabel");
            m_RainLabel = m_Document.rootVisualElement.Q<Label>("RainLabel");
            m_ThunderLabel = m_Document.rootVisualElement.Q<Label>("ThunderLabel");
            
            m_SunLabel.AddManipulator(new Clickable(() => { GameManager.Instance.WeatherSystem?.ChangeWeather(WeatherSystem.WeatherType.Sun); }));
            m_RainLabel.AddManipulator(new Clickable(() => { GameManager.Instance.WeatherSystem?.ChangeWeather(WeatherSystem.WeatherType.Rain); }));
            m_ThunderLabel.AddManipulator(new Clickable(() => { GameManager.Instance.WeatherSystem?.ChangeWeather(WeatherSystem.WeatherType.Thunder); }));

            
        }
        
        
        void Update()
        {
            m_TimerLabel.text = GameManager.Instance.CurrentTimeAsString();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            m_HaveFocus = hasFocus;
            if(!hasFocus)
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            else
                ChangeCursor(m_CurrentCursorType);
        }

        //Need to be called by the player everytime the inventory change.
        public static void UpdateInventory(InventorySystem system)
        {
            s_Instance.UpdateInventory_Internal(system);
            s_Instance.UpdateFullInventory_Internal(system);
        }

        public static void UpdateCoins(int amount)
        {
            s_Instance.UpdateCoins_Internal(amount);
        }

        public static void OpenMarket()
        {
           s_Instance.OpenMarket_Internal();
           GameManager.Instance.Pause();
        }

        public static void CloseMarket()
        {
            SoundManager.Instance.PlayUISound();
            s_Instance.m_MarketPopup.visible = false;
            GameManager.Instance.Resume();
        }

        public static void OpenWarehouse()
        {
            s_Instance.m_WarehouseUI.Open();
        }

        public static void ChangeCursor(CursorType cursorType)
        {
            if (s_Instance.m_HaveFocus)
            {
                switch (cursorType)
                {
                    case CursorType.Interact:
                        Cursor.SetCursor(s_Instance.InteractCursor, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorType.Normal:
                        Cursor.SetCursor(s_Instance.NormalCursor, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorType.System:
                        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                        break;
                }
            }

            s_Instance.m_CurrentCursorType = cursorType;
        }

        public static void UpdateWeatherIcons(WeatherSystem.WeatherType currentWeather)
        {
            s_Instance.m_SunLabel.EnableInClassList("on-button", currentWeather == WeatherSystem.WeatherType.Sun);
            s_Instance.m_RainLabel.EnableInClassList("on-button", currentWeather == WeatherSystem.WeatherType.Rain);
            s_Instance.m_ThunderLabel.EnableInClassList("on-button", currentWeather == WeatherSystem.WeatherType.Thunder);
            
            s_Instance.m_SunLabel.EnableInClassList("off-button", currentWeather != WeatherSystem.WeatherType.Sun);
            s_Instance.m_RainLabel.EnableInClassList("off-button", currentWeather != WeatherSystem.WeatherType.Rain);
            s_Instance.m_ThunderLabel.EnableInClassList("off-button", currentWeather != WeatherSystem.WeatherType.Thunder);
        }

        public static void SceneLoaded()
        {
            //we hide the weather control if there is no weather sytsem in that scene
            s_Instance.m_SunLabel.parent.style.display =
                GameManager.Instance.WeatherSystem == null ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OpenMarket_Internal()
        {
            m_MarketPopup.visible = true;
            
            //we open the Sell Tab by default
            ToggleToSell();

            GameManager.Instance.Player.ToggleControl(false);
        }

        private void ToggleToSell()
        {
            m_SellButton.AddToClassList("activeButton");
            m_BuyButton.RemoveFromClassList("activeButton");

            m_SellButton.SetEnabled(false);
            m_BuyButton.SetEnabled(true);
            
            //clear all the existing entry. A good target for optimization if profiling show bad perf in UI is to pool
            //instead of delete/recreate entries
            m_MarketContentScrollview.contentContainer.Clear();

            for (int i = 0; i < GameManager.Instance.Player.Inventory.Entries.Length; ++i)
            {
                var item = GameManager.Instance.Player.Inventory.Entries[i].Item;
                if (item == null)
                    continue;

                var clone = MarketEntryTemplate.CloneTree();

                clone.Q<Label>("ItemName").text = item.DisplayName;
                clone.Q<VisualElement>("ItemIcone").style.backgroundImage = new StyleBackground(item.ItemSprite);

                var button = clone.Q<Button>("ActionButton");

                if (item is Product product)
                {
                    int count = GameManager.Instance.Player.Inventory.Entries[i].StackSize;
                    button.text = $"Sell {count} for {product.SellPrice * count}";
                    
                    int i1 = i;
                    button.clicked += () =>
                    {
                        GameManager.Instance.Player.SellItem(i1, count);
                        //we remove this entry, we just sold it.
                        m_MarketContentScrollview.contentContainer.Remove(clone.contentContainer);
                    };
                }
                else
                {
                    button.SetEnabled(false);
                    button.text = "Cannot Sell";
                }
                
                m_MarketContentScrollview.Add(clone.contentContainer);
            }
        }
        
        private void ToggleToBuy()
        {
            m_SellButton.RemoveFromClassList("activeButton");
            m_BuyButton.AddToClassList("activeButton");
            
            m_BuyButton.SetEnabled(false);
            m_SellButton.SetEnabled(true);
            
            //clear all the existing entry. A good target for optimization if profiling show bad perf in UI is to pool
            //instead of delete/recreate entries
            m_MarketContentScrollview.contentContainer.Clear();

            for (int i = 0; i < GameManager.Instance.MarketEntries.Length; ++i)
            {
                var item = GameManager.Instance.MarketEntries[i];

                var clone = MarketEntryTemplate.CloneTree();

                clone.Q<Label>("ItemName").text = item.DisplayName;
                clone.Q<VisualElement>("ItemIcone").style.backgroundImage = new StyleBackground(item.ItemSprite);
                
                var button = clone.Q<Button>("ActionButton");

                if (GameManager.Instance.Player.Coins >= item.BuyPrice)
                {
                    button.text = $"Buy 1 for {item.BuyPrice}";
                    int i1 = i;
                    button.clicked += () =>
                    {
                        if (GameManager.Instance.Player.BuyItem(item))
                        {
                            if (GameManager.Instance.Player.Coins < item.BuyPrice)
                            {
                                button.text = $"Cannot afford cost of {item.BuyPrice}";
                                button.SetEnabled(false);
                            }
                        }
                    };
                    button.SetEnabled(true);
                }
                else
                {
                    button.text = $"Cannot afford cost of {item.BuyPrice}";
                    button.SetEnabled(false);
                }
                
                m_MarketContentScrollview.Add(clone.contentContainer);
            }
        }

        public static void PlayBuySellSound(Vector3 location)
        {
            SoundManager.Instance.PlaySFXAt(location, s_Instance.MarketSellSound, false);
        }

        public static void FadeToBlack(System.Action onFinished)
        {
            s_Instance.m_FadeFinishClbk = onFinished;

            s_Instance.m_Blocker.schedule.Execute(() =>
            {
                s_Instance.m_Blocker.style.opacity = 1.0f;
            }).ExecuteLater(10);
        }

        public static void FadeFromBlack(System.Action onFinished)
        {
            s_Instance.m_FadeFinishClbk = onFinished;
            
            s_Instance.m_Blocker.schedule.Execute(() =>
            {
                s_Instance.m_Blocker.style.opacity = 0.0f;
            }).ExecuteLater(10);
        }

        private void UpdateCoins_Internal(int amount)
        {
            m_CointCounter.text = amount.ToString();
        }

        private void UpdateInventory_Internal(InventorySystem system)
        {
            for (int i = 0; i < system.Entries.Length; ++i)
            {
                if (i >= InventorySystem.HotBarSize) break;

                var item = system.Entries[i].Item;
                m_InventorySlots[i][0].style.backgroundImage =
                    item == null ? new StyleBackground((Sprite)null) : new StyleBackground(item.ItemSprite);

                if (item == null || system.Entries[i].StackSize < 2)
                {
                    m_ItemCountLabels[i].style.visibility = Visibility.Hidden;
                }
                else
                {
                    m_ItemCountLabels[i].style.visibility = Visibility.Visible;
                    m_ItemCountLabels[i].text = system.Entries[i].StackSize.ToString();
                }


                if (system.EquippedItemIdx == i)
                {
                    m_InventorySlots[i].AddToClassList("equipped");
                }
                else
                {
                    m_InventorySlots[i].RemoveFromClassList("equipped");
                }
            }
        }

        public static void OpenFishingSpot()
        {
            s_Instance.m_FishingSpotUI.Open();
        }

        public static void UpdateFishingGameUI(float reelPosition)
        {
            s_Instance.m_FishingGameUI.UpdateUI(reelPosition);
        }
        public static void OpenFishingGame()
        {
            s_Instance.m_FishingGameUI.Open();
        }
        public static void CloseFishingGame()
        {
            s_Instance.m_FishingGameUI.Close();
        public static void OpenInventory(InventorySystem system)
        {
            s_Instance.OpenInventory_Internal(system);
        }

        public static void CloseInventory()
        {
            s_Instance.CloseInventory_Internal();
        }

        void OpenInventory_Internal(InventorySystem system)
        {
            if (m_InventoryPopup.style.display == DisplayStyle.Flex) return;

            m_InventoryPopup.style.display = DisplayStyle.Flex;
            GameManager.Instance.Pause(); // �Ȱ��C��
            SoundManager.Instance.PlayUISound();

            // ���sø�s��Ӥj�I�]
            UpdateFullInventory_Internal(system);
        }

        void CloseInventory_Internal()
        {
            m_InventoryPopup.style.display = DisplayStyle.None;
            GameManager.Instance.Resume(); // ��_�C��
        }

        void UpdateFullInventory_Internal(InventorySystem system)
        {
            for (int i = 0; i < system.Entries.Length; ++i)
            {
                var item = system.Entries[i].Item;
                m_FullInventorySlots[i][0].style.backgroundImage =
                    item == null ? new StyleBackground((Sprite)null) : new StyleBackground(item.ItemSprite);

                if (item == null || system.Entries[i].StackSize < 2)
                {
                    m_FullItemCountLabels[i].style.visibility = Visibility.Hidden;
                }
                else
                {
                    m_FullItemCountLabels[i].style.visibility = Visibility.Visible;
                    m_FullItemCountLabels[i].text = system.Entries[i].StackSize.ToString();
                }
            }
        }

        // ���b��l���U�ƹ� (�}�l�즲)
        private void OnSlotDown(PointerDownEvent evt, int index)
        {
            // �T�O�O����A�B�Ӯ�l�����~
            var inventory = GameManager.Instance.Player.Inventory;
            if (evt.button != 0 || inventory.Entries[index].Item == null) return;

            m_IsDragging = true;
            m_DragSourceIndex = index;

            // �]�w Ghost Icon ���Ϯ�
            m_GhostIcon.style.backgroundImage = new StyleBackground(inventory.Entries[index].Item.ItemSprite);
            m_GhostIcon.style.visibility = Visibility.Visible;

            // �]�w��l��m (�N�ƹ��y���ഫ�� UI �y��)
            UpdateGhostPosition(evt.position);

            // ���� Pointer�A�o�˧Y�Ϸƹ����X�����APointerUp �]��Q������ (����d��)
            m_Document.rootVisualElement.CapturePointer(evt.pointerId);
        }

        // ���ƹ����� (���� Ghost Icon)
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!m_IsDragging) return;
            UpdateGhostPosition(evt.position);
        }

        // ���ƹ���} (�����즲�å洫)
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!m_IsDragging) return;

            // ���� Pointer
            m_Document.rootVisualElement.ReleasePointer(evt.pointerId);

            // ����洫�޿�
            // ����G�����즲�즳�Į�l�A�B���O�즲��ۤv���W
            if (m_HoveredSlotIndex != -1 && m_HoveredSlotIndex != m_DragSourceIndex)
            {
                GameManager.Instance.Player.Inventory.SwapItem(m_DragSourceIndex, m_HoveredSlotIndex);

                // �洫������A���s��z UI (UpdateInventory �|�I�s UpdateFullInventory_Internal)
                // SwapItems �����w�g�I�s�F UpdateInventory�A�ҥH�o�̤��ݭn���ƩI�s
            }

            // ���m���A
            m_IsDragging = false;
            m_DragSourceIndex = -1;
            m_GhostIcon.style.visibility = Visibility.Hidden;
            m_GhostIcon.style.backgroundImage = null;
        }

        // ���U�禡�G��s Ghost Icon ��m
        private void UpdateGhostPosition(Vector2 screenPosition)
        {
            // �`�N�GUI Toolkit ���y�Шt���I�b���W��
            // �ڭ����ϥܤ����I����ƹ�
            float halfWidth = m_GhostIcon.layout.width / 2;
            float halfHeight = m_GhostIcon.layout.height / 2;

            m_GhostIcon.style.left = screenPosition.x - halfWidth;
            m_GhostIcon.style.top = screenPosition.y - halfHeight;
        }
    }
}
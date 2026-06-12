using UnityEngine;
using UnityEngine.UIElements;
using KBCore.Refs;
using Cursor = UnityEngine.Cursor;

namespace Platformer
{
    public enum MenuTab { Map, QuestLog, Character, Inventory, Settings }

    /// <summary>
    /// Owns the menu hub shell: toggle via pause button, cycle tabs with LB/RB,
    /// pause behavior (timescale, cursor, disable player input + movement).
    /// TabView handles the visual tab switching itself.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MenuHubUIController : ValidatedMonoBehaviour
    {
        [Header("References")]
        [SerializeField] UIDocument document;
        [SerializeField, Anywhere] InputReader input;
        [SerializeField, Anywhere] PlayerMovement playerMovement;

        [Header("Default")]
        [SerializeField] MenuTab defaultTab = MenuTab.Character;

        // LB/RB cycle through this order. Must match the Tab order in the UXML.
        static readonly MenuTab[] TabOrder =
        {
            MenuTab.Map, MenuTab.QuestLog, MenuTab.Character, MenuTab.Inventory, MenuTab.Settings
        };

        VisualElement panel;
        TabView tabView;

        MenuTab activeTab;
        bool isOpen;

        void Reset() => document = GetComponent<UIDocument>();

        void OnEnable()
        {
            var root = document.rootVisualElement;
            panel = root.Q<VisualElement>("menu-hub");
            tabView = root.Q<TabView>("main-tabs");

            SubscribeInput();
            SetVisible(false);
            SetActiveTab(defaultTab);
        }

        void OnDisable()
        {
            UnsubscribeInput();
            if (isOpen) Close();
        }

        // ---- input ----

        void SubscribeInput()
        {
            if (input == null) return;
            input.Paused      += Toggle;
            input.PreviousTab += OnPreviousTab;
            input.NextTab     += OnNextTab;
        }

        void UnsubscribeInput()
        {
            if (input == null) return;
            input.Paused      -= Toggle;
            input.PreviousTab -= OnPreviousTab;
            input.NextTab     -= OnNextTab;
        }

        void OnPreviousTab()
        {
            if (!isOpen) return;
            SetActiveTab(CycleTab(-1));
        }

        void OnNextTab()
        {
            if (!isOpen) return;
            SetActiveTab(CycleTab(+1));
        }

        MenuTab CycleTab(int delta)
        {
            int idx = System.Array.IndexOf(TabOrder, activeTab);
            int next = (idx + delta + TabOrder.Length) % TabOrder.Length;
            return TabOrder[next];
        }

        // ---- open / close ----

        void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        void Open()
        {
            if (isOpen) return;
            isOpen = true;
            SetVisible(true);

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (input != null) input.DisablePlayerMap();
            if (playerMovement != null) playerMovement.enabled = false;
        }

        void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            SetVisible(false);

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (input != null) input.EnablePlayerMap();
            if (playerMovement != null) playerMovement.enabled = true;
        }

        void SetVisible(bool show) => panel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

        // ---- tabs ----

        void SetActiveTab(MenuTab tab)
        {
            activeTab = tab;
            if (tabView == null) return;
            int idx = System.Array.IndexOf(TabOrder, tab);
            if (idx >= 0) tabView.selectedTabIndex = idx;
        }
    }
}

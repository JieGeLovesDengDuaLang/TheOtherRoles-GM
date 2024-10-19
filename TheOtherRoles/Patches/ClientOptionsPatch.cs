using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.Button;

namespace TheOtherRoles.Patches 
{
    [HarmonyPatch]
    public static class ClientOptionsPatch
    {
        private static SelectionBehaviour[] AllOptions = {
            //new SelectionBehaviour("streamerModeButton", () => TheOtherRolesPlugin.StreamerMode.Value = !TheOtherRolesPlugin.StreamerMode.Value, TheOtherRolesPlugin.StreamerMode.Value),
            new SelectionBehaviour("ghostsSeeTasksButton", () => MapOptions.ghostsSeeTasks = TheOtherRolesPlugin.GhostsSeeTasks.Value = !TheOtherRolesPlugin.GhostsSeeTasks.Value, TheOtherRolesPlugin.GhostsSeeTasks.Value),
            new SelectionBehaviour("ghostsSeeVotesButton", () => MapOptions.ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value = !TheOtherRolesPlugin.GhostsSeeVotes.Value, TheOtherRolesPlugin.GhostsSeeVotes.Value),
            new SelectionBehaviour("ghostsSeeRolesButton", () => MapOptions.ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value = !TheOtherRolesPlugin.GhostsSeeRoles.Value, TheOtherRolesPlugin.GhostsSeeRoles.Value),
            new SelectionBehaviour("showRoleSummaryButton", () => MapOptions.showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value = !TheOtherRolesPlugin.ShowRoleSummary.Value, TheOtherRolesPlugin.ShowRoleSummary.Value),
            new SelectionBehaviour("hideNameplates", () => {
                MapOptions.hideNameplates = TheOtherRolesPlugin.HideNameplates.Value = !TheOtherRolesPlugin.HideNameplates.Value;
                MeetingHudPatch.nameplatesChanged = true;
                return MapOptions.hideNameplates;
            }, TheOtherRolesPlugin.HideNameplates.Value),
            new SelectionBehaviour("showLighterDarker", () => MapOptions.showLighterDarker = TheOtherRolesPlugin.ShowLighterDarker.Value = !TheOtherRolesPlugin.ShowLighterDarker.Value, TheOtherRolesPlugin.ShowLighterDarker.Value),
            new SelectionBehaviour("hideTaskArrows", () => MapOptions.hideTaskArrows = TheOtherRolesPlugin.HideTaskArrows.Value = !TheOtherRolesPlugin.HideTaskArrows.Value, TheOtherRolesPlugin.HideTaskArrows.Value),
        };
        
        private static GameObject popUp;
        private static TextMeshPro titleText;

        private static ToggleButtonBehaviour moreOptions;
        private static List<ToggleButtonBehaviour> modButtons;
        private static TextMeshPro titleTextTitle;

        private static ToggleButtonBehaviour buttonPrefab;
        private static Vector3? _origin;
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static void MainMenuManager_StartPostfix(MainMenuManager __instance)
        {
            // Prefab for the title
            titleText = Object.Instantiate(__instance.quitButton.GetComponentInChildren<TextMeshPro>());
            Object.DontDestroyOnLoad(titleText);
            titleText.gameObject.SetActive(false);
            titleText.GetComponent<TextTranslatorTMP>().Destroy();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
        public static void OptionsMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance)
        {
            if (!__instance.CensorChatButton) return;

            if (!popUp)
            {
                CreateCustom(__instance);
            }

            if (!buttonPrefab)
            {
                buttonPrefab = Object.Instantiate(__instance.CensorChatButton);
                Object.DontDestroyOnLoad(buttonPrefab);
                buttonPrefab.name = "CensorChatPrefab";
                buttonPrefab.gameObject.SetActive(false);
            }
            
            SetUpOptions();
            InitializeMoreButton(__instance);
        }

        private static void CreateCustom(OptionsMenuBehaviour prefab)
        {
            popUp = Object.Instantiate(prefab.gameObject);
            Object.DontDestroyOnLoad(popUp);
            var transform = popUp.transform;
            var pos = transform.localPosition;
            pos.z = -810f; 
            transform.localPosition = pos;

            Object.Destroy(popUp.GetComponent<OptionsMenuBehaviour>());
            foreach (var gObj in popUp.gameObject.GetAllChilds())
            {
                if (gObj.name != "Background" && gObj.name != "CloseButton")
                    Object.Destroy(gObj);
            }
            
            popUp.SetActive(false);
        }

        private static void InitializeMoreButton(OptionsMenuBehaviour __instance)
        {
            moreOptions = Object.Instantiate(buttonPrefab, __instance.CensorChatButton.transform.parent);
            var transform = __instance.CensorChatButton.transform;
            _origin ??= transform.localPosition;
            transform.localScale = new Vector3(0.66f, 1, 1);
            transform.localPosition = _origin.Value + Vector3.left * 0.45f;

            __instance.EnableFriendInvitesButton.transform.localScale = new Vector3(0.66f, 1, 1);
            __instance.EnableFriendInvitesButton.transform.localPosition += Vector3.right * 0.5f;
            __instance.EnableFriendInvitesButton.Text.transform.localScale = new Vector3(1.2f, 1, 1);
            
            moreOptions.transform.localPosition = _origin.Value + Vector3.right * 1.3f;
            moreOptions.transform.localScale = new Vector3(0.66f, 1, 1);

            moreOptions.gameObject.SetActive(true);
            moreOptions.Text.text = ModTranslation.GetString("modOptionsText");
            var moreOptionsButton = moreOptions.GetComponent<PassiveButton>();
            moreOptionsButton.OnClick = new ButtonClickedEvent();
            moreOptionsButton.OnClick.AddListener((Action) (() =>
            {
                if (!popUp) return;

                if (__instance.transform.parent && __instance.transform.parent == HudManager.Instance.transform)
                {
                    popUp.transform.SetParent(HudManager.Instance.transform);
                    popUp.transform.localPosition = new Vector3(0, 0, -800f);
                }
                else
                {
                    popUp.transform.SetParent(null);
                    Object.DontDestroyOnLoad(popUp);
                }

                __instance.Close();
                CheckSetTitle();
                RefreshOpen();
            }));
        }

        private static void RefreshOpen()
        {
            popUp.gameObject.SetActive(false);
            popUp.gameObject.SetActive(true);
            SetUpOptions();
        }
        
        private static void CheckSetTitle()
        {
            if (!popUp || popUp.GetComponentInChildren<TextMeshPro>() || !titleText) return;
            
            var title = titleTextTitle = Object.Instantiate(titleText, popUp.transform);
            title.GetComponent<AspectPosition>().Destroy();
            title.GetComponent<RectTransform>().localPosition = new(0, 2, -2);
            title.gameObject.SetActive(true);
            title.text = ModTranslation.GetString("moreOptionsText");
            title.name = "TitleText";
        }

        private static void SetUpOptions()
        {
            if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;

            modButtons = new List<ToggleButtonBehaviour>();

            for (var i = 0; i < AllOptions.Length; i++)
            {
                var info = AllOptions[i];
                
                var button = Object.Instantiate(buttonPrefab, popUp.transform);
                var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

                var transform = button.transform;
                transform.localPosition = pos;

                button.onState = info.DefaultValue;
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
                
                button.Text.text = ModTranslation.GetString(info.Title);
                button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
                button.Text.font = Object.Instantiate(titleText.font);
                button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

                button.name = info.Title.Replace(" ", "") + "Toggle";
                button.gameObject.SetActive(true);
                
                var passiveButton = button.GetComponent<PassiveButton>();
                var colliderButton = button.GetComponent<BoxCollider2D>();
                
                colliderButton.size = new Vector2(2.2f, .7f);
                
                passiveButton.OnClick = new ButtonClickedEvent();
                passiveButton.OnMouseOut = new UnityEvent();
                passiveButton.OnMouseOver = new UnityEvent();

                passiveButton.OnClick.AddListener((Action) (() =>
                {
                    button.onState = info.OnClick();
                    button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
                }));
                
                passiveButton.OnMouseOver.AddListener((Action) (() => button.Background.color = new Color32(34 ,139, 34, byte.MaxValue)));
                passiveButton.OnMouseOut.AddListener((Action) (() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

                foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
                    spr.size = new Vector2(2.2f, .7f);

                modButtons.Add(button);
            }
        }
        
        private static IEnumerable<GameObject> GetAllChilds(this GameObject Go)
        {
            for (var i = 0; i< Go.transform.childCount; i++)
            {
                yield return Go.transform.GetChild(i).gameObject;
            }
        }

        public static void UpdateTranslations()
        {
            if (titleTextTitle)
                titleTextTitle.text = ModTranslation.GetString("moreOptionsText");

            if (moreOptions)
                moreOptions.Text.text = ModTranslation.GetString("modOptionsText");

            for (int i = 0; i < AllOptions.Length; i++)
            {
                if (i >= modButtons.Count) break;
                modButtons[i].Text.text = ModTranslation.GetString(AllOptions[i].Title);
            }
        }

        private class SelectionBehaviour
        {
            public string Title;
            public Func<bool> OnClick;
            public bool DefaultValue;

            public SelectionBehaviour(string title, Func<bool> onClick, bool defaultValue)
            {
                Title = title;
                OnClick = onClick;
                DefaultValue = defaultValue;
            }
        }
    }
    
 //   [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
	//public static class HiddenTextPatch
	//{
	//	private static void Postfix(TextBoxTMP __instance)
	//	{
	//		bool flag = TheOtherRolesPlugin.StreamerMode.Value && (__instance.name == "GameIdText" || __instance.name == "IpTextBox" || __instance.name == "PortTextBox");
	//		if (flag) __instance.outputText.text = new string('*', __instance.text.Length);
	//	}
	//}
}

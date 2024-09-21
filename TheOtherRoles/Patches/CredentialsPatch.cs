using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.Patches
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {

        public const string BaseCredentials = $@"<size=130%><color=#ff351f>TheOtherRoles GM</color></size> v{TheOtherRolesPlugin.VersionString} <size=80%>({TheOtherRolesPlugin.SupportString})</size>";

        public const string ContributorsCredentials = "Original TORGM (v3.5.4) GitHub Contributors: Alex2911, amsyarasyiq, gendelo3\n{0}\n\nNew Version Among Us Support: JieGeLovesDengDuaLang";


        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TextAlignmentOptions.Top;
                var position = __instance.GetComponent<AspectPosition>();
                position.Alignment = AspectPosition.EdgeAlignments.Top;
                __instance.text.text = BaseCredentials + $"\nPING: <b>{AmongUsClient.Instance.Ping}</b> ms";
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    position.DistanceFromEdge = new Vector3(2.25f, 0.11f, 0);
                }
                else
                {
                    position.DistanceFromEdge = new Vector3(0f, 0.1f, 0);
                }
                position.AdjustPosition();
            }
        }

        const string PopupName = "ModCreditsPopup";

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class MainMenuStartPatch
        {
            static void Postfix(MainMenuManager __instance)
            {
                var popupPrefab = __instance.transform.Find("StatsPopup");
                Buttons.Clear();
                DestroyableSingleton<ModManager>.Instance.ShowModStamp();

                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
                torLogo.transform.localPosition = new Vector3(-0.4f, 1f, 5f);
                var renderer = torLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);

                var creditsPopup = Object.Instantiate(popupPrefab, popupPrefab.transform.parent);
                creditsPopup.name = PopupName;

                CreateButton(__instance, __instance.quitButton, GameObject.Find("RightPanel").transform, new(-1, -1, 0), "BILIBILI",
                    () =>
                    {
                        Application.OpenURL("https://space.bilibili.com/483236840");
                    }, new(0, 174, 236, byte.MaxValue), new(0, 134, 236, byte.MaxValue));

                CreateButton(__instance, __instance.quitButton, GameObject.Find("RightPanel").transform, new(1, -1, 0), "GITHUB",
                    () =>
                    {
                        Application.OpenURL("https://github.com/JieGeLovesDengDuaLang/TheOtherRoles-GM");
                    }, new(153, 153, 153, byte.MaxValue), new(209, 209, 209, byte.MaxValue));

                CreateButton(__instance, __instance.quitButton, GameObject.Find("RightPanel").transform, new(0, -1.5f, 0), ModTranslation.GetString("CreditsLabel"),
                    () =>
                    {
                        creditsPopup.gameObject.SetActive(true);
                    });
            }

            public static List<PassiveButton> Buttons { get; } = new();

            private static void CreateButton(MainMenuManager __instance, PassiveButton template, Transform parent, Vector3 position, string text, Action action, Color colorNormal, Color colorHover)
            {
                if (!parent) return;

                var button = Object.Instantiate(template, parent);
                button.transform.localPosition = position;
                Object.Destroy(button.GetComponent<AspectPosition>());
                var buttonSpriteInactive = button.inactiveSprites.GetComponent<SpriteRenderer>();
                var buttonSpriteActive = button.activeSprites.GetComponent<SpriteRenderer>();
                buttonSpriteInactive.color = colorNormal;
                buttonSpriteActive.color = colorHover;
                __instance.StartCoroutine(Effects.Lerp(0.5f,
                    new Action<float>(_ => { button.GetComponentInChildren<TMP_Text>().SetText(text); })));

                button.OnClick = new();
                button.OnClick.AddListener(action);

                Buttons.Add(button);
            }

            private static void CreateButton(MainMenuManager __instance, PassiveButton template, Transform parent, Vector3 position, string text, Action action)
            {
                if (!parent) return;

                var button = Object.Instantiate(template, parent);
                button.transform.localPosition = position;
                Object.Destroy(button.GetComponent<AspectPosition>());
                __instance.StartCoroutine(Effects.Lerp(0.5f,
                    new Action<float>(_ => { button.GetComponentInChildren<TMP_Text>().SetText(text); })));

                button.OnClick = new();
                button.OnClick.AddListener(action);

                Buttons.Add(button);
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenAccountMenu))]
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenCredits))]
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.OpenGameModeMenu))]
        private static class HidePatch
        {
            static void Postfix()
            {
                GameObject.Find("bannerLogo_TOR")?.SetActive(false);
                MainMenuStartPatch.Buttons.DoIf(b => b, b => b.gameObject.SetActive(false));
            }
        }

        [HarmonyPatch(typeof(StatsPopup), nameof(StatsPopup.DisplayGameStats))]
        private static class PopupPatch
        {
            static bool Prefix(StatsPopup __instance)
            {
                if (__instance.name != PopupName) return true;
                __instance.StatsText.text = string.Format(ContributorsCredentials, ModTranslation.GetString("creditsFull"));
                __instance.transform.Find("GameStatsButton").gameObject.SetActive(false);
                __instance.transform.Find("RoleStatsButton").gameObject.SetActive(false);
                __instance.transform.Find("Title_TMP").GetComponent<TextTranslatorTMP>().Destroy();
                __instance.transform.Find("Title_TMP").GetComponent<TextMeshPro>().text = ModTranslation.GetString("CreditsLabel");
                return false;
            }
        }
    }
}

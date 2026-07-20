using AmongUs.Data;
using AmongUs.QuickChat;
using Assets.CoreScripts;
using Reactor.Networking.Rpc;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsSalem.Roles;

public class SheriffNightChat
{
    [RegisterEvent]
    public static void EjectionEvent(EjectionEvent @event)
    {
        UpdateSheriffChat(true);
    }

    [RegisterEvent]
    public static void RoundStartEvent(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
            return;

        UpdateSheriffChat();

        var sheriffs = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x.IsSheriff()).ToList();
        if (sheriffs.Count >= 2)
        {
            foreach (var sheriffDepartment in sheriffs)
                sheriffDepartment.RpcAddModifier<Confirmed>();
        }
    }

    [RegisterEvent]
    public static void StartMeetingEvent(StartMeetingEvent @event)
    {
        UpdateSheriffChat(true);
    }

    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        if (@event.Target.AmOwner) UpdateSheriffChat(true);
    }

    public static ChatController SheriffChatButton;
    public static Transform Background;
    public static List<(PlayerControl, string, bool)> Messages = new List<(PlayerControl, string, bool)>();
    public static void UpdateSheriffChat(bool disable = false)
    {
        if (SheriffChatButton && (disable || PlayerControl.LocalPlayer.HasDied()))
        {
            if (SheriffChatButton.IsOpenOrOpening) ControllerManager.Instance.CloseOverlayMenu(SheriffChatButton.name);
            Object.Destroy(SheriffChatButton.gameObject);
            Object.Destroy(Background.gameObject);
        }

        if (PlayerControl.LocalPlayer.HasDied() || disable) 
            return;

        if (!PlayerControl.LocalPlayer.IsSheriff())
        {
            if (SheriffChatButton)
            {
                if (SheriffChatButton.IsOpenOrOpening) ControllerManager.Instance.CloseOverlayMenu(SheriffChatButton.name);
                Object.Destroy(SheriffChatButton.gameObject);
                Object.Destroy(Background.gameObject);
            }
            return;
        }

        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            return;

        if (!SheriffChatButton)
        {
            SheriffChatButton = Object.Instantiate(HudManager.Instance.Chat, HudManager.Instance.Chat.transform.parent);
            SheriffChatButton.name = "SheriffChat";
            foreach (var bubble in SheriffChatButton.chatBubblePool.activeChildren)
            {
                Object.Destroy(bubble.gameObject);
            }
            SheriffChatButton.chatBubblePool.activeChildren.Clear();
            SheriffChatButton.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().color = RoleColors.Village;
            SheriffChatButton.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().color = RoleColors.Village;
            SheriffChatButton.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().color = RoleColors.Village;
            var container = SheriffChatButton.chatScreen.transform.Find("ChatScreenContainer");
            container.transform.FindChild("Background").GetComponent<SpriteRenderer>().color = RoleColors.Village;
        }
        if (!Background)
        {
            Background = Object.Instantiate(HudManager.Instance.SettingsButton.transform.GetChild(2), HudManager.Instance.SettingsButton.transform.GetChild(2).transform.parent);
            Background.name = "SheriffChatBackground";
            Background.transform.localPosition = new Vector3(0.717f, -0.631f, 1f);
        }
        SheriffChatButton.gameObject.SetActive(true);
        Background.gameObject.SetActive(true);
        SheriffChatButton.transform.localPosition = new Vector3(1.4386f, -0.7827f, 0f);
        SheriffChatButton.chatButton.transform.GetChild(3).gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    public class ToggleChat
    {
        public static bool Prefix(ChatController __instance)
        {
            if (SheriffChatButton == null) return true;
            if (!SheriffChatButton.isActiveAndEnabled) return true;
            if (SheriffChatButton.IsOpenOrOpening && __instance != SheriffChatButton) return false;
            if (__instance == SheriffChatButton && !SheriffChatButton.IsOpenOrOpening) //Open chat
            {
                Coroutines.Start(WaitForSend(__instance));
            }
            return true;
        }

        public static IEnumerator WaitForSend(ChatController __instance)
        {
            yield return new WaitForSeconds(0.1f);
            while (Messages.Count > 0)
            {
                var message = Messages[0];
                ForceAddChat(__instance, message.Item1, message.Item2, message.Item3);
                Messages.Remove(message);
                yield return null;
            }
            yield break;
        }

        public static void ForceAddChat(ChatController __instance, PlayerControl srcPlayer, string chatText, bool censor)
        {
            NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
            NetworkedPlayerInfo data2 = srcPlayer.Data;
            if (data2 == null || data == null || (data2.IsDead && !data.IsDead))
            {
                return;
            }
            ChatBubble pooledBubble = __instance.GetPooledBubble();
            try
            {
                pooledBubble.transform.SetParent(__instance.scroller.Inner);
                pooledBubble.transform.localScale = Vector3.one;
                bool flag = srcPlayer == PlayerControl.LocalPlayer;
                if (flag)
                {
                    pooledBubble.SetRight();
                }
                else
                {
                    pooledBubble.SetLeft();
                }
                bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(srcPlayer.PlayerId);
                pooledBubble.SetCosmetics(data2);
                __instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2), null);
                if (censor && DataManager.Settings.Multiplayer.CensorChat)
                {
                    chatText = BlockedWords.CensorWords(chatText, false);
                }
                pooledBubble.SetText(chatText);
                pooledBubble.AlignChildren();
                __instance.AlignAllBubbles();
            }
            catch (Exception message)
            {
                ChatController.Logger.Error(message.ToString(), null);
                __instance.chatBubblePool.Reclaim(pooledBubble);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
    public static class SendChat
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] ref string chatText)
        {
            if (SheriffChatButton == null) return true;
            if (!SheriffChatButton.isActiveAndEnabled) return true;
            if (!SheriffChatButton.IsOpenOrOpening) return true;
            if (!__instance.AmOwner) return true;
            chatText = Regex.Replace(chatText, "<.*?>", string.Empty);
            if (string.IsNullOrWhiteSpace(chatText))
            {
                return false;
            }
            if (DestroyableSingleton<HudManager>.Instance)
            {
                SheriffChatButton.AddChat(__instance, chatText, true);
            }
            if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }

            RpcSendCustomChat(__instance, chatText, "SheriffChat");
            return false;
        }

        [MethodRpc((uint)AUSRpc.RpcSendCustomChat)]
        public static void RpcSendCustomChat(PlayerControl player, string chatText, string chatType)
        {
            if (chatType == "SheriffChat")
            {
                if (!PlayerControl.LocalPlayer.HasDied() && SheriffNightChat.SheriffChatButton != null &&
                    player != PlayerControl.LocalPlayer) SheriffNightChat.SheriffChatButton.AddChat(player, chatText, false);
                /*else if (PlayerControl.LocalPlayer.Data.IsDead && Utils.ShowDeadBodies)
                {
                    var text2 = "[Sheriff Chat]\n" + chatText;
                    HudManager.Instance.Chat.AddChat(player, text2, false);
                }*/
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    public static class AddChat
    {
        public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl srcPlayer, [HarmonyArgument(1)] string chatText, [HarmonyArgument(2)] bool censor)
        {
            if (SheriffChatButton == null) return true;
            if (!SheriffChatButton.isActiveAndEnabled) return true;
            if (__instance != SheriffChatButton) return true;
            if (SheriffChatButton.IsOpenOrOpening) return true;
            Messages.Add((srcPlayer, chatText, censor)); // Avoids weird SetFlipXWithoutPet error, really sketchy fix, but couldn't find any better way :sob:
            var flag = srcPlayer == PlayerControl.LocalPlayer;
            if (__instance.notificationRoutine == null)
            {
                __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
            }
            if (!flag)
            {
                SoundManager.Instance.PlaySound(__instance.messageSound, false, 1f, null).pitch = 0.5f + (float)srcPlayer.PlayerId / 15f;
                __instance.chatNotification.SetUp(srcPlayer, chatText);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendQuickChat))]
    public static class SendQuickChat
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] QuickChatPhraseBuilderResult data)
        {
            if (SheriffChatButton == null) return true;
            if (!SheriffChatButton.isActiveAndEnabled) return true;
            if (!SheriffChatButton.IsOpenOrOpening) return true;
            if (!__instance.AmOwner) return true;
            string text = data.ToChatText();
            if (string.IsNullOrWhiteSpace(text) || data == null || !data.IsValid())
            {
                return false;
            }
            if (DestroyableSingleton<HudManager>.Instance)
            {
                SheriffChatButton.AddChat(__instance, text, false);
            }
            if (data.ToChatText().IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }

            SheriffNightChat.SendChat.RpcSendCustomChat(__instance, text, "SheriffChat");
            return false;
        }
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
    public class ChatColor
    {
        public static void Postfix(ChatBubble __instance)
        {
            if (LobbyBehaviour.Instance) return;
            if ((SheriffChatButton != null && SheriffChatButton.isActiveAndEnabled && SheriffChatButton.chatBubblePool.activeChildren.Contains(__instance)) || (__instance.TextArea.text.Contains("[Sheriff Chat]") && PlayerControl.LocalPlayer.Data.IsDead))
            {
                __instance.Background.color = RoleColors.Village;
                __instance.NameText.color = RoleColors.Village;
                var srcPlayer = MiscUtils.PlayerById(__instance.playerInfo.PlayerId);
                if (srcPlayer == PlayerControl.LocalPlayer) return;
                __instance.NameText.text = srcPlayer.Data.PlayerName;
            }
        }
    }

    /*[HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromPlayerOutfit))]
    public class SheriffOutfit
    {
        public static void Prefix(PoolablePlayer __instance, ref NetworkedPlayerInfo.PlayerOutfit outfit)
        {
            if (LobbyBehaviour.Instance) return;
            if (PlayerControl.LocalPlayer.IsJailed() && SheriffChatButton != null)
            {
                var sheriff = PlayerControl.LocalPlayer.GetSheriff();
                if (sheriff == null) return;
                if (SheriffChatButton.chatBubblePool.activeChildren.ToArray().Where(x => x.Cast<ChatBubble>().Player == __instance).ToList().Count <= 0) return;
                var sheriffOutfit = sheriff.Player.Data.Outfits[PlayerOutfitType.Default];
                if (sheriffOutfit != outfit) return;
                outfit = new NetworkedPlayerInfo.PlayerOutfit()
                {
                    ColorId = 14,
                    HatId = "",
                    SkinId = "",
                    VisorId = "",
                    PlayerName = "Sheriff",
                    PetId = ""
                };
            }
        }
    }*/
}
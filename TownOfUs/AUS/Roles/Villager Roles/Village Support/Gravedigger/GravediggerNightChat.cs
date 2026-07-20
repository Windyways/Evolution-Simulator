using AmongUs.Data;
using AmongUs.QuickChat;
using Assets.CoreScripts;
using Reactor.Networking.Rpc;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsSalem.Roles;

public class GravediggerNightChat
{
    public static bool CanChatGravdiggerChat(PlayerControl p) => p.HasDied() || p.IsRole<Gravedigger>();

    [RegisterEvent]
    public static void EjectionEvent(EjectionEvent @event)
    {
        UpdateGravediggerChat(true);
    }

    [RegisterEvent]
    public static void RoundStartEvent(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
            return;

        UpdateGravediggerChat();
    }

    [RegisterEvent]
    public static void StartMeetingEvent(StartMeetingEvent @event)
    {
        UpdateGravediggerChat(true);
    }

    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        if (@event.Target.AmOwner) UpdateGravediggerChat(true);
    }

    public static ChatController GravediggerChatButton;
    public static Transform Background;
    public static List<(PlayerControl, string, bool)> Messages = new List<(PlayerControl, string, bool)>();
    public static void UpdateGravediggerChat(bool disable = false)
    {
        if (GravediggerChatButton && (disable || PlayerControl.LocalPlayer.HasDied()))
        {
            if (GravediggerChatButton.IsOpenOrOpening) ControllerManager.Instance.CloseOverlayMenu(GravediggerChatButton.name);
            Object.Destroy(GravediggerChatButton.gameObject);
            Object.Destroy(Background.gameObject);
        }

        if (!CanChatGravdiggerChat(PlayerControl.LocalPlayer) || disable) 
            return;

        if (!CanChatGravdiggerChat(PlayerControl.LocalPlayer))
        {
            if (GravediggerChatButton)
            {
                if (GravediggerChatButton.IsOpenOrOpening) ControllerManager.Instance.CloseOverlayMenu(GravediggerChatButton.name);
                Object.Destroy(GravediggerChatButton.gameObject);
                Object.Destroy(Background.gameObject);
            }
            return;
        }

        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            return;

        if (!GravediggerChatButton)
        {
            GravediggerChatButton = Object.Instantiate(HudManager.Instance.Chat, HudManager.Instance.Chat.transform.parent);
            GravediggerChatButton.name = "GravediggerChat";
            foreach (var bubble in GravediggerChatButton.chatBubblePool.activeChildren)
            {
                Object.Destroy(bubble.gameObject);
            }
            GravediggerChatButton.chatBubblePool.activeChildren.Clear();
            GravediggerChatButton.chatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().color = RoleColors.Village;
            GravediggerChatButton.chatButton.transform.Find("Active").GetComponent<SpriteRenderer>().color = RoleColors.Village;
            GravediggerChatButton.chatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().color = RoleColors.Village;
            var container = GravediggerChatButton.chatScreen.transform.Find("ChatScreenContainer");
            container.transform.FindChild("Background").GetComponent<SpriteRenderer>().color = RoleColors.Village;
        }
        if (!Background)
        {
            Background = Object.Instantiate(HudManager.Instance.SettingsButton.transform.GetChild(2), HudManager.Instance.SettingsButton.transform.GetChild(2).transform.parent);
            Background.name = "GravediggerChatBackground";
            Background.transform.localPosition = new Vector3(0.717f, -0.631f, 1f);
        }
        GravediggerChatButton.gameObject.SetActive(true);
        Background.gameObject.SetActive(true);
        GravediggerChatButton.transform.localPosition = new Vector3(1.4386f, -0.7827f, 0f);
        GravediggerChatButton.chatButton.transform.GetChild(3).gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    public class ToggleChat
    {
        public static bool Prefix(ChatController __instance)
        {
            if (GravediggerChatButton == null) return true;
            if (!GravediggerChatButton.isActiveAndEnabled) return true;
            if (GravediggerChatButton.IsOpenOrOpening && __instance != GravediggerChatButton) return false;
            if (__instance == GravediggerChatButton && !GravediggerChatButton.IsOpenOrOpening) //Open chat
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
            if (GravediggerChatButton == null) return true;
            if (!GravediggerChatButton.isActiveAndEnabled) return true;
            if (!GravediggerChatButton.IsOpenOrOpening) return true;
            if (!__instance.AmOwner) return true;
            chatText = Regex.Replace(chatText, "<.*?>", string.Empty);
            if (string.IsNullOrWhiteSpace(chatText))
            {
                return false;
            }
            if (DestroyableSingleton<HudManager>.Instance)
            {
                GravediggerChatButton.AddChat(__instance, chatText, true);
            }
            if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }

            RpcSendCustomChat(__instance, chatText, "GravediggerChat");
            return false;
        }

        [MethodRpc((uint)AUSRpc.RpcSendCustomChat)]
        public static void RpcSendCustomChat(PlayerControl player, string chatText, string chatType)
        {
            if (chatType == "GravediggerChat")
            {
                if (!PlayerControl.LocalPlayer.HasDied() && GravediggerNightChat.GravediggerChatButton != null &&
                    player != PlayerControl.LocalPlayer) GravediggerNightChat.GravediggerChatButton.AddChat(player, chatText, false);
                /*else if (PlayerControl.LocalPlayer.Data.IsDead && Utils.ShowDeadBodies)
                {
                    var text2 = "[Gravedigger Chat]\n" + chatText;
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
            if (GravediggerChatButton == null) return true;
            if (!GravediggerChatButton.isActiveAndEnabled) return true;
            if (__instance != GravediggerChatButton) return true;
            if (GravediggerChatButton.IsOpenOrOpening) return true;
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
            if (GravediggerChatButton == null) return true;
            if (!GravediggerChatButton.isActiveAndEnabled) return true;
            if (!GravediggerChatButton.IsOpenOrOpening) return true;
            if (!__instance.AmOwner) return true;
            string text = data.ToChatText();
            if (string.IsNullOrWhiteSpace(text) || data == null || !data.IsValid())
            {
                return false;
            }
            if (DestroyableSingleton<HudManager>.Instance)
            {
                GravediggerChatButton.AddChat(__instance, text, false);
            }
            if (data.ToChatText().IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }

            GravediggerNightChat.SendChat.RpcSendCustomChat(__instance, text, "GravediggerChat");
            return false;
        }
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
    public class ChatColor
    {
        public static void Postfix(ChatBubble __instance)
        {
            if (LobbyBehaviour.Instance) return;
            if ((GravediggerChatButton != null && GravediggerChatButton.isActiveAndEnabled && GravediggerChatButton.chatBubblePool.activeChildren.Contains(__instance)) || (__instance.TextArea.text.Contains("[Gravedigger Chat]") && PlayerControl.LocalPlayer.Data.IsDead))
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
    public class GravediggerOutfit
    {
        public static void Prefix(PoolablePlayer __instance, ref NetworkedPlayerInfo.PlayerOutfit outfit)
        {
            if (LobbyBehaviour.Instance) return;
            if (PlayerControl.LocalPlayer.IsJailed() && GravediggerChatButton != null)
            {
                var gravedigger = PlayerControl.LocalPlayer.GetGravedigger();
                if (gravedigger == null) return;
                if (GravediggerChatButton.chatBubblePool.activeChildren.ToArray().Where(x => x.Cast<ChatBubble>().Player == __instance).ToList().Count <= 0) return;
                var gravediggerOutfit = gravedigger.Player.Data.Outfits[PlayerOutfitType.Default];
                if (gravediggerOutfit != outfit) return;
                outfit = new NetworkedPlayerInfo.PlayerOutfit()
                {
                    ColorId = 14,
                    HatId = "",
                    SkinId = "",
                    VisorId = "",
                    PlayerName = "Gravedigger",
                    PetId = ""
                };
            }
        }
    }*/
}
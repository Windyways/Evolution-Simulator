using Il2CppInterop.Runtime.InteropTypes.Arrays;
using static MeetingHud;

namespace AmongUsSalem;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class VotingCompletePatch
{
    public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] Il2CppStructArray<VoterState> states, [HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
    {
        if (__instance.exiledPlayer != null)
        {
            var votedPlayer = MiscUtils.PlayerById(__instance.exiledPlayer.PlayerId);
            if (votedPlayer.HasModifier<IntervenedModifier>())
            {
                __instance.exiledPlayer = null;
            }
        }
    }
}
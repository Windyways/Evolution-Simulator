using UnityEngine;

namespace AmongUsSalem.Misc;

public static class AUSAssets
{
    private const string RoleCard = "TownOfUs.Resources.AUS.Sprites.RoleCards";
    private const string Abilities = "TownOfUs.Resources.AUS.Sprites.Abilities";
    private const string Other = "TownOfUs.Resources.AUS.Sprites.Other";
    private const string Audio = "TownOfUs.Resources.AUS.Audio";


    public static void PlaySound(LoadableAsset<AudioClip> clip, float vol = 1f)
    {
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(clip.LoadAsset(), false, vol);
        }
    }

    // --- Abilities ---
    public static LoadableAsset<Sprite> Day_KillSprite { get; } = new LoadableResourceAsset($"{Abilities}.MeetingKillButton.png");
    public static LoadableAsset<Sprite> KillSprite { get; } = TouAssets.KillSprite;

    // OTHER
    public static string RoleIconPosName
    {
        get
        {
            var name = "Next To Role";
            switch (AUSPlugin.RoleIconSpot.Value)
            {
                case 1:
                    name = "Next To Name";
                    break;
            }
            return name;
        }
    }
}
using MiraAPI.GameEnd;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsSalem.ImpostorRoles;

public sealed class ImpostorGameOver : CustomGameOver
{
    private Color _roleColor;

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners is not [{ Role: RoleBehaviour role and ICustomAURole tRole }])
        {
            return false;
        }

        var mainRole = role;

        Logger<AUSPlugin>.Error($"VerifyCondition - mainRole: '{mainRole.NiceName}', IsDead: '{role.IsDead}'");

        if (role.IsDead)
        {
            mainRole = role.Player.GetRoleWhenAlive();

            Logger<AUSPlugin>.Error($"VerifyCondition - RoleWhenAlive: '{mainRole?.NiceName}'");
        }

        _roleColor = mainRole.TeamColor;

        return tRole.WinConditionMet();
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, _roleColor);

        var text = Object.Instantiate(endGameManager.WinText);
        text.text = $"Impostor Wins!";
        text.color = _roleColor;
        GameHistory.WinningFaction = $"<color=#{_roleColor.ToHtmlStringRGBA()}>Impostor</color>";
        FactionReferences.UpdateFactionResult(Faction.Crewmate, false);
        FactionReferences.UpdateFactionResult(Faction.Neutral, false);
        FactionReferences.UpdateFactionResult(Faction.Impostor, true);

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);

        text.transform.position = pos;
        text.text = $"<size=4>{text.text}</size>";
        
        //AUSAssets.PlaySound(AUSAssets.CovenWin_SFX);
    }

    public static bool AnyWon(GameOverReason gameOverReason)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Is(Faction.Impostor) && WinConditionMet()) return true;
        }
        return false;
    }

    public static bool WinConditionMet()
    {
        var aliveNKs = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x.Is(Alignment.NeutralKilling));
        var aliveImpostors = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x.Is(Faction.Impostor));
        var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && !x.Is(Faction.Impostor) && x.Data.Role is not INotThreatable);
        return aliveImpostors > alivePlayers && aliveNKs == 0;
    }
}
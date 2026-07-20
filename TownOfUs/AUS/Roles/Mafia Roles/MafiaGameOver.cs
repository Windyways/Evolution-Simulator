using MiraAPI.GameEnd;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsSalem.MafiaRoles;

public sealed class MafiaGameOver : CustomGameOver
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
        text.text = $"Werewolf Wins!";
        text.color = _roleColor;
        GameHistory.WinningFaction = $"<color=#{_roleColor.ToHtmlStringRGBA()}>Werewolf</color>";

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
            if (player.Is(Faction.Mafia) && WinConditionMet()) return true;
        }
        return false;
    }

    public static bool WinConditionMet()
    {
        var aliveWerewolf = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && x.Is(Faction.Mafia));
        var alivePlayers = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && !x.Is(Faction.Mafia));
        return aliveWerewolf > alivePlayers;
    }
}
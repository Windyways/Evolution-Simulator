using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using UnityEngine;

namespace AmongUsSalem.Modifiers;

public sealed class PendingReviveModifier(PlayerControl c) : BaseModifier
{
    public override string ModifierName => "PendingReviveModifier";
    public override bool Unique => false;
    public override bool HideOnUi => true;

    public PlayerControl Caster => c;
    public override void OnMeetingStart()
    {
        base.OnMeetingStart();

        CoRevivePlayer(Player);
        Player.RpcAddModifier<Confirmed>();
        Caster.RpcAddModifier<Confirmed>();

        PlayerControl.LocalPlayer.Notify(Gravedigger.Info(Caster, Player), NotifyMode.InstantlyAndMeeting);
        if (Caster.Data.Role is Gravedigger gravedigger) gravedigger.hasShovel = false;

        Player.RpcRemoveModifier<PendingReviveModifier>();
    }

    [HideFromIl2Cpp]
    public void CoRevivePlayer(PlayerControl dead)
    {
        var roleWhenAlive = dead.GetRoleWhenAlive();

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == dead.PlayerId);
        var position = new Vector2(Player.transform.localPosition.x, Player.transform.localPosition.y);

        if (body != null) position = new Vector2(body.transform.localPosition.x, body.transform.localPosition.y + 0.3636f);

        ReviveUtilities.RevivePlayer(
            reviver: Player,
            revived: dead,
            position: new Vector2(position.x, position.y),
            roleWhenAlive: roleWhenAlive!,
            flashColor: RoleColors.Village, "", "");

        body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == dead.PlayerId);
        if (body != null) UnityEngine.Object.Destroy(body.gameObject);
    }
}
namespace TownOfUs;

public enum AUSRpc : uint
{
    // Roles
    RpcAddRemoveVest,
    RpcArrest,
    RpcRestore,
    RpcSteal,
    RpcPerformInteraction_Doctor,

    // Notifications
    RpcTMDNotify,

    // Mechanics
    StartDayOne,

    // Other
    RpcAddDeathReason,

    // Chat
    RpcSendCustomChat,

    // Misc
    RequestDeathStateValidation,
    SyncDeathState,
    GhostRoleMurder,











    RemoveSpawns,

    UpdateDeathHandler,
    SetMap,
    ChangeRole,
    PlayerExile,
    SetPos,
    SendLoveChat,
    SendJailorChat,
    SendJaileeChat,
    SendImpTeamChat,
    SendVampTeamChat,
    UpdateCelebrityKilled,
    ClericBarrierAttacked,
    Transport,
    SetSwaps,
    CleanBody,
    CatchPlayer,
    MagicMirror,
    ClearMagicMirror,
    MagicMirrorAttacked,
    MirrorcasterUnleash,
    MedicShield,
    ClearMedicShield,
    MedicShieldAttacked,
    EngineerFix,
    EngineerEventFix,
    IgniteSound,
    PlaceVent,
    ShowVent,
    Remember,
    PlantBomb,
    Blackmail,
    Recall,
    MarkLocation,
    Disperse,
    Mediate,
    VampireBite,
    CheckInfected,
    SetGATarget,
    SetOtherLover,
    SetTraitor,
    DragBody,
    DropBody,
    AltruistRevive,
    Prosecute,
    DoomsayerWin,
    SetExeTarget,
    AddInquisTarget,
    Hysteria,
    WardenFortify,
    ClearWardenFortify,
    WardenNotify,
    OracleBlessNotify,
    CatchGhost,
    Guarded,
    PlumberFlush,
    PlumberBlockVent,
    OracleConfess,
    OracleBless,
    InquisitorWin,
    TriggerSixthSense,
    AurialSense,
    ButtonBarry,
    LookoutSeePlayer,
    AnimateNewReveal,
    SheriffMisfire,
    RetrainImpostor,
    AmbushPlayer,
    RetrainConfirm
}
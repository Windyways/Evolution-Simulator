namespace AmongUsSalem.MCI;

public class TempCleared : BaseModifier
{
    public override string ModifierName => "Temp Cleared";
    public override bool HideOnUi => !Debugger.IsDebuggerActive;
}
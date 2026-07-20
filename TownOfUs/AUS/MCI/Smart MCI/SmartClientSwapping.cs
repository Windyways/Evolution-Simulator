using Reactor.Utilities.Extensions;

namespace AmongUsSalem.MCI.SmartMCI;

public static class SmartClientSwapping
{
    public static void RoundStart()
    {
        if (Debugger.IsDebuggerActive)
        {
            Keyboard_Joystick.RefreshSwapTargets();
            if (Debugger.smartSwapping)
            {
                Keyboard_Joystick.Switch(true);
            }
        }
    }
}

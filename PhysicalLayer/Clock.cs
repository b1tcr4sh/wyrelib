using Ardalis.Result;
using BuildSoft.VRChat.Osc;
using BuildSoft.VRChat.Osc.Avatar;

namespace wyrelib.PhysicalLayer;

public class Clock {
    // TODO 
    // Perform checks to make sure writes don't conflict
    // e.g. Check if listening or not before pulsing/reading 
    private OscAvatarConfig _config;

    public Clock(OscAvatarConfig config) {
        _config = config;
    }

    public async Task<Result> PulseAsync(int delayMilliseconds) {
        try {
            OscParameter.SendAvatarParameter("wyre/clock", true);
            await Task.Delay(delayMilliseconds);
            OscParameter.SendAvatarParameter("wyre/clock", false);
        } catch (Exception e) {
            return Result.Error(e.Message);
        }

        return Result.Success();
    }
    public async Task<Result> WaitForPulseAsync() {
        bool pulsed = false;

        try {
            while (!pulsed) { // While loop is kind of crude solution
                pulsed = (bool) _config.Parameters["wyre/clock"];
                await Task.Delay(50);
            }
        } catch (Exception e) {
            return Result.Error(e.Message);
        }

        return Result.Success();
    }
}
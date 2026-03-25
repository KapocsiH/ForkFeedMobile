using System.Windows.Input;

namespace ForkFeedMobile.Helpers;

public class DebounceHelper
{
    private CancellationTokenSource? _cts;

    public async Task DebounceAsync(Func<Task> action, int delayMs = 400)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            await Task.Delay(delayMs, _cts.Token);
            await action();
        }
        catch (TaskCanceledException)
        {
            // TODO
        }
    }
}

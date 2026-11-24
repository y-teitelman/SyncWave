namespace SyncWave.Core.Enums;

/// <summary>
/// Перелічення, що описує стан відтворення медіа.
/// </summary>
public enum PlaybackState
{
    Playing,
    Paused,
    Stopped,
    Buffering, // поки не використовується: знадобиться, коли клієнти повідомлятимуть про буферизацію
}

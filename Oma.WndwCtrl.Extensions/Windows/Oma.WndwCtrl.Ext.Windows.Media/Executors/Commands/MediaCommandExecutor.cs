using System.Runtime.InteropServices;
using JetBrains.Annotations;
using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Ext.Windows.Media.Model.Commands;

namespace Oma.WndwCtrl.Ext.Windows.Media.Executors.Commands;

[UsedImplicitly]
public partial class MediaCommandExecutor : ICommandExecutor<MediaCommand>
{
  private const int KEYEVENTF_EXTENTEDKEY = 1;

  private const int VK_MEDIA_NEXT_TRACK = 0xB0;
  private const int VK_MEDIA_PREV_TRACK = 0xB1;
  private const int VK_MEDIA_STOP = 0xB2;
  private const int VK_MEDIA_PLAY_PAUSE = 0xB3;

  [MustDisposeResource]
  public Task<Either<FlowError, CommandOutcome>> ExecuteAsync(
    MediaCommand command,
    CancellationToken cancelToken = default
  )
  {
    byte keyToSend = command.Action switch
    {
      MediaAction.Play or MediaAction.Pause => VK_MEDIA_PLAY_PAUSE,
      MediaAction.Next => VK_MEDIA_NEXT_TRACK,
      MediaAction.Previous => VK_MEDIA_PREV_TRACK,
      MediaAction.Stop => VK_MEDIA_STOP,
      var err => throw new InvalidOperationException($"Unhandled media action {err.ToString()}."),
    };

    keybd_event(keyToSend, scanCode: 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);

    return Task.FromResult<Either<FlowError, CommandOutcome>>(
      new CommandOutcome
      {
        Success = true,
      }
    );
  }

  [LibraryImport("user32.dll")]
  private static partial void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
}
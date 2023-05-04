using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audio.MAUI;

public enum AudioControllerResult
{
    Success,
    AccessDenied,
    FileNotFound,
    ErrorCreatingRecorder,
    ErrorCreatingPlayer,
    NotInCorrectStatus,
    PlayerNotInitiated,
    MicrophoneNotSelected,
    FormatNotSupported,
    AudioConfigurationError
}

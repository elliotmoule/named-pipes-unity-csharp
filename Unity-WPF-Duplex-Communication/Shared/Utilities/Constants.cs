using System.IO.Pipes;

namespace Shared.Utilities
{
    public static class Constants
    {
        public static readonly string PipeName = "testPipe";

        public static readonly PipeDirection PipeDirection = PipeDirection.InOut;

        public static readonly PipeOptions PipeOptions = PipeOptions.Asynchronous;

        public static readonly PipeTransmissionMode PipeTransmissionMode = PipeTransmissionMode.Byte;

        public static readonly string DisconnectKeyword = "end";
    }
}
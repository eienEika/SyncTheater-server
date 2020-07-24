using System;
using System.Runtime.Serialization;

namespace SyncTheater.Types.Exceptions
{
    [Serializable]
    internal class RoomAlreadyInitializedException : Exception
    {
        public RoomAlreadyInitializedException()
        {
        }

        public RoomAlreadyInitializedException(string message) : base(message)
        {
        }

        public RoomAlreadyInitializedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RoomAlreadyInitializedException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
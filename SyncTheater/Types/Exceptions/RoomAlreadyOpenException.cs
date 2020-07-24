using System;
using System.Runtime.Serialization;

namespace SyncTheater.Types.Exceptions
{
    [Serializable]
    public class RoomAlreadyOpenException : Exception
    {
        public RoomAlreadyOpenException()
        {
        }

        public RoomAlreadyOpenException(string message) : base(message)
        {
        }

        public RoomAlreadyOpenException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RoomAlreadyOpenException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
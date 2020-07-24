using System;
using System.Runtime.Serialization;

namespace SyncTheater.Types.Exceptions
{
    [Serializable]
    public class RoomNotOpenException : Exception
    {
        public RoomNotOpenException()
        {
        }

        public RoomNotOpenException(string message) : base(message)
        {
        }

        public RoomNotOpenException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RoomNotOpenException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
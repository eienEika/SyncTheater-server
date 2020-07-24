using System;
using System.Runtime.Serialization;

namespace SyncTheater.Types.Exceptions
{
    [Serializable]
    public class RoomNotInitializedException : Exception
    {
        public RoomNotInitializedException()
        {
        }

        public RoomNotInitializedException(string message) : base(message)
        {
        }

        public RoomNotInitializedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RoomNotInitializedException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
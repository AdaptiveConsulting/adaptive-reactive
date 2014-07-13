using System;
using System.Runtime.Serialization;

namespace Adaptive.Observables
{
    [Serializable]
    public class ObservableDictionaryUpdateException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ObservableDictionaryUpdateException()
        {
        }

        public ObservableDictionaryUpdateException(string message) : base(message)
        {
        }

        public ObservableDictionaryUpdateException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ObservableDictionaryUpdateException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JsonGen
{
    public class GenerateException : Exception
    {
        public GenerateException()
        {
        }

        public GenerateException(string message) : base(message)
        {
        }

        public GenerateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GenerateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

using System;
using System.Runtime.Serialization;

namespace SuperTokens.Net
{
    [Serializable]
    public sealed class CoreApiResponseException : Exception
    {
        private const string DefaultMessage = "Failed to call the SuperTokens Core API.";

        public CoreApiResponseException() :
            base(DefaultMessage)
        {
        }

        public CoreApiResponseException(string message) :
            base(message)
        {
        }

        public CoreApiResponseException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public CoreApiResponseException(string requestMethod, string requestUrl, int responseStatusCode, string? responseBody) :
            base(DefaultMessage)
        {
            this.RequestMethod = requestMethod ?? throw new ArgumentNullException(nameof(requestMethod));
            this.RequestUrl = requestUrl ?? throw new ArgumentNullException(nameof(requestUrl));
            this.ResponseStatusCode = responseStatusCode;
            this.ResponseBody = responseBody;
        }

        public CoreApiResponseException(string requestMethod, string requestUrl, int responseStatusCode, string? responseBody, Exception innerException) :
            base(DefaultMessage, innerException)
        {
            this.RequestMethod = requestMethod ?? throw new ArgumentNullException(nameof(requestMethod));
            this.RequestUrl = requestUrl ?? throw new ArgumentNullException(nameof(requestUrl));
            this.ResponseStatusCode = responseStatusCode;
            this.ResponseBody = responseBody;
        }

        private CoreApiResponseException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            this.RequestMethod = info.GetString(nameof(this.RequestMethod));
            this.RequestUrl = info.GetString(nameof(this.RequestUrl));
            this.ResponseBody = info.GetString(nameof(this.ResponseBody));
            this.ResponseStatusCode = info.GetInt32(nameof(this.ResponseStatusCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(this.RequestMethod), this.RequestMethod);
            info.AddValue(nameof(this.RequestUrl), this.RequestUrl);
            info.AddValue(nameof(this.ResponseBody), this.ResponseBody);
            info.AddValue(nameof(this.ResponseStatusCode), this.ResponseStatusCode);

            base.GetObjectData(info, context);
        }

        public string? RequestMethod { get; }

        public string? RequestUrl { get; }

        public string? ResponseBody { get; }

        public int ResponseStatusCode { get; }
    }
}

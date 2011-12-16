namespace AzureManagementWithKinect.Model
{
    using System.Runtime.Serialization;

    public static class ErrorCode
    {
        public const string MissingOrIncorrectVersionHeader = "MissingOrIncorrectVersionHeader";
        public const string InvalidRequest = "InvalidRequest";
        public const string InvalidXmlRequest = "InvalidXmlRequest";
        public const string InvalidContentType = "InvalidContentType";
        public const string MissingOrInvalidRequiredQueryParameter = "MissingOrInvalidRequiredQueryParameter";
        public const string InvalidHttpVerb = "InvalidHttpVerb";
        public const string InternalError = "InternalError";
        public const string BadRequest = "BadRequest";
        public const string AuthenticationFailed = "AuthenticationFailed";
        public const string ResourceNotFound = "ResourceNotFound";
        public const string SubscriptionDisabled = "SubscriptionDisabled";
        public const string ServerBusy = "ServerBusy";
        public const string TooManyRequests = "TooManyRequests";
        public const string ConflictError = "ConflictError";
        public const string ConfiguraitonError = "ConfigurationError";
    }

    [DataContract(Name = "Error", Namespace = Constants.ServiceManagementNS)]
    public class ServiceManagementError : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Code { get; set; }

        [DataMember(Order = 2)]
        public string Message { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public ConfigurationWarningsList ConfigurationWarnings { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
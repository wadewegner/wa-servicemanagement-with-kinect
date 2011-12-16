namespace AzureManagementWithKinect.Model
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Operation : IExtensibleDataObject
    {
        [DataMember(Name = "ID", Order = 1)]
        public string OperationTrackingId { get; set; }

        /// <summary>
        /// The class OperationState defines its possible values. 
        /// </summary>
        [DataMember(Order = 2)]
        public string Status { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public int HttpStatusCode { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public ServiceManagementError Error { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
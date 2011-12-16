namespace AzureManagementWithKinect.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [CollectionDataContract(Name = "HostedServices", ItemName = "HostedService", Namespace = Constants.ServiceManagementNS)]
    public class HostedServiceList : List<HostedService>
    {
        public HostedServiceList()
        {
        }

        public HostedServiceList(IEnumerable<HostedService> hostedServices)
            : base(hostedServices)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class HostedService : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public Uri Url { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string ServiceName { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public HostedServiceProperties HostedServiceProperties { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class HostedServiceProperties : IExtensibleDataObject
    {
        private string label;

        [DataMember(Order = 1)]
        public string Description { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string AffinityGroup { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Location { get; set; }

        [DataMember(Order = 4)]
        public string Label
        {
            get
            {
                return this.label;
            }

            set
            {
                this.label = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
        }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
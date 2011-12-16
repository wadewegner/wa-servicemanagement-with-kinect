namespace AzureManagementWithKinect.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Deployment : IExtensibleDataObject
    {
        private string label;
        private string configuration;

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string DeploymentSlot { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string PrivateID { get; set; }

        /// <summary>
        /// The class DeploymentStatus defines its possible values. 
        /// </summary>
        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string Status { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
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

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public Uri Url { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public string Configuration
        {
            get
            {
                return this.configuration;
            }

            set
            {
                this.configuration = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
        }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public RoleInstanceList RoleInstanceList { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public UpgradeStatus UpgradeStatus { get; set; }

        [DataMember(Order = 11, EmitDefaultValue = false)]
        public int UpgradeDomainCount { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public RoleList RoleList { get; set; }

        [DataMember(Order = 13)]
        public string SdkVersion { get; set; }

        [DataMember(Order = 14, EmitDefaultValue = false)]
        public InputEndpointList InputEndpointList { get; set; }

        [DataMember(Order = 15)]
        public bool Locked { get; set; }

        [DataMember(Order = 16)]
        public bool RollbackAllowed { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "RoleList", ItemName = "Role", Namespace = Constants.ServiceManagementNS)]
    public class RoleList : List<Role>
    {
        public RoleList()
        {
        }

        public RoleList(IEnumerable<Role> roles)
            : base(roles)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Role : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string RoleName { get; set; }

        [DataMember(Order = 2)]
        public string OsVersion { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "RoleInstanceList", ItemName = "RoleInstance", Namespace = Constants.ServiceManagementNS)]
    public class RoleInstanceList : List<RoleInstance>
    {
        public RoleInstanceList()
        {
        }

        public RoleInstanceList(IEnumerable<RoleInstance> roles)
            : base(roles)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class RoleInstance : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string RoleName { get; set; }

        [DataMember(Order = 2)]
        public string InstanceName { get; set; }

        [DataMember(Order = 3)]
        public string InstanceStatus { get; set; }

        [DataMember(Order = 4)]
        public int InstanceUpgradeDomain { get; set; }

        [DataMember(Order = 5)]
        public int InstanceFaultDomain { get; set; }

        [DataMember(Order = 6)]
        public string InstanceSize { get; set; }

        [DataMember(Order = 7)]
        public string InstanceStateDetails { get; set; }

        [DataMember(Order = 8)]
        public string InstanceErrorCode { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "InputEndpointList", ItemName = "InputEndpoint", Namespace = Constants.ServiceManagementNS)]
    public class InputEndpointList : List<InputEndpoint>
    {
        public InputEndpointList()
        {
        }

        public InputEndpointList(IEnumerable<InputEndpoint> inputEndpoint)
            : base(inputEndpoint)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class InputEndpoint : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string RoleName { get; set; }

        [DataMember(Order = 2)]
        public string Vip { get; set; }

        [DataMember(Order = 3)]
        public int Port { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "Swap", Namespace = Constants.ServiceManagementNS)]
    public class SwapDeploymentInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Production { get; set; }

        [DataMember(Order = 2)]
        public string SourceDeployment { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class UpgradeStatus : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string UpgradeType { get; set; }

        [DataMember(Order = 2)]
        public string CurrentUpgradeDomainState { get; set; }

        [DataMember(Order = 3)]
        public int CurrentUpgradeDomain { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class ConfigurationWarning : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string WarningCode { get; set; }

        [DataMember(Order = 2)]
        public string WarningMessage { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }

        public override string ToString()
        {
            return string.Format("WarningCode:{0} WarningMessage:{1}", this.WarningCode, this.WarningMessage);
        }
    }

    [CollectionDataContract(Namespace = Constants.ServiceManagementNS)]
    public class ConfigurationWarningsList : List<ConfigurationWarning>
    {
        public override string ToString()
        {
            var warnings = new StringBuilder(string.Format("ConfigurationWarnings({0}):\n", this.Count));

            foreach (ConfigurationWarning warning in this)
            {
                warnings.Append(warning + "\n");
            }

            return warnings.ToString();
        }
    }

    [DataContract(Name = "UpdateDeploymentStatus", Namespace = Constants.ServiceManagementNS)]
    public class UpdateDeploymentStatusInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Status { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "ChangeConfiguration", Namespace = Constants.ServiceManagementNS)]
    public class ChangeConfigurationInput : IExtensibleDataObject
    {
        private string configuration;

        [DataMember(Order = 1)]
        public string Configuration
        {
            get
            {
                return this.configuration;
            }

            set
            {
                this.configuration = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            }
        }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public bool? TreatWarningsAsError { get; set; }

        [DataMember(Order = 3)]
        public string Mode { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
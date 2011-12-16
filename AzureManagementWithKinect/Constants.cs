namespace AzureManagementWithKinect
{
    using System.Xml.Linq;

    public enum DeploymentStatus
    {
        Running,
        Suspended
    }

    public static class Constants
    {
        public const string ServiceManagementEndpoint = "https://management.core.windows.net";
        public const string ServiceManagementNS = "http://schemas.microsoft.com/windowsazure";
        public const string OperationTrackingIdHeader = "x-ms-request-id";
        public const string VersionHeaderName = "x-ms-version";
        public const string VersionHeaderContent = "2011-10-01";
        public const string PrincipalHeader = "x-ms-principal-id";

        public static XNamespace ServiceConfigurationNS
        {
            get { return "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration"; }
        }
    }

    public static class ServiceStatus
    {
        public const string ResolvingDns = "ResolvingDns";
        public const string Creating = "Creating";
        public const string Created = "Created";
    }

    public static class DeploymentState
    {
        public const string Running = "Running";
        public const string Suspended = "Suspended";
        public const string RunningTransitioning = "RunningTransitioning";
        public const string SuspendedTransitioning = "SuspendedTransitioning";
        public const string Starting = "Starting";
        public const string Suspending = "Suspending";
        public const string Deploying = "Deploying";
        public const string Deleting = "Deleting";
    }

    public static class RoleInstanceStatus
    {
        public const string RoleStateUnknown = "RoleStateUnknown";
        public const string CreatingVM = "CreatingVM";
        public const string StartingVM = "StartingVM";
        public const string CreatingRole = "CreatingRole";
        public const string StartingRole = "StartingRole";
        public const string ReadyRole = "ReadyRole";
        public const string BusyRole = "BusyRole";
        public const string StoppingRole = "StoppingRole";
        public const string DeletingVM = "DeletingVM";
        public const string StoppedVM = "StoppedVM";
        public const string RestartingRole = "RestartingRole";
        public const string CyclingRole = "CyclingRole";
        public const string FailedStartingRole = "FailedStartingRole";
        public const string UnresponsiveRole = "UnresponsiveRole";
    }

    public static class OperationState
    {
        public const string InProgress = "InProgress";
        public const string Succeeded = "Succeeded";
        public const string Failed = "Failed";
    }

    public static class DeploymentSlotType
    {
        public const string Staging = "Staging";
        public const string Production = "Production";
    }
}
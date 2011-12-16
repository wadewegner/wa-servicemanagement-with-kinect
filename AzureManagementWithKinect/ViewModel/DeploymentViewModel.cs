namespace AzureManagementWithKinect.ViewModel
{
    using Model;
    using System.Linq;

    public class RoleViewModel : Deployment
    {
        public Deployment Deployment { get; set; }

        public Role Role { get; set; }

        public int RoleInstanceCount
        {
            get { return this.Deployment.RoleInstanceList.Count(i => i.RoleName.Equals(this.Role.RoleName)); }
        }
    }
}

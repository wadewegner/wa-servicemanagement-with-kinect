namespace AzureManagementWithKinect
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Runtime.Serialization;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml.Linq;
    using AzureManagementWithKinect.Model;

    public class AzureManagementWrapper
    {
        private HttpClient client;

        public AzureManagementWrapper(string subscriptionId, X509Certificate2 managementCertificate, string versionHeader = Constants.VersionHeaderContent)
        {
            this.SubscriptionId = subscriptionId;
            this.client = GetClient(subscriptionId, managementCertificate, versionHeader);
        }

        public string SubscriptionId { get; private set; }

        public HostedServiceList GetHostedServices()
        {
            var response = this.client.Get("services/hostedservices");
            return ReadResponseContent<HostedServiceList>(response);
        }

        public HostedService GetHostedService(string name)
        {
            var response = this.client.Get("services/hostedservices/" + name);
            return ReadResponseContent<HostedService>(response);
        }

        public Deployment GetDeployment(string serviceName, string deploymentSlot, bool embedDetails = false)
        {
            var uri = string.Format("services/hostedservices/{0}/deploymentslots/{1}", serviceName, deploymentSlot);
            if (embedDetails)
            {
                uri += "?embed-detail=TRUE";
            }

            var response = this.client.Get(uri);
            return ReadResponseContent<Deployment>(response, false);
        }

        public string SwapDeployment(string serviceName)
        {
            var production = this.GetDeployment(serviceName, DeploymentSlotType.Production);
            var staging = this.GetDeployment(serviceName, DeploymentSlotType.Staging);
            
            if (staging == null || string.IsNullOrWhiteSpace(staging.Name)) return null;

            var swapDeploymentInput = new SwapDeploymentInput
            {
                Production = (production == null || string.IsNullOrWhiteSpace(production.Name)) ? null : production.Name,
                SourceDeployment = staging.Name
            };

            var formatter = new XmlMediaTypeFormatter();
            formatter.SetSerializer<SwapDeploymentInput>(new DataContractSerializer(typeof(SwapDeploymentInput)));

            using (var content = new ObjectContent<SwapDeploymentInput>(swapDeploymentInput, "application/xml", new[] { formatter }))
            {
                var requestUri = string.Format("services/hostedservices/{0}", serviceName);
                var response = this.client.Post(requestUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = GetErrorDetails(response);
                    throw new InvalidOperationException(errorDetails.Message);
                }

                return response.Headers.GetValues(Constants.OperationTrackingIdHeader).FirstOrDefault();
            }
        }

        public string UpdateRoleInstances(string serviceName, string deploymentSlot, string roleName, int instancesCount, string configuration = "")
        {
            if (string.IsNullOrWhiteSpace(configuration))
            {
                configuration = this.GetDeployment(serviceName, deploymentSlot).Configuration;
            }

            var configXml = XDocument.Parse(configuration);
            var roleConfiguration = configXml.Root.Descendants(Constants.ServiceConfigurationNS + "Role").SingleOrDefault(r => r.Attribute("name").Value == roleName);

            if (roleConfiguration != null)
            {
                roleConfiguration.Descendants(Constants.ServiceConfigurationNS + "Instances").First().Attribute("count").Value = instancesCount.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                throw new InvalidOperationException("The role does not exist");
            }

            return this.UpdateDeploymentConfiguration(serviceName, deploymentSlot, configXml.ToString());
        }

        public string UpdateDeploymentConfiguration(string serviceName, string deploymentSlot, string configuration, bool treatWarningsAsError = false, string mode = "Auto")
        {
            var deploymentStatus = new ChangeConfigurationInput
            {
                Configuration = configuration,
                Mode = mode,
                TreatWarningsAsError = treatWarningsAsError
            };

            var formatter = new XmlMediaTypeFormatter();
            formatter.SetSerializer<ChangeConfigurationInput>(new DataContractSerializer(typeof(ChangeConfigurationInput)));

            using (var content = new ObjectContent<ChangeConfigurationInput>(deploymentStatus, "application/xml", new[] { formatter }))
            {
                var requestUri = string.Format("services/hostedservices/{0}/deploymentslots/{1}/?comp=config", serviceName, deploymentSlot);
                var response = this.client.Post(requestUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = GetErrorDetails(response);
                    throw new InvalidOperationException(errorDetails.Message);
                }

                return response.Headers.GetValues(Constants.OperationTrackingIdHeader).FirstOrDefault();
            }
        }

        public string UpdateDeploymentStatus(string serviceName, string deploymentSlot, DeploymentStatus status)
        {
            var deploymentStatus = new UpdateDeploymentStatusInput
            {
                Status = status.ToString()
            };

            var formatter = new XmlMediaTypeFormatter();
            formatter.SetSerializer<UpdateDeploymentStatusInput>(new DataContractSerializer(typeof(UpdateDeploymentStatusInput)));

            using (var content = new ObjectContent<UpdateDeploymentStatusInput>(deploymentStatus, "application/xml", new[] { formatter }))
            {
                var requestUri = string.Format("services/hostedservices/{0}/deploymentslots/{1}/?comp=status", serviceName, deploymentSlot);
                var response = this.client.Post(requestUri, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = GetErrorDetails(response);
                    throw new InvalidOperationException(errorDetails.Message);
                }

                return response.Headers.GetValues(Constants.OperationTrackingIdHeader).FirstOrDefault();
            }
        }

        public Operation GetOperationStatus(string operationId)
        {
            var response = this.client.Get("operations/" + operationId);
            return ReadResponseContent<Operation>(response);
        }

        private static ServiceManagementError GetErrorDetails(HttpResponseMessage response)
        {
            var formatter = new XmlMediaTypeFormatter();
            formatter.SetSerializer<ServiceManagementError>(new DataContractSerializer(typeof(ServiceManagementError)));
            
            var error = response.Content.ReadAsOrDefault<ServiceManagementError>(new[] { formatter });

            return error ?? new ServiceManagementError { Code = response.StatusCode.ToString(), Message = response.ReasonPhrase };
        }

        private static T ReadResponseContent<T>(HttpResponseMessage response, bool throwErrorIfNotFound = true)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound && !throwErrorIfNotFound)
                {
                    return default(T);
                }

                var errorDetails = GetErrorDetails(response);
                throw new InvalidOperationException(errorDetails.Message);
            }

            var formatter = new XmlMediaTypeFormatter();
            formatter.SetSerializer<T>(new DataContractSerializer(typeof(T)));

            return response.Content.ReadAs<T>(new[] { formatter });
        }

        private static HttpClient GetClient(string subscriptionId, X509Certificate2 managementCertificate, string versionHeader)
        {
            var clientHandler = new WebRequestHandler();
            clientHandler.ClientCertificates.Add(managementCertificate);
            clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;

            var client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(string.Format("{0}/{1}/", Constants.ServiceManagementEndpoint, subscriptionId));
            client.DefaultRequestHeaders.Add(Constants.VersionHeaderName, versionHeader);

            return client;
        }
    }
}
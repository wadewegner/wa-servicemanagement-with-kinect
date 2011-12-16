<h1>Windows Azure Service Management API with Kinect</h1>
<p>This repo contains code that you can use to have your Kinect and PC interact with the Windows Azure Service Management API.
</p>

<h2>Getting Started</h2>
<p>Dependencies</p>
<ul>
    <li><strong>Kinect SDK</strong> - <a href="http://kinectforwindows.org/">http://kinectforwindows.org/</a></li>
    <li><strong>KinectToolbox NuGet Package</strong>: Install-Package KinectToolbox</li>
    <li><string>Coding4Fun.Kinect.Wpf NuGet Package</strong>: Install-Package Coding4Fun.Kinect.Wpf</li>
</ul>
<p>Update Configuration in App.Config</p>
<ul>
    <li><strong>Add Certificate PFX</strong>: You need to create an add the PFX file you're using to access the Service Management API. Export the PFX and add it to the solution.</li>
    <li><strong>Add Certificate Password</strong>: Add the PFX password.</li>
    <li><strong>Add Subscription Identifier</strong>: Add your subscription identifier.</li>
    <li><strong>Add Filter</strong> (optional): If you'd like to filter on a particular service name you can add it to the ServiceNameFilter.</li>
    <li><strong>Choose Slot Filter</strong> (optional): Choose "production", "staging", or "production;staging" to choose which services to add.</li>
</ul>
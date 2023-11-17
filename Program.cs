using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Services.Common;
using DevopsPermissionsADO.AdoObjects;

class Program
{
    static async Task Main()
    {
        string organization = ""; // Replace with your organization name
        string personalAccessToken = ""; // Replace with your PAT
        string userToQuery = ""; // The user email you want to query
        string queryNamespace = ""; //"ReleaseManagement";
        string projectToQuery = ""; //"eShop-Containers";
        string adoApiVersion = "7.1-preview.1";        

        // Create an instance of HttpClient
        using (var client = new HttpClient())
        {
            try
            {
                AdoUserResponse userJson;

                using (var userClient = new HttpClient())
                {
                    // Set the authorization header using the Personal Access Token (PAT)
                    userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}")));
                    // Set the base URL for Azure DevOps REST API
                    userClient.BaseAddress = new Uri($"https://vssps.dev.azure.com/{organization}/");

                    //Get information about the user
                    var userResponse = await userClient.GetAsync($"_apis/Identities?searchFilter=General&api-version=5.0-preview.1&filterValue={userToQuery}");
                    if (!userResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("User not found");
                        return;
                    }
                    userJson = JsonConvert.DeserializeObject<AdoUserResponse>(await userResponse.Content.ReadAsStringAsync());

                    if (userJson.Count == 0)
                    {
                        Console.WriteLine("User not found");
                        return;
                    }
                }

                // Set the authorization header using the Personal Access Token (PAT)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}")));
                // Set the base URL for Azure DevOps REST API
                client.BaseAddress = new Uri($"https://dev.azure.com/{organization}/");

                // Get a list of projects
                var projectsResponse = await client.GetAsync($"_apis/projects/?api-version={adoApiVersion}");
                projectsResponse.EnsureSuccessStatusCode();
                var projectsJson = JsonConvert.DeserializeObject<ProjectResponse>(await projectsResponse.Content.ReadAsStringAsync());

                Console.WriteLine("Permissions Report for User: " + userToQuery);

                foreach (var project in projectsJson.value)
                {
                    if(!string.IsNullOrEmpty(projectToQuery) && project.name != projectToQuery)
                    {
                        continue;
                    }

                    // Extract project ID from the project URL
                    string projectUrl = project.url;
                    string pattern = @"/_apis/projects/([a-f0-9-]+)";
                    Match match = Regex.Match(projectUrl, pattern);
                    if (match.Success)
                    {
                        string projectId = match.Groups[1].Value;

                        // Query permissions for the specific user in the project
                        var permissionsResponse = await client.GetAsync($"_apis/securitynamespaces?api-version={adoApiVersion}");
                        permissionsResponse.EnsureSuccessStatusCode();
                        var securityNamespacesJson = JsonConvert.DeserializeObject<NamespaceResponse>(await permissionsResponse.Content.ReadAsStringAsync());

                        Console.WriteLine($"\nProject: {project.name}");

                        foreach (var namespaceItem in securityNamespacesJson.Value)
                        {
                            var namespaceDisplayName = namespaceItem.DisplayName;
                            var namespaceName = namespaceItem.Name;

                            if (!string.IsNullOrEmpty(queryNamespace) && namespaceName != queryNamespace)
                            {
                                continue;
                            }

                            Console.WriteLine($"\nChecking permissions for : {namespaceItem.DisplayName}, project {project.name}");
                            using (var aclClient = new HttpClient())
                            {
                                // Set the authorization header using the Personal Access Token (PAT)
                                aclClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                                    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}")));
                                // Set the base URL for Azure DevOps REST API
                                aclClient.BaseAddress = new Uri($"https://dev.azure.com/{organization}/");

                                // Get a list of projects
                                var aclResponse = await aclClient.GetAsync($"_apis/accesscontrollists/{namespaceItem.NamespaceId}/?api-version={adoApiVersion}&includeExtendedInfo=true&token={project.id}&descriptors={userJson.Value[0].Descriptor.Replace("\\\\", "\\")}");
                                aclResponse.EnsureSuccessStatusCode();
                                var acl = JsonConvert.DeserializeObject<AclResponse>(await aclResponse.Content.ReadAsStringAsync());

                                if (namespaceItem.Actions.Count > 0)
                                {
                                    foreach (var action in namespaceItem.Actions)
                                    {
                                        var permissionBit = (int)action.Bit;
                                        var permissionStatus = "";
                                        // Extract the first entry in AcesDictionary for easier access and readability
                                        var firstAce = acl.Value[0].AcesDictionary.FirstOrDefault().Value;

                                        // Check if the permission bit is set in the effective deny permissions
                                        if ((action.Bit & firstAce.ExtendedInfo.EffectiveDeny) > 0)
                                        {
                                            // If the bit is not set in both effective deny and effective allow, it's "Not Set"
                                            if ((action.Bit & firstAce.ExtendedInfo.EffectiveAllow) >0 )
                                            {
                                                permissionStatus = "Not Set";
                                            }
                                            else
                                            {
                                                permissionStatus = $"Denied";
                                            }
                                        }
                                        else
                                        {
                                            // If the bit is not in effective deny, check if it's in allow
                                            if ((action.Bit & firstAce.ExtendedInfo.EffectiveAllow) > 0)
                                            {
                                                permissionStatus = "Allowed";
                                            }
                                            else
                                            {
                                                permissionStatus = "Not Set";
                                            }
                                        }

                                        Console.WriteLine($"Object: {action.DisplayName}, Status: {permissionStatus}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

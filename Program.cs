using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using DevopsPermissionsADO;
using Microsoft.VisualStudio.Services.Common;

class Program
{
    static async Task Main()
    {
        string organization = "MngEnvMCAP008355demo"; // Replace with your organization name
        string personalAccessToken = "pwpvwx5e4jt4hqrgvctnus2sdb6y7zfkvxdg5gynjpubbrnicidq"; // Replace with your PAT
        string userToQuery = "admin@MngEnvMCAP008355.onmicrosoft.com"; // The user you want to query
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
                }

                // Set the authorization header using the Personal Access Token (PAT)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}")));
                // Set the base URL for Azure DevOps REST API
                client.BaseAddress = new Uri($"https://dev.azure.com/{organization}/");

                // Get a list of projects
                var projectsResponse = await client.GetAsync($"_apis/projects/?api-version={adoApiVersion}");
                projectsResponse.EnsureSuccessStatusCode();
                var projectsJson = JsonConvert.DeserializeObject<ProjectListResponse>(await projectsResponse.Content.ReadAsStringAsync());

                Console.WriteLine("Permissions Report for User: " + userToQuery);

                foreach (var project in projectsJson.value)
                {
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
                        var securityNamespacesJson = await permissionsResponse.Content.ReadAsStringAsync();
                        var securityNamespaces = JObject.Parse(securityNamespacesJson);

                        Console.WriteLine($"Project: {project.name}");
                        Console.WriteLine("Permissions:");

                        foreach (var namespaceItem in securityNamespaces["value"])
                        {
                            var namespaceDisplayName = namespaceItem["displayName"].ToString();
                            var namespaceName = namespaceItem["name"].ToString();
                            //var permissions = GetNamespacePermissions(namespaceItem);

                            if (namespaceItem["actions"] is JArray actions)
                            {
                                using (var aclClient = new HttpClient())
                                {
                                    // Set the authorization header using the Personal Access Token (PAT)
                                    aclClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                                        Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}")));
                                    // Set the base URL for Azure DevOps REST API
                                    aclClient.BaseAddress = new Uri($"https://dev.azure.com/{organization}/");

                                    // Get a list of projects
                                    var aclResponse = await aclClient.GetAsync($"{{DevOpsOrg}}/_apis/accesscontrollists/{namespaceItem["namespaceId"]}/?api-version={adoApiVersion}&includeExtendedInfo=true&token={project.id}&descriptors={userJson.Value[0].Descriptor}");
                                    aclResponse.EnsureSuccessStatusCode();
                                    var acl = JsonConvert.DeserializeObject<AclResponse>(await aclResponse.Content.ReadAsStringAsync());

                                    foreach (var action in actions)
                                    {
                                        var permissionName = action["name"].ToString();
                                        var permissionBit = (int)action["bit"];
                                        var permissionStatus = permissionBit > 0 ? "Allowed" : "Denied";
                                        var permissionDisplayName = action["displayName"].ToString();
                                    }
                                }
                            }

                            //Console.WriteLine($"- Namespace: {namespaceName} {namespaceDisplayName}");
                            //if (permissions.Count > 0)
                            //{
                            //    foreach (var permission in permissions)
                            //    {
                            //        Console.WriteLine($"  - Permission: {permission.Name}, { permission.DisplayName}, Status: {permission.Status}");
                            //    }
                            //}
                            //else
                            //{
                            //    Console.WriteLine("  - No permissions found.");
                            //}
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

    // Classes to deserialize JSON responses
    class ProjectListResponse
    {
        public Project[] value { get; set; }
    }

    class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    class PermissionInfo
    {
        public string Name { get; set; }
        public string Status { get; set; }

        public string DisplayName { get; set; }
    }
}

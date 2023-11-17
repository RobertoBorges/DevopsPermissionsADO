// Classes to deserialize JSON responses
namespace DevopsPermissionsADO.AdoObjects
{
    class ProjectResponse
    {
        public Project[]? value { get; set; }
    }

    class Project
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
    }
}
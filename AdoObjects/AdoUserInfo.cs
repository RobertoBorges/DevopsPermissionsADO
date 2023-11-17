using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevopsPermissionsADO.AdoObjects
{
    public class AdoUserResponse
    {
        public int Count { get; set; }
        public List<AdoUser>? Value { get; set; }
    }

    public class AdoUser
    {
        public string? Id { get; set; }
        public string? Descriptor { get; set; }
        public string? SubjectDescriptor { get; set; }
        public string? ProviderDisplayName { get; set; }
        public bool IsActive { get; set; }
        public List<string>? Members { get; set; }
        public List<string>? MemberOf { get; set; }
        public List<string>? MemberIds { get; set; }
        public Dictionary<string, UserProperty>? Properties { get; set; }
        public int ResourceVersion { get; set; }
        public int MetaTypeId { get; set; }
    }

    public class UserProperty
    {
        public string Type { get; set; }
        public string Value { get; set; }

        // Custom constructor to help with JSON deserialization
        public UserProperty(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }

}


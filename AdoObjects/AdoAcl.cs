using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevopsPermissionsADO.AdoObjects
{
    public class AclResponse
    {
        public int Count { get; set; }
        public List<AclItem>? Value { get; set; }
    }

    public class AclItem
    {
        public bool InheritPermissions { get; set; }
        public string? Token { get; set; }
        public Dictionary<string, AccessControlEntry>? AcesDictionary { get; set; }
        public bool IncludeExtendedInfo { get; set; }
    }

    public class AccessControlEntry
    {
        public string? Descriptor { get; set; }
        public int Allow { get; set; }
        public int Deny { get; set; }
        public ExtendedInfo? ExtendedInfo { get; set; }
    }

    public class ExtendedInfo
    {
        public int EffectiveAllow { get; set; }
        public int EffectiveDeny { get; set; }
    }
}

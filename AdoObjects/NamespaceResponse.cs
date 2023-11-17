using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevopsPermissionsADO.AdoObjects
{
    public class NamespaceResponse
    {
        public int Count { get; set; }
        public List<NamespaceItem>? Value { get; set; }
    }

    public class NamespaceItem
    {
        public string? NamespaceId { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? SeparatorValue { get; set; }
        public int ElementLength { get; set; }
        public int WritePermission { get; set; }
        public int ReadPermission { get; set; }
        public string? DataspaceCategory { get; set; }
        public List<ActionItem>? Actions { get; set; }
        public int StructureValue { get; set; }
        public string? ExtensionType { get; set; }
        public bool IsRemotable { get; set; }
        public bool UseTokenTranslator { get; set; }
        public int SystemBitMask { get; set; }
    }

    public class ActionItem
    {
        public int Bit { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? NamespaceId { get; set; }
    }
}

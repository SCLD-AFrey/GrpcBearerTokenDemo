using System.Collections.Generic;
using System.Xml.Serialization;

namespace ConfigOS_Windows_Agent
{
    [XmlRoot(ElementName = "Group")]
    public class PolicyControl
    {
        [XmlElement(ElementName = "GroupId")] 
        public string GroupId { get; set; }

        [XmlElement(ElementName = "GroupTitle")]
        public string GroupTitle { get; set; }

        [XmlElement(ElementName = "RuleId")] 
        public string RuleId { get; set; }
        [XmlElement(ElementName = "Severity")] 
        public string Severity { get; set; }

        [XmlElement(ElementName = "RuleVersion")]
        public string RuleVersion { get; set; }

        [XmlElement(ElementName = "RuleTitle")]
        public string RuleTitle { get; set; }

        [XmlElement(ElementName = "Where")] 
        public string Where { get; set; }
        [XmlElement(ElementName = "Applied")] 
        public string Applied { get; set; }
        [XmlElement(ElementName = "Type")] 
        public string Type { get; set; }
        [XmlElement(ElementName = "Value")] 
        public string Value { get; set; }
        [XmlElement(ElementName = "Ignore")] 
        public string Ignore { get; set; }

        [XmlElement(ElementName = "IgnoreReason")]
        public string IgnoreReason { get; set; }
        
        public string OriginalValue { get; set; }
        public string PostRemediationValue { get; set; }
        public string PostGpoUpdateValue { get; set; }
        
        public bool HasGpoConflict => !PostRemediationValue.ToUpper().Equals(PostGpoUpdateValue.ToUpper());
    }

    [XmlRoot(ElementName = "Items")]
    public class PolicyItems
    {
        [XmlElement(ElementName = "Group")] 
        public List<PolicyControl> Controls { get; set; }
    }

    [XmlRoot(ElementName = "Groups")]
    public class PolicyContents
    {
        [XmlElement(ElementName = "Items")] 
        public PolicyItems Items { get; set; }

        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }

        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }

        [XmlAttribute(AttributeName = "CreateDate")]
        public string CreateDate { get; set; }

        [XmlAttribute(AttributeName = "AuthorName")]
        public string AuthorName { get; set; }

        [XmlAttribute(AttributeName = "OSType")]
        public string OsType { get; set; }

        [XmlAttribute(AttributeName = "Version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "SysArea")]
        public string SysArea { get; set; }
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static Frends.Salesforce.CreateSObject.Definitions.Enums;

namespace Frends.Salesforce.CreateSObject
{
    /// <summary>
    /// Input-class for CreateSObject-task.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Salesforce Domain.
        /// /services/data/v52.0/query will be added automatically, so the domain is enough.
        /// </summary>
        [DefaultValue(@"https://example.my.salesforce.com")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Domain { get; set; }

        /// <summary>
        /// SObject structure as json.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string SObjectAsJson { get; set; }

        /// <summary>
        /// SObject type. Can be Account, Case, CustomObject, CustomSettings, CustomMetadata, etc...
        /// </summary>
        [DefaultValue("Account")]
        [DisplayFormat(DataFormatString = "Text")]
        public string SObjectType { get; set; }
    }
}

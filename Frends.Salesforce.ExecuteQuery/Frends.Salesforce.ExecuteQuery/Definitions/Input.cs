using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Salesforce.ExecuteQuery
{
    /// <summary>
    /// Input-class for ExecuteQuery-task.
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
        /// Query which will be executed.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Query { get; set; }
    }
}

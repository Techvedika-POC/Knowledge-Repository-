using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowLedger_Synaptix.Models
{
    public partial class Attachment
    {
        /// <summary>
        /// Vector embedding for AI semantic search, in-memory only
        /// </summary>
        [NotMapped]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}

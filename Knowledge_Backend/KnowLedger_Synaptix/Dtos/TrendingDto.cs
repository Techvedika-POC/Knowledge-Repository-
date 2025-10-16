using System;

namespace KnowLedger_Synaptix.Dtos
{
    public class TrendingDto
    {
        public Guid ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Rank { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public string ContributorName { get; set; }
        public string ContributorAvatarUrl { get; set; }
        public string[] Tags { get; set; }
    }
}

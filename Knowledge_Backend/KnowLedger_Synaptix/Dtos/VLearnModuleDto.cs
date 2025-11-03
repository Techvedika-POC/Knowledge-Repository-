namespace KnowLedger_Synaptix.Dtos
{
    public class VLearnModuleDto
    {
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public string ContentLink { get; set; }
        public int? OrderNo { get; set; }
        public string Status { get; set; }   
        public string TestStatus { get; set; }  
        public bool IsLocked { get; set; } = true;
    }
}

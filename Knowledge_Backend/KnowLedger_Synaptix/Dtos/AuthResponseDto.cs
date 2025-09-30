namespace KnowLedger_Synaptix.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; }            
        public string Name { get; set; }             
        public string Email { get; set; }             
        public List<string> Roles { get; set; }
    }
}

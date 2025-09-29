namespace KnowLedger_Synaptix.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; }             // JWT token
        public string Name { get; set; }              // User full name
        public string Email { get; set; }             // User email
        public List<string> Roles { get; set; }
    }
}
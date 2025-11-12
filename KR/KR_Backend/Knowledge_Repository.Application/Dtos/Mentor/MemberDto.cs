using System;

namespace Knowledge_Repository.Application.Dtos.Mentor
{
    public class MemberDto
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}

// File: Knowledge_Repository.Application.Dtos/LessonDto.cs
using System;

namespace Knowledge_Repository.Application.Dtos
{
    public class LessonDto
    {
        public Guid LessonId { get; set; }
        public Guid ModuleId { get; set; }          
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string LessonType { get; set; } = "Video"; 
        public int OrderIndex { get; set; } = 0;
        public string Overview { get; set; } = string.Empty;
        public string Prerequisites { get; set; } = string.Empty;
        public int? DurationMinutes { get; set; }
        public bool IsCompleted { get; set; }
    }
}

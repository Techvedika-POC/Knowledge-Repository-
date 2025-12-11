using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Knowledge_Repository.Application.Dtos
{
    public class KnowledgeItemUpdateDto
    {
        public string? Title { get; set; }                   
        public string? Description { get; set; }             
        public Guid? DomainId { get; set; }                  
        public Guid? CategoryId { get; set; }               
        public string? Status { get; set; }                  
        public List<string>? Language { get; set; } = new(); 
        public List<string>? Framework { get; set; } = new();
        public string? Visibility { get; set; } = "Public"; 

        public List<string>? Tags { get; set; } = new();  
        public List<FileAttachmentDto>? Attachments { get; set; } = new();
        public string? KnowledgeText { get; set; }           
        public List<float>? Embedding { get; set; }          
        public int? CurrentVersion { get; set; }
        public string? ChangesSummary { get; set; }          
        public bool ReplaceAttachments { get; set; } = false; 
    }
}


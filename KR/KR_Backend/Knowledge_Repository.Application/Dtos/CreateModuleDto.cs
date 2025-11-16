using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Dtos
{
   public class CreateModuleDto
    {
        public string ModuleName { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? ContentLink { get; init; }
        public int OrderNo { get; init; }
    }
}

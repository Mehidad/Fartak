using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Application.Role.Queries.DTO
{
    public class RoleDto
    {
        public long Id { get; set; }
        public string? Title { get; set; } = null!;
        public string? Code { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SGCM.Domain.Entities
{
    public class Specialty
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Relationship: One specialty has many doctors
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}

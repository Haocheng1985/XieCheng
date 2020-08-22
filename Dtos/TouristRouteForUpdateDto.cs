using MyFakexiecheng.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Dtos
{
    public class TouristRouteForUpdateDto:TouristRouteForManipulationDto
    {
       
        [Required(ErrorMessage ="must have for update")]
        [MaxLength(1500)]
        public override string Description { get; set; }//father func must be virtual
        
    }
}

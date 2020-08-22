using MyFakexiecheng.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Dtos
{
    [TouristRouteTitleMustBeDifferentFromDescriptionAttribute]
    public class TouristRouteForCreationDto//:IValidatableObject
    {
        
        [Required(ErrorMessage = "title is null")]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1500)]
        public string Description { get; set; }
        // OriginalPrice*DiscountPresenthttps://localhost:3001/api/TouristRoutes/
        public decimal Price { get; set; }
        //public decimal OriginalPrice { get; set; }
        //public double? DiscountPresent { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public string Features { get; set; }
        public string Fees { get; set; }
        public string Notes { get; set; }
        public double? Rating { get; set; }
        public string TravelDays { get; set; }
        public string TripType { get; set; }
        public string DepartureCity { get; set; }
        public ICollection<TouristRoutePictureForCreationDto> TouristRoutePictures { get; set; }// 创建父子资源
            = new List<TouristRoutePictureForCreationDto>();

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Title == Description)
        //    {
        //        yield return new ValidationResult(
        //            "路线名称必须与路线描述不同",
        //            new[] { "TouristRouteForCreationDto" }//抛出异常的路径，函数名
        //        );
        //    }
        //}
    }
}

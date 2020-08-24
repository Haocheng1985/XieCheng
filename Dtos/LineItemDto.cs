using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Dtos
{
    public class LineItemDto
    {
        public int Id { get; set; }
        public Guid TouristRouteId { get; set; }//12-2 6:30
        public TouristRouteDto TouristRoute { get; set; }//automapper profile
        public Guid? ShoppingCartId { get; set; }
        //public Guid? OrderId { get; set; }
        public decimal OriginalPrice { get; set; }
        public double? DiscountPresent { get; set; }
    }
}

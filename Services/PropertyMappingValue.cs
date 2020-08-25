using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Services
{
    public class PropertyMappingValue
    {
        public IEnumerable<string> DestinationProperties { get; private set; }//将被映射的目标属性
        public PropertyMappingValue(IEnumerable<string> destinationProperties)
        {
            DestinationProperties = destinationProperties;
        }
    }
}

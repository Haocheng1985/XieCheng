using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Services
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
        bool IsMappingExists<TSource, TDestination>(string fields);
    }
}

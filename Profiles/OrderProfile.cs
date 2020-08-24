using AutoMapper;
using MyFakexiecheng.Dtos;
using MyFakexiecheng.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(
                    dest => dest.State,
                    opt =>
                    {
                        opt.MapFrom(src => src.State.ToString());//枚举类型无法自动映射，需要手动配置
                    }
                );
        }
    }
}

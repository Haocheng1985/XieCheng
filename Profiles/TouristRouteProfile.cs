﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MyFakexiecheng.Dtos;
using MyFakexiecheng.Models;

namespace MyFakexiecheng.Profiles
{
    public class TouristRouteProfile : Profile
    {
        public TouristRouteProfile() 
        {
            CreateMap<TouristRoute, TouristRouteDto>()
                .ForMember(
                    dest => dest.Price,
                    opt => opt.MapFrom(src => src.OriginalPrice * (decimal)(src.DiscountPresent ?? 1))
                            )
                .ForMember(
                    dest=>dest.TravelDays,
                    opt=>opt.MapFrom(src=>src.TravelDays.ToString())
                )
                .ForMember(
                    dest => dest.TripType,
                    opt => opt.MapFrom(src => src.TripType.ToString())
                )
                .ForMember(
                    dest => dest.DepartureCity,
                    opt => opt.MapFrom(src => src.DepartureCity.ToString())
                );
        
        
        }
    }
}

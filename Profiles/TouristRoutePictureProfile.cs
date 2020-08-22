using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MyFakexiecheng.Dtos;
using MyFakexiecheng.Models;

namespace MyFakexiecheng.Profiles
{
    public class TouristRoutePictureProfile:Profile
    {
        public TouristRoutePictureProfile()
        {
            CreateMap<TouristRoutePicture, TouristRoutePictureDto>();
            CreateMap<TouristRoutePictureForCreationDto, TouristRoutePicture>();
        }

    }
}

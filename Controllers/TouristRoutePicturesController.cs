using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyFakexiecheng.Dtos;
using MyFakexiecheng.Models;
using MyFakexiecheng.Services;

namespace MyFakexiecheng.Controllers
{
    [Route("/api/touristRoutes/{touristRouteId}/pictures")]
    [ApiController]
    public class TouristRoutePicturesController:ControllerBase
    {
        private ITouristRouteRepository _touristRouteRepository;
        private IMapper _mapper;
        public TouristRoutePicturesController(ITouristRouteRepository touristRouteRepository, IMapper mapper)
        {
            _touristRouteRepository = touristRouteRepository??
                throw new ArgumentException(nameof(touristRouteRepository));
            _mapper = mapper??
                throw new ArgumentException(nameof(mapper)); 
        }
        [HttpGet]
        public IActionResult GetPictureListForTouristRoute(Guid touristRouteId)
        {
            if (!_touristRouteRepository.TouristRouteExists(touristRouteId))
            {
                return NotFound("route not found");
            }
            var picturesFromRepo = _touristRouteRepository.GetPicturesByTouristRouteId(touristRouteId);

            if (picturesFromRepo == null || picturesFromRepo.Count() <= 0)
            {
                return NotFound("pictures not found");
            }

            return Ok(_mapper.Map<IEnumerable<TouristRoutePictureDto>>(picturesFromRepo));
            
        }

        [HttpGet("{pictureId}",Name = "GetPicture")]
        public IActionResult GetPicture(Guid touristRouteId, int pictureId)
        {
            if (!_touristRouteRepository.TouristRouteExists(touristRouteId))
            {
                return NotFound("route not found");
            }
            var pictureFromRepo = _touristRouteRepository.GetPicture(pictureId);
            if(pictureFromRepo==null)
            {
                return NotFound("picture not found!");

            }

            return Ok(_mapper.Map<TouristRoutePictureDto>(pictureFromRepo));

        }

        [HttpPost]
        public IActionResult CreateTouristRoutePicture(
            [FromRoute] Guid touristRouteId,
            [FromBody] TouristRoutePictureForCreationDto touristRoutePictureForCreationDto
        )
        {
            if (!_touristRouteRepository.TouristRouteExists(touristRouteId))
            {
                return NotFound("route not exits");
            }

            var pictureModel = _mapper.Map<TouristRoutePicture>(touristRoutePictureForCreationDto);
            _touristRouteRepository.AddTouristRoutePicture(touristRouteId, pictureModel);
            _touristRouteRepository.Save();
            var pictureToReturn = _mapper.Map<TouristRoutePictureDto>(pictureModel);
            return CreatedAtRoute(
                "GetPicture",
                new
                {
                    touristRouteId = pictureModel.TouristRouteId,
                    pictureId = pictureModel.Id
                },
                pictureToReturn
            );
        }
    }
}

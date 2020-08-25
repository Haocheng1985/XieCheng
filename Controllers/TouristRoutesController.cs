using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyFakexiecheng.Dtos;
using MyFakexiecheng.Services;
using AutoMapper;
using System.Text.RegularExpressions;
using FakeXiecheng.API.ResourceParameters;
using MyFakexiecheng.Models;
using Microsoft.AspNetCore.JsonPatch;
using MyFakexiecheng.Helper;
using Microsoft.AspNetCore.Authorization;
using MyFakexiecheng.ResourceParameters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace MyFakexiecheng.Controllers
{
    [Route("api/[controller]")]// api/touritsroutes
    [ApiController]
    public class TouristRoutesController : ControllerBase//Controller(controller more functions, support view)
    {
        private ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingService _propertyMappingService;
        public TouristRoutesController(ITouristRouteRepository touristRouteRepository,
            IMapper mapper,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IPropertyMappingService propertyMappingService
            )
        {
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _propertyMappingService = propertyMappingService;
        }

        private string GenerateTouristRouteResourceURL(
            TouristRouteResourceParamaters paramaters,
            PaginationResourceParamaters paramaters2,
            ResourceUriType type
        )
        {
            return type switch
            {
                ResourceUriType.PreviousPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber - 1,
                        pageSize = paramaters2.PageSize
                    }),
                ResourceUriType.NextPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber + 1,
                        pageSize = paramaters2.PageSize
                    }),
                _ => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber,
                        pageSize = paramaters2.PageSize
                    })
            };
        }

        //api/touristroutes?keyword=......
        [HttpGet(Name = "GetTouristRoutes")]
        [HttpHead]
        public async Task<IActionResult> GetTouristRoutes(
            [FromQuery] TouristRouteResourceParamaters paramaters,
            [FromQuery] PaginationResourceParamaters paramaters2
            //[FromQuery] string keyword,
            //string rating//lessthan,largerthan,equalto,lessthan3,largethan2,equalto1
            )//frombody vs fromQuery(could be omitted when use apicontroller,for better understand, should not omit),
                                                                        //when keyword is not same, [FromQuery(name="")]
        {
            //Regex regex = new Regex(@"([A-Za-z0-9\-]+)(\d+)");
            //string operatorType = "";
            //int ratingValue = -1;
            //Match match = regex.Match(paramaters.Rating);
            //if (match.Success)
            //{
            //    operatorType = match.Groups[1].Value;
            //    ratingValue = Int32.Parse(match.Groups[2].Value);
            //}
            if (!_propertyMappingService
                .IsMappingExists<TouristRouteDto, TouristRoute>(
                paramaters.OrderBy))
            {
                return BadRequest("请输入正确的排序参数");
            }
            if (!_propertyMappingService
                .IsPropertiesExists<TouristRouteDto>(paramaters.Fields))
            {
                return BadRequest("请输入正确的塑性参数");
            }

            var touristRoutesFromRepo =await _touristRouteRepository
                .GetTouristRoutesAsync(paramaters.Keyword, 
                paramaters.RatingOperator, 
                paramaters.RatingValue,
                paramaters2.PageSize,
                paramaters2.PageNumber,
                paramaters.OrderBy
                );//在这里异步等待，在这里挂起，直到数据库返回数据10-3 5：30
            if(touristRoutesFromRepo==null|| touristRoutesFromRepo.Count()<=0)
            {
                return NotFound("no routes found!");
            }
            var touristRouteDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);

            var previousPageLink = touristRoutesFromRepo.HasPrevious
                ? GenerateTouristRouteResourceURL(
                    paramaters, paramaters2, ResourceUriType.PreviousPage)//数据过滤参数，分页参数，url类型
                : null;

            var nextPageLink = touristRoutesFromRepo.HasNext
                ? GenerateTouristRouteResourceURL(
                    paramaters, paramaters2, ResourceUriType.NextPage)
                : null;

            // x-pagination
            var paginationMetadata = new
            {
                previousPageLink,
                nextPageLink,
                totalCount = touristRoutesFromRepo.TotalCount,
                pageSize = touristRoutesFromRepo.PageSize,
                currentPage = touristRoutesFromRepo.CurrentPage,
                totalPages = touristRoutesFromRepo.TotalPages
            };

            Response.Headers.Add("x-pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));


            return Ok(touristRouteDto.ShapeData(paramaters.Fields));
        }

        // api/tourisrtoutes/{touristRouteId}
        [HttpGet("{touristRouteId}",Name = "GetTouristRouteById")]// limit guid {touristRouteId:guid}
        [HttpHead("{touristRouteId}")]
        public async Task<IActionResult> GetTouristRouteById(Guid touristRouteId, string fields)
        {

            var touristRouteFromRepo =await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            if (touristRouteFromRepo == null)
            {
                return NotFound($"route {touristRouteId} not found!");
            }
            //var touristRouteDto = new TouristRouteDto()
            //{
            //    Id = touristRouteFromRepo.Id,
            //    Title = touristRouteFromRepo.Title,
            //    Description = touristRouteFromRepo.Description,
            //    Price = touristRouteFromRepo.OriginalPrice * (decimal)(touristRouteFromRepo.DiscountPresent ?? 1),
            //    CreateTime = touristRouteFromRepo.CreateTime,
            //    UpdateTime = touristRouteFromRepo.UpdateTime,
            //    Features = touristRouteFromRepo.Features,
            //    Fees = touristRouteFromRepo.Fees,
            //    Notes = touristRouteFromRepo.Notes,
            //    Rating = touristRouteFromRepo.Rating,
            //    TravelDays = touristRouteFromRepo.TravelDays.ToString(),
            //    TripType = touristRouteFromRepo.TripType.ToString(),
            //    DepartureCity = touristRouteFromRepo.DepartureCity.ToString()
            //};
            var touristRouteDto = _mapper.Map<TouristRouteDto>(touristRouteFromRepo);
            return Ok(touristRouteDto.ShapeData(fields));
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> CreateTouristRoute([FromBody] TouristRouteForCreationDto touristRouteForCreationDto)//deserialize input info. DTO format ref to TouristRouteDto
        {
            var touristRouteModel = _mapper.Map<TouristRoute>(touristRouteForCreationDto);//dto map to model.<target map model>(map data)
            _touristRouteRepository.AddTouristRoute(touristRouteModel);
            await _touristRouteRepository.SaveAsync();
            var touristRouteToReture = _mapper.Map<TouristRouteDto>(touristRouteModel);
            return CreatedAtRoute(
                "GetTouristRouteById",//route api
                new { touristRouteId = touristRouteToReture.Id },//api 路径参数
                touristRouteToReture
            );
        }
        [HttpPut("{touristRouteId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTouristRoute(
            [FromRoute] Guid touristRouteId,
            [FromBody] TouristRouteForUpdateDto touristRouteForUpdateDto
        )
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("旅游路线找不到");
            }

            var touristRouteFromRepo =await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            // 1. map dto
            // 2. update dto
            // 3. map model
            _mapper.Map(touristRouteForUpdateDto, touristRouteFromRepo);//update profile

            await _touristRouteRepository.SaveAsync();//8-2 07:00    

            return NoContent();
        }

        [HttpPatch("{touristRouteId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PartiallyUpdateTouristRoute(
            [FromRoute] Guid touristRouteId,
            [FromBody] JsonPatchDocument<TouristRouteForUpdateDto> patchDocument//JsonPatchDocument framework
        )
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("route not found");
            }

            var touristRouteFromRepo =await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            var touristRouteToPatch = _mapper.Map<TouristRouteForUpdateDto>(touristRouteFromRepo);
            //patchDocument.ApplyTo(touristRouteToPatch);

            patchDocument.ApplyTo(touristRouteToPatch, ModelState);//modelstate bind to touristRouteToPatch 8-6 3:10
            if (!TryValidateModel(touristRouteToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(touristRouteToPatch, touristRouteFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{touristRouteId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTouristRoute([FromRoute] Guid touristRouteId)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("route not found");
            }

            var touristRoute =await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            _touristRouteRepository.DeleteTouristRoute(touristRoute);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("({touristIDs})")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteByIDs(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))][FromRoute] IEnumerable<Guid> touristIDs)//string to GUID list
        {
            if (touristIDs == null)
            {
                return BadRequest();
            }

            var touristRoutesFromRepo =await _touristRouteRepository.GetTouristRoutesByIDListAsync(touristIDs);
            _touristRouteRepository.DeleteTouristRoutes(touristRoutesFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }


    }
}

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
using Microsoft.Net.Http.Headers;
using System.Dynamic;

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

        // 1. application/json -> 旅游路线资源
        // 2. application/vnd.aleks.hateoas+json
        // 3. application/vnd.aleks.touristRoute.simplify+json -> 输出简化版资源数据
        // 4. application/vnd.aleks.touristRoute.simplify.hateoas+json -> 输出简化版hateoas超媒体资源数据

        [Produces(
            "application/json",
            "application/vnd.aleks.hateoas+json",
            "application/vnd.aleks.touristRoute.simplify+json",
            "application/vnd.aleks.touristRoute.simplify.hateoas+json"
            )]
        [HttpGet(Name = "GetTouristRoutes")]
        [HttpHead]
        public async Task<IActionResult> GetTouristRoutes(
            [FromQuery] TouristRouteResourceParamaters paramaters,
            [FromQuery] PaginationResourceParamaters paramaters2,
            [FromHeader(Name = "Accept")] string mediaType
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
            if (!MediaTypeHeaderValue
                .TryParse(mediaType, out MediaTypeHeaderValue parsedMediatype))
            {
                return BadRequest();
            }

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

            bool isHateoas = parsedMediatype.SubTypeWithoutSuffix//忽略返回类型
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            var primaryMediaType = isHateoas
                ? parsedMediatype.SubTypeWithoutSuffix
                    .Substring(0, parsedMediatype.SubTypeWithoutSuffix.Length - 8)
                : parsedMediatype.SubTypeWithoutSuffix;


            //var touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);

            //var shapedDtoList = touristRoutesDto.ShapeData(paramaters.Fields);

            IEnumerable<object> touristRoutesDto;
            IEnumerable<ExpandoObject> shapedDtoList;

            if (primaryMediaType == "vnd.aleks.touristRoute.simplify")
            {
                touristRoutesDto = _mapper
                    .Map<IEnumerable<TouristRouteSimplifyDto>>(touristRoutesFromRepo);

                shapedDtoList = ((IEnumerable<TouristRouteSimplifyDto>)touristRoutesDto)
                    .ShapeData(paramaters.Fields);
            }
            else
            {
                touristRoutesDto = _mapper
                    .Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
                shapedDtoList =
                    ((IEnumerable<TouristRouteDto>)touristRoutesDto)
                    .ShapeData(paramaters.Fields);
            }


            if (isHateoas)
            {
                var linkDto = CreateLinksForTouristRouteList(paramaters, paramaters2);

                var shapedDtoWithLinklist = shapedDtoList.Select(t =>//js 的map方法
                {
                    var touristRouteDictionary = t as IDictionary<string, object>;
                    var links = CreateLinkForTouristRoute(
                        (Guid)touristRouteDictionary["Id"], null);
                    touristRouteDictionary.Add("links", links);
                    return touristRouteDictionary;
                });

                var result = new
                {
                    value = shapedDtoWithLinklist,
                    links = linkDto
                };
                return Ok(result);
            }
            //return Ok(touristRouteDto.ShapeData(paramaters.Fields));
            return Ok(shapedDtoList);
        }

        private IEnumerable<LinkDto> CreateLinksForTouristRouteList(
            TouristRouteResourceParamaters paramaters,
            PaginationResourceParamaters paramaters2)
        {
            var links = new List<LinkDto>();
            // 添加self，自我链接
            links.Add(new LinkDto(
                    GenerateTouristRouteResourceURL(
                        paramaters, paramaters2, ResourceUriType.CurrnetPage),
                    "self",
                    "GET"
                ));

            // "api/touristRoutes"
            // 添加创建旅游路线
            links.Add(new LinkDto(
                    Url.Link("CreateTouristRoute", null),
                    "create_tourist_route",
                    "POST"
                ));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinkForTouristRoute(
            Guid touristRouteId,
            string fields)
        {
            var links = new List<LinkDto>();

            links.Add(
                new LinkDto(
                    Url.Link("GetTouristRouteById", new { touristRouteId, fields }),
                    "self",
                    "GET"
                    )
                );

            // 更新
            links.Add(
                new LinkDto(
                    Url.Link("UpdateTouristRoute", new { touristRouteId }),
                    "update",
                    "PUT"
                    )
                );

            // 局部更新 
            links.Add(
                new LinkDto(
                    Url.Link("PartiallyUpdateTouristRoute", new { touristRouteId }),
                    "partially_update",
                    "PATCH")
                );

            // 删除
            links.Add(
                new LinkDto(
                    Url.Link("DeleteTouristRoute", new { touristRouteId }),
                    "delete",
                    "DELETE")
                );

            // 获取路线图片
            links.Add(
                new LinkDto(
                    Url.Link("GetPictureListForTouristRouteAsync", new { touristRouteId }),
                    "get_pictures",
                    "GET")
                );

            // 添加新图片
            links.Add(
                new LinkDto(
                    Url.Link("CreateTouristRoutePicture", new { touristRouteId }),
                    "create_picture",
                    "POST")
                );

            return links;
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
            //return Ok(touristRouteDto.ShapeData(fields));//输出的是dto和link组成的数据

            var linkDtos = CreateLinkForTouristRoute(touristRouteId, fields);

            var result = touristRouteDto.ShapeData(fields)
                as IDictionary<string, object>;  //expandobject底层实现是字典类型,可以转换为字典类型
            result.Add("links", linkDtos);

            return Ok(result);


        }

        [HttpPost(Name = "CreateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        //[Authorize(Roles ="Admin")]
        public async Task<IActionResult> CreateTouristRoute([FromBody] TouristRouteForCreationDto touristRouteForCreationDto)//deserialize input info. DTO format ref to TouristRouteDto
        {
            var touristRouteModel = _mapper.Map<TouristRoute>(touristRouteForCreationDto);//dto map to model.<target map model>(map data)
            _touristRouteRepository.AddTouristRoute(touristRouteModel);
            await _touristRouteRepository.SaveAsync();
            var touristRouteToReture = _mapper.Map<TouristRouteDto>(touristRouteModel);

            var links = CreateLinkForTouristRoute(touristRouteModel.Id, null);

            var result = touristRouteToReture.ShapeData(null)
                as IDictionary<string, object>;

            result.Add("links", links);

            return CreatedAtRoute(
                "GetTouristRouteById",
                new { touristRouteId = result["Id"] },//与相应对象保持一致
                result
            );

            //return CreatedAtRoute(
            //    "GetTouristRouteById",//route api
            //    new { touristRouteId = touristRouteToReture.Id },//api 路径参数
            //    touristRouteToReture
            //);
        }


        [HttpPut("{touristRouteId}",Name = "UpdateTouristRoute")]
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

        [HttpPatch("{touristRouteId}",Name = "PartiallyUpdateTouristRoute")]
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

        [HttpDelete("{touristRouteId}",Name = "DeleteTouristRoute")]
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

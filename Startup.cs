﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyFakexiecheng.Database;
using MyFakexiecheng.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MyFakexiecheng.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace MyFakexiecheng
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var secretByte = Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]);
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["Authentication:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = Configuration["Authentication:Audience"],

                        ValidateLifetime = true,

                        IssuerSigningKey = new SymmetricSecurityKey(secretByte)
                    };
                });
            services.AddControllers(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;//default false,return json，ignore other format including input & output
            })
            .AddNewtonsoftJson(setupAction => {
                setupAction.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            })
            .AddXmlDataContractSerializerFormatters()
            .ConfigureApiBehaviorOptions(setupAction =>
            {
                setupAction.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetail = new ValidationProblemDetails(context.ModelState)
                    {
                        Type = "type",
                        Title = "wrong data type",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Detail = "detail",
                        Instance = context.HttpContext.Request.Path
                    };
                    problemDetail.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                    return new UnprocessableEntityObjectResult(problemDetail)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            })
            ; // 


                services.AddTransient<ITouristRouteRepository, TouristRouteRepository>();
            //services.AddSingleton
            //services.AddScoped

            services.AddDbContext<AppDbContext>(options => {
                options.UseSqlServer(Configuration["DbContext:ConnectionString"]);
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());//scan profile 
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();


            services.Configure<MvcOptions>(config =>
            {
                var outputFormatter = config.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

                if (outputFormatter != null)
                {
                    outputFormatter.SupportedMediaTypes
                    .Add("application/vnd.aleks.hateoas+json");
                }
            });//全局添加自定义媒体类型

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //where are you
            app.UseRouting();
            //who are u
            app.UseAuthentication();
            //what u can do
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});
                endpoints.MapControllers();
            });
        }
    }
}

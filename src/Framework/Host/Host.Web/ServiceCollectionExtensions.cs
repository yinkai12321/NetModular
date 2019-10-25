﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NetModular.Lib.Auth.Jwt;
using NetModular.Lib.Cache.Integration;
using NetModular.Lib.Data.Integration;
using NetModular.Lib.Mapper.AutoMapper;
using NetModular.Lib.Module.AspNetCore;
using NetModular.Lib.Swagger.Core;
using NetModular.Lib.Swagger.Core.Conventions;
using NetModular.Lib.Utils.Core;
using NetModular.Lib.Utils.Mvc;
using NetModular.Lib.Validation.FluentValidation;
using HostOptions = NetModular.Lib.Host.Web.Options.HostOptions;

namespace NetModular.Lib.Host.Web
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加WebHost
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostOptions"></param>
        /// <param name="env">环境</param>
        /// <returns></returns>
        public static IServiceCollection AddWebHost(this IServiceCollection services, HostOptions hostOptions, IHostingEnvironment env)
        {
            services.AddSingleton(hostOptions);

            services.AddUtils();

            services.AddUtilsMvc();

            //加载模块
            var modules = services.AddModules(env.EnvironmentName);

            //添加对象映射
            services.AddMappers(modules);

            //添加缓存
            services.AddCache(env.EnvironmentName);

            //主动或者开发模式下开启Swagger
            if (hostOptions.Swagger || env.IsDevelopment())
            {
                services.AddSwagger(modules);
            }

            //Jwt身份认证
            services.AddJwtAuth(env.EnvironmentName);

            //添加MVC功能
            services.AddMvc(c =>
            {
                if (hostOptions.Swagger || env.IsDevelopment())
                {
                    //API分组约定
                    c.Conventions.Add(new ApiExplorerGroupConvention());
                }

                //模块中的MVC配置
                foreach (var module in modules)
                {
                    ((ModuleDescriptor)module).Initializer?.ConfigureMvc(c);
                }

            })
            .AddJsonOptions(options =>
            {
                //设置日期格式化格式
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            })
            .AddValidators(services)//添加验证器
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //CORS
            services.AddCors(options =>
            {
                options.AddPolicy("Default",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition"));//下载文件时，文件名称会保存在headers的Content-Disposition属性里面
            });

            //添加数据库，数据库依赖ILoginInfo，所以需要在添加身份认证以及MVC后添加数据库
            services.AddDb(env.EnvironmentName, modules);

            //解决Multipart body length limit 134217728 exceeded
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });

            //添加模块的自定义服务
            services.AddModuleServices(modules);

            return services;
        }
    }
}

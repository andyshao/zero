using AutoMapper;
using Fireasy.Common.Caching;
using Fireasy.Common.Serialization;
using Fireasy.Common.Subscribes;
using Fireasy.Data;
using Fireasy.Data.Entity;
using Fireasy.Data.Extensions;
using Fireasy.Web.Mvc;
using Fireasy.Zero.Dtos;
using Fireasy.Zero.Helpers;
using Fireasy.Zero.Infrastructure;
using Fireasy.Zero.Models;
using Fireasy.Zero.Services;
using Fireasy.Zero.Services.Impls;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Zero.AspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            SerializeOption.GlobalConverters.Add(new LightEntityJsonConverter());

            services.AddFireasy(Configuration)
                .AddIoc(); //��� appsettings.json ��� ioc ����

            // ############################ ��ʾ����ʵ�������ĵ����� ############################
            services.AddEntityContext(builder =>
                {
                    builder.Options.NotifyEvents = true; //������Ϊ true ʱ, �����ʵ��־û�����֪ͨ�Żᴥ��
                });

            // mongodb ��������¼��־��
            services.AddEntityContext<MongodbContext>(builder =>
                {
                    builder.Options.ConfigName = "mongodb"; //ָ�������ļ��е�ʵ������
                    //builder.UseMongoDB("server=mongodb://192.168.1.106;database=test"); //ָ�����Ӵ�
                });

            // ############################ ��ʾ�־û��¼����� ############################
            services.AddPersistentSubscriber<BaseEntitySubscriber>();
            services.AddAsyncPersistentSubscriber<AsyncBaseEntitySubscriber>();

            // ############################ ��ʾ��Ϣ���еĶ����뷢�� ############################
            // ������������Ҫע�͵�����һ��
            // redis����
            services.AddRedisSubscriber(options =>
                {
                    options.ConfigName = "redis"; //ͨ��ָ����������
                    //options.Hosts = "localhost";
                    options.Initializer = s => s.AddSubscriber<CommandLogSubject>(d =>
                        {
                            Console.ForegroundColor = d.Level == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                            Console.WriteLine("========= ���� redis ����Ϣ֪ͨ =========");
                            if (d.Level == 0)
                            {
                                Console.WriteLine($"��ʱ��{d.Period} ����");
                            }
                            Console.WriteLine(d.CommandText);
                            Console.ResetColor();
                        });
                });

            // rabbitmq����
            //services.AddRabbitMQSubscriber(options =>
            //    {
            //        options.ConfigName = "rabbit"; //ͨ��ָ����������
            //        //options.Server = "amqp://127.0.0.1:5672";
            //        //options.UserName = "guest";
            //        //options.Password = "123";
            //        options.Initializer = s => s.AddSubscriber<CommandLogSubject>(d =>
            //            {
            //                Console.ForegroundColor = d.Type == 0 ? ConsoleColor.Green : ConsoleColor.Red;
            //                Console.WriteLine("========= ���� rabbitmq ����Ϣ֪ͨ =========");
            //                if (d.Level == 0)
            //                {
            //                    Console.WriteLine($"��ʱ��{d.Period} ����");
            //                }
            //                Console.WriteLine(d.CommandText);
            //                Console.ResetColor();
            //            });
            //    });

            // �ı����ݿ����sql��־���٣���� mq ��������־����
            // ������ MQCommandTracker ����Ϣ���͵� mq��mq����������ô��ֱ�ʹ�� redis �� rabbitmq ��������Ϣ��ʾ
            services.AddTransient<ICommandTracker, MQCommandTracker>();

            // ############################ ��ʾʹ��ʵ��־û��¼����� ############################
            //EntitySubscribeManager.AddSubscriber(subject => new BaseEntitySubscriber().Accept(subject));
            //EntitySubscribeManager.AddAsyncSubscriber(subject => new AsyncBaseEntitySubscriber().AcceptAsync(subject));

            // ############################ ��ʾʹ�õ���������־��� ############################
            // NLog��־
            //services.AddNLogger();

            // Log4net��־
            //services.AddLog4netLogger();

            // ############################ ��ʾʹ�õ����������������� ############################
            // ʹ�� Quartz ���ȹ�����
            //services.AddQuartzScheduler();

            services.AddAutoMapper(s =>
                {
                    s.AddProfile<AutoProfile>();
                });

            services.AddMvc()
                .AddSessionStateTempDataProvider()
                .ConfigureFireasyMvc(options =>
                    {
                        options.UseErrorHandleFilter = false;
                        options.JsonSerializeOption.Converters.Add(new LightEntityJsonConverter());
                    })
                .ConfigureEasyUI();

            // ############################ ��ʾSession�Զ����� ############################
            services.AddSession()
                .AddSessionRevive<SessionReviveNotification>();

            // ############################ ��ʾʹ�û�����淶������������դ�� ############################
            services.AddSingleton<ICacheKeyNormalizer, CacheKeyNormalizer>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                    {
                        options.LoginPath = new PathString("/login");
                    });

            services.AddOData();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //��Ӿ�̬�ļ�ӳ��
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseSession();
            app.UseSessionRevive();

            var builder = new ODataConventionModelBuilder(app.ApplicationServices);
            builder.EntitySet<SysOrg>("Orgs");
            builder.EntitySet<SysUser>("Users");

            app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "areas",
                        template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");

                    routes.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                    routes.MapODataServiceRoute("ODataRoute", "odata", builder.GetEdmModel());
                    routes.EnableDependencyInjection();
                });
        }
    }

    /// <summary>
    /// Session ����ʱ�Զ����� <see cref="SessionContext"/> ����
    /// </summary>
    public class SessionReviveNotification : ISessionReviveNotification
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var adminService = context.RequestServices.GetRequiredService<IAdminService>();
            var userId = context.GetIdentity();
            if (userId != 0)
            {
                var user = await adminService.GetUserAsync(userId);
                if (user != null)
                {
                    var session = new SessionContext { UserID = userId, UserName = user.Name, OrgID = user.OrgID };
                    context.SetSession(session);
                }
            }
        }
    }

    /// <summary>
    /// ʹ�� mq ��������־��
    /// </summary>
    public class MQCommandTracker : ICommandTracker
    {
        private readonly ISubscribeManager subMgr;

        public MQCommandTracker(ISubscribeManager subMgr)
        {
            this.subMgr = subMgr;
        }

        public void Write(IDbCommand command, TimeSpan period)
        {
            subMgr.Publish(new CommandLogSubject { Level = 0, CommandText = command.Output(), Period = period.TotalMilliseconds });
        }

        public async Task WriteAsync(IDbCommand command, TimeSpan period, CancellationToken cancellationToken = default)
        {
            await subMgr.PublishAsync(new CommandLogSubject { Level = 0, CommandText = command.Output(), Period = period.TotalMilliseconds });
        }

        public void Fail(IDbCommand command, Exception exception)
        {
            subMgr.Publish(new CommandLogSubject { Level = 1, CommandText = command.Output() });
        }

        public async Task FailAsync(IDbCommand command, Exception exception, CancellationToken cancellationToken = default)
        {
            await subMgr.PublishAsync(new CommandLogSubject { Level = 1, CommandText = command.Output() });
        }
    }

    /// <summary>
    /// �������׼����
    /// </summary>
    public class CacheKeyNormalizer : ICacheKeyNormalizer
    {
        /// <summary>
        /// ��׼����
        /// </summary>
        /// <param name="cacheKey">�������</param>
        /// <param name="additional">���ӵġ�</param>
        /// <returns></returns>
        public string NormalizeKey(string cacheKey, object additional = null)
        {
            if (additional != null && cacheKey.StartsWith(additional.ToString()))
            {
                return cacheKey;
            }

            if (cacheKey.StartsWith("zero:"))
            {
                return cacheKey;
            }

            return "zero:" + cacheKey;
        }
    }

    /// <summary>
    /// ִ�� sql �ű��Ķ������⡣
    /// </summary>
    public class CommandLogSubject
    {
        /// <summary>
        /// ��ȡ�����ü���
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// ��ȡ������ sql �ű���
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// ��ȡ�����ú�ʱ��
        /// </summary>
        public double Period { get; set; }
    }

    public class AutoProfile : Profile
    {
        public AutoProfile()
        {
            CreateMap<SysUser, UserDto>().ForMember(s => s.IDCard, s => s.MapFrom(t => t.IDCard.ToString()));
        }
    }
}

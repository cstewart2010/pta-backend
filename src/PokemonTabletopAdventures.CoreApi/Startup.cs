using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.Infrastructure;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokemonTabletopAdventures.CoreApi;

public class Startup(IConfiguration configuration)
{
    private const string PolicyName = "MyPolicy";

    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddCors(options =>
        {
            options.AddPolicy(name: PolicyName,
                  builder =>
                  {
                      builder.AllowAnyOrigin();
                      builder.AllowAnyHeader();
                      builder.AllowAnyMethod();
                      builder.WithExposedHeaders(HeaderNames.AccessToken, HeaderNames.SessionAuth);
                  });
        });
        services.AddSingleton<IDexService, DexService>();
        
        services.AddSingleton<IEncryptionService, EncryptionService>();
        
        services.AddSingleton<IPokedexService, PokedexService>();
        services.AddSingleton<IPokemonService, PokemonService>();
        services.AddSingleton<ISettingService, SettingService>();
        services.AddSingleton<ITrainerService, TrainerService>();
        services.AddSingleton<IGameService, GameService>();

        services.AddSingleton<IExportService, ExportService>();

        services.AddSingleton<INpcService, NpcService>();

        services.AddSingleton<IShopService, ShopService>();

        services.AddSingleton<ISpriteService, SpriteService>();

        services.AddSingleton<IUserMessageThreadService, UserMessageThreadService>();

        services.AddSingleton<IUserService, UserService>();
        
        services.AddTransient<ProblemDetailsFactory, PtaProblemDetailsFactory>();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseExceptionHandler("/error");
        app.UseStatusCodePages();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors(PolicyName);

        app.UseAuthorization();

        app.UseWebSockets();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}

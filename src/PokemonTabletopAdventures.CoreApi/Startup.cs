using Microsoft.AspNetCore.Mvc.Infrastructure;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.Domain.Mappers;
using PokemonTabletopAdventures.CoreApi.Infrastructure;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

namespace PokemonTabletopAdventures.CoreApi;

internal class Startup(IConfiguration configuration)
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
        services.AddSingleton<IRepositoryService, RepositoryService>();

        services.AddSingleton<IDexService, DexService>();
        
        services.AddSingleton<IEncryptionService, EncryptionService>();
        
        services.AddSingleton<IPokedexService, PokedexService>();
        services.AddSingleton<IPokemonService, PokemonService>();
        services.AddSingleton<IShopService, ShopService>();
        services.AddSingleton<ISettingService, SettingService>();
        services.AddSingleton<ITrainerService, TrainerService>();
        services.AddSingleton<INpcService, NpcService>();
        services.AddSingleton<IGameService, GameService>();

        services.AddSingleton<ISpriteService, SpriteService>();

        services.AddSingleton<IUserService, UserService>();

        services.AddSingleton<IUserMessageThreadService, UserMessageThreadService>();

        services.AddSingleton<IModelToDtoMapper, ModelToDtoMapper>();

        services.AddSingleton<IDtoToModelMapper, DtoToModelMapper>();
        
        services.AddTransient<ProblemDetailsFactory, PtaProblemDetailsFactory>();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Version = "v2",
                Title = "Pokemon Tabletop Adventures CoreApi",
                Description = "Backend API for Pokemon Tabletop Adventures Web Application",
            });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
            options.RoutePrefix = "swagger";
        });
        
        if (env.IsDevelopment())
        {
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

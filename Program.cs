using System.Text;
using Microsoft.EntityFrameworkCore;
using queryHelp.Automapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using queryHelp.Data;
using queryHelp.Interfaces;
using queryHelp.Security;
using queryHelp.Service;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using queryHelp.Settings;
using queryHelp.Interfaces.AiSmartQueryBuilder.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Bearer definition
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter JWT Bearer token **_without_** the word 'Bearer'.\n\nExample: `eyJhbGci...`",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

   
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });


    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));


var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var keyBytes = Encoding.UTF8.GetBytes(jwt.Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.FromSeconds(30) 
    };
});
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AI"));
builder.Services.AddHttpClient("openai");
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<ISchemaService, SchemaService>();
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AI"));
builder.Services.AddHttpClient("openai");


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Smart Query Builder API v1");

      
        c.ConfigObject.DefaultModelsExpandDepth = -1; 
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    SeedData.EnsureSeedData(db, builder.Configuration);
}
app.MapControllers();

app.Run();

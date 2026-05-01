 

using Shared.Email;
using Shared.FileStorage;
using Shared.Pdf;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add Services to the container

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IStoreContext, Shared.StoreContext.StoreContext>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});
// common services: carter , mediatr , fluent validation
var identityModule = typeof(IdentityModule).Assembly;
var storesModule = typeof(StoresModule).Assembly;
var catalogModule = typeof(CatalogModules).Assembly;
var ordersModule = typeof(OrdersModule).Assembly;
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddStoresModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddOrdersModule(builder.Configuration);

builder.Services.AddMediatRWithAssemblies(identityModule , storesModule, catalogModule, ordersModule);
builder.Services.AddCarterWithAssemblies(identityModule , storesModule, catalogModule, ordersModule);
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

// Add Azure Blob Storage
builder.Services.AddBlobStorage(builder.Configuration);

// Add Email Service
builder.Services.AddEmailService(builder.Configuration);

// Add PDF Service
builder.Services.AddPdfService();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure the HTTP request pipeline.
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UsePathBase(new PathString("/api"));


app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.UseExceptionHandler(options => { });
app.UseIdentityModule().UseStoresModule().UseCatalogModule().UseOrdersModule();

app.Run();

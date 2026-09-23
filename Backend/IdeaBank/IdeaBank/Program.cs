using FluentValidation;
using IdeaBank.Data;
using IdeaBank.Infrastructure;
using IdeaBank.Models;
using IdeaBank.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173/") // Angular app URL
              .AllowAnyMethod()                     // Allows GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();                    // Allows all request headers
                                                    // .AllowCredentials();               
    });
});

// 1. Register the exception handler and problem details services
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2.Configure SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Register Dependency Injection Services
builder.Services.AddScoped<IIdeaService, IdeaService>();

// 4. Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// 5. Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<IdeaUpsertDtoValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseStaticFiles();

// 1. Initialize routing first 
app.UseRouting();

// 2. Apply CORS right after routing so it intercepts incoming requests properly
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

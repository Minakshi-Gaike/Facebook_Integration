
using FacebookLeadGeneration.Application.Interfaces.Repositories;
using FacebookLeadGeneration.Application.Interfaces.Services;
using FacebookLeadGeneration.Application.Services;
using FacebookLeadGeneration.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddHttpClient<MetaLeadService>();

builder.Services.AddScoped<IMetaLeadService>(provider =>
    provider.GetRequiredService<MetaLeadService>());

builder.Services.AddScoped<IMetaLeadRepository, MetaLeadRepository>();

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


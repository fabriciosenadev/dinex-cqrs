var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiConfig(builder.Configuration);

builder.Services.AddSwaggerConfiguration();

builder.Services.RegisterAllDepdencies(builder.Configuration);

var app = builder.Build();

var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseApiConfig(app.Environment, app.Services);

app.UseSwaggerConfiguration(apiVersionDescriptionProvider);

app.Run();

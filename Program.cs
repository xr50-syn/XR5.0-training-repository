using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Certificate;
using XR5_0TrainingRepo.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;


var builder = WebApplication.CreateBuilder(args);
var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Add services to the container.

builder.Services.AddControllers();
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<XR50RepoContext>(opt =>
    opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));    
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
/*builder.Services.AddAuthentication(
        CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = context =>
            {
                if (validationService.ValidateCertificate(context.ClientCertificate))
                {
                    context.Success();
                }
                else
                {
                    context.Fail("invalid cert");
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                context.Fail("invalid cert");
                return Task.CompletedTask;
            }
        };
    });
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options =>
        options.ClientCertificateMode = ClientCertificateMode.RequireCertificate);
});*/
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("appsettings.json");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();    
app.UseAuthorization();

app.MapControllers();

app.Run();

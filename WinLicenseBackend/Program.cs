using WinLicenseBackend;
using WinLicenseBackend.DataProviders;
using WinLicenseBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowCredentials()
              .AllowAnyMethod()
              .WithExposedHeaders("Content-Disposition");
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var provider = new XmlDataProvider(
    "Resources/database_projects.xml", 
    "Resources/database_activationdevices.xml", 
    "Resources/database_customers.xml",
    "Resources/database_orders.xml",
    "Resources/database_products.xml",
    "Resources/database_activations.xml"
    );
var generator = new LicenseGenerator();
builder.Services.AddSingleton < IDataProvider >( sp =>  provider);
builder.Services.AddSingleton< LicenseGenerator >(sp => generator);
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton(sp =>
    builder.Configuration.GetSection("AppSettings").Get<AppSettings>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.Run();

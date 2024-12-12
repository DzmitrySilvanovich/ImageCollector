using Azure.Storage.Blobs;
using ImageCollector.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var _configuration = builder.Configuration;
string keyVaultDirectoryId = _configuration.GetValue<string>("SecretCredential:KeyVaultDirectoryId");
string KeyVaultClientId = _configuration.GetValue<string>("SecretCredential:KeyVaultClientId");
string KeyVaultClientSecret = _configuration.GetValue<string>("SecretCredential:KeyVaultClientSecret");
string KeyVaultUri = _configuration.GetValue<string>("KeyVaultUri");

var credential = new ClientSecretCredential(keyVaultDirectoryId, KeyVaultClientId, KeyVaultClientSecret);

var secretClient = new SecretClient(new Uri(KeyVaultUri), credential);

string subscriptionKey = secretClient.GetSecret("CvSubscriptionKey").Value.Value.ToString();
string endpoint = secretClient.GetSecret("CVEndPoint").Value.Value.ToString();

string blobConnectionString = secretClient.GetSecret("BlobConnectionString").Value.Value.ToString();
string blobContainerName = secretClient.GetSecret("BlobContainerName").Value.Value.ToString();


var connectionString = secretClient.GetSecret("DefaultConnection").Value.Value.ToString(); ;
builder.Services.AddDbContext<AppDbContext>( options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, connectionString));

BlobServiceClient blobServiceClient;
BlobContainerClient containerClient;

blobServiceClient = new BlobServiceClient(blobConnectionString);
containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
containerClient.CreateIfNotExists();

builder.Services.AddSingleton(containerClient);

ComputerVisionClient computerVisionClient = new(new ApiKeyServiceClientCredentials(subscriptionKey))
{
    Endpoint = endpoint
};

builder.Services.AddSingleton(computerVisionClient);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

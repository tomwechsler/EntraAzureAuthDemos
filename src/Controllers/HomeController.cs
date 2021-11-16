using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using ManagedIdentities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedIdentities.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static string ContainerName = "files";
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Message = "Click a button to get started!";
            return View(null);
        }

        public async Task<IActionResult> SimpleConnection()
        {
            ViewBag.Message = "Got some data just using a connection string with a secret key.";

            try
            {
                string connectionString = "BlobEndpoint=https://ir77storage.blob.core.windows.net/;QueueEndpoint=https://ir77storage.queue.core.windows.net/;FileEndpoint=https://ir77storage.file.core.windows.net/;TableEndpoint=https://ir77storage.table.core.windows.net/;SharedAccessSignature=sv=2020-08-04&ss=bfqt&srt=sco&sp=rwdlacupitfx&se=2021-11-15T19:26:26Z&st=2021-11-15T11:26:26Z&spr=https&sig=j982Wo%2B8c8XfSp%2FnIII9qSAOqul78GqHYyRXEV38jqo%3D";

                BlobContainerClient client = new BlobContainerClient(connectionString, ContainerName);

                var results = new List<MyFile>();
                foreach (var file in client.GetBlobs()) {
                    results.Add(new MyFile() { FileName = file.Name, DateCreated = file.Properties.CreatedOn });
                }

                return View("Index", results);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View("Index");
        }

        public async Task<IActionResult> KeyVaultConnection()
        {
            ViewBag.Message = "Got some data but get the key from the key vault.";
            string keyVaultUri = "https://ir77keyvault.vault.azure.net/";

            

            try
            {
                //Get the connection string key from a KeyVault/

                var kv = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
                var secret = await kv.GetSecretAsync("StorageConnectionString");
                string connectionString = secret.Value.Value;

                BlobContainerClient client = new BlobContainerClient(connectionString, ContainerName);

                var results = new List<MyFile>();
                foreach (var file in client.GetBlobs())
                {
                    results.Add(new MyFile() { FileName = file.Name, DateCreated = file.Properties.CreatedOn });
                }
                return View("Index", results);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View("Index");

        }

        public async Task<IActionResult> ManagedIdentityConnection()
        {
            ViewBag.Message = "Got some data using a managed identity.";

            
            try
            {
                BlobContainerClient client = new BlobContainerClient(new Uri("https://ir77storage.blob.core.windows.net/files"), new DefaultAzureCredential());


                var results = new List<MyFile>();
                foreach (var file in client.GetBlobs())
                {
                    results.Add(new MyFile() { FileName = file.Name, DateCreated = file.Properties.CreatedOn });
                }
                return View("Index", results);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View("Index");

        }

        public async Task<IActionResult>SPNConnection()
        {
            ViewBag.Message = "Got some data using a service principle.";
            string tenantId = "e9cb468e-94b6-4bcc-b1dd-8a3874ce75b8";
            string clientSecret = "DTG7Q~R_iCE1xa4w6NRcOsKzfiVeDIZBHBI94";
            string clientId = "f2e333dd-a081-4273-a2f0-cffd3d1bcae0";

            try
            {
                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                BlobContainerClient client = new BlobContainerClient(new Uri("https://ir77storage.blob.core.windows.net/files"), credential);


                var results = new List<MyFile>();
                foreach (var file in client.GetBlobs())
                {
                    results.Add(new MyFile() { FileName = file.Name, DateCreated = file.Properties.CreatedOn });
                }
                return View("Index", results);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View("Index");

        }

        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using BlobDownload.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure;
using Microsoft.AspNetCore.StaticFiles;

namespace BlobDownload.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=scanupload;AccountKey=Ag2SLJkp9xLK7Cq/VZNAvpn8vi2YeQxM2l71ry4UO65knNAM/pS81RWh62GZ81yBdRtwP9CX6n8++AStJC2pOQ==;EndpointSuffix=core.windows.net";
        private BlobServiceClient blobServiceClient;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            blobServiceClient = new BlobServiceClient(storageConnectionString);
        }

        public async Task<IActionResult> Index()
        {           
            var contaners = await GetContainers();            
            IEnumerable<BlobContaner>? blobs = contaners as IEnumerable<BlobContaner>;
            return View(blobs);
        }

        private async Task<List<BlobContaner>> GetContainers()
        {            
            List<BlobContaner> cntrs = new List<BlobContaner>();                      
            foreach (var item in blobServiceClient.GetBlobContainers())
            {
                BlobContaner contaner = new BlobContaner
                {
                    Name = item.Name
                };
                cntrs.Add(contaner);
            }
            foreach (var ct in cntrs)
            {
                ct.Blobis = new List<Blobs>();
                var lst = await GetAllBlobFiles(ct.Name);
                foreach (var item in lst)
                {
                    Blobs b = new Blobs();
                    b.FileName = item;
                    ct.Blobis.Add(b);
                } 
            }
            return cntrs;
        }
        public async Task<IActionResult> Download(string fName, string container)
        {
            CloudStorageAccount mycloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = mycloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer contaner = blobClient.GetContainerReference(container);
            CloudBlockBlob cloudBlockBlob = contaner.GetBlockBlobReference(fName);
            await cloudBlockBlob.FetchAttributesAsync();
            using (var file = new MemoryStream())
            {
                await cloudBlockBlob.DownloadToStreamAsync(file);
                
                return File(file.ToArray(), cloudBlockBlob.Properties.ContentType, fName);
            }

            //return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        private async Task<List<string>> GetAllBlobFiles(string container_name)
        {

            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            //get container
            var container = blobServiceClient.GetBlobContainerClient(container_name);

            List<string> blobNames = new List<string>();

            //Enumerating the blobs may make multiple requests to the service while fetching all the values
            //Blobs are ordered lexicographically by name
            //if you want metadata set BlobTraits - BlobTraits.Metadata
            var blobHierarchyItems = container.GetBlobsByHierarchyAsync(BlobTraits.None, BlobStates.None, "/");

            await foreach (var blobHierarchyItem in blobHierarchyItems)
            {
                //check if the blob is a virtual directory.
                if (blobHierarchyItem.IsPrefix)
                {
                    // You can also access files under nested folders in this way,
                    // of course you will need to create a function accordingly (you can do a recursive function)
                    // var prefix = blobHierarchyItem.Name;
                    // blobHierarchyItem.Name = "folderA\"
                    // var blobHierarchyItems= container.GetBlobsByHierarchyAsync(BlobTraits.None, BlobStates.None, "/", prefix);     
                }
                else
                {
                    blobNames.Add(blobHierarchyItem.Blob.Name);
                }
            }
            return blobNames;
        }

        private async void GetBlob(string filetoDownload, string azure_ContainerName)
        {
            Console.WriteLine("Inside downloadfromBlob()");

            CloudStorageAccount mycloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = mycloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(azure_ContainerName);
            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(filetoDownload);
            using (var file = new MemoryStream())
            {
                await cloudBlockBlob.DownloadToStreamAsync(file);

                var f = file.CanSeek;
            }



            // provide the file download location below            
            //Stream file = File.OpenWrite(@ "E:\Tools\BlobFile\" + filetoDownload);    


            //     cloudBlockBlob.DownloadToStream(file);

            Console.WriteLine("Download completed!");


        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
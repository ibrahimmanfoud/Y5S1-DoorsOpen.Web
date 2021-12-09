using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoorsOpen.Data;
using DoorsOpen.Models;

using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DoorsOpen.Controllers
{
    public class BuildingImageController : Controller
    {
        private readonly SiteDbContext _context;
        private readonly IConfiguration _config;

        public BuildingImageController(SiteDbContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        // GET: BuildingImage
        public async Task<IActionResult> Index()
        {
            var buildingModels = _context.Buildings.ToList();
            ViewBag.allBuildingModels = buildingModels;
            
            return View(await _context.BuildingImages.OrderBy(m => m.BuildingId).ToListAsync());
        }

        // GET: BuildingImage/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingImageModel = await _context.BuildingImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingImageModel == null)
            {
                return NotFound();
            }

            return View(new BuildingImageViewModel(buildingImageModel,
                _config.GetValue<string>("AzureImagePrefix")));
        }

        // GET: BuildingImage/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.allBuildingModels = await  _context.Buildings.ToListAsync();

            return View();
        }

        // POST: BuildingImage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BuildingId,ImageURL,AltText")] BuildingImageModel buildingImageModel, IFormFile upload)
        {
            if (ModelState.IsValid)
            {
                if (upload != null)
                {
                    string imageName = GetFileName(upload);
                    buildingImageModel.ImageURL = imageName;
                    UploadToAzure(imageName, upload);
                }

                _context.Add(buildingImageModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(buildingImageModel);
        }

        // GET: BuildingImage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var buildingImageModel = await _context.BuildingImages.FindAsync(id);
            if (buildingImageModel == null)
            {
                return NotFound();
            }

            ViewBag.selectedBuilding = await _context.Buildings
                .FirstOrDefaultAsync(m => m.Id == buildingImageModel.BuildingId);
            ViewBag.allBuildingModels = await _context.Buildings.ToListAsync();

            return View(new BuildingImageViewModel(buildingImageModel, _config.GetValue<string>("AzureImagePrefix")));
        }

        // POST: BuildingImage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BuildingId,ImageURL,AltText")] BuildingImageModel buildingImageModel, IFormFile upload)
        {
            if (id != buildingImageModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var imageToEdit = await _context.BuildingImages.Where(b => b.Id == buildingImageModel.Id).FirstOrDefaultAsync();
                var deleteImage = false;
                if(imageToEdit.ImageURL != buildingImageModel.ImageURL || upload != null)
                {
                    if (!string.IsNullOrEmpty(buildingImageModel.ImageURL))
                    {
                        deleteImage = true;
                    }
                }

                if (deleteImage)
                {
                    if (!string.IsNullOrEmpty(buildingImageModel.ImageURL))
                    {
                        DeleteFromAzure(buildingImageModel.ImageURL);
                        buildingImageModel.ImageURL = null;
                    }
                }

                if (upload != null)
                {
                    string imageName = GetFileName(upload);
                    buildingImageModel.ImageURL = imageName;
                    UploadToAzure(imageName, upload);
                    Console.WriteLine(buildingImageModel.ImageURL);
                }

                imageToEdit.ImageURL = buildingImageModel.ImageURL;
                imageToEdit.AltText = buildingImageModel.AltText;

                _context.SaveChanges();

                return RedirectToAction("Index", "BuildingImage");
            }
            return View(buildingImageModel);
        }

        // GET: BuildingImage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingImageModel = await _context.BuildingImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingImageModel == null)
            {
                return NotFound();
            }

            return View(new BuildingImageViewModel(buildingImageModel, _config.GetValue<string>("AzureImagePrefix")));
        }

        // POST: BuildingImage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var buildingImageModel = await _context.BuildingImages.FindAsync(id);
            _context.BuildingImages.Remove(buildingImageModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BuildingImageModelExists(int id)
        {
            return _context.BuildingImages.Any(e => e.Id == id);
        }


        public string GetFileName(IFormFile upload)
        {
            int indexExt = 0;
            string ext;
            string imageName = null;
            if (upload != null)
            {
                indexExt = upload.FileName.IndexOf(".");
                ext = upload.FileName.Substring(indexExt);
                imageName = Guid.NewGuid() + ext;
            }
            return imageName;
        }

        public void UploadToAzure(string imageName, IFormFile upload)
        {
            // Azure needs your connection string like a db
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _config.GetValue<string>("AzureConnectionString");
            }
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            // Azure needs to know what folder you want to save in
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_config.GetValue<string>("AzureImageFolder"));
            // Then, you can get a blob writer thing from azure
            containerClient.UploadBlob(imageName, upload.OpenReadStream());
        }

        public void DeleteFromAzure(string imageName)
        {
            // need connection to Azure just like a local db
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _config.GetValue<string>("AzureConnectionString");
            }
            // connect to Azure
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            // go to a specific folder
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_config.GetValue<string>("AzureImageFolder"));
            // find a specific file
            BlobClient blob = containerClient.GetBlobClient($"{imageName}");
            // delete if exists
            if (blob.Exists())
            {
                blob.Delete();
            }
        }
    }
}

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
            ViewBag.allBuildingModels = await _context.Buildings.ToListAsync();

            var buildingImages = await _context.BuildingImages.OrderBy(m => m.BuildingId).ToListAsync();
            foreach(var img in buildingImages)
            {
                if (img.ImageURL == null)
                    img.ImageURL = " ";
                img.ImageURL = _config.GetValue<string>("AzureImagePrefix") + img.ImageURL;
            }

            return View(buildingImages);
        }

        // GET: BuildingImage/Details/(id)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            
            var selectedImage = await _context.BuildingImages.FindAsync(id);

            if (selectedImage == null)
                return NotFound();

            ViewBag.buildingName = await _context.Buildings.FirstOrDefaultAsync(m => m.Id == selectedImage.BuildingId);

            return View(new BuildingImageViewModel(selectedImage,
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

        // GET: BuildingImage/Edit/(id)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var selectedImage = await _context.BuildingImages.FindAsync(id);
            if (selectedImage == null)
                return NotFound();

            ViewBag.imageURL = _config.GetValue<string>("AzureImagePrefix") + selectedImage.ImageURL;
            ViewBag.allBuildings = await _context.Buildings.ToListAsync();

            return View(selectedImage);
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
                var deleteImage = false;
                if (!string.IsNullOrEmpty(buildingImageModel.ImageURL))
                {
                    deleteImage = true;
                }

                if (deleteImage)
                {
                    DeleteFromAzure(buildingImageModel.ImageURL);
                    buildingImageModel.ImageURL = null;
                }

                if (upload != null)
                {
                    buildingImageModel.ImageURL = GetFileName(upload);
                    UploadToAzure(buildingImageModel.ImageURL, upload);
                }
                else
                {
                    buildingImageModel.ImageURL = "";
                }

                _context.Attach(buildingImageModel);
                _context.Entry(buildingImageModel).Property(p => p.BuildingId).IsModified = true;
                _context.Entry(buildingImageModel).Property(p => p.ImageURL).IsModified = true;
                _context.Entry(buildingImageModel).Property(p => p.AltText).IsModified = true;
                await _context.SaveChangesAsync();

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

            // Create a buildingImageModel variable which contains the appropriate buldingImage based upon the id parameter passed 
            var selectedImage = await _context.BuildingImages.FindAsync(id);
            if (selectedImage == null)
            {
                return NotFound();
            }

            // Return the delete view with a buldingImageViewModel based on the previously created selectedBuilding
            return View(new BuildingImageViewModel(selectedImage, _config.GetValue<string>("AzureImagePrefix")));
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
            int indexExt;
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

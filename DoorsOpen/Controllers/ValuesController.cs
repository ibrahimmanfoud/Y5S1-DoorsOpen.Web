using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using DoorsOpen.Data;
using DoorsOpen.Models;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DoorsOpen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly SiteDbContext _context;
        private readonly IConfiguration _config;

        public ValuesController(SiteDbContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public List<APIModel> Get()
        {
            Dictionary<int, List<BuildingImageViewModel>> imagesDictionary = new Dictionary<int, List<BuildingImageViewModel>>();

            List<BuildingViewModel> buildings = _context.Buildings.Select(b => new BuildingViewModel(b, _config.GetValue<string>("AzureImagePrefix"))).ToList();
            foreach (BuildingViewModel buildingModel in buildings)
            {
                List<BuildingImageViewModel> buildingImageViews = _context.BuildingImages.Where(b => b.BuildingId == buildingModel.Id).Select(b => new BuildingImageViewModel(b, _config.GetValue<string>("AzureImagePrefix"))).ToList();
                imagesDictionary.Add(buildingModel.Id, buildingImageViews);
            }

            return _context.Buildings.Select(b => new APIModel(b, _config.GetValue<string>("AzureImagePrefix"), imagesDictionary.GetValueOrDefault(b.Id))).ToList();
        }
    }
}

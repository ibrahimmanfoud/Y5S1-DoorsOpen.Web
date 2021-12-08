using System;
using System.ComponentModel.DataAnnotations;

namespace DoorsOpen.Models
{
	public class BuildingImageModel
	{
		public int Id { get; set; }
		public int BuildingId { get; set; }
		public string ImageURL { get; set; }
		public string AltText { get; set; }

	}


	public class BuildingImageViewModel
	{

		public BuildingImageViewModel(BuildingImageModel original, string baseURL)
		{
			Id = original.Id;
			BuildingId = original.BuildingId;
			ImageURL = baseURL + original.ImageURL;
			AltText = original.AltText;
		}

		public int Id { get; set; }
		public int BuildingId { get; set; }
		public string ImageURL { get; set; }
        [Display(Name = "Alt Text for Screen Readers")]
		public string AltText { get; set; }
		


    }
}

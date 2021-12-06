using System;


namespace DoorsOpen.Models
{
	public class BuildingImageModel
	{
		public int Id { get; set; }
		public int BuildingId { get; set; }
		public string ImageURL { get; set; }
		public string AltText { get; set; }
			
	}
}

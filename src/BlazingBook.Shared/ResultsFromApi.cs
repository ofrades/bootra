using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace BlazingBook {
	public class Root {
		[JsonPropertyName("count")]
		public long Count { get; set; }

		[JsonPropertyName("results")]
		public List<Result> Results { get; set; } = new List<Result>();
	}
	public class Result {
		[JsonPropertyName("id")]
		public int Id { get; set; }

		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("authors")]
		public List<Author> Authors { get; set; } = new List<Author>();
		public decimal BasePrice { get; set; } = 12.00m;

		[JsonPropertyName("subjects")]
		public List<string> Subjects { get; set; }

		[JsonPropertyName("bookshelves")]
		public List<string> Bookshelves { get; set; }

		[JsonPropertyName("languages")]
		public List<string> Languages { get; set; } = new List<string>();

		[JsonPropertyName("copyright")]
		public bool Copyright { get; set; }

		[JsonPropertyName("media_type")]
		public string MediaType { get; set; }

		[JsonPropertyName("download_count")]
		public long DownloadCount { get; set; }
	}
	public partial class Author {
		[JsonPropertyName("name")]
		public string Name { get; set; } = "";

		[JsonPropertyName("birth_year")]
		public long? BirthYear { get; set; }

		[JsonPropertyName("death_year")]
		public long? DeathYear { get; set; }
	}
}
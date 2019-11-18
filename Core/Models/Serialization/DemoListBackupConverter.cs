using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Models.Serialization
{
	/// <summary>
	/// JSON Converter to write / read a list of demos with only custom data that we need for a backup
	/// </summary>
	public class DemoListBackupConverter: JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsGenericType;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// Store application version for future compatibility
			writer.WriteStartObject();
			writer.WritePropertyName("Version");
			writer.WriteValue(AppSettings.APP_VERSION.ToString());
			// The demos list is stored in an JSON array
			writer.WritePropertyName("Demos");
			writer.WriteStartArray();
			foreach (Demo demo in (List<Demo>)value)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("Id");
				writer.WriteValue(demo.Id);
				writer.WritePropertyName("Comment");
				writer.WriteValue(demo.Comment);
				writer.WritePropertyName("status");
				writer.WriteValue(demo.Status);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			List<Demo> demos = new List<Demo>();

			string version = string.Empty;
			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.PropertyName && reader.Path == "Version")
				{
					version = reader.ReadAsString();
					continue;
				}

				// update logic based on version if needed
				if (reader.TokenType == JsonToken.StartObject)
				{
					JObject obj = (JObject) serializer.Deserialize(reader);

					Demo demo = new Demo
					{
						Id = Convert.ToString(((JValue) obj["Id"]).Value),
						Comment = Convert.ToString(((JValue) obj["Comment"]).Value),
						Status = Convert.ToString(((JValue) obj["status"]).Value)
					};
					if (!string.IsNullOrEmpty(demo.Comment) || !string.IsNullOrEmpty(demo.Status)) demos.Add(demo);
                    demo.EnableUpdates();
				}
			}

			return demos;
		}
	}
}
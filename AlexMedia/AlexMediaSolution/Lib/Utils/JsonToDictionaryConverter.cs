using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AlexMedia.Utils
{
	public class JsonToDictionaryConverter
	{
		/// <summary>
		///  Deserialize to Dictionary<string,object> or List<object> or nesting of those
		/// </summary>
		/// <param name="json"></param>
		/// <param name="isArray"></param>
		/// <returns></returns>
		public static IEnumerable deserializeToDictionaryOrList(string json, bool isArray = false)
		{
			IEnumerable valuesReturn = null;

			if (!isArray)
			{
				isArray = json.StartsWith("[");
			}


			if (isArray)
			{
				var values = JsonConvert.DeserializeObject<List<object>>(json);


				if (values != null)
				{
					var values2 = new List<object>();
					foreach (var d in values)
					{
						if (d is JObject)
						{
							values2.Add(deserializeToDictionaryOrList(d.ToString()));
						}
						else if (d is JArray)
						{
							values2.Add(deserializeToDictionaryOrList(d.ToString(), true));
						}
						else
						{
							values2.Add(d);
						}

						valuesReturn = values2;
					}
				}
			}
			else
			{
				var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);


				if (values != null)
				{
					var values2 = new Dictionary<string, object>();

					foreach (KeyValuePair<string, object> d in values)
					{
						if (d.Value is JObject)
						{
							values2.Add(d.Key, deserializeToDictionaryOrList(d.Value.ToString()));
						}
						else if (d.Value is JArray)
						{
							values2.Add(d.Key, deserializeToDictionaryOrList(d.Value.ToString(), true));
						}
						else
						{
							values2.Add(d.Key, d.Value);
						}
					}

					valuesReturn = values2;
				}
			}

			return valuesReturn;
		}
	}
}

using System;
using System.Threading.Tasks;
using AlexMedia.Interfaces;
using AlexMedia.Utils;

namespace AlexMedia.Services
{
    public class RenderService : IRenderService
    {

        /// <summary>
        /// Render the template with the marketing data
        /// </summary>
        /// <param name="template"> let say template is <h1>{dictionary key}</h1></param>
        /// <param name="marketingData">json string
        /// {"title":"something"}
        /// </param>
        /// <returns></returns>
        public async Task<string> RenderTemplateAsync(string template, string marketingData)
        {
            var data=JsonToDictionaryConverter.deserializeToDictionaryOrList(marketingData);
       
            return await Task.FromResult(DictionarySmartFormatter.Formatter.Format(template, data));
        }
    }
}
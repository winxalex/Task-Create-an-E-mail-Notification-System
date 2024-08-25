using System.Threading.Tasks;
using System.Xml;

namespace WINX.Extensions
{
    public static class XmlReaderExtensions // Create extension class
    {
        public static async Task<bool> ReadToDescendantAsync(this XmlReader reader, string name) // Define extension method
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

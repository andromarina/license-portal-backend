using System.IO;
using System.Reflection;

namespace WebApi.Helpers
{
    public class FileReaderUtils
    {
        public static string ReadFileContent(string fileName)
        {            
            string resourcePath = GetFullFilePathFromResources(fileName);

            using (StreamReader reader = new StreamReader(resourcePath))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetFullFilePathFromResources(string fileName)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string resourcePath = Path.Combine(assemblyFolder, "Resources", fileName);
            return resourcePath;
        }
    }
}

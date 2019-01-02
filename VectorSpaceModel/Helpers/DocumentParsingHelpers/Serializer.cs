using System.IO;
using Newtonsoft.Json;

namespace VectorSpaceModel.Helpers.DocumentParsingHelpers
{
  internal class Serializer
  {
    public void Serialize(object obj, string fileName)
    {                                       
      // serialize JSON directly to a file
      using (var file = File.CreateText(fileName))
      {
        var serializer = new JsonSerializer();
        serializer.Serialize(file, obj);
      }
    }
  }
}

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VectorSpaceModel.Helpers.DocumentParsingHelpers
{
  internal class Deserializer
  {
    public T Deserialize<T>(string fileName)
    {
      T result;
      // deserialize JSON directly from a file
      using (StreamReader file = File.OpenText(fileName))
      {
        var serializer = new JsonSerializer();
        result = (T)serializer.Deserialize(file, typeof(T));
      }

      return result;
    }
  }
}

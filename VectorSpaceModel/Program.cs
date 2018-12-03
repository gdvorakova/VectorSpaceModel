using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorSpaceModel.Helpers.CommandLineHelpers;
using VectorSpaceModel.Helpers.DocumentParsingHelpers;

namespace VectorSpaceModel
{
  class Program
  {
    static void Main(string[] args)
    {
      var options = new List<Option>();

      var optionQ = new Option("q", true);
      var optionD = new Option("d", true);
      var optionR = new Option("r", true);
      var optionO = new Option("o", true);

      options.Add(optionQ);
      options.Add(optionD);
      options.Add(optionR);
      options.Add(optionO);


      var commandLineParser = new CommandLineParser(args, options);
      commandLineParser.Parse();

      var topicsFile = commandLineParser.GetOptionValue("q");
      if (topicsFile == null)
      {
        throw new ArgumentException("No topic files are specified");
      }
      var topics = DocumentReader.LinesToList(topicsFile);


      var documentsFile = commandLineParser.GetOptionValue("d");
      if (documentsFile == null)
      {
        throw new ArgumentException("No document files are specified");
      }
      var documents = DocumentReader.LinesToList(documentsFile);

      var runId = commandLineParser.GetOptionValue("r");
      if (runId == null)
      {
        throw new ArgumentException("No run id is specified");
      }

      var outputFile = commandLineParser.GetOptionValue("o");
      if (outputFile == null)
      {
        throw new ArgumentException("No output file was specified");
      }

      var vsmIndex = new VectorSpaceModelIndex(topics, documents, runId, outputFile);
      vsmIndex.Create();
    }
  }
}

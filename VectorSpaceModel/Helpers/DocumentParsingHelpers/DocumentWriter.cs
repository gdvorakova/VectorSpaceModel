using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorSpaceModel.Data;

namespace VectorSpaceModel.Helpers.DocumentParsingHelpers
{
  class DocumentWriter : IDisposable
  {
    private const string _SeparatorChar = "\t";
    private const int _N = 1000;
    private string fFilePath;
    private StreamWriter fWriter;
    private List<Document> fDocuments;

    public DocumentWriter(string filePath, List<Document> documents)
    {
      fFilePath = filePath;
      fWriter = new StreamWriter(fFilePath);
      fDocuments = documents; 
    }
    public void PrintQueryResults(Dictionary<int, double> scores, string qid, string iter, string runId)
    {
      int rank = -1;
      foreach (var score in scores)
      {
        rank++;

        if (rank >= _N)
        {
          break;
        }

        var sb = new StringBuilder();
        sb.Append(qid).Append(_SeparatorChar);
        sb.Append(iter).Append(_SeparatorChar);

        var docno = fDocuments[score.Key].DocNo;
        sb.Append(docno).Append(_SeparatorChar);
        //rank 
        sb.Append(rank).Append(_SeparatorChar);
        // similarity score
        sb.Append(Math.Round(score.Value, 4)).Append(_SeparatorChar);
        sb.Append(runId);

        fWriter.WriteLine(sb);
      }
    }

    public void Dispose()
    {
      if (fWriter != null)
      {
        fWriter.Close();
      }
    }                     
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSpaceModel.Data
{
  class Document : IDocument
  {
    public readonly string DocId;
    public readonly string DocNo;
    public readonly int Length;

    // counts number of times a term occurs in doc
    public Dictionary<string, double> TermFrequency { get; set; } 

    public Document(string docId, string docNo, Dictionary<string, double> termFrequency, int length)
    {
      DocId = docId;
      DocNo = docNo;
      TermFrequency = termFrequency;
      Length = length;
    }

    public double GetTf(string term)
    {
      if (TermFrequency.ContainsKey(term))
      {     
        return TermFrequency[term];
      }  
      else
      {
        return 0.0;
      }
    }
  }
}

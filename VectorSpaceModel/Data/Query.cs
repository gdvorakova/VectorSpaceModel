using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSpaceModel.Data
{
  class Query : IDocument
  {
    public readonly string QueryId;

    public readonly int Length;

    // counts number of times a term occurs in doc
    public Dictionary<string, double> TermFrequency { get; set; }

    public Query(string queryId, Dictionary<string, double> termFrequency, int length)
    {
      QueryId = queryId;
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

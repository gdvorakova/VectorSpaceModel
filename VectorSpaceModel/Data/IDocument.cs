using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSpaceModel.Data
{
  interface IDocument
  {
    Dictionary<string, double> TermFrequency { get; set; }
  }
}

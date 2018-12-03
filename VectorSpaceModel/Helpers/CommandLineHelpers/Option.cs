using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSpaceModel.Helpers.CommandLineHelpers
{
  class Option
  {
    public bool HasArguments { get; }

    public string OptionName { get; }

    public int NumberOfArguments { get; private set; }

    public List<string> Values { get; private set; }

    public Option(string option, bool hasArguments)
    {
      HasArguments = hasArguments;
      OptionName = option;

      if (HasArguments)
      {  
        NumberOfArguments = 1;
        Values = new List<string>();
      }                           
      else
        NumberOfArguments = 0;
    }

    public void AddValue(string value)
    {
      if (!HasArguments)
      {
        throw new FormatException();
      }

      Values.Add(value);
      NumberOfArguments = Values.Count;
    }
  }
}

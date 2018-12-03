using System;
using System.Collections.Generic;

namespace VectorSpaceModel.Helpers.CommandLineHelpers
{
  internal class CommandLineParser
  {
    private readonly string[] fArguments;
    private List<Option> fOptions;

    private Option fCurrentOption;

    public CommandLineParser(string[] args, List<Option> options)
    {
      fArguments = args;
      fOptions = options;
    }

    public void Parse()
    {
      foreach (string arg in fArguments)
      {
        if (arg == "run" && fCurrentOption == null)
        {
          continue;
        }
        // argument is an option
        if (arg.StartsWith("-"))
        {
          var option = GetOption(arg.Substring(1));
          if (option.HasArguments)
            fCurrentOption = option;
          else
            fCurrentOption = null;
        }
        else
        {
          if (fCurrentOption == null)
            throw new FormatException();

          fCurrentOption.AddValue(arg);
        }

      }
    }

    public List<string> GetOptionValues(string option)
    {
      foreach (var opt in fOptions)
      {
        if (opt.OptionName == option)
        {
          return opt.Values;
        }
      }

      return null;
    }

    public string GetOptionValue(string option)
    {
      foreach (var opt in fOptions)
      {
        if (opt.OptionName == option && opt.HasArguments)
        {
          return opt.Values[0];
        }
      }

      return null;
    }

    private Option GetOption(string opt)
    {
      Option option = null;
      opt = opt.ToLower();

      fOptions.ForEach(o =>
      {
        if (o.OptionName == opt)
        {
          option = o;
        }
      });

      if (option == null)
      {
        Console.WriteLine("Invalid option.");
        throw new FormatException();
      }

      return option;
    }
  }
}

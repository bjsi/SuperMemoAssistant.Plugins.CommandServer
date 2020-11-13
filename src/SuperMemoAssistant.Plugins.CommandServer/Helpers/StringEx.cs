using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Helpers
{
  public static class StringEx
  {
    public static string OnlyLetters(this string input)
    {
      string x = string.Concat(input.Where(x => char.IsLetter(x)));
      return x;
    }
  }
}

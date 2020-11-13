using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace SuperMemoAssistant.Plugins.CommandServer.Helpers
{
  public static class CodeDomEx
  {
    public static string GenerateSourceForMember(this CodeTypeMember m)
    {
      var codeGeneratorOptions = new CodeGeneratorOptions();
      using (var tw = new StringWriter())
      using (var provider = new CSharpCodeProvider())
      {
        provider.GenerateCodeFromMember(m, tw, codeGeneratorOptions);
        return tw.ToString();
      }
    }
  }
}

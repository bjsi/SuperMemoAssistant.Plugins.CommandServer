using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperMemoAssistant.Plugins.CommandServer.Compiler
{
  public abstract class SvcCompilerBase
  {

    protected CodeCompileUnit TargetUnit { get; }
    protected CodeTypeDeclaration TargetClass { get; }
    protected CodeNamespace Namespace { get; }

    public SvcCompilerBase(string className, string nameSpace, IEnumerable<string> imports)
    {
      TargetUnit = new CodeCompileUnit();
      TargetClass = new CodeTypeDeclaration(className);
      Namespace = new CodeNamespace(nameSpace);
      TargetClass.IsClass = true;
      TargetClass.TypeAttributes = TypeAttributes.Public;
      Namespace.Types.Add(TargetClass);
      Namespace.Imports.AddRange(
        imports.Select(x => new CodeNamespaceImport(x)).ToArray());
      TargetUnit.Namespaces.Add(Namespace);
    }
  }
}

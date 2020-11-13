using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace SuperMemoAssistant.Plugins.CommandServer.Compiler
{
  interface ITransformer
  {
    void TransformResistryMemberGetter(string targetProp, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass);
    CodeMethodReferenceExpression TransformMethodThisParam(string targetMethodName, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass);
    CodePropertyReferenceExpression TransformSetterMethodThisParam(string propName, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass);
    CodeVariableReferenceExpression TransformSetterMethodNonThisParam(ParameterInfo param, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass);
    void TransformMethodNonThisParam(ParameterInfo param, List<CodeExpression> targetMethodArgs, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass);
  }
}

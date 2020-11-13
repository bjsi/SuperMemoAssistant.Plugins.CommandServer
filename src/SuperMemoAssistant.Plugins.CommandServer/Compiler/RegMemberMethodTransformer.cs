using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperMemoAssistant.Plugins.CommandServer.Compiler
{
  public class RegMemberMethodTransformer<T> : ITransformer
  {

    public string FirstParameterName { get; } = "id";
    public Type FirstParameterType { get; } = typeof(int);

    // TODO: What if there are multiple required registries?
    // Use a dictionary?
    private string RegFieldName { get; } = "_" + typeof(T).Name.OnlyLetters() + "Registry";

    private void AddRegFieldIfNotExists(CodeConstructor cons, CodeTypeDeclaration klass)
    {

      // Only add if it doesn't exist
      // TODO: check the type NOT name
      if (klass.Members.Cast<CodeTypeMember>().Any(x => x.Name == RegFieldName))
        return;

      // TODO: Doesn't work when testing
      var regType = typeof(IRegistry<T>);

      // Add Field
      CodeMemberField field = new CodeMemberField();
      field.Name = RegFieldName;
      field.Type = new CodeTypeReference(regType);
      field.Attributes = MemberAttributes.Private;
      klass.Members.Add(field);

      // Add initializer to the constructor
      var parameterName = RegFieldName.ToLowerInvariant();
      cons.Parameters.Add(new CodeParameterDeclarationExpression(
             regType, parameterName));

      // Add wrapped object field initialization logic
      CodeFieldReferenceExpression wrappedObjReference =
          new CodeFieldReferenceExpression(
          new CodeThisReferenceExpression(), RegFieldName);

      // Assign to the wrapped object field
      cons.Statements.Add(new CodeAssignStatement(wrappedObjReference,
          new CodeArgumentReferenceExpression(parameterName)));

    }

    public CodeMethodReferenceExpression TransformMethodThisParam(string targetMethodName, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass)
    {
      AddRegFieldIfNotExists(cons, klass);

      var paramName = "thisId";
      var type = typeof(int);

      // Add as the method's first parameter
      method.Parameters.Add(new CodeParameterDeclarationExpression(type, paramName));

      // Get the object with Id "thisId" from the corresponding registry
      var thisVarType = typeof(T);
      var thisVarName = "thisObj";

      // Create the local variable to store the reference to ...
      var thisVarDeclaration = new CodeVariableDeclarationStatement(
          typeof(T),
          thisVarName);

      method.Statements.Add(thisVarDeclaration);

      // reference the registry
      CodeFieldReferenceExpression registryReference =
          new CodeFieldReferenceExpression(
          new CodeThisReferenceExpression(), RegFieldName);

      CodeVariableReferenceExpression thisVarReference = new CodeVariableReferenceExpression(thisVarName);

      // eg. IElement thisIElement = Svc.SM.Registry.Element[thisId];
      CodeAssignStatement thisVarAssignment = new CodeAssignStatement(
          new CodeVariableReferenceExpression(thisVarName),
          new CodeIndexerExpression(registryReference, new CodeArgumentReferenceExpression(paramName)));

      method.Statements.Add(thisVarAssignment);

      // Create a reference to the method we want to call on thisVar
      return new CodeMethodReferenceExpression(thisVarReference, targetMethodName);

    }

    public void TransformMethodNonThisParam(ParameterInfo param, List<CodeExpression> targetMethodArgs, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass)
    {
      AddRegFieldIfNotExists(cons, klass);

      var paramName = param.Name + "Id";
      var paramType = typeof(int);

      method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));

      // eg. IElement :: bool MoveTo(IElement parent)
      var localVarType = typeof(T); // IElement
      var localVarName = param.Name; // eg. parent

      // Declare localVar
      var localVarDeclaration = new CodeVariableDeclarationStatement(localVarType, localVarName);
      method.Statements.Add(localVarDeclaration);

      CodeFieldReferenceExpression wrappedObjReference =
          new CodeFieldReferenceExpression(
          new CodeThisReferenceExpression(), RegFieldName);

      // Assign to localVar
      var localVarAssignment = new CodeAssignStatement(
        new CodeVariableReferenceExpression(localVarName),
        new CodeIndexerExpression(wrappedObjReference, new CodeArgumentReferenceExpression(paramName)));

      method.Statements.Add(localVarAssignment);

      targetMethodArgs.Add(new CodeVariableReferenceExpression(localVarName));

    }

    public void TransformResistryMemberGetter(string targetProp, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass)
    {

      AddRegFieldIfNotExists(cons, klass);

      var paramName = "thisId";
      var paramType = typeof(int);

      // Add as the method's first parameter
      method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));

      var thisVarType = typeof(T);
      var thisVarName = "obj";
      // Create the local variable to store the reference to the registry member
      var thisVarDeclaration = new CodeVariableDeclarationStatement(
          thisVarType,
          thisVarName);

      method.Statements.Add(thisVarDeclaration);

      // reference the registry
      CodeFieldReferenceExpression registryReference =
          new CodeFieldReferenceExpression(
          new CodeThisReferenceExpression(), RegFieldName);

      CodeVariableReferenceExpression thisVarReference = new CodeVariableReferenceExpression(thisVarName);

      // Get the object with Id "thisId" from the corresponding registry
      // eg. IElement thisIElement = Svc.SM.Registry.Element[thisId];
      CodeAssignStatement thisVarAssignment = new CodeAssignStatement(
          new CodeVariableReferenceExpression(thisVarName),
          new CodeIndexerExpression(registryReference, new CodeArgumentReferenceExpression(paramName)));

      method.Statements.Add(thisVarAssignment);

      CodePropertyReferenceExpression propertyReference =
        new CodePropertyReferenceExpression(thisVarReference, targetProp);

      method.Statements.Add(new CodeMethodReturnStatement(propertyReference));
    }

    public CodePropertyReferenceExpression TransformSetterMethodThisParam(string propName, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass)
    {
      AddRegFieldIfNotExists(cons, klass);

      var paramName = "thisId";
      var paramType = typeof(int);

      // Add as the method's first parameter
      method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));

      var thisVarType = typeof(T);

      var thisVarName = "thisObj";

      // Create the local variable to store the reference to the registry member
      var thisVarDeclaration = new CodeVariableDeclarationStatement(
          typeof(T),
          thisVarName);

      // Get the object with Id "thisId" from the corresponding registry

      method.Statements.Add(thisVarDeclaration);

      // reference the registry
      CodeFieldReferenceExpression registryReference =
          new CodeFieldReferenceExpression(
          new CodeThisReferenceExpression(), RegFieldName);

      // Get the object with Id "thisId" from the corresponding registry
      // eg. IElement thisIElement = Svc.SM.Registry.Element[thisId];
      CodeAssignStatement thisVarAssignment = new CodeAssignStatement(
          new CodeVariableReferenceExpression(thisVarName),
          new CodeIndexerExpression(registryReference, new CodeArgumentReferenceExpression(thisVarName)));

      method.Statements.Add(thisVarAssignment);
      return new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(thisVarName), propName);

    }

    public CodeVariableReferenceExpression TransformSetterMethodNonThisParam(ParameterInfo param, CodeMemberMethod method, CodeConstructor cons, CodeTypeDeclaration klass)
    {

      AddRegFieldIfNotExists(cons, klass);

      var paramName = param.Name + "Id";
      var paramType = typeof(int);

      method.Parameters.Add(new CodeParameterDeclarationExpression(paramType, paramName));

      // eg. IElement :: bool MoveTo(IElement parent)
      var localVarType = typeof(T); // IElement
      var localVarName = param.Name; // eg. parent

      // Declare localVar
      var localVarDeclaration = new CodeVariableDeclarationStatement(localVarType, localVarName);
      method.Statements.Add(localVarDeclaration);

      CodeFieldReferenceExpression wrappedObjReference =
          new CodeFieldReferenceExpression(
          new CodeThisReferenceExpression(), RegFieldName);

      // Assign to localVar
      var localVarAssignment = new CodeAssignStatement(
        new CodeVariableReferenceExpression(localVarName),
        new CodeIndexerExpression(wrappedObjReference, new CodeArgumentReferenceExpression(paramName)));

      method.Statements.Add(localVarAssignment);

      return new CodeVariableReferenceExpression(localVarName);

    }
  }
}

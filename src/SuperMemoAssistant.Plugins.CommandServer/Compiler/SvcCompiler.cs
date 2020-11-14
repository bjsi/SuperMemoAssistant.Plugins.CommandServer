﻿using Anotar.Serilog;
using Microsoft.CSharp;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Sys.Remoting;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SuperMemoAssistant.Plugins.CommandServer.Compiler
{
  public class SvcCompiler : SvcCompilerBase
  {

    private const string WrappedObjName = "_wrapped";
    private object WrappedObj { get; }
    private Type WrappedObjType { get; }
    private CodeConstructor Constructor { get; set; }
    private Dictionary<Type, Type> RegMemberToRegTypeMap { get; }
    private HashSet<Type> Registries { get; }

    private const MemberAttributes PublicFinal = MemberAttributes.Public | MemberAttributes.Final;
    private readonly CompilerParameters CompilerParams = new CompilerParameters { GenerateInMemory = true, GenerateExecutable = false };

    public SvcCompiler(string className,
                       string nameSpace,
                       IEnumerable<string> imports,
                       IEnumerable<string> referencedAssemblies,
                       object wrapped, // TODO: can be null
                       Type wrappedType,
                       Dictionary<Type, Type> regMap)

      : base(className, nameSpace, imports)
    {
      wrappedType.ThrowIfArgumentNull("Failed to create compiler because wrapped object type is null");
      regMap.ThrowIfArgumentNull("Failed to create compiler because reg type map is null");
      referencedAssemblies.ThrowIfArgumentNull("Failed to create compiler because assemblies were null");

      Constructor = CreateConstructor(PublicFinal);
      CompilerParams.ReferencedAssemblies.AddRange(referencedAssemblies.ToArray());
      this.WrappedObj = wrapped;
      this.WrappedObjType = wrappedType;
      this.RegMemberToRegTypeMap = regMap;
      this.Registries = regMap.Values.ToHashSet();
    }

    public SvcCompiler WithAllAttributes()
    {
      return this.WithWrappedObjectField()
                 .WithMethods()
                 .WithEvents()
                 .WithPropertiesAsMethods();
    }

    public string GenerateSourceCode()
    {
      TargetClass.Members.Add(Constructor);

      var codeGeneratorOptions = new CodeGeneratorOptions();
      using (var sw = new StringWriter())
      using (var provider = new CSharpCodeProvider())
      {
        provider.GenerateCodeFromCompileUnit(TargetUnit, sw, codeGeneratorOptions);
        return sw.ToString();
      }
    }

    /// <summary>
    /// Call after running the other methods
    /// </summary>
    /// <returns></returns>
    public CompilerResults Compile()
    {

      TargetClass.Members.Add(Constructor);

      using (var provider = new CSharpCodeProvider())
      {
        var compilerResults = provider.CompileAssemblyFromDom(CompilerParams, TargetUnit);
        if (compilerResults.Errors.HasErrors)
        {
          foreach (var compilerError in compilerResults.Errors)
          {
            var error = compilerError.ToString();
            LogTo.Error(error);
          }

          throw new InvalidOperationException($"Service compilation failed with {compilerResults.Errors.Count} errors");
        }

        LogTo.Debug("Service compiled successfully");
        return compilerResults;
      }
    }

    private void AddVoidActionEvent(CodeMemberEvent newEvent, CodeMemberMethod method)
    {
      // Set the return type of the event
      newEvent.Type = new CodeTypeReference("System.Action");

      // Add the code to invoke the event from the method
      // when the event in the wrapped object fires

      CodeEventReferenceExpression newEventRef = new CodeEventReferenceExpression(
        new CodeThisReferenceExpression(), newEvent.Name);

      var invoke = new CodeDelegateInvokeExpression(newEventRef);
      method.Statements.Add(invoke);
    }

    private void AddActionEventWithRetVal(CodeMemberEvent newEvent, Type eventType, CodeMemberMethod method)
    {
      // Set the return type of the event
      newEvent.Type = new CodeTypeReference("System.Action",
        new CodeTypeReference[] { new CodeTypeReference(eventType) });

      // Add the code to invoke the event from the method
      // when the event in the wrapped object fires

      // Add event parameters to the method
      var obj = new CodeParameterDeclarationExpression(eventType, "e");
      method.Parameters.Add(obj);

      CodeEventReferenceExpression newEventRef = new CodeEventReferenceExpression(
        new CodeThisReferenceExpression(), newEvent.Name);
      
      // Invoke the event with the parameters from the wrapped object event
      CodeArgumentReferenceExpression eventArg = new CodeArgumentReferenceExpression("e");
      var invoke = new CodeDelegateInvokeExpression(newEventRef,
              new CodeExpression[] { eventArg });

      method.Statements.Add(invoke);
    }

    private CodeMemberMethod CreateMethod(string name, Type retType, MemberAttributes attrs)
    {
      CodeMemberMethod method = new CodeMemberMethod();
      method.Name = name;
      method.ReturnType = new CodeTypeReference(retType);
      method.Attributes = attrs;
      return method;
    }

    private CodeMemberEvent CreateEvent(string name, MemberAttributes attrs)
    {
      var newEvent = new CodeMemberEvent();
      newEvent.Name = name;
      newEvent.Attributes = attrs;
      return newEvent;
    }

    /// <summary>
    /// TODO: Explain why this is necessary
    /// TODO: Not working with registry member types like IElement yet.
    /// </summary>
    public SvcCompiler WithEvents()
    {

      // Skip registry member types like IElement
      if (RegMemberToRegTypeMap.ContainsKey(WrappedObjType))
        return this;

      // Necessary because we are using ActionProxy to sub to wrapped obj events
      var pmi = "PluginManager.Interop";
      bool add = true;
      foreach (var s in CompilerParams.ReferencedAssemblies)
      {
        if (s.Contains(pmi))
        {
          add = false;
          break;
        }
      }

      if (add) CompilerParams.ReferencedAssemblies.Add(pmi + ".Dll");

      foreach (var e in WrappedObjType.GetEvents())
      {
        var eventName = e.Name;
        var eventType = e.EventHandlerType
          .GetGenericArguments()
          .FirstOrDefault();

        // Creates an Action event that doesn't require ActionProxy when subscribing
        var newEvent = CreateEvent(eventName, PublicFinal);

        // Create method a which gets called when the Action event fires, and forwards the event to the normal C# event
        string methodName = "Forward" + eventName + "Event";
        var privateFinal = MemberAttributes.Private | MemberAttributes.Final;
        CodeMemberMethod newMethod = CreateMethod(methodName, typeof(void), privateFinal);

        if (eventType == null)
          AddVoidActionEvent(newEvent, newMethod);
        else
          AddActionEventWithRetVal(newEvent, eventType, newMethod);

        TargetClass.Members.Add(newEvent);
        TargetClass.Members.Add(newMethod);

        // Add statements to the constructor to invoke the new event when the event in the
        // wrapped object fires.

        var actionProxyType = eventType == null
          ? typeof(ActionProxy)
          : typeof(ActionProxy<>).MakeGenericType(eventType);

        CodeDelegateCreateExpression createDelegate1 = new CodeDelegateCreateExpression(
        new CodeTypeReference(actionProxyType), new CodeThisReferenceExpression(), methodName);

        CodeFieldReferenceExpression wrappedObjReference =
            new CodeFieldReferenceExpression(
            new CodeThisReferenceExpression(), WrappedObjName);

        CodeEventReferenceExpression actionEventRef = new CodeEventReferenceExpression(
          wrappedObjReference, eventName);

        // Attaches an EventHandler delegate pointing to TestMethod to the TestEvent event.
        CodeAttachEventStatement attachStatement1 = new CodeAttachEventStatement(actionEventRef, createDelegate1);

        Constructor.Statements.Add(attachStatement1);
      }

      return this;
    }

    private CodeMemberField CreateField(string name, Type type, MemberAttributes attrs)
    {
      CodeMemberField field = new CodeMemberField();
      field.Name = name;
      field.Type = new CodeTypeReference(type);
      field.Attributes = attrs;
      return field;
    }

    public SvcCompiler WithWrappedObjectField()
    {
      // Skip if the Type is a Registry member type
      if (!RegMemberToRegTypeMap.ContainsKey(WrappedObjType))
      {
        // Add wrapped object parameter
        const string parameterName = "obj";
        Constructor.Parameters.Add(new CodeParameterDeclarationExpression(
                WrappedObjType, parameterName));

        // Add wrapped object field initialization logic
        CodeFieldReferenceExpression wrappedObjReference =
            new CodeFieldReferenceExpression(
            new CodeThisReferenceExpression(), WrappedObjName);

        // Assign to the wrapped object field
        Constructor.Statements.Add(new CodeAssignStatement(wrappedObjReference,
            new CodeArgumentReferenceExpression(parameterName)));

        var field = CreateField(WrappedObjName, WrappedObjType, MemberAttributes.Private);
        TargetClass.Members.Add(field);
      }

      return this;
    }

    private CodeConstructor CreateConstructor(MemberAttributes attrs)
    {
      var cons = new CodeConstructor();
      cons.Attributes = attrs;
      return cons;
    }

    public SvcCompiler WithPropertiesAsMethods()
    {
      foreach (var p in WrappedObjType.GetProperties())
      {
        var get = p.GetMethod;
        if (get != null && get.Attributes.HasFlag(MethodAttributes.Public))
          ConvertGetAccessorToMethod(get);

        var set = p.SetMethod;
        if (set != null && set.Attributes.HasFlag(MethodAttributes.Public))
          ConvertSetAccessorToMethod(set);
      }
      return this;
    }

    private void ConvertGetAccessorToMethod(MethodInfo get)
    {
      var propName = get.Name.Split('_')[1];
      var type = get.ReturnType;
      CodeMemberMethod newMethod = CreateMethod("Get" + propName, type, PublicFinal);

      bool isRegMemberType = RegMemberToRegTypeMap.ContainsKey(WrappedObjType);

      if (isRegMemberType)
      {
        var regType = RegMemberToRegTypeMap[WrappedObjType];
        var transformer = CreateTransformer(regType);
        transformer.TransformResistryMemberGetter(propName, newMethod, Constructor, TargetClass);
      }
      else
      {
        // Body
        CodeFieldReferenceExpression wrappedObjReference =
            new CodeFieldReferenceExpression(
            new CodeThisReferenceExpression(), WrappedObjName);

        CodePropertyReferenceExpression propertyReference =
          new CodePropertyReferenceExpression(wrappedObjReference, propName);

        newMethod.Statements.Add(new CodeMethodReturnStatement(propertyReference));
      }

      TargetClass.Members.Add(newMethod);
    }

    private ITransformer CreateTransformer(Type type)
    {
      if (!Registries.Contains(type))
        throw new InvalidOperationException("Failed to create transformer because type is not a registry type");
      var transType = typeof(RegMemberMethodTransformer<>).MakeGenericType(type);
      return (ITransformer)Activator.CreateInstance(transType, RegMemberToRegTypeMap);
    }

    private void ConvertSetAccessorToMethod(MethodInfo set)
    {
      var propName = set.Name.Split('_')[1];
      var parameter = set.GetParameters()[0];
      var paramType = parameter.ParameterType;

      CodeMemberMethod newMethod = CreateMethod("Set" + propName, typeof(void), PublicFinal);

      bool isRegMemberType = RegMemberToRegTypeMap.ContainsKey(WrappedObjType);

      CodeFieldReferenceExpression wrappedObjReference =
          new CodeFieldReferenceExpression(
          new CodeThisReferenceExpression(), WrappedObjName);

      CodePropertyReferenceExpression targetProp =
        new CodePropertyReferenceExpression(wrappedObjReference, propName);

      // Transformation #1
      if (isRegMemberType)
      {
        var regType = RegMemberToRegTypeMap[WrappedObjType];
        var transformer = CreateTransformer(regType);
        // Changes the target property to the locally defined registry member
        targetProp = transformer.TransformSetterMethodThisParam(propName, newMethod, Constructor, TargetClass);
      }

      // Transformation #2
      if (RegMemberToRegTypeMap.ContainsKey(paramType))
      {
        var regType = RegMemberToRegTypeMap[paramType];
        var transformer = CreateTransformer(regType);

        // Changes the target property to the locally defined registry member
        var localArg = transformer.TransformSetterMethodNonThisParam(parameter, newMethod, Constructor, TargetClass);
        newMethod.Statements.Add(new CodeAssignStatement(targetProp, localArg));
        return;
      }

      var param = new CodeParameterDeclarationExpression(paramType, propName);
      newMethod.Parameters.Add(param);
      newMethod.Statements.Add(new CodeAssignStatement(targetProp, new CodeArgumentReferenceExpression(propName)));
      TargetClass.Members.Add(newMethod);
    }

    // TODO: staticmethod
    private List<MethodInfo> GetExtendedInterfaceMethods(Type t)
    {
      var allMethods = new List<MethodInfo>();
      foreach (var iface in t.GetInterfaces())
      {
        foreach (var method in iface.GetMethods().Where(x => !x.IsSpecialName))
        {
          if (!allMethods.Any(x => x.Name == method.Name))
          {
            allMethods.Add(method);
          }
        }
      }

      return allMethods;
    }

    public SvcCompiler WithMethods()
    {

      Type type = WrappedObjType;
      bool isRegMemberType = RegMemberToRegTypeMap.ContainsKey(WrappedObjType);

      var methods = type
        .GetMethods()
        .Where(x => !x.IsSpecialName)
        .Where(x => !x.GetParameters().Any(p => p.IsOut)) // TODO: excludes the Add() function on registries
        .ToList();

      // Need to add all methods from interfaces including extended interfaces
      if (type.IsInterface)
        methods.AddRange(GetExtendedInterfaceMethods(type));

      // All methods where the declaring class or a paramteter is a specical reg type
      // have to be modified to pass a unique id which is used to retrive the instance
      // from a registry

      foreach (var m in methods)
      {
        var targetMethodName = m.Name;
        var oldParams = m.GetParameters().ToList();
        var newMethod = CreateMethod(m.Name, m.ReturnType, PublicFinal);

        var wrappedObjReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), WrappedObjName);
        var targetMethodRef = new CodeMethodReferenceExpression(wrappedObjReference, targetMethodName);
        var targetMethodArgs = new List<CodeExpression>();

        if (isRegMemberType)
        {
          var regType = RegMemberToRegTypeMap[WrappedObjType];
          var transformer = CreateTransformer(regType);
          // Changes the target method call to the registry object retrieved inside the method
          targetMethodRef = transformer.TransformMethodThisParam(targetMethodName, newMethod, Constructor, TargetClass);
        }

        foreach (var p in oldParams)
        {
          var t = p.ParameterType;

          if (RegMemberToRegTypeMap.ContainsKey(t))
          {
            var regType = RegMemberToRegTypeMap[t];
            var transformer = CreateTransformer(regType);
            transformer.TransformMethodNonThisParam(p, targetMethodArgs, newMethod, Constructor, TargetClass);
          }
          else
          {
            var name = p.Name;
            var param = new CodeParameterDeclarationExpression(t, name);
            newMethod.Parameters.Add(param);
            targetMethodArgs.Add(new CodeArgumentReferenceExpression(name));
          }
        }

        newMethod.Statements.Add(
            new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(targetMethodRef, targetMethodArgs.ToArray())));

        TargetClass.Members.Add(newMethod);

      }

      return this;
    }
  }
}

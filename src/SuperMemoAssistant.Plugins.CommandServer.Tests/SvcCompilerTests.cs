#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   11/4/2020 9:50:20 PM
// Modified By:  james

#endregion




using SuperMemoAssistant.Interop.SuperMemo;
using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
using SuperMemoAssistant.Interop.SuperMemo.Elements;
using SuperMemoAssistant.Interop.SuperMemo.Registry.Types;
using SuperMemoAssistant.Plugins.CommandServer.Compiler;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SuperMemoAssistant.Plugins.CommandServer.Tests
{
  public class SvcCompilerTests
  {

    public class Test
    {
      public string Name { get; set; } = "James";
      public int AddInt(int a, int b) => a + b;
      public event Action<int> OnEvent;
    }

    private Test obj = new Test();

    [Fact]
    public void CompilerConvertsActionToEvents()
    {

      var refs = obj.GetType().GetReferencedAssemblyPaths().ToList();
      var compiler = new SvcCompiler(Shared.ClassName,
                                     Shared.NameSpace,
                                     Shared.Imports,
                                     refs,
                                     obj,
                                     typeof(Test),
                                     Shared.RegistryMap);

      compiler.WithWrappedObjectField()
              .WithEvents();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var objec = Activator.CreateInstance(type, obj);
      Assert.NotNull(objec);

      var ev = objec.GetType().GetEvent("OnEvent");
      Assert.NotNull(ev);

    }

    [Fact]
    public void CompilerAddsMethods()
    {
      var refs = obj.GetType().GetReferencedAssemblyPaths();
      var compiler = new SvcCompiler(Shared.ClassName,
                                     Shared.NameSpace,
                                     Shared.Imports,
                                     refs,
                                     obj,
                                     typeof(Test),
                                     Shared.RegistryMap);

      compiler.WithWrappedObjectField()
              .WithMethods();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      dynamic objec = Activator.CreateInstance(type, obj);
      Assert.NotNull(objec);

      var x = objec.AddInt(1, 2);
      Assert.Equal(3, x);

    }

    [Fact]
    public void CompilerConvertsPropertiesToMethods()
    {
      var obj = new Test();
      var refs = obj.GetType().GetReferencedAssemblyPaths();
      var compiler = new SvcCompiler(Shared.ClassName,
                                     Shared.NameSpace,
                                     Shared.Imports,
                                     refs,
                                     obj,
                                     typeof(Test),
                                     Shared.RegistryMap);

      compiler.WithWrappedObjectField()
              .WithPropertiesAsMethods();

      var compilerResults = compiler.Compile();

      var type = compilerResults.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      dynamic objec = Activator.CreateInstance(type, obj);
      Assert.NotNull(objec);

      var x = objec.GetName();
      Assert.Equal("James", x);

      objec.SetName("Frank");
      var result = objec.GetName();
      Assert.Equal("Frank", result);

    }

    [Fact]
    public void ObjectGetsWrappedSuccessfully()
    {
      var refs = obj.GetType().GetReferencedAssemblyPaths();
      var compiler = new SvcCompiler(Shared.ClassName,
                                     Shared.NameSpace,
                                     Shared.Imports,
                                     refs,
                                     obj,
                                     typeof(Test),
                                     Shared.RegistryMap);

      compiler.WithWrappedObjectField();

      var output = compiler.Compile();

      var type = output.CompiledAssembly.GetType($"{Shared.NameSpace}.{Shared.ClassName}");
      Assert.NotNull(type);

      var result = Activator.CreateInstance(type, obj);
      Assert.NotNull(result);

      var resultType = result.GetType();
      var field = resultType.GetField("_wrapped", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.NotNull(field);

    }
  }
}

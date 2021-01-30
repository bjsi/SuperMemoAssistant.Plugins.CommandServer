using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Anotar.Serilog;
using SMAInteropConverter;
using SMAInteropConverter.Helpers;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Plugins.CommandServer.Helpers;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.UI.Configuration;

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
// Created On:   11/4/2020 9:50:13 PM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.CommandServer
{

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class CommandServerPlugin : SMAPluginBase<CommandServerPlugin>
  {
    #region Constructors

    #endregion




    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "CommandServer";

    /// <inheritdoc />
    public override bool HasSettings => true;
    public CommandServerCfg Config { get; private set; }
    private WebsocketServer Server { get; set; }

    #endregion

    #region Methods Impl

    private List<object> GetAllPropertyValues(object obj)
    {
      var vals = new List<object>();
      var type = obj.GetType();
      foreach (var property in type.GetProperties().Where(x => !x.IsSpecialName))
      {
        vals.Add(property.GetValue(obj));
      }

      return vals;
    }

    private async Task LoadConfig()
    {
      Config = await Svc.Configuration.Load<CommandServerCfg>().ConfigureAwait(false) ?? new CommandServerCfg();
    }

    private string GetCurrentInteropVersion()
    {
      var a = Assembly.GetExecutingAssembly()
                      .GetReferencedAssemblies()
                      .First(x => x.Name == "SuperMemoAssistant.Interop");
      return Assembly.ReflectionOnlyLoad(a.FullName).Location;
    }

    private Type CompileService(List<RegistryType> regTypes, Type type)
    {
      var converter = new Converter(regTypes, type, s => LogTo.Debug(s));
      var results = converter.WithGetters()
                             .WithSetters()
                             .WithMethods()
                             .WithEvents()
                             .Compile(type.GetReferencedAssemblyPaths());
      return results.CompiledAssembly.GetType($"{type.GetSvcNamespace()}.{type.GetSvcName()}");
    }

    /// <inheritdoc />
    protected override void PluginInit()
    {

      LoadConfig().Wait();

      var dll = GetCurrentInteropVersion();
      var typeExtractor = new InteropTypeExtractor(dll);

      var regPairs = typeExtractor.GetRegistries();

      var services = new List<object>();
      var skip = new string[] { "IText", "IVideo", "ISound", "IImage", "ITemplate" };

      // Add registry services
      foreach (var regPair in regPairs)
      {
        if (skip.Any(x => regPair.Registry.Name.Contains(x)))
          continue;

        var regSvc = Activator.CreateInstance(CompileService(regPairs, regPair.Registry));
        if (regSvc == null)
          LogTo.Debug($"Registry service {regPair.Registry.Name} was null");
        services.Add(regSvc);

        var regMemSvc = Activator.CreateInstance(CompileService(regPairs, regPair.Member));
        if (regMemSvc == null)
          LogTo.Debug($"Registry member service {regPair.Member.Name} was null");
        services.Add(regMemSvc);
      }

      var uis = typeExtractor.GetUITypes();
      foreach (var ui in uis)
      {
        var uiSvc = Activator.CreateInstance(CompileService(regPairs, ui));
        if (uiSvc == null)
          LogTo.Debug($"UI service {ui.Name} was null");
        services.Add(uiSvc);
      }

      Server = new WebsocketServer(services);
      Server.Start(Config.Host, Config.Port);
    }

    public override void Dispose()
    {
      Server.StopAsync();
      base.Dispose();
    }

    /// <inheritdoc />
    public override void ShowSettings()
    {
      ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, Config);
    }

    #endregion


    #region Methods

    #endregion
  }
}

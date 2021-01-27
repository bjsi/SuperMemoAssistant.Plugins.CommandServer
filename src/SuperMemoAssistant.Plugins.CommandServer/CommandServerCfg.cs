using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;
using System.ComponentModel;

namespace SuperMemoAssistant.Plugins.CommandServer
{
  [Form(Mode = DefaultFields.None)]
  [Title("Media Player Settings",
          IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
                "Cancel",
                IsCancel = true)]
  [DialogAction("save",
                "Save",
                IsDefault = true,
                Validates = true)]
  public class CommandServerCfg : CfgBase<CommandServerCfg>, INotifyPropertyChangedEx
  {

    [Field(Name = "Command Server Host")]
    public string Host { get; set; } = "localhost";

    [Field(Name = "Command Server Port")]
    public int Port { get; set; } = 13000;

    #region Methods Impl

    public override string ToString()
    {
      return "Command Server";
    }

    [JsonIgnore]
    public bool IsChanged { get; set; }

    #endregion

    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}

﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestNamespace
{


  public class TestClass
  {

    private SuperMemoAssistant.Interop.SuperMemo.Elements.IElementRegistry _IElementRegistry;

    public TestClass(SuperMemoAssistant.Interop.SuperMemo.Elements.IElementRegistry _ielementregistry)
    {
      this._IElementRegistry = _ielementregistry;
    }

    public int GetId(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.Id;
    }

    public string GetTitle(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.Title;
    }

    public bool GetDeleted(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.Deleted;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Elements.Models.ElementType GetType(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.Type;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Content.IComponentGroup GetComponentGroup(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.ComponentGroup;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Registry.Members.ITemplate GetTemplate(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.Template;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Registry.Members.IConcept GetConcept(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.Concept;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement GetParent(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.Parent;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement GetFirstChild(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.FirstChild;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement GetLastChild(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.LastChild;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement GetNextSibling(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.NextSibling;
    }

    public SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement GetPrevSibling(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.PrevSibling;
    }

    public int GetDescendantCount(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.DescendantCount;
    }

    public int GetChildrenCount(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.ChildrenCount;
    }

    public System.Collections.Generic.IEnumerable<SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement> GetChildren(int thisId)
    {
      SuperMemoAssistant.Interop.SuperMemo.Elements.Types.IElement obj;
      obj = this._IElementRegistry[thisId];
      return obj.Children;
    }
  }
}
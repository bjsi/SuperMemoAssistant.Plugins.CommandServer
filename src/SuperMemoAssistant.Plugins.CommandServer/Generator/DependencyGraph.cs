﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.CommandServer.Generator
{
  // From: https://github.com/marccarre/DependencyGraph 
  public class DirectedGraph<T>
  {
    private readonly IDictionary<T, Edges> graph = new Dictionary<T, Edges>();

    public Edges AddVertex(T vertex)
    {
      Edges adjacencySets;
      if (!graph.TryGetValue(vertex, out adjacencySets))
      {
        adjacencySets = new Edges();
        graph[vertex] = adjacencySets;
      }
      return adjacencySets;
    }

    public void AddEdge(T from, T to)
    {
      AddVertex(from).outBound.Add(to);
      AddVertex(to).inBound.Add(from);
    }

    public Edges EdgesFor(T vertex)
    {
      return graph[vertex];
    }

    public IEnumerable<T> DependeesFor(T rootVertex, int maxDistance = -1)
    {
      var dependencies = new List<T>();
      var visited = new HashSet<T>();
      var verticesToBreadthFirstSearch = new Queue<Item>();

      visited.Add(rootVertex);
      verticesToBreadthFirstSearch.Enqueue(new Item(rootVertex, 0));
      while (verticesToBreadthFirstSearch.Any())
      {
        var item = verticesToBreadthFirstSearch.Dequeue();
        if (item.distance == maxDistance)
          continue;

        foreach (var dependency in graph[item.vertex].inBound)
        {
          if (!visited.Contains(dependency))
          {
            visited.Add(dependency);
            dependencies.Add(dependency);
            verticesToBreadthFirstSearch.Enqueue(new Item(dependency, item.distance + 1));
          }
        }
      }

      return dependencies;
    }

    private class Item
    {
      public T vertex { get; private set; }
      public int distance { get; private set; }
      public Item(T vertex, int distance)
      {
        this.vertex = vertex;
        this.distance = distance;
      }
    }

    public class Edges
    {
      [JsonProperty("isUsedBy")]
      public ISet<T> inBound { get; }

      [JsonProperty("dependsOn")]
      public ISet<T> outBound { get; }

      public Edges()
      {
        inBound = new HashSet<T>();
        outBound = new HashSet<T>();
      }
    }

    public int Count()
    {
      return graph.Count();
    }

    public void ExportAsJson(TextWriter outputWriter, Formatting formatting = Formatting.Indented)
    {
      var serializer = new JsonSerializer();
      serializer.Formatting = formatting;
      using (var jsonWriter = new JsonTextWriter(outputWriter))
      {
        serializer.Serialize(jsonWriter, graph);
      }
    }
  }
}

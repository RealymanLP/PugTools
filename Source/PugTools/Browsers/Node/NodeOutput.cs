using System;

namespace PugTools {
  internal class NodeOutput {
    internal String node;
    internal String item;
    internal String parent;
    internal String name;
    internal String value;

    internal NodeOutput(String node, String item, String parent, String name, String value) {
      this.node = node;
      this.item = item;
      this.parent = parent;
      this.name = name;
      this.value = value;
    }
  }
}

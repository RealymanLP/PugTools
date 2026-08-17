using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PugTools;

namespace TreeViewFast.Controls {

  public class TreeViewFast : TreeView {
    #region Fields
    private readonly Dictionary<String, TreeNode> _treeNodes = new Dictionary<String, TreeNode>();
    #endregion

    #region Properties
    #endregion

    #region  Methods
    /// <summary>
    /// Load the TreeView with items.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="items">Collection of items</param>
    /// <param name="getId">Function to parse Id value from item object</param>
    /// <param name="getParentId">Function to parse parentId value from item object</param>
    /// <param name="getDisplayName">Function to parse display name value from item object. 
    /// This is used as node text.</param>
    public void LoadItems<T>(IEnumerable<T> items,
                             Func<T, String> getId,
                             Func<T, String> getParentId,
                             Func<T, String> getDisplayName) {

      // Clear view and internal dictionary
      Nodes.Clear();
      _treeNodes.Clear();

      // Load internal dictionary with nodes
      foreach (T item in items) {
        String id = getId(item);
        String displayName = getDisplayName(item);
        TreeNode node = new TreeNode {
          Name = id.ToString(),
          Text = displayName,
          Tag = item
        };
        _treeNodes.Add(getId(item), node);
      }

      // Create hierarchy and load into view
      foreach (String id in _treeNodes.Keys) {
        TreeNode node = GetNode(id);
        T obj = (T)node.Tag;
        String parentId = getParentId(obj);

        if (parentId != "") {
          TreeNode parentNode = GetNode(parentId);
          parentNode.Nodes.Add(node);
        } else {
          Nodes.Add(node);
        }
      }
    }

    /// <summary>
    /// Get a handle to the object collection.
    /// This is convenient if you want to search the object collection.
    /// </summary>
    public IQueryable<T> GetItems<T>() {
      return _treeNodes.Values.Select(x => (T)x.Tag).AsQueryable();
    }

    /// <summary>
    /// Retrieve TreeNode by Id.
    /// Useful when you want to select a specific node.
    /// </summary>
    /// <param name="id">Item id</param>
    public TreeNode GetNode(String id) {
      return _treeNodes[id];
    }

    /// <summary>
    /// Retrieve item object by Id.
    /// Useful when you want to get hold of object for reading or further manipulating.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="id">Item id</param>
    /// <returns>Item object</returns>
    public T GetItem<T>(String id) {
      return (T)GetNode(id).Tag;
    }


    /// <summary>
    /// Get parent item.
    /// Will return NULL if item is at top level.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="id">Item id</param>
    /// <returns>Item object</returns>
    public T GetParent<T>(String id) where T : class {
      TreeNode parentNode = GetNode(id).Parent;
      return parentNode == null ? null : (T)Parent.Tag;
    }

    /// <summary>
    /// Retrieve descendants to specified item.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="id">Item id</param>
    /// <param name="deepLimit">Number of generations to traverse down. 1 means only direct 
    /// children. Null means no limit.</param>
    /// <returns>List of item objects</returns>
    public List<T> GetDescendants<T>(String id, Int32? deepLimit = null) {
      TreeNode node = GetNode(id);
      IEnumerator enumerator = node.Nodes.GetEnumerator();
      List<T> items = new List<T>();

      if (deepLimit.HasValue && deepLimit.Value <= 0)
        return items;

      while (enumerator.MoveNext()) {
        // Add child
        TreeNode childNode = (TreeNode)enumerator.Current;
        T childItem = (T)childNode.Tag;
        items.Add(childItem);

        // If requested add grandchildren recursively
        Int32? childDeepLimit = deepLimit.HasValue ? deepLimit.Value - 1 : null;

        if (!deepLimit.HasValue || childDeepLimit > 0) {
          String childId = childNode.Name.ToString();
          List<T> descendants = GetDescendants<T>(childId, childDeepLimit);
          items.AddRange(descendants);
        }
      }
      return items;
    }

    internal /*async*/ void LoadItems<T1>(Dictionary<String, TreeListItem> testDict,
                                Func<TreeListItem, String> getId,
                                Func<TreeListItem, String> getParentId,
                                Func<TreeListItem, String> getDisplayName) {
      // Clear view and internal dictionary
      Nodes.Clear();
      _treeNodes.Clear();

      List<String> keys = testDict.Keys.ToList();
      keys.Sort(delegate (String x, String y) {
        TreeListItem tx = testDict[x];
        TreeListItem ty = testDict[y];

        if (tx.HashInfo.File == null && ty.HashInfo.File != null)
          return -1;
        else if (tx.HashInfo.File != null && ty.HashInfo.File == null)
          return 1;
        else if (tx.HashInfo.File == null && ty.HashInfo.File == null)
          return String.Compare(x, y);
        else
          return String.Compare(x, y);
      });

      // Load internal dictionary with nodes
      // _treeNodes = await Task.Run(async delegate {
      //   Dictionary<String, TreeNode> treeNodes = new Dictionary<String, TreeNode>();

      //   foreach (String key in keys) {
      //     TreeListItem item = testDict[key];
      //     String id = await Task.Run(() => getId(item));
      //     String displayName = await Task.Run(() => getDisplayName(item));
      //     TreeNode node = new TreeNode {
      //       Name = id.ToString(),
      //       Text = displayName,
      //       Tag = item
      //     };

      //     if (item.HashInfo.File != null) {
      //       node.ImageIndex = 2;
      //       node.SelectedImageIndex = 2;
      //     } else {
      //       node.ImageIndex = 1;
      //       node.SelectedImageIndex = 1;
      //     }

      //     treeNodes.Add(id, node);
      //   }

      //   return treeNodes;
      // });

      // Load internal dictionary with nodes
      foreach (String key in keys) {
        TreeListItem item = testDict[key];
        String id = getId(item);
        String displayName = getDisplayName(item);
        TreeNode node = new TreeNode {
          Name = id.ToString(),
          Text = displayName,
          Tag = item
        };

        if (item.HashInfo.File != null) {
          node.ImageIndex = 2;
          node.SelectedImageIndex = 2;
        } else {
          node.ImageIndex = 1;
          node.SelectedImageIndex = 1;
        }

        _treeNodes.Add(getId(item), node);
      }

      // TreeNode[] nodeArr = await Task.Run(async delegate {
      //   List<TreeNode> nodes = new List<TreeNode>();

      //   foreach (String id in _treeNodes.Keys) {
      //     TreeNode node = await Task.Run(() => GetNode(id));
      //     TreeListItem obj = node.Tag as TreeListItem;
      //     String parentId = await Task.Run(() => getParentId(obj));

      //     if (!String.IsNullOrEmpty(parentId)) {
      //       TreeNode parentNode = await Task.Run(() => GetNode(parentId));
      //       parentNode.Nodes.Add(node);
      //     } else {
      //       nodes.Add(node);
      //     }
      //   }
      //   return nodes.ToArray();
      // });

      // Invoke(new Action(() => Nodes.AddRange(nodeArr)));

      // Create hierarchy and load into view
      foreach (String id in _treeNodes.Keys) {
        TreeNode node = GetNode(id);
        TreeListItem obj = (TreeListItem)node.Tag;
        String parentId = getParentId(obj);

        if (parentId != "") {
          TreeNode parentNode = GetNode(parentId);
          parentNode.Nodes.Add(node);
        } else {
          Nodes.Add(node);
        }
      }
    }

    internal void LoadItems<T1>(Dictionary<String, TreeListItem> assetDict,
                                Func<TreeListItem, String> getId,
                                Func<TreeListItem, String> getParentId,
                                Func<TreeListItem, String> getDisplayName,
                                String filter,
                                String type) {

      if (filter == null) throw new ArgumentNullException(nameof(filter));
      if (type == null) throw new ArgumentNullException(nameof(type));

      // Clear view and internal dictionary
      Nodes.Clear();
      _treeNodes.Clear();

      List<String> keys = assetDict.Keys.ToList();
      keys.Sort();

      // Load internal dictionary with nodes
      foreach (String key in keys) {
        TreeListItem item = assetDict[key];
        String id = getId(item);
        String displayName = getDisplayName(item);
        TreeNode node = new TreeNode {
          Name = id.ToString(),
          Text = displayName,
          Tag = item
        };

        if (item.HashInfo.File != null) {
          node.ImageIndex = 2;
          node.SelectedImageIndex = 2;
        } else {
          node.ImageIndex = 1;
          node.SelectedImageIndex = 1;
        }

        _treeNodes.Add(getId(item), node);
      }

      // Create hierarchy and load into view
      foreach (String id in _treeNodes.Keys) {
        TreeNode node = GetNode(id);
        TreeListItem obj = (TreeListItem)node.Tag;
        String parentId = getParentId(obj);

        if (parentId != "") {
          TreeNode parentNode = GetNode(parentId);
          parentNode.Nodes.Add(node);
        } else {
          Nodes.Add(node);
        }
      }
    }

    internal void LoadItems<T1>(Dictionary<String, NodeAsset> assetDict,
                                Func<NodeAsset, String> getId,
                                Func<NodeAsset, String> getParentId,
                                Func<NodeAsset, String> getDisplayName) {
      // Clear view and internal dictionary
      Nodes.Clear();
      _treeNodes.Clear();

      List<String> keys = assetDict.Keys.ToList();
      keys.Sort(delegate (String x, String y) {
        NodeAsset tx = assetDict[x];
        NodeAsset ty = assetDict[y];

        if (tx.Obj == null
            && tx.dynObject == null
            && tx.objData == null
            && (ty.Obj != null || ty.dynObject != null || ty.objData != null))
          return -1;
        else if ((tx.Obj != null
                  || tx.dynObject != null
                  || tx.objData != null)
                  && ty.Obj == null && ty.dynObject == null && ty.objData == null)
          return 1;
        else if (tx.Obj == null
                 && tx.dynObject == null
                 && tx.objData == null
                 && ty.Obj == null
                 && ty.dynObject == null
                 && ty.objData == null)
          return String.Compare(x, y);
        else
          return String.Compare(tx.id, ty.id);
      });

      // Load internal dictionary with nodes
      foreach (String key in keys) {
        NodeAsset item = assetDict[key];
        String id = getId(item);
        String displayName = getDisplayName(item);
        TreeNode node = new TreeNode {
          Name = id.ToString(),
          Text = displayName,
          Tag = item
        };

        if (item.Obj != null || item.dynObject != null || item.objData != null) {
          node.ImageIndex = 2;
          node.SelectedImageIndex = 2;
        } else {
          node.ImageIndex = 1;
          node.SelectedImageIndex = 1;
        }

        _treeNodes.Add(getId(item), node);
      }

      // Create hierarchy and load into view
      foreach (String id in _treeNodes.Keys) {
        TreeNode node = GetNode(id);
        NodeAsset obj = (NodeAsset)node.Tag;
        String parentId = getParentId(obj);

        if (parentId != "") {
          TreeNode parentNode = GetNode(parentId);
          parentNode.Nodes.Add(node);
        } else {
          Nodes.Add(node);
        }
      }
    }

    internal void LoadItems<T1>(Dictionary<String, ArchTreeListItem> testDict,
                                Func<ArchTreeListItem, String> getId,
                                Func<ArchTreeListItem, String> getParentId,
                                Func<ArchTreeListItem, String> getDisplayName) {

      // Clear view and internal dictionary
      Nodes.Clear();
      _treeNodes.Clear();

      List<String> keys = testDict.Keys.ToList();
      keys.Sort(delegate (String x, String y) {
        ArchTreeListItem tx = testDict[x];
        ArchTreeListItem ty = testDict[y];

        return String.Compare(x, y);
      });

      // Load internal dictionary with nodes
      foreach (String key in keys) {
        ArchTreeListItem item = testDict[key];
        String id = getId(item);
        String displayName = getDisplayName(item);
        TreeNode node = new TreeNode {
          Name = id.ToString(),
          Text = displayName,
          Tag = item
        };
        node.ImageIndex = 1;
        node.SelectedImageIndex = 1;
        _treeNodes.Add(getId(item), node);
      }

      // Create hierarchy and load into view
      foreach (String id in _treeNodes.Keys) {
        TreeNode node = GetNode(id);
        ArchTreeListItem obj = (ArchTreeListItem)node.Tag;
        String parentId = getParentId(obj);

        if (parentId != "") {
          TreeNode parentNode = GetNode(parentId);
          parentNode.Nodes.Add(node);
        } else {
          Nodes.Add(node);
        }
      }
    }
    #endregion
  }
}

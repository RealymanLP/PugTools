using System;
using GomLib;

namespace PugTools {
  internal class NodeAsset {
    private readonly DataObjectModel _dom;
    private GomObject _obj;
    internal String displayName;
    internal Object dynObject;
    internal String id;
    internal GomObjectData objData;
    internal String parentId;

    public GomObject Obj {
      get => _obj ??= _dom != null ? _dom.GetObject(id) : _obj;
    }

    // public GomObject Obj {
    //   get {
    //     if (_obj == null && _dom != null)
    //       _obj = _dom.GetObject(id);
    //     return _obj;
    //   }
    // }

    public NodeAsset(String id, String parent, String display, Object obj) {
      this.id = id;
      parentId = parent;
      displayName = display;

      if (obj is DataObjectModel model)
        _dom = model;
      else if (obj is GomObject @object)
        _obj = @object;
      else if (obj is GomObjectData data)
        objData = data;
      else if (obj is FileFormats.GR2 gR)
        dynObject = gR;
      else if (obj is FileFormats.GR2_Material material)
        dynObject = material;
      else if (obj is FileFormats.GR2_Mesh mesh)
        dynObject = mesh;
      else if (obj is TorArchive.HashFileInfo info)
        dynObject = info;
      else if (obj is System.Drawing.Bitmap bitmap)
        dynObject = bitmap;
      else if (obj is FileFormats.GR2_Bone_Skeleton skeleton)
        dynObject = skeleton;
    }
  }
}

using System;

using nsHashDictionary;

namespace TorArchive {
  public class HashDictionaryInstance {

    #region Constructors
    static HashDictionaryInstance() {
      s_instance = new HashDictionaryInstance();
    }

    public HashDictionaryInstance() {
      Dictionary = new HashDictionary();
      Dictionary.LoadBinaryHashList();
      Loaded = true;
    }

    #endregion Constructors

    #region Fields
    private static readonly HashDictionaryInstance s_instance;

    #endregion Fields

    #region Methods
    public void Load() {
      if (Loaded) {
        return;
      }

      Dictionary.LoadBinaryHashList();
      Loaded = true;
    }

    public void Unload() {
      Dictionary = new HashDictionary();
      Loaded = false;
      GC.Collect();
    }

    #endregion Methods

    #region Properties
    public HashDictionary Dictionary { get; private set; }
    public Boolean Loaded { get; private set; }
    public static HashDictionaryInstance Instance => s_instance;

    #endregion Properties
  }
}

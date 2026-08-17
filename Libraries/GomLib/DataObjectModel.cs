using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using GomLib.ModelLoader;

using TorArchive;
using File = TorArchive.File;

namespace GomLib {
  public class DataObjectModel : IDisposable {

    #region Constructors
    public DataObjectModel(Assets assets) {
      Assets = assets;

      m_bucketFiles = new List<String>();
      m_namedMap = new Dictionary<String, HashSet<String>>();
      m_prototypeLoader = new DomTypeLoaders.FileInstanceLoader();
      m_storedIdMap = new Dictionary<UInt64, String>();
      m_storedNameMap = new Dictionary<String, UInt64>();
      m_typeLoaderMap = new Dictionary<Int32, DomTypeLoaders.IDomTypeLoader>();
      m_unnamedMap = new Dictionary<String, HashSet<UInt64>>();

      DomTypeMap = new Dictionary<UInt64, DomType>();
      NodeLookup = new Dictionary<Type, Dictionary<String, DomType>>();

      AddTypeLoader(new DomTypeLoaders.EnumLoader());
      AddTypeLoader(new DomTypeLoaders.FieldLoader());
      AddTypeLoader(new DomTypeLoaders.AssociationLoader());
      AddTypeLoader(new DomTypeLoaders.ClassLoader());
      AddTypeLoader(new DomTypeLoaders.InstanceLoader());
    }

    #endregion Constructors

    #region Fields
    private List<String> m_bucketFiles;
    private Boolean m_crossLinked;
    private Boolean m_loaded;
    private readonly Dictionary<String, HashSet<String>> m_namedMap;
    private DomTypeLoaders.FileInstanceLoader m_prototypeLoader;
    private Dictionary<UInt64, String> m_storedIdMap;
    private Dictionary<String, UInt64> m_storedNameMap;
    private Dictionary<Int32, DomTypeLoaders.IDomTypeLoader> m_typeLoaderMap;
    private Dictionary<String, HashSet<UInt64>> m_unnamedMap;

    #endregion Fields

    #region IDisposable
    private Boolean m_disposed = false;

    ~DataObjectModel() {
      Dispose(false);
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(Boolean disposing) {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        m_typeLoaderMap.Clear();
        NodeLookup.Clear();
        DomTypeMap = null;
        m_storedNameMap.Clear();
        m_storedNameMap = null;
        m_unnamedMap.Clear();
        m_unnamedMap = null;
        m_bucketFiles.Clear();

        if (Data != null) {
          Data.Dispose();
        }
      }

      m_disposed = true;
    }

    #endregion IDisposable

    #region Methods
    public void AddCrossLink(Int64 id, String type, UInt64 reference) {
      UInt64 id2 = Math.Sign(id) == -1 ? UInt64.MaxValue - (UInt64)Math.Abs(id) + 1UL : (UInt64)id;

      AddCrossLink(id2, type, reference);
    }

    public void AddCrossLink(String name, String type, UInt64 reference) {
      GomObject testNode = GetObjectNoLoad(name);

      if (testNode != null) {
        if (testNode.References == null) {
          testNode.References = new Dictionary<String, SortedSet<UInt64>>();
        }

        if (!testNode.References.ContainsKey(type)) {
          testNode.References.Add(type, new SortedSet<UInt64>());
        }

        if (!testNode.References[type].Contains(reference)) {
          testNode.References[type].Add(reference);
        }
      }
    }

    public void AddCrossLink(UInt64 id, String type, UInt64 reference) {
      GomObject testNode = GetObjectNoLoad(id);

      if (testNode != null) {
        if (testNode.References == null) {
          testNode.References = new Dictionary<String, SortedSet<UInt64>>();
        }

        if (!testNode.References.ContainsKey(type)) {
          testNode.References.Add(type, new SortedSet<UInt64>());
        }

        if (!testNode.References[type].Contains(reference)) {
          testNode.References[type].Add(reference);
        }
      }
    }

    public void AddCrossLinkRange(UInt64 id, String type, List<UInt64> reference) {
      GomObject testNode = GetObjectNoLoad(id);

      if (testNode != null) {
        if (testNode.References == null) {
          testNode.References = new Dictionary<String, SortedSet<UInt64>>();
        }

        if (!testNode.References.ContainsKey(type)) {
          testNode.References.Add(type, new SortedSet<UInt64>());
        }

        testNode.References[type].UnionWith(reference);
        testNode.References[type].Remove(id); //remove self references
      }
    }

    public void AddProtoCrossLink(UInt64 protoId, UInt64 id, String type, UInt64 reference) {
      GomObject testNode = GetObjectNoLoad(protoId);

      if (testNode != null) {
        if (testNode.ProtoReferences == null) {
          testNode.ProtoReferences =
            new Dictionary<UInt64, Dictionary<String, SortedSet<UInt64>>>();
        }

        if (!testNode.ProtoReferences.ContainsKey(id)) {
          testNode.ProtoReferences.Add(id, new Dictionary<String, SortedSet<UInt64>>());
        }

        if (!testNode.ProtoReferences[id].ContainsKey(type)) {
          testNode.ProtoReferences[id].Add(type, new SortedSet<UInt64>());
        }

        if (!testNode.ProtoReferences[id][type].Contains(reference)) {
          testNode.ProtoReferences[id][type].Add(reference);
        }
      }
    }

    private void AddToNameLookup(DomType type) {
      if (String.IsNullOrEmpty(type.Name)) {
        return;
      }

      Type typeType = type.GetType();

      if (!NodeLookup.TryGetValue(typeType, out _)) {
        Dictionary<String, DomType> nameMap = new Dictionary<String, DomType>();
        NodeLookup[typeType] = nameMap;
      }

      NodeLookup[typeType].Add(type.Name, type);
    }

    private void AddTypeLoader(DomTypeLoaders.IDomTypeLoader loader) {
      Int32 type = loader.SupportedType;
      m_typeLoaderMap.Add(type, loader);
    }

    public void CrossLink() {
      if (m_crossLinked) {
        return;
      }

      //foreach (DomType t in DomTypeMap.Values)

      Parallel.ForEach(DomTypeMap.Values, t => {
        if (t.GetType() == typeof(GomObject)) {
          GomObject tG = t as GomObject;
          tG.FindReferences();
          tG.Unload();
        }
      });

      m_crossLinked = true;
    }

    public T Get<T>(String name) where T : DomType {
      if (name == null) {
        return null;
      }

      Type resultType = typeof(T);

      if (!NodeLookup.TryGetValue(resultType, out Dictionary<String, DomType> nameMap)) {
        return null;
      }

      if (!nameMap.TryGetValue(name, out DomType t)) {
        return null;
      }

      return t as T;
    }

    public T Get<T>(UInt64 typeId) where T : DomType {
      if (!DomTypeMap.TryGetValue(typeId, out DomType t)) {
        return null;
      }

      T result = t as T;

      if (result == null) {
        Debug.WriteLine("Type 0x{0:X} is not of type {1}", t.Id, typeof(T));
      }

      return result;
    }

    public SortedDictionary<String, Int64> GetAllInstanceNames() {
      SortedDictionary<String, Int64> results = new SortedDictionary<String, Int64>();
      Type resultType = typeof(GomObject);

      if (!NodeLookup.TryGetValue(resultType, out Dictionary<String, DomType> nameMap)) {
        return results;
      }

      foreach (KeyValuePair<String, DomType> kvp in nameMap) {
        results.Add(kvp.Key, ((GomObject)kvp.Value).Checksum);
      }

      return results;
    }

    public GomObject GetObject(String name) {
      GomObject result = Get<GomObject>(name);
      result?.Load();
      return result;
    }

    public GomObject GetObject(UInt64 id) {
      GomObject result = Get<GomObject>(id);
      result?.Load();
      return result;
    }

    public UInt64 GetObjectId(String name) {
      GomObject result = Get<GomObject>(name);
      return result != null ? result.Id : 0;
    }

    public GomObject GetObjectNoLoad(String name) {
      return Get<GomObject>(name);
    }

    public GomObject GetObjectNoLoad(UInt64 id) {
      return Get<GomObject>(id);
    }

    public List<GomObject> GetObjectsStartingWith(String txt) {
      List<GomObject> results = new List<GomObject>();
      Type resultType = typeof(GomObject);

      if (!NodeLookup.TryGetValue(resultType, out Dictionary<String, DomType> nameMap)) {
        return results;
      }

      foreach (KeyValuePair<String, DomType> kvp in nameMap) {
        if (kvp.Key.StartsWith(txt)) {
          results.Add((GomObject)kvp.Value);
        }
      }

      return results;
    }

    public UInt64 GetStoredTypeId(String name) {
      m_storedNameMap.TryGetValue(name, out UInt64 id);
      return id;
    }

    public String GetStoredTypeName(UInt64 id) {
      if (m_storedIdMap.TryGetValue(id, out String result)) {
        return result;
      }

      return null;
    }

    private void InitializeModelLoaders() {
      // THESE MUST BE IN THIS ORDER!
      ScriptObjectReader = new ScriptObjectReader(this);
      Data = new Data(this);
      StringTable = new StringTable(this);
      Ami = new AMI(this);

      //ADD NEW MODELS HERE
      AbilityLoader = new AbilityLoader(this);
      AbilityPackageLoader = new AbilityPackageLoader(this);
      AchievementLoader = new AchievementLoader(this);
      AchievementCategoryLoader = new AchievementCategoryLoader(this);
      AdvancedClassLoader = new AdvancedClassLoader(this);
      AppearanceLoader = new AppearanceLoader(this);
      AreaLoader = new AreaLoader(this);
      ClassSpecLoader = new ClassSpecLoader(this);
      CodexLoader = new CodexLoader(this);
      MtxStorefrontEntryLoader = new MtxStorefrontEntryLoader(this);
      CollectionLoader = new CollectionLoader(this);
      CompanionLoader = new CompanionLoader(this);
      NewCompanionLoader = new NewCompanionLoader(this);
      ConquestLoader = new ConquestLoader(this);
      ConversationLoader = new ConversationLoader(this);
      DecorationLoader = new DecorationLoader(this);
      DisciplineLoader = new DisciplineLoader(this);
      NewDisciplineLoader = new NewDisciplineLoader(this);
      EffectLoader = new EffectLoader(this);
      EncounterLoader = new EncounterLoader(this);
      ItemLoader = new ItemLoader(this);
      MapNoteLoader = new MapNoteLoader(this);
      NpcLoader = new NpcLoader(this);
      PackageAbilityLoader = new PackageAbilityLoader(this);
      PlaceableLoader = new PlaceableLoader(this);
      QuestBranchLoader = new QuestBranchLoader(this);
      QuestLoader = new QuestLoader(this);
      QuestStepLoader = new QuestStepLoader(this);
      QuestTaskLoader = new QuestTaskLoader(this);
      SCFFColorOptionLoader = new SCFFColorOptionLoader(this);
      SCFFComponentLoader = new SCFFComponentLoader(this);
      SCFFPatternLoader = new SCFFPatternLoader(this);
      SCFFShipLoader = new SCFFShipLoader(this);
      SchematicLoader = new SchematicLoader(this);
      SpawnerLoader = new SpawnerLoader(this);
      StrongholdLoader = new StrongholdLoader(this);
      TalentLoader = new TalentLoader(this);
      SetBonusLoader = new SetBonusLoader(this);
      CdxCatTotalsLoader = new CodexCatByFactionLoader(this);
      SchemVariationLoader = new SchematicVariationLoader(this);
      LegacyTitleLoader = new LegacyTitleLoader(this);
      ReputationGroupLoader = new ReputationGroupLoader(this);
      ReputationRankLoader = new ReputationRankLoader(this);
      DetailedAppearanceColorLoader = new DetailedAppearanceColorLoader(this);
      PlayerTitleLoader = new PlayerTitleLoader(this);

      AreaDatLoader = new FileLoaders.AreaDatLoader(this);
      RoomDatLoader = new FileLoaders.RoomDatLoader(this);

      Models.Tooltip.Flush();
    }

    public void Load() {
      if (m_loaded) {
        return;
      }

      LoadTypeNames();

      GomTypeLoader = new GomTypeLoader(this);

      LoadClientGom();
      LoadBuckets();
      LoadPrototypes();

      // Debug.WriteLine(
      //   "Warning: loading of individual (non-bucketed) prototype files is currently disabled");

      m_loaded = true;

      StatData = new Models.StatData(this);
      FactionData = new Models.FactionData(this);
      EnhancementData = new Models.EnhancementData(this);
      SocialTierData = new Models.SocialTierData(this);
      AlignmentData = new Models.AlignmentData(this);
      GroupFinderContentData = new Models.GroupFinderContentData(this);

      foreach (DomType domType in DomTypeMap.Values) {
        // Debug.WriteLine(t.Name);
        domType.Link(this);
      }

      InitializeModelLoaders();
    }

    private void LoadBucketFiles() {
      foreach (String bucketFileName in m_bucketFiles) {
        String path = $"/resources/systemgenerated/buckets/{bucketFileName}";
        File bucketFile = Assets.FindFile(path);

        using (Stream fs = bucketFile.Open())
        using (GomBinaryReader br = new GomBinaryReader(fs, Encoding.UTF8, this)) {
          br.ReadBytes(0x24); // Skip 24 header bytes

          ReadAllItems(br, 0x24);
        }
      }
    }

    private void LoadBucketList() {
      File gomFile = Assets.FindFile("/resources/systemgenerated/buckets.info");

      using (Stream fs = gomFile.Open())
      using (GomBinaryReader br = new GomBinaryReader(fs, Encoding.UTF8, this)) {
        br.ReadBytes(8); // Skip 8 header bytes

        Byte c9 = br.ReadByte();

        if (c9 != 0xC9) {
          throw new InvalidOperationException(
            $"Unexpected character in buckets.info @ offset 0x8 - expected 0xC9 found {c9:X2}");
        }

        Int16 numEntries = br.ReadInt16(Endianness.BigEndian);

        for (Int32 i = 0; i < numEntries; i++) {
          String fileName = br.ReadLengthPrefixString();
          m_bucketFiles.Add(fileName);
        }
      }
    }

    private void LoadBuckets() {
      LoadBucketList();
      LoadBucketFiles();
    }

    private void LoadClientGom() {
      File gomFile = Assets.FindFile("/resources/systemgenerated/client.gom");

      using (Stream fs = gomFile.Open())
      using (GomBinaryReader br = new GomBinaryReader(fs, Encoding.UTF8, this)) {
        Int32 magic = br.ReadInt32(); // Check DBLB

        if (magic != 0x424C4244) {
          throw new InvalidOperationException("client.gom does not begin with DBLB.");
        }

        _ = br.ReadInt32(); // Skip 4 bytes

        ReadAllItems(br, 8);
      }
    }

    private void LoadPrototype(UInt64 id) {
      String path = $"/resources/systemgenerated/prototypes/{id}.node";
      File protoFile = Assets.FindFile(path);

      if (protoFile == null) {
        Debug.WriteLine("Unable to find {0}", path);
      }

      using (Stream fs = protoFile.Open())
      using (GomBinaryReader br = new GomBinaryReader(fs, Encoding.UTF8, this)) {
        Int32 magicNum = br.ReadInt32(); // Check PROT

        if (magicNum != 0x544F5250) {
          throw new InvalidOperationException($"{path} does not begin with PROT");
        }

        br.ReadInt32(); // Skip 4 bytes

        GomObject proto = m_prototypeLoader.Load(br) as GomObject;
        proto.Dom_ = this;
        proto.Checksum = protoFile.FileInfo.Checksum;

        if (!DomTypeMap.ContainsKey(proto.Id)) {
          DomTypeMap.Add(proto.Id, proto);
          AddToNameLookup(proto);
        }
      }
    }

    private void LoadPrototypes() {
      File prototypeList = Assets.FindFile("/resources/systemgenerated/prototypes.info");

      using (Stream fs = prototypeList.Open())
      using (GomBinaryReader br = new GomBinaryReader(fs, Encoding.UTF8, this)) {
        Int32 magicNum = br.ReadInt32(); // Check PINF

        if (magicNum != 0x464E4950) {
          throw new InvalidOperationException("prototypes.info does not begin with PINF");
        }

        br.ReadInt32(); // Skip 4 bytes

        Int32 numPrototypes = (Int32)br.ReadNumber();
        Int32 protoLoaded = 0;

        for (Int32 i = 0; i < numPrototypes; i++) {
          UInt64 protId = br.ReadNumber();
          Byte flag = br.ReadByte();

          if (flag == 1) {
            LoadPrototype(protId);
            protoLoaded++;
          }
        }

        Debug.WriteLine("Loaded {0} prototype files", protoLoaded);
      }
    }

    private void LoadTypeNames() {
      using (StringReader fs = new(Properties.Resources.gom_type_names)) {
        //var inFilePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "gom_type_names.xml");
        //using var fs = System.IO.File.OpenRead(inFilePath);

        XmlDocument doc = new XmlDocument();

        doc.Load(fs);

        XPathNavigator nav = doc.DocumentElement.CreateNavigator();

        foreach (XPathNavigator node in nav.Select("//gom_type")) {
          node.MoveToAttribute("Id", "");

          UInt64 id = UInt64.Parse(node.Value);

          node.MoveToParent();
          node.MoveToAttribute("name", "");

          String name = node.Value;

          m_storedNameMap.Add(name, id);
          m_storedIdMap.Add(id, name);
        }
      }
    }

    public void OutputTypeNames(String path) {
      // Create XML mapping GomType IDs to names
      String outFilePath = $"{path}Gom_Fields.xml";

      using (XmlTextWriter writer = new XmlTextWriter(outFilePath, Encoding.UTF8)) {
        writer.WriteStartDocument();
        writer.WriteStartElement("Gom_Fields");

        foreach (KeyValuePair<Type, Dictionary<String, DomType>> nodeTypeMap in NodeLookup) {
          Type type = nodeTypeMap.Key;

          if (type == typeof(GomObject)) { continue; }

          foreach (KeyValuePair<String, DomType> kvp in nodeTypeMap.Value) {
            DomType domType = kvp.Value;
            String name = kvp.Key;

            writer.WriteStartElement("Gom_Field");
            writer.WriteAttributeString("Id", domType.Id.ToString());
            writer.WriteString(name);
            writer.WriteEndElement();
            writer.WriteString(Environment.NewLine);
          }
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
      }
    }

    public void ReadAllItems(GomBinaryReader br, Int64 offset) {

      while (true) {
        // Begin Reading Gom Definitions
        Int32 defLength = br.ReadInt32();

        // Length == 0 means we've read them all!
        if (defLength == 0) {
          break;
        }

        Byte[] defBuffer = new Byte[defLength];

        _ = br.ReadInt32();              // Skip 4 bytes

        UInt64 defId = br.ReadUInt64();  // UInt64 type ID
        Int16 defFlags = br.ReadInt16(); // Int16  flag field
        Int32 defType = (defFlags >> 3) & 0x7;

        //Byte[] defData = br.ReadBytes(defLength - 6);
        Byte[] defData = br.ReadBytes(defLength - 18);
        Buffer.BlockCopy(defData, 0, defBuffer, 18, defData.Length);

        using (MemoryStream memStream = new MemoryStream(defBuffer))
        using (GomBinaryReader defReader = new GomBinaryReader(memStream, Encoding.UTF8, this)) {
          if (m_typeLoaderMap.TryGetValue(defType, out DomTypeLoaders.IDomTypeLoader loader)) {
            DomType domType = loader.Load(defReader);
            domType.Dom_ = this;
            domType.Id = defId;

            // if (defId == 16141050636868461855) {
            //   String sfiino = "";
            // }

            if (!DomTypeMap.ContainsKey(domType.Id)) {
              DomTypeMap.Add(domType.Id, domType);

              String type = domType.GetType().ToString();

              if (String.IsNullOrEmpty(domType.Name)) {
                if (m_storedIdMap.TryGetValue(domType.Id, out String storedTypeName)) {
                  domType.Name = storedTypeName;
                }
              }

              // if (type != "GomLib.DomEnum" 
              //     && type != "GomLib.DomAssociation" 
              //     && type != "GomLib.DomField" 
              //     && type != "GomLib.DomClass") {
              //   GomObjectData dat = ((GomObject)domType).Data;
              //   String pausehere = "";
              // }

              AddToNameLookup(domType);
            }
          } else {
            throw new InvalidOperationException(
              $"No loader for DomType 0x{defType:X} as offset 0x{offset:X}");
          }
        }

        // Read the required number of padding bytes
        Int32 padding = (8 - (defLength & 0x7)) & 0x7;
        if (padding > 0) {
          br.ReadBytes(padding);
        }

        offset = offset + defLength + padding;
      }
    }

    public XDocument ReturnTypeNames() {
      XElement typeNames = new XElement("Gom_Fields");

      foreach (KeyValuePair<Type, Dictionary<String, DomType>> nodeTypeMap in NodeLookup) {
        Type type = nodeTypeMap.Key;

        if (type == typeof(GomObject)) { continue; }

        foreach (KeyValuePair<String, DomType> kvp in nodeTypeMap.Value) {
          DomType domType = kvp.Value;
          String name = kvp.Key;

          typeNames.Add(new XElement("Gom_Field",
                                     new XAttribute("Id", domType.Id.ToString()),
                                     name));
        }
      }

      //typeNames.ReplaceNodes(typeNames.Elements("Gom_Field")
      //.OrderBy(x => (string)x.Attribute("Id")));

      XElement fieldUseInDomClass = new XElement("FieldUseInDomClass");

      CrossLink(); // need to scan nodes to find all values

      foreach (KeyValuePair<String, HashSet<UInt64>> kvp in m_unnamedMap) {
        fieldUseInDomClass.Add(
          new XElement("DomClass",
                       new XAttribute("Id", kvp.Key.ToString()),
                       new XElement("Gom_Fields",
                                    new XAttribute("Id", "UnNamed"),
                                    kvp.Value.ToList()
                                             .Select(x => new XElement("Gom_Field", new XAttribute("Id", x))))));
      }

      foreach (KeyValuePair<String, HashSet<String>> kvp in m_namedMap) {
        IEnumerable<XElement> xe = fieldUseInDomClass.Elements()
                                                     .Where(x => x.Attribute("Id").Value == kvp.Key);
        XElement ta = new XElement("Gom_Fields",
                                   new XAttribute("Id", "Named"),
                                   kvp.Value.ToList()
                                            .Select(x => new XElement("Gom_Field", new XAttribute("Id", x))));

        if (!xe.Any()) {
          fieldUseInDomClass.Add(new XElement("DomClass",
                                              new XAttribute("Id", kvp.Key.ToString()),
                                              ta));
        } else {
          xe.First().Add(ta);
        }
      }

      return new XDocument(new XElement("Wrapper", typeNames, fieldUseInDomClass));
    }

    public void Unload() { // This unloads Assets allowing the loading of different assets without relaunching
      if (!m_loaded) {
        return;
      }

      DomTypeMap = new Dictionary<UInt64, DomType>();
      NodeLookup = new Dictionary<Type, Dictionary<String, DomType>>();

      m_bucketFiles = new List<String>();
      m_prototypeLoader = new DomTypeLoaders.FileInstanceLoader();
      m_storedIdMap = new Dictionary<UInt64, String>();
      m_storedNameMap = new Dictionary<String, UInt64>();
      m_typeLoaderMap = new Dictionary<Int32, DomTypeLoaders.IDomTypeLoader>();

      AlignmentData = null;
      EnhancementData = null;
      FactionData = null;
      GomTypeLoader = null;
      GroupFinderContentData = null;
      StatData = null;
      SocialTierData = null;

      // Flush the ModelLoader Stored entries
      Data.Flush();
      ScriptObjectReader.Flush();
      StringTable.Flush();

      AbilityLoader.Flush();
      AbilityPackageLoader.Flush();
      AchievementLoader.Flush();
      AdvancedClassLoader.Flush();
      AppearanceLoader.Flush();
      AreaLoader.Flush();
      ClassSpecLoader.Flush();
      CodexLoader.Flush();
      CompanionLoader.Flush();
      DecorationLoader.Flush();
      DetailedAppearanceColorLoader.Flush();
      DisciplineLoader.Flush();
      NewDisciplineLoader.Flush();
      EffectLoader.Flush();
      EncounterLoader.Flush();
      ItemLoader.Flush();
      LegacyTitleLoader.Flush();
      MapNoteLoader.Flush();
      MtxStorefrontEntryLoader.Flush();
      NewCompanionLoader.Flush();
      NpcLoader.Flush();
      PackageAbilityLoader.Flush();
      PlaceableLoader.Flush();
      PlayerTitleLoader.Flush();
      QuestLoader.Flush();
      ReputationGroupLoader.Flush();
      ReputationRankLoader.Flush();
      SCFFComponentLoader.Flush();
      SCFFPatternLoader.Flush();
      SCFFShipLoader.Flush();
      SchematicLoader.Flush();
      SchemVariationLoader.Flush();
      SetBonusLoader.Flush();
      StrongholdLoader.Flush();
      TalentLoader.Flush();

      /*foreach (var DomEntry in DomTypeMap)
      {
          if (DomEntry.Value.GetType() == typeof(GomObject))
          {
              ((GomObject)DomEntry.Value).Unload();
          }
      }*/

      GC.Collect();
      m_loaded = false;
    }

    #endregion Methods

    #region Properties
    public AMI Ami { get; private set; }
    public Assets Assets { get; }
    public Data Data { get; private set; }
    public Dictionary<UInt64, DomType> DomTypeMap { get; private set; }
    public GomTypeLoader GomTypeLoader { get; private set; }
    public Dictionary<String, HashSet<String>> NamedMap { get => m_namedMap; }
    public Dictionary<Type, Dictionary<String, DomType>> NodeLookup { get; private set; }
    public ScriptObjectReader ScriptObjectReader { get; private set; }
    public StringTable StringTable { get; private set; }
    public Dictionary<String, HashSet<UInt64>> UnnamedMap { get => m_unnamedMap; }
    public String Version { get; set; }

    // ADD NEW MODELS HERE
    public AbilityLoader AbilityLoader { get; private set; }
    public AbilityPackageLoader AbilityPackageLoader { get; private set; }
    public AchievementCategoryLoader AchievementCategoryLoader { get; private set; }
    public AchievementLoader AchievementLoader { get; private set; }
    public AdvancedClassLoader AdvancedClassLoader { get; private set; }
    public Models.AlignmentData AlignmentData { get; private set; }
    public AppearanceLoader AppearanceLoader { get; private set; }
    public FileLoaders.AreaDatLoader AreaDatLoader { get; private set; }
    public AreaLoader AreaLoader { get; private set; }
    public CodexCatByFactionLoader CdxCatTotalsLoader { get; private set; }
    public ClassSpecLoader ClassSpecLoader { get; private set; }
    public CodexLoader CodexLoader { get; private set; }
    public CollectionLoader CollectionLoader { get; private set; }
    public CompanionLoader CompanionLoader { get; private set; }
    public ConquestLoader ConquestLoader { get; private set; }
    public ConversationLoader ConversationLoader { get; private set; }
    public DecorationLoader DecorationLoader { get; private set; }
    public DetailedAppearanceColorLoader DetailedAppearanceColorLoader { get; private set; }
    public DisciplineLoader DisciplineLoader { get; private set; }
    public NewDisciplineLoader NewDisciplineLoader { get; private set; }
    public EffectLoader EffectLoader { get; private set; }
    public EncounterLoader EncounterLoader { get; private set; }
    public Models.EnhancementData EnhancementData { get; private set; }
    public Models.FactionData FactionData { get; private set; }
    public Models.GroupFinderContentData GroupFinderContentData { get; private set; }
    public ItemLoader ItemLoader { get; private set; }
    public LegacyTitleLoader LegacyTitleLoader { get; private set; }
    public MapNoteLoader MapNoteLoader { get; private set; }
    public MtxStorefrontEntryLoader MtxStorefrontEntryLoader { get; internal set; }
    public NewCompanionLoader NewCompanionLoader { get; private set; }
    public NpcLoader NpcLoader { get; private set; }
    public PackageAbilityLoader PackageAbilityLoader { get; private set; }
    public PlaceableLoader PlaceableLoader { get; private set; }
    public PlayerTitleLoader PlayerTitleLoader { get; private set; }
    public QuestBranchLoader QuestBranchLoader { get; private set; }
    public QuestLoader QuestLoader { get; private set; }
    public QuestStepLoader QuestStepLoader { get; private set; }
    public QuestTaskLoader QuestTaskLoader { get; private set; }
    public ReputationGroupLoader ReputationGroupLoader { get; private set; }
    public ReputationRankLoader ReputationRankLoader { get; private set; }
    public FileLoaders.RoomDatLoader RoomDatLoader { get; private set; }
    public SCFFColorOptionLoader SCFFColorOptionLoader { get; private set; }
    public SCFFComponentLoader SCFFComponentLoader { get; private set; }
    public SCFFPatternLoader SCFFPatternLoader { get; private set; }
    public SCFFShipLoader SCFFShipLoader { get; private set; }
    public SchematicLoader SchematicLoader { get; private set; }
    public SchematicVariationLoader SchemVariationLoader { get; private set; }
    public SetBonusLoader SetBonusLoader { get; private set; }
    public Models.SocialTierData SocialTierData { get; private set; }
    public SpawnerLoader SpawnerLoader { get; private set; }
    public Models.StatData StatData { get; private set; }
    public StrongholdLoader StrongholdLoader { get; private set; }
    public TalentLoader TalentLoader { get; private set; }

    #endregion Properties

  }
}

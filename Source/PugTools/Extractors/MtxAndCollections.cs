using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GomLib;
using GomLib.Models;
using TorArchive;

namespace PugTools {
  internal partial class Tools {
    /*
    private string CollectionDataFromFqnList(Dictionary<object, object> collectionDataProto) {
      Double i = 0;
      String n = Environment.NewLine;

      StringBuilder txtFile = new StringBuilder();
      foreach (var collection in collectionDataProto) {
        GomLib.Models.Collection col = new GomLib.Models.Collection();
        currentDom.collectionLoader.Load(
          col, 
          (Int64)collection.Key, 
          (GomLib.GomObjectData)collection.Value
        );

        addtolist2("Name: " + col.Name);

        txtFile.Append("------------------------" + n);
        txtFile.Append("Id: " + col.Id + n); 
        txtFile.Append("Title: " + col.Name + n);
        txtFile.Append("Rarity: " + col.RarityDesc + n);
        txtFile.Append("Unknown: " + col.unknowntext + n);
        txtFile.Append("Icon: " + col.Icon + n); 
        txtFile.Append("Info: " + n);
        foreach (var bullet in col.BulletPoints) {
          txtFile.Append("  " + bullet + n);
        }
        // txtFile.Append("------------------------" + n );
        // txtFile.Append("Collection INFO" + n );
        txtFile.Append("------------------------" + n + n);
        i++;
      }
      addtolist("The Collection list has been generated there are " + i + " Collection Entries");
      return txtFile.ToString();
    }
    */
    internal static String FileNameToHash(String filename) {
      FileId id = FileId.FromFilePath(filename);
      return String.Format("{0}_{1}", id.Ph, id.Sh);
    }
    internal void FindNewMtxImages() {
      LoadData();

      Library lib =
        CurrentAssets.Libraries.Where(
          x => x.Name.Contains("main_gfx_assets")
        ).Single();

      if (!lib.Loaded) lib.Load();

      Dictionary<String, String> names = MtxIcons();
      HashDictionaryInstance hashData = HashDictionaryInstance.Instance;
      Boolean previousLoad = true;

      if (!hashData.Loaded) {
        previousLoad = false;
        hashData.Load();
      }

      hashData.Dictionary.CreateHelpers();

      AddToList2("Extracting new Cartel Images");

      foreach (var arch in lib.Archives) {
        foreach (TorArchive.File file in arch.Value.EnumerateFiles()) {
          HashFileInfo hashInfo =
            new HashFileInfo(file.FileInfo.PrimaryHash, file.FileInfo.SecondaryHash, file);

          if (hashInfo.FileState == HashFileInfo.State.New && hashInfo.Extension == "dds") {
            if (file != null) {
              DevIL.ImageImporter imp = new DevIL.ImageImporter();
              DevIL.Image dds;

              using (MemoryStream iconStream = (MemoryStream)file.OpenCopyInMemory())
                dds = imp.LoadImageFromStream(DevIL.ImageType.Dds, iconStream);

              using MemoryStream outputStream = new MemoryStream();

              if (dds.Width >= 400 && dds.Height >= 400) {
                // Needs cropped
                DevIL.ImageExporter exp = new DevIL.ImageExporter();
                // Save DDS to stream in PNG format
                exp.SaveImageToStream(dds, DevIL.ImageType.Png, outputStream);

                String filename = hashInfo.FileName;

                if (!hashInfo.IsNamed) {
                  if (names.ContainsKey(filename))
                    filename = names[filename];
                }

                AddToList2(filename);
                WriteFile(outputStream, String.Format("/MtxImages/unnamed/{0}.png", filename));
              }
            }
          }
        }
      }

      // Unload the hash data to prevent anything that runs after this from seeing the new gfx 
      // files as new.
      hashData.Unload();

      if (previousLoad) {
        //If the hash data was previously loaded before we ran then load it again.
        hashData.Load();
        hashData.Dictionary.CreateHelpers();
      }

      EnableButtons();
    }
    private Dictionary<String, String> MtxIcons() {
      Dictionary<Object, Object> mtxDataProto = new Dictionary<Object, Object>();
      GomObject dataObject = CurrentDom.GetObject("mtxStorefrontInfoPrototype");

      if (dataObject != null) { // Fix to ensure old game assets don't throw exceptions.
        mtxDataProto = dataObject.Data.Get<Dictionary<Object, Object>>("mtxStorefrontData");
        dataObject.Unload();
      }

      Dictionary<Object, Object>.KeyCollection mtxIds = mtxDataProto.Keys;

      Dictionary<Object, Object> colDataProto = new Dictionary<Object, Object>();
      dataObject = CurrentDom.GetObject("colCollectionItemsPrototype");
      if (dataObject != null) { // fix to ensure old game assets don't throw exceptions.
        colDataProto = dataObject.Data.Get<Dictionary<Object, Object>>("colCollectionItemsData");
        dataObject.Unload();
      }

      Dictionary<Object, Object>.KeyCollection colIds = colDataProto.Keys;
      Dictionary<String, String> icons = new Dictionary<String, String>();

      ClearProgress();
      AddToList2("Loading filenames...");

      Int32 i = 0;
      Int32 count = mtxIds.Count + colIds.Count;

      foreach (Object id in mtxIds) {
        ProgressUpdate(i, count);
        mtxDataProto.TryGetValue(id, out Object curData);
        PseudoGameObject curObj = PseudoGameObject.Load("MtxStoreFronts", CurrentDom, id, curData);

        String filename =
          String.Format(
            "/resources/gfx/mtxstore/{0}_400x400.dds",
            ((MtxStorefrontEntry)curObj).Icon
          );

        if (!icons.ContainsValue(filename))
          icons.Add(FileNameToHash(filename), filename);

        i++;
      }

      foreach (Object id in colIds) {
        ProgressUpdate(i, count);
        colDataProto.TryGetValue(id, out Object curData);
        PseudoGameObject curObj = PseudoGameObject.Load("Collections", CurrentDom, id, curData);

        String filename =
          String.Format("/resources/gfx/mtxstore/{0}.dds", ((Collection)curObj).Icon);

        if (!icons.ContainsValue(filename))
          icons.Add(FileNameToHash(filename), filename);

        i++;
      }

      ClearProgress();
      AddToList2("Done.");

      return icons;
    }
    /* code moved to GomLib.Models.Collection.cs */

    /*
    private string MtxStoreFrontDataFromFqnList(Dictionary<object, object> mtxStorefrontDataProto) {
      Double i = 0;
      String n = Environment.NewLine;

      StringBuilder txtFile = new StringBuilder();
      foreach (var mtxStorefrontEntry in mtxStorefrontDataProto) {
        GomLib.Models.MtxStorefrontEntry col = new GomLib.Models.MtxStorefrontEntry();
        currentDom.mtxStorefrontEntryLoader.Load(
          col, 
          (long)mtxStorefrontEntry.Key, 
          (GomLib.GomObjectData)mtxStorefrontEntry.Value
        );

        addtolist2("Name: " + col.Name);

        txtFile.Append("------------------------" + n);
        txtFile.Append("Id: " + col.Id + n);
        txtFile.Append("Title: " + col.Name + n);
        txtFile.Append("Rarity: " + col.RarityDesc + n);
        txtFile.Append("Unknown: " + col.unknowntext + n);
        txtFile.Append("Icon: " + col.Icon + n);
        txtFile.Append("Info: " + n);
        foreach (var bullet in col.BulletPoints) {
          txtFile.Append("  " + bullet + n);
        }
        // txtFile.Append("------------------------" + n );
        // txtFile.Append("MtxStoreFront INFO" + n );
        txtFile.Append("------------------------" + n + n);
      
        i++;
      }
      
      addtolist(
        "The MtxStoreFront list has been generated there are " + i + " MtxStoreFront Entries"
      );
      
      return txtFile.ToString();
    }
    */

    /* code moved to GomLib.Models.MtxStorefrontEntry.cs */

    private static XElement SortCollections(XElement collections) {
      //addtolist("Sorting Collection Entries");
      collections.ReplaceNodes(
        collections.Elements("Collection").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Attribute("Id")
        ).ThenBy(
          x => (String)x.Element("Name")
        )
      );

      return collections;
    }
    private static XElement SortMtxStoreFronts(XElement mtxStoreFront) {
      //addtolist("Sorting MtxStoreFront Entries");
      mtxStoreFront.ReplaceNodes(
        mtxStoreFront.Elements("MtxStoreFront").OrderBy(
          x => (String)x.Attribute("Status")
        ).ThenBy(
          x => (String)x.Attribute("Id")
        ).ThenBy(
          x => (String)x.Element("Name")
        )
      );

      return mtxStoreFront;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace GomLib.Models {
  public class AchievementCategory : PseudoGameObject, IEquatable<AchievementCategory> {
    //public Dictionary<String, String> LocalizedTitle { get; set; }
    //public String Title { get; set; }
    public Int64 CatId { get; set; }
    public String CodexIcon { get; set; }
    public String HashedIcon {
      get {
        TorArchive.FileId fileId =
          TorArchive.FileId.FromFilePath(String.Format("/resources/gfx/icons/{0}.dds", Icon));
        return String.Format("{0}_{1}", fileId.Ph, fileId.Sh);
      }
    }
    public String Icon { get; set; }
    public Int64 Index { get; set; }
    public Dictionary<String, String> LocalizedName { get; set; }
    public UInt64 NameId { get; set; }
    public Int64 ParentCategory { get; set; }
    [JsonIgnore]
    public List<List<AchievementCategoryEntry>> Rows { get; set; }
    public override List<SQLProperty> SQLProperties {
      get {
        return new List<SQLProperty> { 
          // (SQL Column Name, C# Property Name, SQL Column type statement, isUnique/PrimaryKey, Serialize value to json)
          new SQLProperty("CatId", "CatId", "bigint(20) signed NOT NULL", true),
          new SQLProperty("Name", "Name", "varchar(255) COLLATE utf8_unicode_ci NOT NULL"),
          new SQLProperty("NameId", "NameId", "bigint(20) NOT NULL"),
          new SQLProperty("Index", "Index", "int(11) NOT NULL"),
          new SQLProperty("ParentCatId", "ParentCategory", "bigint(20) unsigned NOT NULL"),
          new SQLProperty("CodexIcon", "CodexIcon", "varchar(255) COLLATE utf8_unicode_ci NOT NULL"),
          new SQLProperty("Icon", "Icon", "varchar(255) COLLATE utf8_unicode_ci NOT NULL"),
          new SQLProperty("HashedIcon", "HashedIcon", "varchar(255) COLLATE utf8_unicode_ci NOT NULL"),
          new SQLProperty("SubCategories", "SubCategories", "varchar(600) COLLATE utf8_unicode_ci NOT NULL", false, true),
          new SQLProperty("Rows", "Rows", "varchar(3000) COLLATE utf8_unicode_ci NOT NULL", false, true)
        };
      }
    }
    [JsonIgnore]
    public List<Int64> SubCategories { get; set; }

    public Boolean Equals(AchievementCategory obj) {
      if (obj == null) return false;

      if (ReferenceEquals(this, obj)) return true;

      if (CatId != obj.CatId)
        return false;
      if (Icon != obj.Icon)
        return false;
      if (HashedIcon != obj.HashedIcon)
        return false;
      if (CodexIcon != obj.CodexIcon)
        return false;
      if (NameId != obj.NameId)
        return false;
      if (Name != obj.Name)
        return false;
      if (Index != obj.Index)
        return false;
      //TODO: SubCategories
      if (ParentCategory != obj.ParentCategory)
        return false;
      //TODO: Achievements

      return true;
    }

    public override Boolean Equals(Object obj) {
      if (obj == null) return false;

      if (ReferenceEquals(this, obj)) return true;

      if (obj is not AchievementCategory obj2) return false;

      return Equals(obj2);
    }

    public override Int32 GetHashCode() {
      Int32 hash = CatId.GetHashCode();
      hash ^= Icon.GetHashCode();
      hash ^= CodexIcon.GetHashCode();
      hash ^= NameId.GetHashCode();
      hash ^= Name.GetHashCode();

      foreach (var x in LocalizedName) {
        hash ^= x.GetHashCode();
      } // dictionaries need to hashed like this
        // hash ^= LocalizedName.GetHashCode(); //not like this
      hash ^= Index.GetHashCode();

      //SubCategories
      foreach (var x in SubCategories) {
        hash ^= x.GetHashCode();
      }
      hash ^= ParentCategory.GetHashCode();

      //Achievements
      foreach (var x in Rows) foreach (var y in x) {
          hash ^= y.GetHashCode();
        }

      return hash;
    }

    public override XElement ToXElement(Boolean verbose) {
      XElement item = new XElement("AchievementCategory");

      item.Add(new XElement("UnImplemented", new XAttribute("Hash", GetHashCode())));
      return item;
    }
  }

  public class AchievementCategoryEntry : IEquatable<AchievementCategoryEntry> {
    public Boolean DrawArrow;//Whether to draw an arrow to the next achievement on this row, indicating that that achievement is a continuation of the current achievement
    public UInt64 Id;//Id of the achievement

    public Boolean Equals(AchievementCategoryEntry obj) {
      if (obj == null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (Id != obj.Id) return false;
      if (DrawArrow != obj.DrawArrow) return false;

      return true;
    }

    public override Boolean Equals(Object obj) {
      if (obj == null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj is not AchievementCategoryEntry obj2) return false;

      return Equals(obj2);
    }

    public override Int32 GetHashCode() {
      Int32 hash = Id.GetHashCode();
      hash ^= DrawArrow.GetHashCode();
      return hash;
    }
  }
}

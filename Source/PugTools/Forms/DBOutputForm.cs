using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PugTools {
  public partial class DBOutputForm : Form {
    public List<String> fileTypes = new List<String>();

    public DBOutputForm() => InitializeComponent();
    private void VersionTexBox_TextChanged(Object sender, EventArgs e) {
      if (!string.IsNullOrEmpty(versionTexBox.Text)) btnOK.Enabled = true;
      else btnOK.Enabled = false;
    }
  }
}

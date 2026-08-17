using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PugTools {
  public partial class AssetBrowserExtractExt : Form {
    private readonly HashSet<String> _extensions; // = new HashSet<string>();

    public AssetBrowserExtractExt() {
      InitializeComponent();
      _extensions = new HashSet<String>();
    }
    private void AssetBrowserExtractExt_FormClosing(Object sender, FormClosingEventArgs e) {
      if (_extensions.Count == 0) {
        MessageBox.Show(
          "Please enter a list of extensions sperated by a space.",
          "ERROR: Empty List",
          MessageBoxButtons.OK,
          MessageBoxIcon.Exclamation
        );

        e.Cancel = true;
      }
    }
    private void BtnOK_Click(Object sender, EventArgs e) {
      String[] temp = txtExts.Text.Split(' ');

      foreach (String item in temp) {
        if (item != "")
          _extensions.Add(item.ToUpper());
      }
    }
    public HashSet<String> GetExtensions() {
      return _extensions;
    }
    private void TxtExts_KeyDown(Object sender, KeyEventArgs e) {
      if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) {
        btnOK.PerformClick();
      }
    }
  }
}

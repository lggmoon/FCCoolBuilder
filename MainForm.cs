using com.clusterrr.Famicom.Containers;
using System.Text;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FCCoolBuilder;

partial class MainForm : Form
{
    readonly Config config = Config.Load();

    internal MainForm()
    {
        InitializeComponent();

        Text = "FCCool合卡工具 v1.1";

        ListViewFiles.AllowDrop = true;
        ListViewFiles.ShowItemToolTips = true;

        ComboBoxCardtype.DropDownStyle = ComboBoxStyle.DropDownList;
        ComboBoxTheme.DropDownStyle = ComboBoxStyle.DropDownList;
        ComboBoxMusic.DropDownStyle = ComboBoxStyle.DropDownList;
        ComboBoxSfx.DropDownStyle = ComboBoxStyle.DropDownList;
        ComboxCursorBling.DropDownStyle = ComboBoxStyle.DropDownList;

        int selectIdx = 0;
        // load cartype
        foreach (var d in Config.CardtypeNameMap) {
            ComboBoxCardtype.Items.Add(d.Value);
            if (d.Key == config.Cardtype)
                selectIdx = ComboBoxCardtype.Items.Count - 1;
        }
        ComboBoxCardtype.SelectedIndex = selectIdx;

        // load theme
        selectIdx = 0;
        var dirs = Directory.GetDirectories(config.ThemesDir).OrderBy((g) => {
            string name = Path.GetFileName(g).Trim();
            return name.StartsWith("default") ? "_" + name : name;
        });
        foreach (var d in dirs) {
            string name = Path.GetFileName(d).Trim();
            ComboBoxTheme.Items.Add(name);
            if (name == config.ThemeName)
                selectIdx = ComboBoxTheme.Items.Count - 1;
        }
        ComboBoxTheme.SelectedIndex = selectIdx;

        // load 
        selectIdx = 1;
        ComboBoxSfx.Items.Add("<无音效>");
        ComboBoxSfx.Items.Add("默认音效1");
        ComboBoxSfx.Items.Add("默认音效2");
        config.MaxSfxIdx = ComboBoxSfx.Items.Count - 1;
        dirs = Directory.GetFiles(config.SfxesDir, "*.asm").OrderBy((g) => g);
        foreach (var d in dirs) {
            string name = Path.GetFileNameWithoutExtension(d).Trim();
            ComboBoxSfx.Items.Add(name);
            if (name == config.SfxName)
                selectIdx = ComboBoxSfx.Items.Count - 1;
        }
        ComboBoxSfx.SelectedIndex = selectIdx;

        selectIdx = 0;
        ComboBoxMusic.Items.Add("<无音乐>");
        dirs = Directory.GetFiles(config.MusicsDir, "*.asm").OrderBy((g) => g);
        foreach (var d in dirs) {
            string name = Path.GetFileNameWithoutExtension(d).Trim();
            ComboBoxMusic.Items.Add(name);
            if (name == config.MusicName)
                selectIdx = ComboBoxMusic.Items.Count - 1;
        }
        ComboBoxMusic.SelectedIndex = selectIdx;
        ComboBoxMusic.Enabled = ComboBoxSfx.SelectedIndex != 1;

        selectIdx = config.CursorBlingCount - 1;
        ComboxCursorBling.Items.Add("<无闪烁>");
        ComboxCursorBling.Items.Add("2色交替");
        ComboxCursorBling.Items.Add("3色交替");
        ComboxCursorBling.Items.Add("4色交替");
        if (selectIdx >= ComboxCursorBling.Items.Count)
            selectIdx = 0;
        ComboxCursorBling.SelectedIndex = selectIdx;


        CheckBoxEnableLastGameSaveing.Checked = config.EnableLastGameSaveing;
        CheckBoxIdxFillZero.Checked = config.IdxFillZero;
        CheckBoxShowIdx.Checked = config.ShowIdx;

        trackBarCurSpeed.Value = convtSpeedToVlaue(config.CursorSpeed);
    }

    List<string> error_adds = new List<string>();
    private void AddToListView(string rompath, string? menuname = null, bool ischecked = true)
    {
        Game game;
        try {
            game = Game.TryLoadRom(rompath);
        } catch {
            error_adds.Add(rompath);
            return;
        }
        // 在这里处理文件，例如添加到ListView中
        ListViewRom item = new ListViewRom();
        item.RomPath = System.IO.Path.GetFullPath(rompath);

        item.Checked = ischecked;
        item.SubItems.Add($"{ListViewFiles.Items.Count + 1}");
        item.SubItems.Add(System.IO.Path.GetFileName(rompath));
        item.SubItems.Add(menuname ?? Game.AutoMenuName(rompath));
        item.SubItems.Add($"{game.PRG.Length / 1024 / 16}");
        item.SubItems.Add($"{game.CHR.Length / 1024 / 8}");
        item.SubItems.Add(game.MapperName);

        item.ToolTipText = System.IO.Path.GetFullPath(rompath);
        ListViewFiles.Items.Add(item);
    }

    private void UpdateListIndexs()
    {
        foreach (ListViewItem item in ListViewFiles.Items) {
            item.SubItems[1].Text = $"{item.Index + 1}";
        }
    }

    private void SaveList()
    {
        StringBuilder builder = new();

        foreach (ListViewRom item in ListViewFiles.Items) {
            builder.AppendLine(item.Checked ? "1" : "0");
            builder.AppendLine(item.RomPath);
            builder.AppendLine(item.SubItems[3].Text);
        }
        File.WriteAllText(config.GameListFile, builder.ToString());
    }

    private void LoadList()
    {
        if (!File.Exists(config.GameListFile)) return;

        error_adds.Clear();
        StreamReader reader = new StreamReader(config.GameListFile);
        while (true) {
            string? ischecked = reader.ReadLine();
            if (ischecked == null) break;
            string? rompath = reader.ReadLine();
            if (rompath == null) break;
            string? menuname = reader.ReadLine();
            if (menuname == null) break;
            AddToListView(rompath, menuname, ischecked == "1");
        }
        reader.Close();
    }

    private void EditTextBox_LostFocus(object sender, EventArgs e)
    {
        //if (editingItem != null)
        //{
        //    editingItem.SubItems[editingSubItemIndex].Text = editTextBox.Text;
        //    editTextBox.Visible = false;
        //    editingItem = null;
        //}
    }
    private void ListViewFiles_DoubleClick(object sender, EventArgs e)
    {
    }

    private void ListViewFiles_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        //if (editingItem == null)
        //{
        //    var hitTestInfo = ListViewFiles.HitTest(e.Location);
        //    if (hitTestInfo.Item != null)
        //    {
        //        editingItem = hitTestInfo.Item;
        //        var subitem = editingItem.SubItems[editingSubItemIndex];
        //        editTextBox.Text = subitem.Text;
        //        //editTextBox.Location = ListViewFiles.PointToScreen(subitem.Bounds.Location);
        //        Point location = PointToClient(ListViewFiles.PointToScreen(subitem.Bounds.Location));
        //        location.Offset(0, 2);
        //        editTextBox.Location = location;
        //        editTextBox.Size = subitem.Bounds.Size;
        //        editTextBox.Visible = true;
        //        editTextBox.Focus();
        //        editTextBox.BringToFront();
        //        //editTextBox.SelectAll();                    
        //    }
        //}
    }

    private void ListViewFiles_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop)) {
            e.Effect = DragDropEffects.Copy; // 或者 DragDropEffects.Move，取决于你的需求
        }
    }


    private void ListViewFiles_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data == null) return;

        var data = e.Data.GetData(DataFormats.FileDrop);
        if (data != null) {
            error_adds.Clear();
            var texts = ((string[])data).OrderBy(a => a);
            foreach (string file in texts) {
                AddToListView(file);
            }
            if (error_adds.Count > 0) {
                MessageBox.Show(string.Join("\n", error_adds), "加载错错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return;
        }

        data = e.Data.GetData(typeof(ListViewRom));
        if (data != null) {
            Point targetPoint = ListViewFiles.PointToClient(new Point(e.X, e.Y));
            int targetIndex = ListViewFiles.HitTest(targetPoint.X, targetPoint.Y).Item?.Index ?? -1;
            int sourceIndex = ((ListViewItem)data).Index;
            if (targetIndex != -1 && targetIndex != sourceIndex) {
                ListViewItem draggedItem = ListViewFiles.Items[sourceIndex];
                ListViewFiles.Items.RemoveAt(sourceIndex);
                ListViewFiles.Items.Insert(targetIndex, draggedItem);

                UpdateListIndexs();
            }
            return;
        }
    }
    private void ListViewFiles_ItemDrag(object sender, ItemDragEventArgs e)
    {
        if (e.Item == null) return;
        DoDragDrop(e.Item, DragDropEffects.Move);
    }

    private void ListViewFiles_DragOver(object sender, DragEventArgs e)
    {
        e.Effect = DragDropEffects.Move;
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        config.Save();
        SaveList();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        LoadList();
    }

    private void ButtonRemove_Click(object sender, EventArgs e)
    {

        foreach (ListViewItem item in ListViewFiles.SelectedItems) {
            ListViewFiles.Items.Remove(item);
        }
        UpdateListIndexs();
    }

    private void ButtonClean_Click(object sender, EventArgs e)
    {
        ListViewFiles.Items.Clear();
    }

    private void ButtonBuild_Click(object sender, EventArgs e)
    {
        var roms = new List<RomFileInfo>();
        foreach (ListViewRom item in ListViewFiles.Items) {
            if (item.Checked) {
                roms.Add(new RomFileInfo(item.RomPath, item.SubItems[3].Text, item.Index + 1));
            }
        }
        if (roms.Count == 0) return;

        Builder builder;
        if (config.Cardtype == Config.Cardtypes.Coolgirl) {
            builder = new CoolgirlBuilder(config);
        } else {
            builder = new CoolemuBuilder(config);
        }
        if (builder.Build(roms)) {
            if (builder.GetSkipInfoCnt() > 0) {
                MessageBox.Show($"Not All Success! {builder.GetSkipInfoText()}", "Build Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                var slist = builder.GetSkipInfoIdxList();
                foreach (int idx in slist) {
                    ListViewFiles.Items[idx - 1].Checked = false;
                }
            } else {
                MessageBox.Show("Success ALL!", "Build Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        } else {
            var skip = builder.GetSkipInfoCnt() > 0 ? builder.GetSkipInfoText() : "";
            MessageBox.Show($"{skip}\n{builder.GetProblemText()}", "Build Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        config.Save();
    }

    private void ComboBoxTheme_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ComboBoxTheme.SelectedItem == null) return;
        config.ThemeName = ComboBoxTheme.SelectedItem.ToString()!;
        //ToolTipTheme.SetToolTip(ComboBoxTheme, ComboBoxTheme.SelectedItem.ToString());
    }

    private void CheckBoxIdx_CheckedChanged(object sender, EventArgs e)
    {
        config.IdxFillZero = CheckBoxIdxFillZero.Checked;
    }

    private void CheckBoxShowIdx_CheckedChanged(object sender, EventArgs e)
    {
        config.ShowIdx = CheckBoxShowIdx.Checked;
        CheckBoxIdxFillZero.Enabled = config.ShowIdx;
    }

    private void CheckBoxEnableLastGameSaveing_CheckedChanged(object sender, EventArgs e)
    {
        config.EnableLastGameSaveing = CheckBoxEnableLastGameSaveing.Checked;
    }

    private void ComboBoxSfx_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ComboBoxSfx.SelectedItem == null) return;
        config.SfxIdx = ComboBoxSfx.SelectedIndex;
        config.SfxName = ComboBoxSfx.SelectedItem.ToString()!;
        //ToolTipSfx.SetToolTip(ComboBoxSfx, ComboBoxSfx.SelectedItem.ToString());

        ComboBoxMusic.Enabled = config.SfxIdx == 0 || config.SfxIdx > config.MaxSfxIdx;

    }

    private void ComboBoxMusic_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ComboBoxMusic.SelectedItem == null) return;
        config.MusicIdx = ComboBoxMusic.SelectedIndex;
        config.MusicName = ComboBoxMusic.SelectedItem.ToString()!;
        //ToolTipMusic.SetToolTip(ComboBoxMusic, ComboBoxMusic.SelectedItem.ToString());
    }

    private void ComboxCursorBling_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ComboxCursorBling.SelectedItem == null) return;
        config.CursorBlingCount = ComboxCursorBling.SelectedIndex + 1;
    }

    int convtSpeedToVlaue(int speed)
    {
        if (speed > 4) speed = 4 + (speed - 4 + 1) / 2;
        int value = trackBarCurSpeed.Maximum - (speed - trackBarCurSpeed.Minimum);
        return value;
    }

    int convtValueToSpeed(int value)
    {
        int speed = trackBarCurSpeed.Maximum - (value - trackBarCurSpeed.Minimum);
        if (speed > 4) speed = 4 + (speed - 4) * 2;
        return speed;
    }

    private void trackBarCurSpeed_Scroll(object sender, EventArgs e)
    {
        config.CursorSpeed = convtValueToSpeed(trackBarCurSpeed.Value);
    }

    private void ComboBoxCardtype_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ComboBoxCardtype.SelectedItem == null) return;
        foreach(var d in Config.CardtypeNameMap) {
            if(d.Value == ComboBoxCardtype.SelectedItem.ToString()) {
                config.Cardtype = d.Key;
                return;
            }
        }        
    }
}

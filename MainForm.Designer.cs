namespace FCCoolBuilder;

partial class MainForm: Form
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        ButtonRemove = new Button();
        ListViewFiles = new MyListView();
        columnHeader1 = new ColumnHeader();
        columnHeader2 = new ColumnHeader();
        columnHeader3 = new ColumnHeader();
        columnHeader4 = new ColumnHeader();
        columnHeader5 = new ColumnHeader();
        columnHeader6 = new ColumnHeader();
        columnHeader7 = new ColumnHeader();
        ButtonClean = new Button();
        ButtonBuild = new Button();
        ComboBoxTheme = new MyComboBox();
        label1 = new Label();
        CheckBoxIdxFillZero = new CheckBox();
        CheckBoxShowIdx = new CheckBox();
        CheckBoxEnableLastGameSaveing = new CheckBox();
        label2 = new Label();
        ComboBoxSfx = new MyComboBox();
        label3 = new Label();
        ComboBoxMusic = new MyComboBox();
        label4 = new Label();
        ComboxCursorBling = new MyComboBox();
        label5 = new Label();
        trackBarCurSpeed = new TrackBar();
        ComboBoxCardtype = new MyComboBox();
        ((System.ComponentModel.ISupportInitialize)trackBarCurSpeed).BeginInit();
        SuspendLayout();
        // 
        // ButtonRemove
        // 
        ButtonRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        ButtonRemove.Location = new Point(834, 12);
        ButtonRemove.Name = "ButtonRemove";
        ButtonRemove.Size = new Size(104, 29);
        ButtonRemove.TabIndex = 1;
        ButtonRemove.Text = "移除";
        ButtonRemove.UseVisualStyleBackColor = true;
        ButtonRemove.Click += ButtonRemove_Click;
        // 
        // ListViewFiles
        // 
        ListViewFiles.AllowDrop = true;
        ListViewFiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        ListViewFiles.CheckBoxes = true;
        ListViewFiles.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5, columnHeader6, columnHeader7 });
        ListViewFiles.FullRowSelect = true;
        ListViewFiles.GridLines = true;
        ListViewFiles.Location = new Point(9, 9);
        ListViewFiles.Margin = new Padding(0);
        ListViewFiles.Name = "ListViewFiles";
        ListViewFiles.Size = new Size(811, 610);
        ListViewFiles.TabIndex = 0;
        ListViewFiles.UseCompatibleStateImageBehavior = false;
        ListViewFiles.View = View.Details;
        ListViewFiles.ItemDrag += ListViewFiles_ItemDrag;
        ListViewFiles.DragDrop += ListViewFiles_DragDrop;
        ListViewFiles.DragEnter += ListViewFiles_DragEnter;
        ListViewFiles.DragOver += ListViewFiles_DragOver;
        ListViewFiles.DoubleClick += ListViewFiles_DoubleClick;
        ListViewFiles.MouseDoubleClick += ListViewFiles_MouseDoubleClick;
        // 
        // columnHeader1
        // 
        columnHeader1.Text = "";
        columnHeader1.Width = 25;
        // 
        // columnHeader2
        // 
        columnHeader2.Text = "#";
        columnHeader2.Width = 40;
        // 
        // columnHeader3
        // 
        columnHeader3.Text = "文件名";
        columnHeader3.Width = 180;
        // 
        // columnHeader4
        // 
        columnHeader4.Text = "菜单名";
        columnHeader4.Width = 180;
        // 
        // columnHeader5
        // 
        columnHeader5.Text = "PRG(16KB)";
        columnHeader5.TextAlign = HorizontalAlignment.Right;
        columnHeader5.Width = 100;
        // 
        // columnHeader6
        // 
        columnHeader6.Text = "CHR(8KB)";
        columnHeader6.TextAlign = HorizontalAlignment.Right;
        columnHeader6.Width = 100;
        // 
        // columnHeader7
        // 
        columnHeader7.Text = "Mapper";
        columnHeader7.TextAlign = HorizontalAlignment.Right;
        columnHeader7.Width = 80;
        // 
        // ButtonClean
        // 
        ButtonClean.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        ButtonClean.Location = new Point(834, 60);
        ButtonClean.Name = "ButtonClean";
        ButtonClean.Size = new Size(104, 29);
        ButtonClean.TabIndex = 2;
        ButtonClean.Text = "清空";
        ButtonClean.UseVisualStyleBackColor = true;
        ButtonClean.Click += ButtonClean_Click;
        // 
        // ButtonBuild
        // 
        ButtonBuild.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        ButtonBuild.Location = new Point(834, 573);
        ButtonBuild.Name = "ButtonBuild";
        ButtonBuild.Size = new Size(104, 29);
        ButtonBuild.TabIndex = 12;
        ButtonBuild.Text = "生成";
        ButtonBuild.UseVisualStyleBackColor = true;
        ButtonBuild.Click += ButtonBuild_Click;
        // 
        // ComboBoxTheme
        // 
        ComboBoxTheme.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        ComboBoxTheme.FormattingEnabled = true;
        ComboBoxTheme.IntegralHeight = false;
        ComboBoxTheme.Location = new Point(834, 274);
        ComboBoxTheme.Name = "ComboBoxTheme";
        ComboBoxTheme.Size = new Size(104, 28);
        ComboBoxTheme.TabIndex = 7;
        ComboBoxTheme.SelectedIndexChanged += ComboBoxTheme_SelectedIndexChanged;
        // 
        // label1
        // 
        label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        label1.AutoSize = true;
        label1.Location = new Point(834, 251);
        label1.Name = "label1";
        label1.Size = new Size(39, 20);
        label1.TabIndex = 13;
        label1.Text = "主题";
        // 
        // CheckBoxIdxFillZero
        // 
        CheckBoxIdxFillZero.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        CheckBoxIdxFillZero.AutoSize = true;
        CheckBoxIdxFillZero.Checked = true;
        CheckBoxIdxFillZero.CheckState = CheckState.Checked;
        CheckBoxIdxFillZero.Location = new Point(834, 215);
        CheckBoxIdxFillZero.Name = "CheckBoxIdxFillZero";
        CheckBoxIdxFillZero.Size = new Size(100, 24);
        CheckBoxIdxFillZero.TabIndex = 6;
        CheckBoxIdxFillZero.Text = "序号填充0";
        CheckBoxIdxFillZero.UseVisualStyleBackColor = true;
        CheckBoxIdxFillZero.CheckedChanged += CheckBoxIdx_CheckedChanged;
        // 
        // CheckBoxShowIdx
        // 
        CheckBoxShowIdx.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        CheckBoxShowIdx.AutoSize = true;
        CheckBoxShowIdx.Checked = true;
        CheckBoxShowIdx.CheckState = CheckState.Checked;
        CheckBoxShowIdx.Location = new Point(834, 185);
        CheckBoxShowIdx.Name = "CheckBoxShowIdx";
        CheckBoxShowIdx.Size = new Size(91, 24);
        CheckBoxShowIdx.TabIndex = 5;
        CheckBoxShowIdx.Text = "显示序号";
        CheckBoxShowIdx.UseVisualStyleBackColor = true;
        CheckBoxShowIdx.CheckedChanged += CheckBoxShowIdx_CheckedChanged;
        // 
        // CheckBoxEnableLastGameSaveing
        // 
        CheckBoxEnableLastGameSaveing.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        CheckBoxEnableLastGameSaveing.AutoSize = true;
        CheckBoxEnableLastGameSaveing.Location = new Point(834, 155);
        CheckBoxEnableLastGameSaveing.Name = "CheckBoxEnableLastGameSaveing";
        CheckBoxEnableLastGameSaveing.Size = new Size(91, 24);
        CheckBoxEnableLastGameSaveing.TabIndex = 4;
        CheckBoxEnableLastGameSaveing.Text = "存储功能";
        CheckBoxEnableLastGameSaveing.UseVisualStyleBackColor = true;
        CheckBoxEnableLastGameSaveing.CheckedChanged += CheckBoxEnableLastGameSaveing_CheckedChanged;
        // 
        // label2
        // 
        label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        label2.AutoSize = true;
        label2.Location = new Point(834, 443);
        label2.Name = "label2";
        label2.Size = new Size(69, 20);
        label2.TabIndex = 16;
        label2.Text = "按键音效";
        // 
        // ComboBoxSfx
        // 
        ComboBoxSfx.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        ComboBoxSfx.FormattingEnabled = true;
        ComboBoxSfx.IntegralHeight = false;
        ComboBoxSfx.Location = new Point(834, 466);
        ComboBoxSfx.Name = "ComboBoxSfx";
        ComboBoxSfx.Size = new Size(104, 28);
        ComboBoxSfx.TabIndex = 10;
        ComboBoxSfx.SelectedIndexChanged += ComboBoxSfx_SelectedIndexChanged;
        // 
        // label3
        // 
        label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        label3.AutoSize = true;
        label3.Location = new Point(834, 501);
        label3.Name = "label3";
        label3.Size = new Size(69, 20);
        label3.TabIndex = 17;
        label3.Text = "背景音乐";
        // 
        // ComboBoxMusic
        // 
        ComboBoxMusic.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        ComboBoxMusic.FormattingEnabled = true;
        ComboBoxMusic.IntegralHeight = false;
        ComboBoxMusic.Location = new Point(834, 524);
        ComboBoxMusic.Name = "ComboBoxMusic";
        ComboBoxMusic.Size = new Size(104, 28);
        ComboBoxMusic.TabIndex = 11;
        ComboBoxMusic.SelectedIndexChanged += ComboBoxMusic_SelectedIndexChanged;
        // 
        // label4
        // 
        label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        label4.AutoSize = true;
        label4.Location = new Point(834, 313);
        label4.Name = "label4";
        label4.Size = new Size(69, 20);
        label4.TabIndex = 14;
        label4.Text = "光标闪烁";
        // 
        // ComboxCursorBling
        // 
        ComboxCursorBling.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        ComboxCursorBling.FormattingEnabled = true;
        ComboxCursorBling.Location = new Point(834, 336);
        ComboxCursorBling.Name = "ComboxCursorBling";
        ComboxCursorBling.Size = new Size(104, 28);
        ComboxCursorBling.TabIndex = 8;
        ComboxCursorBling.SelectedIndexChanged += ComboxCursorBling_SelectedIndexChanged;
        // 
        // label5
        // 
        label5.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        label5.AutoSize = true;
        label5.Location = new Point(834, 375);
        label5.Name = "label5";
        label5.Size = new Size(69, 20);
        label5.TabIndex = 15;
        label5.Text = "光标速度";
        // 
        // trackBarCurSpeed
        // 
        trackBarCurSpeed.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        trackBarCurSpeed.Location = new Point(823, 397);
        trackBarCurSpeed.Minimum = 1;
        trackBarCurSpeed.Name = "trackBarCurSpeed";
        trackBarCurSpeed.Size = new Size(125, 56);
        trackBarCurSpeed.TabIndex = 9;
        trackBarCurSpeed.Value = 1;
        trackBarCurSpeed.Scroll += trackBarCurSpeed_Scroll;
        // 
        // ComboBoxCardtype
        // 
        ComboBoxCardtype.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        ComboBoxCardtype.FormattingEnabled = true;
        ComboBoxCardtype.Location = new Point(834, 112);
        ComboBoxCardtype.Name = "ComboBoxCardtype";
        ComboBoxCardtype.Size = new Size(104, 28);
        ComboBoxCardtype.TabIndex = 3;
        ComboBoxCardtype.SelectedIndexChanged += ComboBoxCardtype_SelectedIndexChanged;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(953, 628);
        Controls.Add(ComboBoxCardtype);
        Controls.Add(label5);
        Controls.Add(ComboxCursorBling);
        Controls.Add(label4);
        Controls.Add(ComboBoxMusic);
        Controls.Add(label3);
        Controls.Add(ComboBoxSfx);
        Controls.Add(label2);
        Controls.Add(CheckBoxEnableLastGameSaveing);
        Controls.Add(CheckBoxShowIdx);
        Controls.Add(CheckBoxIdxFillZero);
        Controls.Add(label1);
        Controls.Add(ComboBoxTheme);
        Controls.Add(ButtonBuild);
        Controls.Add(ButtonClean);
        Controls.Add(ButtonRemove);
        Controls.Add(ListViewFiles);
        Controls.Add(trackBarCurSpeed);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "FCCoolBuilder";
        FormClosed += MainForm_FormClosed;
        Load += MainForm_Load;
        ((System.ComponentModel.ISupportInitialize)trackBarCurSpeed).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button ButtonRemove;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private ColumnHeader columnHeader5;
    private ColumnHeader columnHeader6;
    private ColumnHeader columnHeader7;
    private Button ButtonClean;
    private Button ButtonBuild;
    private MyListView ListViewFiles;
    private MyComboBox ComboBoxTheme;
    private Label label1;
    private CheckBox CheckBoxIdxFillZero;
    private CheckBox CheckBoxShowIdx;
    private CheckBox CheckBoxEnableLastGameSaveing;
    private Label label2;
    private MyComboBox ComboBoxSfx;
    private Label label3;
    private MyComboBox ComboBoxMusic;
    private Label label4;
    private MyComboBox ComboxCursorBling;
    private Label label5;
    private TrackBar trackBarCurSpeed;
    private MyComboBox ComboBoxCardtype;
}

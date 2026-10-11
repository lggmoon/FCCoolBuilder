using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCCoolBuilder;

internal class ListViewRom : ListViewItem
{
    public string RomPath = "";
}

internal class MyListView : ListView
{
    private TextBox editTextBox;
    private ListViewItem? editingItem = null;
    private int editingSubItemIndex = 3;

    public MyListView() : base()
    {
        editTextBox = new TextBox();
        editTextBox.BorderStyle = BorderStyle.None;
        editTextBox.Visible = false;
        editTextBox.LostFocus += EditTextBox_LostFocus;
        editTextBox.KeyDown += EditTextBox_KeyDown;
        Controls.Add(editTextBox);

        
    }

    protected void EditTextBox_LostFocus(object? sender, EventArgs e)
    {            
        if (editingItem != null)
        {
            editingItem.SubItems[editingSubItemIndex].Text = editTextBox.Text;
            editTextBox.Visible = false;
            editingItem = null;
        }
    }

    protected void EditTextBox_KeyDown(object? sender, KeyEventArgs e)
    {

        if (editingItem!=null && e.KeyCode == Keys.Enter)
        {
            EditTextBox_LostFocus(sender, EventArgs.Empty);
        }
        else if (editingItem!=null && e.KeyCode == Keys.Escape)
        {
            editTextBox.Text = editingItem.SubItems[editingSubItemIndex].Text; // 撤销更改
            EditTextBox_LostFocus(sender, EventArgs.Empty);
        }
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_MOUSEWHEEL = 0x0203;
        if (m.Msg == WM_MOUSEWHEEL)
        {
            if (Columns.Count >= editingSubItemIndex)
            {
                Point mousepos = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
                if (editingItem == null)
                {
                    var hitTestInfo = HitTest(mousepos);
                    if (hitTestInfo.Item != null)
                    {
                        editingItem = hitTestInfo.Item;
                        var subitem = editingItem.SubItems[editingSubItemIndex];
                        editTextBox.Text = subitem.Text;                            
                        Point loc = subitem.Bounds.Location;
                        loc.Offset(4, 2);
                        editTextBox.Location = loc;
                        editTextBox.Size = new Size(subitem.Bounds.Size.Width - 8, subitem.Bounds.Size.Height);
                        editTextBox.Visible = true;
                        editTextBox.Focus();
                        editTextBox.BringToFront();
                        mousepos = editTextBox.PointToClient(Control.MousePosition);                            
                        editTextBox.Select(editTextBox.GetCharIndexFromPosition(mousepos), 0);
                        //editTextBox.SelectAll();              
                    }
                }
                return;
            }
        }
        base.WndProc(ref m);
    }
}

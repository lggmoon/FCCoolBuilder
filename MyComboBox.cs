using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCCoolBuilder;

internal class MyComboBox : ComboBox
{
    private ToolTip ToolTipBox = new ToolTip();
    protected override void OnDropDown(EventArgs e)
    {
        base.OnDropDown(e);
        AdjustDropListWidth();
    }

    private void AdjustDropListWidth()
    {
        int vertScrollBarWidth = (this.Items.Count > this.MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;

        int maxWidth = this.Width;
        foreach (var layouts in this.Items)
        {
            int measureTextWidth = TextRenderer.MeasureText(layouts.ToString(), this.Font).Width;
            maxWidth = maxWidth < measureTextWidth ? measureTextWidth : maxWidth;
        }

        this.DropDownWidth = maxWidth + vertScrollBarWidth;
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);            

        if (SelectedItem != null){
            int measureTextWidth = TextRenderer.MeasureText(SelectedItem.ToString(), this.Font).Width;
            if (measureTextWidth > this.Width - 16)
            {
                ToolTipBox.SetToolTip(this, SelectedItem.ToString());
                return;
            }
        }
        ToolTipBox.SetToolTip(this, null);
    }

}

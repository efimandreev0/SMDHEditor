using System;
using System.Net.Mail;

namespace SMDHEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new();
            if (o.ShowDialog() == DialogResult.OK)
            {
                SMDH.Read(o.FileName);
                checkedListBox1.Items.Clear();
                foreach (var item in SMDH.flagDescriptions)
                {
                    checkedListBox1.Items.Add(item.Value);
                    if ((SMDH.f.settings.FlagsTMP & (uint)item.Key) != 0) checkedListBox1.SetItemChecked(checkedListBox1.Items.Count - 1, true);
                    else checkedListBox1.SetItemChecked(checkedListBox1.Items.Count - 1, false);
                }
                dataGridView1.DataSource = SMDH.f.titles;

            }
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            bool result = false;
            if (checkedListBox1.SelectedIndex > -1)
            {
                //SMDH.f.settings.Flags.Clear();
                int i = 0;
                foreach (var item in SMDH.flagDescriptions)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        if (checkedListBox1.Items[e.Index] == item.Value)
                            SMDH.f.settings.Flags.Add(item.Key);
                    }

                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        if (checkedListBox1.Items[e.Index] == item.Value)
                            SMDH.f.settings.Flags.Remove(item.Key);
                    }
                    i++;
                }
                SMDH.f.settings.FlagsTMP = CombineFlags(SMDH.f.settings.Flags);
            }
        }
        public static uint CombineFlags(List<GameFlag> flags)
        {
            // Используем LINQ для объединения всех флагов
            return flags.Aggregate(0u, (current, flag) => current | (uint)flag);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new();
            if (s.ShowDialog() == DialogResult.OK)
            {
                SMDH.f.titles = (List<SMDH.Title>)dataGridView1.DataSource;
                SMDH.Write(s.FileName);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SMDH.f != null)
            {
                if (listBox1.SelectedIndex == 0)
                    pictureBox1.Image = SMDH.smallIcon;
                if (listBox1.SelectedIndex == 1)
                    pictureBox1.Image = SMDH.bigIcon;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new();
            if (s.ShowDialog() == DialogResult.OK)
            {
                if (SMDH.f != null)
                {
                    if (listBox1.SelectedIndex == 0)
                        SMDH.smallIcon.Save(s.FileName);
                    if (listBox1.SelectedIndex == 1)
                        SMDH.bigIcon.Save(s.FileName);
                }
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog s = new();
            if (s.ShowDialog() == DialogResult.OK)
            {
                if (SMDH.f.IconSmall != null)
                {
                    if (listBox1.SelectedIndex == 0)
                        SMDH.smallIcon = new Bitmap(s.FileName);
                    if (listBox1.SelectedIndex == 1)
                        SMDH.bigIcon = new Bitmap(s.FileName);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new();
            if (s.ShowDialog() == DialogResult.OK)
            {
                if (SMDH.f != null)
                {
                    if (listBox1.SelectedIndex == 0)
                        SMDH.smallIcon.Save(s.FileName);
                    if (listBox1.SelectedIndex == 1)
                        SMDH.bigIcon.Save(s.FileName);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog s = new();
            if (s.ShowDialog() == DialogResult.OK)
            {
                if (SMDH.f.IconSmall != null)
                {
                    if (listBox1.SelectedIndex == 0)
                        SMDH.smallIcon = new Bitmap(s.FileName);
                    if (listBox1.SelectedIndex == 1)
                        SMDH.bigIcon = new Bitmap(s.FileName);
                }
            }
        }
    }
}

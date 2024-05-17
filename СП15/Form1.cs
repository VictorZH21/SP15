using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic;

namespace СП15
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadDrives();
        }
        private void LoadDrives()
        {
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();

                foreach (var drive in drives)
                {
                    if (drive.IsReady)
                    {
                        TreeNode treeNode = treeView1.Nodes.Add(drive.Name);
                        treeNode.Tag = drive.RootDirectory.FullName;
                        treeNode.ImageIndex = 0;
                        treeNode.Nodes.Add("");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("При загрузке дисков произошла ошибка: " + ex.Message);
            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {
                TreeNode selectedNode = e.Node;
                selectedNode.Nodes.Clear();
                string path = (string)selectedNode.Tag;

                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                foreach (var dir in directoryInfo.GetDirectories())
                {
                    TreeNode node = selectedNode.Nodes.Add(dir.Name);
                    node.Tag = dir.FullName;

                    node.ImageIndex = 1;
                    node.Nodes.Add("");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("При развертывании узла произошла ошибка: " + ex.Message);
            }
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                listView1.Items.Clear();
                string path = (string)e.Node.Tag;

                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                foreach (var file in directoryInfo.GetFiles())
                {
                    ListViewItem item = new ListViewItem(file.Name);
                    item.SubItems.Add(file.Length.ToString());
                    item.SubItems.Add(file.LastWriteTime.ToString());
                    item.ImageIndex = 2;
                    listView1.Items.Add(item);
                }
                DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(path));
                label1.Text = string.Format("Drive: {0}", path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("При выборе узла произошла ошибка: " + ex.Message);
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    string path = (string)listView1.SelectedItems[0].Tag;
                    if (File.Exists(path))
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                    else if (Directory.Exists(path))
                    {
                        treeView1.SelectedNode = FindNodeByPath(treeView1.Nodes, path);
                    }
                    else
                    {
                        MessageBox.Show("Выбранный элемент не является файлом или папкой.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("При открытии файла произошла ошибка: " + ex.Message);
            }
        }
        private TreeNode FindNodeByPath(TreeNodeCollection nodes, string path)
        {
            foreach (TreeNode node in nodes)
            {
                if ((string)node.Tag == path)
                {
                    return node;
                }
                TreeNode found = FindNodeByPath(node.Nodes, path);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        private void RefreshTreeNode(TreeNode node)
        {
            node.Nodes.Clear();
            string path = (string)node.Tag;

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (var dir in directoryInfo.GetDirectories())
            {
                TreeNode newNode = new TreeNode(dir.Name);
                newNode.Tag = dir.FullName;
                newNode.ImageIndex = 1;
                newNode.Nodes.Add("");
                node.Nodes.Add(newNode);
            }
        }
        private void RefreshListView(string path)
        {
            listView1.Items.Clear();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (var dir in directoryInfo.GetDirectories())
            {
                ListViewItem item = new ListViewItem(dir.Name);
                item.SubItems.Add("Папка");
                item.SubItems.Add(dir.LastWriteTime.ToString());
                item.ImageIndex = 1;
                item.Tag = dir.FullName;
                listView1.Items.Add(item);
            }

            foreach (var file in directoryInfo.GetFiles())
            {
                ListViewItem item = new ListViewItem(file.Name);
                item.SubItems.Add(file.Length.ToString());
                item.SubItems.Add(file.LastWriteTime.ToString());
                item.ImageIndex = 2;
                item.Tag = file.FullName;
                listView1.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;

            string newFolderName = Interaction.InputBox("Введите имя новой папки.", "Создать новую папку.", "Новая папка");
            if (string.IsNullOrEmpty(newFolderName)) return;

            string newPath = Path.Combine((string)treeView1.SelectedNode.Tag, newFolderName);

            try
            {
                Directory.CreateDirectory(newPath);
                RefreshTreeNode(treeView1.SelectedNode);
                RefreshListView((string)treeView1.SelectedNode.Tag);
                MessageBox.Show("Папка успешно создана! ");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании папки: " + ex.Message);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string path = treeView1.SelectedNode.FullPath + "\\";
            string newFileName = Interaction.InputBox("Новое имя файла.", "Создать новый файл.", "новое имя");

            StreamWriter st = File.CreateText(path + newFileName + ".txt");
            st.Close();

            listView1.Items.Add(newFileName + ".txt", 2);
            MessageBox.Show("Текстовый файл успешно создан! ");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string oldfile = treeView1.SelectedNode.FullPath + "\\" + listView1.FocusedItem.Text;
                string newName = Interaction.InputBox("Новое имя файла.", "Переименуйте файл.", "новое имя");
                if (string.IsNullOrEmpty(newName))
                    return;

                FileSystem.Rename(oldfile, treeView1.SelectedNode.FullPath + "\\" + newName + ".txt");

                listView1.FocusedItem.Remove();
                listView1.Items.Add(newName + ".txt", 2);
                Validate();
                MessageBox.Show("Текстовый файл успешно переименован! ");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string path = (string)listView1.SelectedItems[0].Tag;

                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    else if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    RefreshTreeNode(treeView1.SelectedNode);
                    RefreshListView((string)treeView1.SelectedNode.Tag);
                    MessageBox.Show("Ярлык успешно удалён! ");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
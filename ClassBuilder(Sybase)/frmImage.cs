using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;

namespace ClassBuilderSybase
{
    public partial class frmImage : Form
    {
        string ConnStr = "Server=localhost;Port=3306;Database=CIYFIM;Uid=MykAdmin;Pwd=michale@153;";

        public frmImage()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            //SaveFileDialog sfd = new SaveFileDialog();
            //sfd.Filter = "JPEG Files|*.jpg| PNG Files|*.png";
            //sfd.ShowDialog();
            //pbxPic.ImageLocation = sfd.FileName;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPEG Files|*.jpg| PNG Files|*.png";
            ofd.ShowDialog();
            if (ofd.FileName.Trim().Length > 0)
            {
                pbxPic.ImageLocation = ofd.FileName;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                FileStream fs = new FileStream(pbxPic.ImageLocation, FileMode.Open, FileAccess.Read);
                long FileSize = fs.Length;

                if (FileSize > 65535L)
                {
                    MessageBox.Show("Image size must be less than 65K(65535 bytes)");
                    return;
                }

                byte[] RawData = new byte[FileSize];
                fs.Read(RawData, 0, (int)FileSize);
                fs.Close();

                using (MySqlConnection con = new MySqlConnection(ConnStr))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        MySqlParameter[] Params = new MySqlParameter[2];
                        Params[0] = new MySqlParameter("?a", RawData);
                        //Params[0].MySqlDbType = MySqlDbType.MediumBlob;
                        Params[1] = new MySqlParameter("?b", "filename");
                        Params[1].MySqlDbType = MySqlDbType.VarChar;

                        cmd.Connection = con;
                        cmd.CommandText = "insert into ciyfim.mempic(ImgPic, path) values(?a, ?b)";

                        foreach (MySqlParameter Param in Params)
                        {
                            cmd.Parameters.Add(Param);
                        }

                        int Count = cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Suck");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRetrieve_Click(object sender, EventArgs e)
        {
            byte[] ImgBuffer = null;

            //using (MySqlConnection con = new MySqlConnection(ConnStr))
            //{
            //    con.Open();
            //    using (MySqlCommand cmd = new MySqlCommand())
            //    {
            //        cmd.Connection = con;
            //        cmd.CommandText = "select ImgPic from ciyfim.mempic limit 1";

            //        MySqlDataReader dr = cmd.ExecuteReader();

            //        if (dr.Read())
            //        {
            //            ImgBuffer = (byte[])dr.GetValue(0);
            //        }
            //    }
            //}

            DataTable dt = frmBuilder.GetDataTable("select ImgPic from ciyfim.mempic limit 1");

            ImgBuffer = (byte[])dt.Rows[0][0];

            MemoryStream ms = new MemoryStream(ImgBuffer);
            pbxPic.Image = Image.FromStream(ms);
        }
    }
}
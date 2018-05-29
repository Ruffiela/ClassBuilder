using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;

namespace ClassBuilderSybase
{
    public partial class frmBuilder : Form
    {
        static string ConnStr = "";

        public frmBuilder()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            string SelectCmd = "";
            string OutputPath = txtPath.Text;
            string OutputFile = "";

            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);

            DataTable dt = new DataTable();

            if (!ConnectToDB())
            {
                return;
            }

            foreach (DataGridViewRow dgvr in dgvTables.Rows)
            {

                if (Convert.ToBoolean(dgvr.Cells[0].Value))
                {
                    StringBuilder strBuilder = new StringBuilder();
                    StreamWriter sw;

                    string strTableName = Convert.ToString(dgvr.Cells[1].Value);

                    OutputFile = OutputPath + "\\" + strTableName.ToString() + ".cs";
              
                    SelectCmd = "exec sp_columns " + strTableName;

                    dt = GetDataTable(SelectCmd);
                    if (dt != null)
                    {
                        sw = new StreamWriter(OutputFile);

                        int RowCount = dt.Rows.Count;
                        strBuilder.AppendLine("using System;");
                        strBuilder.AppendLine("using System.Collections.Generic;");
                        strBuilder.AppendLine("using System.Text;");                        
                        strBuilder.AppendLine("namespace " + txtNameSpace.Text);
                        strBuilder.AppendLine("{");
                        strBuilder.AppendLine("\tpublic enum E" + strTableName.ToString());
                        strBuilder.AppendLine("\t{");

                        for (int x = 0; x < RowCount; x++)
                        {
                            string ColName = dt.Rows[x]["column_name"].ToString();
                            if (x == RowCount - 1)
                            {
                                strBuilder.AppendLine("\t\t" + ColName);
                            }
                            else
                            {
                                strBuilder.AppendLine("\t\t" + ColName + ",");
                            }
                        }

                        strBuilder.AppendLine("\t}\r\n");

                        strBuilder.AppendLine("\tpublic class " + strTableName.ToString());
                        strBuilder.AppendLine("\t{");
                        strBuilder.AppendLine(String.Format("\t\tpublic static string Name = typeof({0}).Name;\n", strTableName.ToString()));
                        //strBuilder.AppendLine("\t\t{");
                        //strBuilder.AppendLine("\t\t\tget { return \"" + strTableName.ToString() + "\";}");
                        //strBuilder.AppendLine("\t\t}");

                        for (int x = 0; x < RowCount; x++)
                        {
                            string ColName = dt.Rows[x]["column_name"].ToString();
                            string DataType = dt.Rows[x]["type_name"].ToString();

                            if ("unitext,univarchar,varchar,nvarchar, datetime, date, time, timestamp, year, uniqueidentifier, nchar".Contains(DataType.ToLower()))
                            {//string
                                strBuilder.AppendLine("\t\tstring _" + ColName + " = null;");
                            }
                            else if ("int,smallint ".Contains(DataType.ToLower()))
                            {//int
                                strBuilder.AppendLine("\t\tint? _" + ColName + " = null;");
                            }
                            else if ("decimal,numeric, numeric identity".Contains(DataType.ToLower()))
                            {//decimal
                                strBuilder.AppendLine("\t\tdecimal? _" + ColName + " = null;");
                            }
                            else if ("tinyint, bit".Contains(DataType.ToLower()))
                            {//boolean
                                strBuilder.AppendLine("\t\tbool _" + ColName + " = false;");
                            }
                            else if ("char".Contains(DataType.ToLower()))
                            {//char
                                strBuilder.AppendLine("\t\tchar _" + ColName + " = new char();");
                            }
                            else 
                            {
                                if (MessageBox.Show("Unreconized datatype found " + DataType.ToString() + ".\nDo you still want to continue?", "Unrecognized Type", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                                {
                                    sw.Close();
                                    return;
                                }
                            }
                        }

                        strBuilder.AppendLine("\r\n");

                        for (int x = 0; x < RowCount; x++)
                        {
                            string ColName = dt.Rows[x]["column_name"].ToString();
                            string DataType = dt.Rows[x]["type_name"].ToString();

                            if ("unitext, univarchar, varchar,nvarchar, datetime, date, time, timestamp, year, uniqueidentifier, nchar".Contains(DataType.ToLower()))
                            {//string
                                strBuilder.AppendLine("\t\tpublic string " + ColName);
                            }
                            else if ("int, smallint".Contains(DataType.ToLower()))
                            {//int
                                strBuilder.AppendLine("\t\tpublic int? " + ColName);
                            }
                            else if ("decimal, numeric, numeric identity".Contains(DataType.ToLower()))
                            {//decimal
                                strBuilder.AppendLine("\t\tpublic decimal? " + ColName);
                            }
                            else if ("tinyint, bit".Contains(DataType.ToLower()))
                            {//boolean
                                strBuilder.AppendLine("\t\tpublic bool " + ColName);
                            }
                            else if ("char".Contains(DataType.ToLower()))
                            {//char
                                strBuilder.AppendLine("\t\tpublic char " + ColName);
                            }
                            else
                            {
                                if (MessageBox.Show("Unreconized datatype found " + DataType.ToString() + ".\nDo you still want to continue?", "Unrecognized Type", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                                {
                                    sw.Close();
                                    return;
                                }
                            }
                            strBuilder.AppendLine("\t\t{");
                            strBuilder.AppendLine("\t\t\tget{ return _" + ColName + "; }");
                            strBuilder.AppendLine("\t\t\tset{ _" + ColName + " = value; }");
                            strBuilder.AppendLine("\t\t}");

                        }
                        strBuilder.AppendLine("\t}");
                        string BlString = string.Format("\tpublic class {0} : System.ComponentModel.BindingList<{1}>", "bl" + strTableName.ToString(), strTableName.ToString());
                        BlString += " { }";
                        strBuilder.AppendLine(BlString);
                        strBuilder.AppendLine("}");

                        sw.Write(strBuilder.ToString());
                        sw.WriteLine("");
                        sw.Close();

                    }

                }
            }

            #region MyRegion
            //foreach (object strTableName in lbxTables.Items)
            //{
            //    OutputFile = OuputPath + "\\" + strTableName.ToString() + ".cs";

            //    SelectCmd = string.Format("select COLUMN_NAME, DATA_TYPE from information_schema.COLUMNS where TABLE_NAME = '{0}' order by ORDINAL_POSITION", strTableName.ToString());

            //    dt = GetDataTable(SelectCmd);
            //    if (dt != null)
            //    {
            //        sw = new StreamWriter(OutputFile);

            //        int RowCount = dt.Rows.Count;
            //        strBuilder.AppendLine("using System;");
            //        strBuilder.AppendLine("using System.Collections.Generic;");
            //        strBuilder.AppendLine("using System.Text;");
            //        strBuilder.AppendLine("namespace " + txtNameSpace.Text);
            //        strBuilder.AppendLine("{");
            //        strBuilder.AppendLine("\tpublic enum E" + strTableName.ToString());
            //        strBuilder.AppendLine("\t{");

            //        for (int x = 0; x < RowCount; x++)
            //        {
            //            string ColName = dt.Rows[x][0].ToString();
            //            strBuilder.AppendLine("\t\t" + ColName + ",");
            //        }

            //        strBuilder.AppendLine("\t}\r\n");

            //        strBuilder.AppendLine("\tpublic class " + strTableName.ToString());
            //        strBuilder.AppendLine("\t{");

            //        for (int x = 0; x < RowCount; x++)
            //        {
            //            string ColName = dt.Rows[x][0].ToString();
            //            string DataType = dt.Rows[x][1].ToString();

            //            if ("varchar, datetime, date, time, timestamp, year".Contains(DataType.ToLower()))
            //            {//string
            //                strBuilder.AppendLine("\t\tstring _" + ColName + " = \"\";");
            //            }
            //            else if ("int, ".Contains(DataType.ToLower()))
            //            {//int
            //                strBuilder.AppendLine("\t\tint _" + ColName + " = 0;");
            //            }
            //            else if ("decimal".Contains(DataType.ToLower()))
            //            {//decimal
            //                strBuilder.AppendLine("\t\tdecimal _" + ColName + " = 0M;");
            //            }
            //            else if ("tinyint".Contains(DataType.ToLower()))
            //            {//boolean
            //                strBuilder.AppendLine("\t\tbool _" + ColName + " = false;");
            //            }
            //            else if ("char".Contains(DataType.ToLower()))
            //            {//char
            //                strBuilder.AppendLine("\t\tchar _" + ColName + " = new char();");
            //            }
            //        }

            //        strBuilder.AppendLine("\r\n");

            //        for (int x = 0; x < RowCount; x++)
            //        {
            //            string ColName = dt.Rows[x][0].ToString();
            //            string DataType = dt.Rows[x][1].ToString();

            //            if ("varchar, datetime, date, time, timestamp, year".Contains(DataType.ToLower()))
            //            {//string
            //                strBuilder.AppendLine("\t\tpublic string " + ColName);
            //            }
            //            else if ("int, ".Contains(DataType.ToLower()))
            //            {//int
            //                strBuilder.AppendLine("\t\tpublic int " + ColName);
            //            }
            //            else if ("decimal".Contains(DataType.ToLower()))
            //            {//decimal
            //                strBuilder.AppendLine("\t\tpublic decimal " + ColName);
            //            }
            //            else if ("tinyint".Contains(DataType.ToLower()))
            //            {//boolean
            //                strBuilder.AppendLine("\t\tpublic bool " + ColName);
            //            }
            //            else if ("char".Contains(DataType.ToLower()))
            //            {//char
            //                strBuilder.AppendLine("\t\tpublic char " + ColName);
            //            }
            //            strBuilder.AppendLine("\t\t{");
            //            strBuilder.AppendLine("\t\t\tget{ return _" + ColName + "; }");
            //            strBuilder.AppendLine("\t\t\tset{ _" + ColName + " = value; }");
            //            strBuilder.AppendLine("\t\t}");

            //        }
            //        strBuilder.AppendLine("\t}");
            //        strBuilder.AppendLine("}");

            //        sw.Write(strBuilder.ToString());
            //        sw.WriteLine("");
            //        sw.Close();

            //    }
            //} 
            #endregion
            MessageBox.Show("Done!");
            
        }

        public static DataTable GetDataTable(string SelectCommand)
        {
            try
            {
                //string ConnStr = "Driver={MyOleDb ODBC 3.51 Driver};Server=localhost; Port = 3306;Database=CIYFIM; User=MykAdmin;Password = michale@153; Initial Catalog = CIYFIM; Option=3";

                using (OleDbConnection con = new OleDbConnection(ConnStr))
                {
                    using (OleDbDataAdapter da = new OleDbDataAdapter(SelectCommand, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            txtPath.Text = fbd.SelectedPath;
        }

       


        private void btnConnect_Click(object sender, EventArgs e)
        {
            string Select = "SELECT name FROM master..sysdatabases";

            DataTable dt = GetDataTable(Select);

            dgvDatabases.DataSource = dt;

            foreach (DataGridViewColumn dgvc in dgvDatabases.Columns)
            {
                dgvc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void dgvDatabases_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDatabases.DataSource != null)
            {
                if (dgvDatabases.SelectedRows.Count > 0)
                {
                    string SelectCmd = String.Format("select [name],[id] from {0}..sysobjects where type = 'U' order by lower(name)", dgvDatabases.SelectedRows[0].Cells[0].Value.ToString());

                    DataTable dt = GetDataTable(SelectCmd);

                    dgvTables.Rows.Clear();                    

                    if (dt != null)
                    {
                        foreach (DataRow Drow in dt.Rows)
                        {
                            dgvTables.Rows.Add(false,Drow[0].ToString(),Drow[1]);
                        }
                    }                    
                }
            }
        }

        private void dgvTables_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Space)
            {
                foreach (DataGridViewRow dgvr in dgvTables.SelectedRows)
                {
                    dgvr.Cells[0].Value = !Convert.ToBoolean(dgvr.Cells[0].Value);
                }
            }
        }

        private void dgvDatabases_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtInitialCat.Text = dgvDatabases.Rows[e.RowIndex].Cells[0].Value.ToString();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (ConnectToDB())
                MessageBox.Show("Connection testing success");
            else
            {               
                btnConnect.Enabled = false;
            }
        }

        private bool ConnectToDB()
        {
            try
            {
                string[] Values = new string[5];
                Values[0] = txtServer.Text;
                Values[1] = txtPort.Text;
                Values[2] = txtUsername.Text;
                Values[3] = txtPassword.Text;
                Values[4] = txtInitialCat.Text.Trim();

                ConnStr = String.Format("Provider=ASEOLEDB.1;Data Source={0}:{1};userid={2};Initial Catalog={4}; password={3};charset=utf8", Values);

                OleDbConnection con = new OleDbConnection(ConnStr);
                con.Open();
                con.Close();
                btnConnect.Enabled = true;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection cannot be established." + ex.Message);
                return false;
            }
        }
    }
}
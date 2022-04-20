using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows.Forms;

namespace Yxl
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class FormMain : Form
    {
        #region 字段
        /// <summary>
        /// 内存表
        /// </summary>
        DataTable dt;
        #endregion

        #region 构造方法
        /// <summary>
        /// 主窗体 构造方法
        /// </summary>
        public FormMain()
        {
            InitializeComponent(); 
            dgv.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dgv, true);//双缓冲
            


            /*ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process");
            List<object> values;
            foreach (ManagementObject mo in searcher.Get())
            {
                values = new List<object>();
                foreach(var v in mo.Properties)
                {
                    values.Add(v.Value);
                }
            }*/
            tsrServices_Click(null,null);
        }
        #endregion

        #region 工具栏句柄
        /// <summary>
        /// 过滤框文本改变
        /// </summary>
        /// <param name="sender">控件</param>
        /// <param name="e">事件参数</param>
        private void tstFilter_TextChanged(object sender, EventArgs e)
        {
            if (tsrServices.Checked)
            {
                dt.DefaultView.RowFilter = String.Format("名称 like '%{0}%' or 描述 like '%{0}%' or 可执行文件路径 like '%{0}%'", tstFilter.Text.Trim().Replace("%", "[%]").Replace("*", "[*]").Replace("[", "[[]"));
            }
            else if (tsrProcess.Checked)
            {
                dt.DefaultView.RowFilter = String.Format("名称 like '%{0}%' or 主窗口标题 like '%{0}%' or 可执行文件路径 like '%{0}%'", tstFilter.Text.Trim().Replace("%", "[%]").Replace("*", "[*]").Replace("[", "[[]"));
            }
        }


        /// <summary>
        /// 点击刷新按钮
        /// </summary>
        /// <param name="sender">控件</param>
        /// <param name="e">事件参数</param>
        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            if (tsrServices.Checked)
            {
                tsrServices_Click(null, null);
            }
            else if (tsrProcess.Checked)
            {
                tsrProcess_Click(null, null);
            }
        }

        /// <summary>
        /// 点击服务按钮
        /// </summary>
        /// <param name="sender">控件</param>
        /// <param name="e">事件参数</param>
        private void tsrServices_Click(object sender, EventArgs e)
        {
            tsrProcess.Checked = false;
            tsrServices.Checked = true;
            dt = new DataTable();
            dt.DefaultView.ListChanged += (o, x) => tslCount.Text = dt.DefaultView.Count.ToString();//内存行数增减
            dgv.DataSource = dt;//绑定
            dt.Columns.Add("名称");
            dt.Columns.Add("描述");
            dt.Columns.Add("启动类型");
            dt.Columns.Add("状态");
            dt.Columns.Add("可执行文件路径");
            dt.Columns.Add("登录为");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
            string sStartMode = null;
            string sStartName = null;
            foreach (ManagementObject mo in searcher.Get())
            {
                switch(mo["StartMode"].ToString())
                {
                    case "Manual":
                        sStartMode = "手动";
                        break;
                    case "Auto":
                        sStartMode = "自动";
                        break;
                    case "Disabled":
                        sStartMode = "禁用";
                        break;
                }
                switch ($"{mo["StartName"]}")
                {
                    case "LocalSystem":
                        sStartName = "本地系统";
                        break;
                    case "NT AUTHORITY\\LocalService":
                        sStartName = "本地服务";
                        break;
                    case "NT Authority\\NetworkService":
                        sStartName = "网络服务";
                        break;
                }
                try
                {
                    dt.Rows.Add(mo["Name"], mo["DisplayName"], sStartMode, Convert.ToBoolean(mo["Started"])? "正运行" : "已停止", mo["PathName"], sStartName);
                }
                catch
                {

                }
            }
            dgv.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// 点击进程按钮
        /// </summary>
        /// <param name="sender">控件</param>
        /// <param name="e">事件参数</param>

        private void tsrProcess_Click(object sender, EventArgs e)
        {
            tsrServices.Checked = false;
            tsrProcess.Checked = true;
            dt = new DataTable();
            dt.DefaultView.ListChanged += (o, x) => tslCount.Text = dt.DefaultView.Count.ToString();//内存行数增减
            dgv.DataSource = dt;//绑定
            dt.Columns.Add("PID");
            dt.Columns.Add("名称");
            dt.Columns.Add("主窗口标题");
            dt.Columns.Add("启动时间",Type.GetType("System.DateTime"));
            dt.Columns.Add("可执行文件路径");
            foreach(Process p in Process.GetProcesses())
            {
                try
                {
                    dt.Rows.Add(p.Id, p.ProcessName, p.MainWindowTitle,p.StartTime , p.MainModule.FileName);
                }
                catch
                {
                }
            }
            dgv.Columns[3].DefaultCellStyle.Format = "dd HH:mm:ss.ff";
        }
        #endregion

        #region 表格句柄
        /// <summary>
        /// 鼠标单击单元格
        /// </summary>
        /// <param name="sender">控件</param>
        /// <param name="e">事件参数</param>
        private void dgv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip cms = new ContextMenuStrip();
                if (e.RowIndex == -1)
                {
                    cms.Items.Add("全部", null, delegate { dt.DefaultView.RowFilter = ""; });
                    string s = dgv.Columns[e.ColumnIndex].DataPropertyName;
                    cms.Items.Add("空", null, delegate { dt.DefaultView.RowFilter = $"{s} is null or trim({s})=''"; });
                    DataRowCollection drc = new DataView(dt) { RowFilter = $"{s} is not null and trim({s})<>''" }.ToTable(true, s).Rows;
                    if (drc.Count < 10)
                    {
                        foreach (DataRow dr in drc)
                        {
                            cms.Items.Add(dr[0].ToString(), null, delegate { dt.DefaultView.RowFilter = $"{s}='{dr[0]}'"; });
                        }
                    }
                }
                else
                {
                    if (dt.Columns.Contains("可执行文件路径"))
                    {
                        dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[0];
                        cms.Items.Add("打开路径", null, (o, x) => Process.Start("explorer", $"/select,\"{Convert.ToString(dgv.CurrentRow.Cells["可执行文件路径"].Value).Split(new string[] { " -" }, StringSplitOptions.None)[0]}\""));
                    }
                }
                cms.Show(MousePosition);
            }
        }
        #endregion
    }
}

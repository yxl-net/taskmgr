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
            dgv.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(dgv, true);//表格 双缓冲
            tsrServices_Click(null,null);//服务按钮点击
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
            if (tsrServices.Checked)//服务按钮选中
            {
                dt.DefaultView.RowFilter = String.Format("名称 like '%{0}%' or 描述 like '%{0}%' or 可执行文件路径 like '%{0}%'", tstFilter.Text.Trim().Replace("%", "[%]").Replace("*", "[*]").Replace("[", "[[]"));//内存视图 模糊搜索 名称、描述、可执行文件路径
            }
            else if (tsrProcess.Checked)//进程按钮选中
            {
                dt.DefaultView.RowFilter = String.Format("名称 like '%{0}%' or 主窗口标题 like '%{0}%' or 可执行文件路径 like '%{0}%'", tstFilter.Text.Trim().Replace("%", "[%]").Replace("*", "[*]").Replace("[", "[[]"));//内存视图 模糊搜索 名称、主窗体标题、可执行文件路径
            }
        }


        /// <summary>
        /// 点击刷新按钮
        /// </summary>
        /// <param name="sender">控件</param>
        /// <param name="e">事件参数</param>
        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            if (tsrServices.Checked)//服务按钮选中
            {
                tsrServices_Click(null, null);//服务按钮点击
            }
            else if (tsrProcess.Checked)//进程按钮选中
            {
                tsrProcess_Click(null, null);//进程按钮点击
            }
        }

        /// <summary>
        /// 点击服务按钮
        /// </summary>
        /// <param name="sender">控件</param>
        /// <param name="e">事件参数</param>
        private void tsrServices_Click(object sender, EventArgs e)
        {
            tsrProcess.Checked = false;//取消选中进程按钮
            tsrServices.Checked = true;//选中服务按钮
            dt = new DataTable();//创建内存表
            dt.DefaultView.ListChanged += (o, x) => tslCount.Text = dt.DefaultView.Count.ToString();////内存视图 行数增减
            dgv.DataSource = dt;//表格绑定内存表
            dt.Columns.Add("名称");//内存表添加字段
            dt.Columns.Add("描述");//内存表添加字段
            dt.Columns.Add("启动类型");//内存表添加字段
            dt.Columns.Add("状态");//内存表添加字段
            dt.Columns.Add("可执行文件路径");//内存表添加字段
            dt.Columns.Add("登录为");//内存表添加字段
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");//使用MOS查询服务
            string sStartMode = null;//启动模式
            string sStartName = null;//启动权限
            foreach (ManagementObject mo in searcher.Get())//遍历MO对象集
            {
                switch(mo["StartMode"].ToString())//启动模式
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
                switch ($"{mo["StartName"]}")//启动权限
                {
                    case "LocalSystem":
                        sStartName = "本地系统";
                        break;
                    case "NT AUTHORITY\\LocalService":
                        sStartName = "本地服务";
                        break;//
                    case "NT Authority\\NetworkService":
                        sStartName = "网络服务";
                        break;//
                }
                try//有异常
                {
                    dt.Rows.Add(mo["Name"], mo["DisplayName"], sStartMode, Convert.ToBoolean(mo["Started"])? "正运行" : "已停止", mo["PathName"], sStartName);//内存表 添加行 名称,描述,启动模式,状态,可执行文件路径,启动权限
                }
                catch//不报错
                {

                }
            }
            dgv.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//启动模式居中
            dgv.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//服务状态居中
            dgv.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//启动权限居中
        }

        /// <summary>
        /// 点击进程按钮
        /// </summary>
        /// <param name="sender">控件</param>
        /// <param name="e">事件参数</param>

        private void tsrProcess_Click(object sender, EventArgs e)
        { 
            /*ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process");//
            List<object> values;//
            foreach (ManagementObject mo in searcher.Get())
            {
                values = new List<object>();//
                foreach(var v in mo.Properties)
                {
                    values.Add(v.Value);//
                }
            }*/
            tsrServices.Checked = false;//取消选中服务按钮
            tsrProcess.Checked = true;//选中进程按钮
            dt = new DataTable();//创建内存表
            dt.DefaultView.ListChanged += (o, x) => tslCount.Text = dt.DefaultView.Count.ToString();//内存视图 行数增减
            dgv.DataSource = dt;//表格绑定内存表
            dt.Columns.Add("PID");//内存表添加字段
            dt.Columns.Add("名称");//内存表添加字段
            dt.Columns.Add("主窗口标题");//内存表添加字段
            dt.Columns.Add("启动时间",Type.GetType("System.DateTime"));//内存表添加字段
            dt.Columns.Add("可执行文件路径");//内存表添加字段
            foreach (Process p in Process.GetProcesses())//遍历进程集
            {
                try//有异常
                {
                    dt.Rows.Add(p.Id, p.ProcessName, p.MainWindowTitle,p.StartTime , p.MainModule.FileName);//内存表 添加行 PID,名称,主窗口标题,启动时间,可执行文件路径
                }
                catch//不报错
                {
                }
            }
            dgv.Columns[3].DefaultCellStyle.Format = "dd HH:mm:ss.ff";//启动时间格式 日 时:分:秒:十毫秒
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
            if (e.Button == MouseButtons.Right)//右击
            {
                ContextMenuStrip cms = new ContextMenuStrip();//创建菜单
                if (e.RowIndex == -1)//列头
                {
                    cms.Items.Add("全部", null, delegate { dt.DefaultView.RowFilter = ""; });//添加菜单项 过滤全部
                    string sField = dgv.Columns[e.ColumnIndex].DataPropertyName;//列字段名
                    cms.Items.Add("空", null, delegate { dt.DefaultView.RowFilter = $"{sField} is null or trim({sField})=''";});//添加菜单项 未录入
                    DataRowCollection drc = new DataView(dt) { RowFilter = $"{sField} is not null and trim({sField})<>''" }.ToTable(true, sField).Rows;//非空唯一内存行集
                    if (drc.Count < 10)//小于10个才显示
                    {
                        foreach (DataRow dr in drc)//遍历内存行集
                        {
                            cms.Items.Add(dr[0].ToString(), null, delegate { dt.DefaultView.RowFilter = $"{sField}='{dr[0]}'"; });//添加菜单项
                        }
                    }
                }
                else//单元格
                {
                    if (dt.Columns.Contains("可执行文件路径"))//要打开路径的列
                    {
                        dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[0];//选中右击行
                        cms.Items.Add("打开路径", null, (o, x) => Process.Start("explorer", $"/select,\"{Convert.ToString(dgv.CurrentRow.Cells["可执行文件路径"].Value).Split(new string[] { " -" }, StringSplitOptions.None)[0]}\""));//打开目录并选中可执行文件
                    }
                }
                cms.Show(MousePosition);//弹出菜单
            }
        }
        #endregion
    }
}

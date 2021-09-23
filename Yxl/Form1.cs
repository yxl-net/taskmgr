using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yxl
{
    public partial class Form1 : Form
    {
        DataTable dt=new DataTable();
        public Form1()
        {
            InitializeComponent();
            dt.Clear();
            dt.Columns.Add("名称");
            dt.Columns.Add("描述");
            dt.Columns.Add("启动类型");
            dt.Columns.Add("状态");
            dt.Columns.Add("可执行文件路径");
            dt.Columns.Add("登录为");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
            string StartMode(object o)
            {
                switch (Convert.ToString(o))
                {
                    case "Manual":
                        return "手动";
                    case "Auto":
                        return "自动";
                    case "Disabled":
                        return "禁用";
                    default:
                        return "未知";
                }
            }
            string Started(object o)
            {
                return Convert.ToBoolean(o) ? "正在运行" : "已停止";
            }
            string StartName(object o)
            {
                switch (Convert.ToString(o))
                {
                    case "LocalSystem":
                        return "本地系统";
                    case "NT AUTHORITY\\LocalService":
                        return "本地服务";
                    case "NT Authority\\NetworkService":
                        return "网络服务";
                    default:
                        return "未知";
                }
            }
            foreach (ManagementObject mo in searcher.Get())
            {
                dt.Rows.Add( mo["Name"],mo["DisplayName"], StartMode(mo["StartMode"].ToString()), Started(mo["Started"]),mo["PathName"], StartName(mo["StartName"]));
            }
            //打开应用程序文件夹
            dgv.DataSource = dt;
            dgv.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

    }
}

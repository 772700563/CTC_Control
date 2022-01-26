using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using CTCDB;
using System.Windows;
using Oracle.ManagedDataAccess.Client;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using CTC_Control.Classes.Helper;
using CTC_Control.Classes;
using System.Threading;
using System.Windows.Threading;

namespace CTC_Control.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            IniteProperties();
            IniteCommands();
        }


        #region 属性
        //程序是否繁忙
        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; RaisePropertyChanged(); }
        }

        //仿真速率
        private List<ComboBoxItem> simSpeedItems;
        public List<ComboBoxItem> SimSpeedItems
        {
            get { return simSpeedItems; }
            set { simSpeedItems = value; RaisePropertyChanged(); }
        }

        //仿真开始时间
        private DateTime simTime;
        public DateTime SimTime
        {
            get { return simTime; }
            set { simTime = value; RaisePropertyChanged(); }
        }

        private int simSpeedIdx;
        public int SimSpeedIdx
        {
            get { return simSpeedIdx; }
            set { simSpeedIdx = value; RaisePropertyChanged(); }
        }

        private string ipAddress;
        public string IpAddress
        {
            get { return ipAddress; }
            set { loginfo.IpAddress = ipAddress = value; RaisePropertyChanged(); }
        }

        private string port;
        public string Port
        {
            get { return port; }
            set { loginfo.Port = port = value; RaisePropertyChanged(); }
        }

        private string user;
        public string User
        {
            get { return user; }
            set { loginfo.User = user = value; RaisePropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { loginfo.Password = password = value; RaisePropertyChanged(); }
        }

        private OracleConnection db;
        //数据库登录信息
        private DBConSetInfo loginfo;
        //数据库登录信息缓存文件路径
        private string login_path;

        //数据库是否连接
        private bool dbConnected;
        public bool DbConnected
        {
            get { return dbConnected; }
            set { dbConnected = value; RaisePropertyChanged(); }
        }

        private int simCMD;
        /// <summary>
        /// 当前仿真命令: 0-停止，1-开始，2-暂停，3-继续
        /// </summary>
        public int SimCMD
        {
            get { return simCMD; }
            set
            {
                simCMD = value;
                switch (simCMD)
                {
                    case 0:
                        SimStartEnable = true;
                        SimPauseEnable = SimContiEnable = SimEndEnable = false;
                        SimEnd = true;
                        SimOn = false;
                        //更新时间
                        last_update_time = DateTime.Now;
                        break;
                    case 1:
                        SimPauseEnable = SimEndEnable = true;
                        SimStartEnable = SimContiEnable = false;
                        SimOn = true;
                        SimEnd = false;
                        break;
                    case 2:
                        SimContiEnable = SimEndEnable = true;
                        SimStartEnable = SimPauseEnable = false;
                        SimOn = false;
                        SimEnd = false;
                        break;
                    case 3:
                        SimPauseEnable = SimEndEnable = true;
                        SimStartEnable = SimContiEnable = false;
                        SimOn = true;
                        SimEnd = false;
                        break;
                }
            }
        }

        //指示对应的按钮是否可用
        private bool simStartEnable;
        public bool SimStartEnable
        {
            get { return simStartEnable; }
            set { simStartEnable = value;RaisePropertyChanged(); }
        }

        private bool simPauseEnable;
        public bool SimPauseEnable
        {
            get { return simPauseEnable; }
            set { simPauseEnable = value; RaisePropertyChanged(); }
        }

        private bool simContiEnable;
        public bool SimContiEnable
        {
            get { return simContiEnable; }
            set { simContiEnable = value; RaisePropertyChanged(); }
        }

        private bool simEndEnable;
        public bool SimEndEnable
        {
            get { return simEndEnable; }
            set { simEndEnable = value; RaisePropertyChanged(); }
        }

        //指示是否正在进行仿真(仿真开始且未暂停)
        private bool simOn;
        public bool SimOn
        {
            get { return simOn; }
            set { simOn = value; RaisePropertyChanged(); }
        }

        //指示是否已结束仿真(仿真结束按钮)
        private bool simEnd;
        public bool SimEnd
        {
            get { return simEnd; }
            set { simEnd = value; RaisePropertyChanged(); }
        }

        private string simText;
        public string SimText
        {
            get { return simText; }
            set
            {
                simText = value;
                RaisePropertyChanged();
            }
        }

        //局部时间
        private DateTime localTime;
        public DateTime LocalTime
        {
            get { return localTime; }
            set { localTime = value; LocalTimeText = localTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        private string localTimeText;
        public string LocalTimeText
        {
            get { return localTimeText; }
            set
            {
                localTimeText = value;
                RaisePropertyChanged();
            }
        }

        //仿真开始持续时间
        private int simInterv;

        private DispatcherTimer local_time_timer;
        //500 ms
        private int tick_precis = 500;

        //线程列表
        List<Timer> timers;

        //多车仿真器信息
        private string infoText;
        public string InfoText
        {
            get { return infoText; }
            set { infoText = value; RaisePropertyChanged(); }
        }

        //上一次更新InfoText的时间（服务器时间）
        private DateTime last_update_time;

        #endregion

        #region 命令
        public RelayCommand MainCloseCMD { get; set; }
        public RelayCommand SimStartCMD { get; set; }
        public RelayCommand SimPauseCMD { get; set; }
        public RelayCommand SimContiCMD { get; set; }
        public RelayCommand SimEndCMD { get; set; }
        public RelayCommand LogInCMD { get; set; }
        public RelayCommand LogOutCMD { get; set; }
        #endregion

        #region 方法
        private void IniteProperties()
        {
            IsBusy = false;
            SimSpeedItems = new List<ComboBoxItem>
            {
                new ComboBoxItem(){Content="1"},
                new ComboBoxItem(){Content="2"},
                new ComboBoxItem(){Content="5"},
                new ComboBoxItem(){Content="10"},
                new ComboBoxItem(){Content="20"},
            };
            DbConnected = false;
            SimTime = DateTime.Now;
            SimSpeedIdx = 0;
            loginfo = new DBConSetInfo();
            login_path = RootPathHelper.GetRootPath();
            login_path += "\\Cache\\login.dbif";
            //如果存在数据库登录缓存文件则反序列化读取信息
            if (File.Exists(@login_path))
            {
                try
                {
                    loginfo = SerializeHelper.DeSerializeNow<DBConSetInfo>(login_path);
                    IpAddress = loginfo.IpAddress;
                    Port = loginfo.Port;
                    user = loginfo.User;
                    Password = loginfo.Password;
                }
                catch { }
            }
            SimCMD = 0;
            //启动线程更新界面时间
            simInterv = 0;
            local_time_timer = new DispatcherTimer();
            local_time_timer.Tick += new EventHandler(UpdateTime);
            local_time_timer.Interval = new TimeSpan(0, 0, 0, 0, tick_precis);
            local_time_timer.Start();

            timers = new List<Timer>();
            //timers.Add(new Timer(new TimerCallback(UpdateTime), null, 0, 500));
            //启动线程读取多车仿真器传输的信息
            timers.Add(new Timer(new TimerCallback(UpdateInfoText), null, 0, 1000));
        }

        private void IniteCommands()
        {
            MainCloseCMD = new RelayCommand(MainCloseFunc);
            SimStartCMD = new RelayCommand(SimStartFunc);
            SimPauseCMD = new RelayCommand(SimPauseFunc);
            SimContiCMD = new RelayCommand(SimContiFunc);
            SimEndCMD = new RelayCommand(SimEndFunc);
            LogInCMD = new RelayCommand(LogInFunc);
            LogOutCMD = new RelayCommand(LogOutFunc);
        }

        private void MainCloseFunc()
        {
            //同步主控状态至数据库
            SimCMD = 0;
            if (DbConnected)
            {
                WriteInfoToDB();
            }          
        }

        //仿真开始
        private void SimStartFunc()
        {
            SimCMD = 1;
            SimText = "仿真运行中";
            //设置时间
            simInterv = 0;
            LocalTime = SimTime;
            WriteInfoToDB();
        }

        //仿真暂停
        private void SimPauseFunc()
        {
            SimCMD = 2;
            SimText = "仿真暂停";
            WriteInfoToDB();
        }

        //仿真继续
        private void SimContiFunc()
        {
            SimCMD = 3;
            SimText = "仿真运行中";
            WriteInfoToDB();
        }

        //仿真结束
        private void SimEndFunc()
        {
            SimCMD = 0;
            SimText = "仿真未运行";
            WriteInfoToDB();
        }

        //登录按钮功能
        private void LogInFunc()
        {
            db = DBHELPER.OpenConn(IpAddress, Port, "ORCL", User, Password);
            try
            {
                if (!(db.State == System.Data.ConnectionState.Open))
                {
                    MessageBox.Show("数据库连接失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("数据库连接成功", "通知", MessageBoxButton.OK, MessageBoxImage.Information);
                    DbConnected = true;
                    //序列化当前的连接设置
                    {
                        SerializeHelper.SerializeNow(loginfo, login_path);
                    }
                }
            }
            catch
            {
                MessageBox.Show("数据库连接失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //注销按钮功能
        private void LogOutFunc()
        {
            if (MessageBox.Show("是否关闭数据库连接？", "通知", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                //关闭前需要将总控仿真设置为终止模式并向数据库写入该状态信息
                {
                    SimCMD = 0;
                    WriteInfoToDB();
                }
                DBHELPER.CloseConn(db);
                DbConnected = false;
                MessageBox.Show("关闭成功", "通知", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //将当前总控信息写入数据库
        private void WriteInfoToDB()
        {
            string sqlstr = string.Format("insert into {0}.\"{1}\"(\"Time\",\"SimTime\",\"CTCTime\",\"SimCMD\",\"SimSpeed\") " +
                "values(", User, "FromMainControl");
            sqlstr += CommonHelper.DateToOracleDate(DateTime.Now) + ",";
            sqlstr += CommonHelper.DateToOracleDate(SimTime) + ",";
            sqlstr += CommonHelper.DateToOracleDate(LocalTime) + ",";
            sqlstr += SimCMD + ",";
            sqlstr += SimSpeedItems[SimSpeedIdx].Content.ToString() + ")";
            DBHELPER.ExcuteDataTable(db, sqlstr);
        }

        //更新局部时间
        private void UpdateTime(object obj, EventArgs e)
        {
            //仿真正在进行
            if (SimOn)
            {
                simInterv += tick_precis;
                LocalTime = SimTime.AddMilliseconds(simInterv);
            }
        }

        //更新多车仿真器传输来的文本信息
        private void UpdateInfoText(object obj)
        {
            if(DbConnected)
            {
                {
                    string sqlstr = string.Format("insert into {0}.\"{1}\"(\"Time\",\"CTCTime\",\"TextInfo\") values" +
                    "({2},{3},'{4}')", User, "ToMainControl",
                    CommonHelper.DateToOracleDate(DateTime.Now), CommonHelper.DateToOracleDate(LocalTime),
                    "test");
                    DBHELPER.ExcuteDataTable(db, sqlstr);
                }

                //只有仿真进行过程中才会从数据库读取信息
                if (SimOn)
                {
                    //返回ToMainControl表中时间在last_update_time之后的所有信息
                    string sqlstr = string.Format("select * from {0}.\"{1}\" where \"Time\" > {2}",
                        User, "ToMainControl", CommonHelper.DateToOracleDate(last_update_time));
                    var info_table = DBHELPER.ExcuteDataTable(db, sqlstr);
                    var rows = info_table.Rows;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        InfoText += string.Format("{0}  {1}\n", rows[i]["Time"], rows[i]["TextInfo"]);
                    }
                    last_update_time = DateTime.Now;
                }
            }         
        }
        #endregion
    }
}

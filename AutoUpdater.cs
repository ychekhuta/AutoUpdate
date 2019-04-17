using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using IWshRuntimeLibrary;
using System.Data.SqlClient;

namespace AutoUpdate
{
    public partial class AutoUpdater  : Form
    {
        public const string CON_STR = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MyDB;User ID=AutoUpdate_User;Password=Au123login";

        private const int NUMBER_OF_CHECKBOXES = 48;

        private Process[] processes;
        private string[] procName;
        private string[] appName;
        private string appPath = null;
        private string sourcePath = null;
        BackgroundWorker updater;
        private string[] result;
        private CheckBox[] allCheckBoxes;
        private CheckBox[] checkBox;
        private CheckBox[] securedCheckBoxes;

        UserInfo user;
        
        int exitCount = 600;
        bool isUpdateRunning;

        public AutoUpdater()
        {            
            InitializeComponent();

            statusUpdatePanel.Visible = false;

            isUpdateRunning = false;
            exitLbl.Text = "";

            InitConfigVar_DB(0);

            UpdateUser("");

            exitTimer.Start();
        }

        #region Init

        private void InitConfigVar_DB(int areaId)
        {
            allCheckBoxes = new CheckBox[20] {  checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7, checkBox8, checkBox9, checkBox10,
                                                checkBox11, checkBox12, checkBox13, checkBox14, checkBox15, checkBox16, checkBox17, checkBox18, checkBox19, checkBox20 };

            for (int i = 0; i < allCheckBoxes.Length; i++)
                allCheckBoxes[i].Visible = false;

            byte countProcesses = 0;
            byte countSrcPasses = 0;

            try
            {
                countProcesses = areaId > 0 ? GetCountProcesses(areaId) : (byte)0;
                countSrcPasses = areaId > 0 ? GetCountSrcPasses(areaId) : (byte)0;

                if (countProcesses > allCheckBoxes.Length)
                {
                    MessageBox.Show("The number of processes has exceeded the number of controls. Please, notify the software engineers. \rThe program will now close.");
                    Close();
                }
                else
                {
                    procName = new string[countProcesses];
                    appName = new string[countProcesses];
                    result = new string[countProcesses];
                    checkBox = new CheckBox[countProcesses];
                    List<int> secureItems = new List<int>();

                    countProcesses = 0;

                    DataTable dtPrograms = GetPrograms(areaId);

                    foreach (DataRow dr in dtPrograms.Rows)
                    {
                        appName[countProcesses] = dr.ItemArray[0].ToString();
                        procName[countProcesses] = dr.ItemArray[1].ToString();

                        if ((bool)dr.ItemArray[2])
                            secureItems.Add(countProcesses);

                        result[countProcesses] = "";
                        checkBox[countProcesses] = allCheckBoxes[countProcesses];

                        countProcesses++;
                    }

                    securedCheckBoxes = new CheckBox[secureItems.Count];
                    for (int i = 0; i < securedCheckBoxes.Length; i++)
                    {
                        securedCheckBoxes[i] = checkBox[secureItems[i]];
                    }

                    Dictionary<string, string> srcPaths = new Dictionary<string, string>();
                    DataTable dtPaths = GetPaths(areaId);
                    foreach (DataRow dr in dtPaths.Rows)
                    {
                        appPath = dr.ItemArray[0].ToString();
                        srcPaths.Add(dr.ItemArray[1].ToString(), dr.ItemArray[2].ToString());
                    }

                    if (srcPaths.Count > 0)
                        sourcePath = srcPaths.Keys.FirstOrDefault().ToString();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }

            for (int i = 0; i < checkBox.Length; i++)
            {
                checkBox[i].Visible = true;
                checkBox[i].Text = procName[i];
            }

            SecureApps();
        }

        public byte GetCountProcesses(int areaId)
        {
            byte countProcesses = 0;

            using (SqlConnection con = new SqlConnection(CON_STR))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.AutoUpdate_Programs WHERE AreaId = " + areaId, con))
                {
                    if (cmd.ExecuteScalar() == null)
                        countProcesses = 0;
                    else
                        countProcesses = Convert.ToByte(cmd.ExecuteScalar());
                }

                con.Close();
            }

            return countProcesses;
        }

        public byte GetCountSrcPasses(int areaId)
        {
            byte countSrcPasses = 0;

            using (SqlConnection con = new SqlConnection(CON_STR))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.AutoUpdate_Paths WHERE AreaId = " + areaId, con))
                {
                    if (cmd.ExecuteScalar() == null)
                        countSrcPasses = 0;
                    else
                        countSrcPasses = Convert.ToByte(cmd.ExecuteScalar());
                }

                con.Close();
            }

            return countSrcPasses;
        }

        public DataTable GetPrograms(int areaId)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AppName", typeof(string));
            dt.Columns.Add("ProcessName", typeof(string));
            dt.Columns.Add("IsSecure", typeof(bool));

            using (SqlConnection con = new SqlConnection(CON_STR))
            {
                con.Open();

                using (SqlDataReader rdr = new SqlCommand("SELECT AppName, ProcessName, IsSecure FROM dbo.AutoUpdate_Programs WHERE AreaId = " + areaId, con).ExecuteReader())
                    while (rdr.Read())
                    {
                        dt.Rows.Add(rdr.GetString(0), rdr.GetString(1), rdr.GetBoolean(2));
                    }

                con.Close();
            }

            return dt;
        }

        public DataTable GetPaths(int areaId)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("DirPath", typeof(string));
            dt.Columns.Add("SrcPath", typeof(string));
            dt.Columns.Add("Comments", typeof(string));

            using (SqlConnection con = new SqlConnection(CON_STR))
            {
                con.Open();

                using (SqlDataReader rdr = new SqlCommand("SELECT DirPath, SrcPath, Comments FROM dbo.AutoUpdate_Paths WHERE AreaId = " + areaId, con).ExecuteReader())
                    while (rdr.Read())
                    {
                        dt.Rows.Add(rdr.GetString(0), rdr.GetString(1), rdr.IsDBNull(2) ? "" : rdr.GetString(2));
                    }

                con.Close();
            }

            return dt;
        }

        #endregion

        #region User

        private void loginTB_MouseClick(object sender, MouseEventArgs e)
        {
            (sender as TextBox).Focus();
            (sender as TextBox).SelectAll();
        }

        private void loginTB_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            TextBox tb = sender as TextBox;
            UpdateUser(tb.Text);
        }

        private void UpdateUser(string scanCode)
        {
            user = new UserInfo(scanCode);

            if (user.id != 0)
            {
                UnsecureApps();
                loginLbl.Visible = true;
                loginLbl.Text = user.name;
                logoutBtn.Visible = true;
                loginTB.Visible = false;
                loginLbl.Focus();
            }
            else
            {
                SecureApps();
                loginLbl.Visible = false;
                logoutBtn.Visible = false;
                loginTB.Visible = true;
                loginTB.Text = "";
            }
        }

        private void logoutBtn_Click(object sender, EventArgs e)
        {
            UpdateUser("");
            loginTB.Focus();
        }

        #endregion

        #region Methods
                
        private void SecureApps()
        {
            for (int i = 0; i < checkBox.Length; i++)
            {
                checkBox[i].Enabled = true;
            }

            for (int i = 0; i < securedCheckBoxes.Length; i++)
            {
                if (user == null || user.id == 0)
                {
                    securedCheckBoxes[i].Enabled = false;
                    securedCheckBoxes[i].Checked = false;
                }
            }
        }
        
        private void UnsecureApps()
        {
            for (int i = 0; i < securedCheckBoxes.Length; i++)
            {
                securedCheckBoxes[i].Enabled = true;
            }
        }               

        private int GetAppVersion(string path)
        {
            try
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);
                //AssemblyName currentAssemblyName = AssemblyName.GetAssemblyName(path);
                string version = fvi.FileVersion.ToString().Replace(".", string.Empty);
                if (version.Length == 1) version += "000";
                else if (version.Length == 2) version += "00";
                else if (version.Length == 3) version += "0";
                return Convert.ToInt32(version);
                //return Convert.ToInt32(currentAssemblyName.Version.ToString().Replace(".", string.Empty));                
            }
            catch
            {
                return 0;
            }
        }

        private void appShortcutToDesktop(string shortcutName, string shortcutPath, string targetFileLocation, string workingDir)
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            string shortcutName_Upd = "";
            string argument_Upd = "";
            
            shortcutName_Upd = shortcutName;
            
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName_Upd + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.WorkingDirectory = workingDir;
            shortcut.TargetPath = targetFileLocation;

            if (argument_Upd.Length > 0)
                shortcut.Arguments = argument_Upd;

            shortcut.Save();            
        }

        private void StartUpdateProcess()
        {
            for (int i = 0; i < procName.Length; i++)
            {
                processes = Process.GetProcessesByName(appName[i]);

                if (processes != null)
                {
                    if (!checkBox[i].Checked)
                    {
                        result[i] = procName[i] + ": N/A";
                        continue;
                    }
                    else
                    {
                        appUpdatingLbl.Invoke((MethodInvoker)delegate {
                            appUpdatingLbl.Text = procName[i];
                        });

                        bool createdNew = false;
                        if (!Directory.Exists(appPath + "\\" + procName[i]))
                        {
                            Directory.CreateDirectory(appPath + "\\" + procName[i]);
                            createdNew = true;
                        }

                        if (!System.IO.File.Exists(sourcePath + "\\" + procName[i] + "\\" + appName[i] + ".exe"))
                        {
                            result[i] = procName[i] + ": No source program found.";
                            continue;
                        }

                        int appPathV = GetAppVersion(appPath + "\\" + procName[i] + "\\" + appName[i] + ".exe");
                        int srcParhV = GetAppVersion(sourcePath + "\\" + procName[i] + "\\" + appName[i] + ".exe");

                        if (appPathV < srcParhV || (appPathV == 0 && srcParhV == 0))
                        {
                            try
                            {
                                foreach (Process proc in processes)
                                {
                                    proc.CloseMainWindow();
                                    proc.WaitForExit();
                                }

                                if (DeleteRecursiveFolder(appPath + "\\" + procName[i], createdNew))
                                {
                                    UpdateAppFolder(sourcePath + "\\" + procName[i], appPath + "\\" + procName[i], createdNew);
                                    
                                    if (createdNew)
                                        appShortcutToDesktop(procName[i], Environment.GetFolderPath(Environment.SpecialFolder.Desktop), appPath + "\\" + procName[i] + "\\" + appName[i] + ".exe", appPath + "\\" + procName[i]);
                                }

                                Thread.Sleep(1000);

                                result[i] = procName[i] + ": updated successfully.";
                            }
                            catch (System.NullReferenceException)
                            {
                                result[i] = procName[i] + ": no matches found.";
                            }
                        }
                        else
                        {
                            result[i] = procName[i] + ": no updates available.";
                        }
                    }
                }
                else
                    result[i] = procName[i] + ": No process found.";
            }

            string resultStr = "";
            for (int i = 0; i < result.Length; i++)
            {
                if (resultStr == "")
                    resultStr = result[i];
                else
                    resultStr = resultStr + "\n" + result[i];
            }

            isUpdateRunning = false;
            MessageBox.Show(resultStr);           

            Invoke(new MethodInvoker(delegate { this.Dispose(); }));
        }

        private void StartUpdateProcess(string applicationName, string processName)
        {            
            processes = Process.GetProcessesByName(applicationName);
            string resultThis = "";

            if (processes != null)
            {        
                bool createdNew = false;
                if (!Directory.Exists(appPath + "\\" + processName))
                {
                    Directory.CreateDirectory(appPath + "\\" + processName);
                    createdNew = true;
                }

                if (!System.IO.File.Exists(sourcePath + "\\" + processName + "\\" + applicationName + ".exe"))
                {
                    resultThis = processName + ": No source program found.";
                }
                else
                {                    
                    int appPathV = GetAppVersion(appPath + "\\" + processName + "\\" + applicationName + ".exe");
                    int srcParhV = GetAppVersion(sourcePath + "\\" + processName + "\\" + applicationName + ".exe");

                    if (appPathV < srcParhV || (appPathV == 0 && srcParhV == 0))
                    {
                        try
                        {
                            foreach (Process proc in processes)
                            {
                                proc.CloseMainWindow();
                                proc.WaitForExit();
                            }                            

                            if (DeleteRecursiveFolder(appPath + "\\" + processName, createdNew))
                            {
                                UpdateAppFolder(sourcePath + "\\" + processName, appPath + "\\" + processName, createdNew);

                                if (createdNew)
                                    appShortcutToDesktop(processName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), appPath + "\\" + processName + "\\" + applicationName + ".exe", appPath + "\\" + processName);
                            }

                            Thread.Sleep(1000);

                            resultThis = processName + ": updated successfully.";                            
                        }
                        catch (System.NullReferenceException)
                        {
                            resultThis = processName + ": no matches found.";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        resultThis = processName + ": no updates available.";
                    }
                }
            }
            else
                resultThis = processName + ": No process found.";

            string resultStr = "";
            for (int i = 0; i < result.Length; i++)
            {
                if (resultStr == "")
                    resultStr = result[i];
                else
                    resultStr = resultStr + "\n" + result[i];
            }

            isUpdateRunning = false;

            MessageBox.Show(resultThis);

            Process newProcess = new Process();
            newProcess.StartInfo.FileName = @"" + appPath + "\\" + processName + "\\" + applicationName + ".exe";
            newProcess.Start();

            Invoke(new MethodInvoker(delegate { this.Dispose(); }));
        }

        #endregion

        #region Events
        
        private void btnStop_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            statusUpdatePanel.Visible = true;
            appUpdatingLbl.Text = "";
            filesUpdatedLbl.Text = "";

            for (int i = 0; i < checkBox.Length; i++)
                checkBox[i].Enabled = false;
            btnStart.Enabled = false;

            updater = new BackgroundWorker();
            updater.WorkerReportsProgress = true;
            updater.DoWork += new DoWorkEventHandler(UpdaterDoWork);
            pbUpdate.Style = ProgressBarStyle.Marquee;
            pbUpdate.MarqueeAnimationSpeed = 50;
            updater.RunWorkerAsync();
        }

        private void departmentRB_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (!rb.Checked) return;

            int tag = Convert.ToInt32(rb.Tag);

            InitConfigVar_DB(tag);
        }
        
        private bool DeleteRecursiveFolder(string pFolderPath, bool createdNew)
        {
            if (!SaveLatestVersion(pFolderPath)) return false;
            else
            {
                try
                {
                    foreach (string file in Directory.GetFiles(pFolderPath))
                    {
                        if (createdNew)
                        {
                            var pPath = Path.Combine(pFolderPath, file);
                            System.IO.File.SetAttributes(pPath, FileAttributes.Normal);
                            System.IO.File.Delete(file);
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool SaveLatestVersion(string pFolderPath)
        {
            try
            {
                string newFolder = @"" + pFolderPath.ToString() + "\\Old\\" + DateTime.Now.ToString("MM-dd-yyyy HH.mm tt");
                
                string[] fileArray = Directory.GetFiles(pFolderPath);
                string fileName = null;

                if (fileArray.Length > 0)
                    if (!Directory.Exists(newFolder))
                        Directory.CreateDirectory(newFolder);

                for (int i = 0; i < fileArray.Length; i++)
                {
                    fileName = Path.GetFileName(fileArray[i]);
                    System.IO.File.Copy(fileArray[i], newFolder + "\\" + fileName, true);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        private void UpdateAppFolder(string pTargetPath, string sDirPath, bool createdNew)
        {
            int totalCount = 0;

            string[] fileArray = Directory.GetFiles(pTargetPath);
            totalCount = fileArray.Length;

            string[] foldersArray = Directory.GetDirectories(pTargetPath);
            foreach (string folder in foldersArray)
            {                
                DirectoryInfo di = new DirectoryInfo(folder);
                string folderName = di.Name;

                string[] folderFileArray = Directory.GetFiles(pTargetPath + "\\" + folderName);
                totalCount += folderFileArray.Length;
            }

            string fileName = null;

            int currCount = 0;

            filesUpdatedLbl.Invoke((MethodInvoker)delegate {
                filesUpdatedLbl.Text = "0 / " + totalCount;
            });

            for (int i = 0; i < fileArray.Length; i++)
            {
                fileName = Path.GetFileName(fileArray[i]);
                System.IO.File.Copy(fileArray[i], sDirPath + "\\" + fileName, true);

                currCount++;

                filesUpdatedLbl.Invoke((MethodInvoker)delegate {
                    filesUpdatedLbl.Text = currCount + " / " + totalCount;
                });
            }

            foreach(string folder in foldersArray)
            {
                string folderName;
                DirectoryInfo di = new DirectoryInfo(folder);
                folderName = di.Name;
                Directory.CreateDirectory(sDirPath + "\\" + folderName);
                string[] folderFileArray = Directory.GetFiles(pTargetPath + "\\" + folderName);
                string folderFileName = null;

                for (int i = 0; i < folderFileArray.Length; i++)
                {
                    folderFileName = Path.GetFileName(folderFileArray[i]);
                    System.IO.File.Copy(folderFileArray[i], sDirPath + "\\" + folderName + "\\" + folderFileName, true);

                    currCount++;

                    filesUpdatedLbl.Invoke((MethodInvoker)delegate {
                        filesUpdatedLbl.Text = currCount + " / " + totalCount;
                    });
                }
            }
        }

        private void exitTimer_Tick(object sender, EventArgs e)
        {
            if (isUpdateRunning) return;

            exitCount--;

            int exitMins = exitCount / 60;
            int exitSeconds = exitCount % 60;
            exitLbl.Text = "This application will exit in " + exitMins + " minutes and " + exitSeconds + " seconds";

            if (exitCount < 60)
                exitLbl.ForeColor = Color.Red;
            else
                exitLbl.ForeColor = Color.Black;

            if (exitCount <= 0)
            {
                this.Close();
            }
        }

        #endregion

        #region Threading

        void UpdaterDoWork(object sender, DoWorkEventArgs e)
        {
            isUpdateRunning = true;
            this.StartUpdateProcess();
        }

        void UpdaterRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isUpdateRunning = false;
            pbUpdate.MarqueeAnimationSpeed = 0;
            pbUpdate.Style = ProgressBarStyle.Blocks; 
            pbUpdate.Value = pbUpdate.Minimum;
            this.updater.Dispose();
        }

        #endregion

    }
}

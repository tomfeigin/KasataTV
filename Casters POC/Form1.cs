using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Svt.Caspar;
using Svt.Network;
using System.Xml;
using System.Xml.XPath;
using CastersPOC.Controls;
using System.IO;
using WMPLib;
using AxWMPLib;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System.Threading;
using System.Net;
using System.Web.Script.Serialization;

namespace CastersPOC
{
    public partial class Form1 : Form
    {

        // deletages
        private delegate void UpdateGUI(object parameter);

        //För att kommunicera med Caspar.
        CasparDevice caspar_ = new CasparDevice();
        CasparCGDataCollection cgData = new CasparCGDataCollection();

        // Spara-dialogruta
        private SaveDialog saveDialog = null;

        // data set for teams
        DataSet teamsDs = new DataSet();
        DataTable teamsDt = new DataTable("TEAMS");
        DataView team1Dv = new DataView();
        DataView team2Dv = new DataView();

        public Form1()
        {
            InitializeComponent();

            disableControls();

            // skapa ny spara-ruta
            this.saveDialog = new SaveDialog();

            //Fixa till datarelationer för lagen.
            team1Dv.Table = teamsDt;
            team2Dv.Table = teamsDt;

            // initiera och fyll med data
            FillDatatable();

            this.clearAllData();
            this.initFieldsData();

            //Hanterare för events från Caspar.
            caspar_.Connected += new EventHandler<NetworkEventArgs>(caspar__Connected);
            caspar_.FailedConnect += new EventHandler<NetworkEventArgs>(caspar__FailedConnected);
            caspar_.Disconnected += new EventHandler<NetworkEventArgs>(caspar__Disconnected);
            //caspar_.UpdatedTemplates += new EventHandler<EventArgs>(caspar__UpdatedTemplates);
            //caspar_.UpdatedMediafiles += new EventHandler<EventArgs>(caspar__UpdatedMediafiles);
            caspar_.DataRetrieved += new EventHandler<DataEventArgs>(caspar__DataRetrieved);
            caspar_.UpdatedDatafiles += new EventHandler<EventArgs>(caspar__UpdatedDataFiles);
            caspar_.UpdatedMediafiles += new EventHandler<EventArgs>(caspar__UpdatedMediaFiles);
            updateConnectButtonText();

            // Show the CheckBox and display the control as an up-down control.
            //dtCountDownTime.CustomFormat = "yyyy-MM-dd HH:mm";
        }

        private object GetLogoDataSource()
        {
            object[] rowArray = new object[Properties.Settings.Default.Logos.Count];
            for (int i = 0; i < Properties.Settings.Default.Logos.Count; i++)
            {
                rowArray[i] = Properties.Settings.Default.Logos[i];
            }
            return rowArray;
        }

        private void initFieldsData()
        {
            this.tbCasparServer.Text = Properties.Settings.Default.Hostname;
        }

        #region key handling
        private void Form1_Load(object sender, System.EventArgs e)
        {
            // Set these when the form loads:
            // Have the form capture keyboard events first.
            this.KeyPreview = true;
            // Assign the event handler to the form.
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            //tabControl1.DrawItem += this.ChangeTabColor;

            ax1.uiMode = "none";
            ax2.uiMode = "none";
            ax3.uiMode = "none";

            string[] something = { "http://embed.tlk.io/api/chats/643046/messages/" };

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (caspar_.IsConnected)
            {
                if (e.KeyCode == Keys.F3)
                {
                    handleF3();

                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": Showing chat messages");
                    //System.Diagnostics.Debug.WriteLine(e.KeyCode + ": gameTimeStartStop");
                }
                else if (e.KeyCode == Keys.F4)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": showHideClock");
                }
                else if (e.KeyCode == Keys.F5)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": bottomFlarpShowHide");
                }
                else if (e.KeyCode == Keys.F6)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": showOvertime");
                }
                else if (e.KeyCode == Keys.F7)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": showTickerText");
                }
            }
        }

        private void handleF3()
        {
            if (chatTimer.Enabled)
            {
                chatTimer.Stop();
                StopTickerExternal();
            }
            else
            {
                chatTimer.Start();
            }
        }
        #endregion

        #region Fill and init data tables
        private void FillDatatable()
        {

        }
        #endregion


        #region caspar connection
        //button handlers
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            buttonConnect.Enabled = false;

            if (!caspar_.IsConnected)
            {
                caspar_.Settings.Hostname = this.tbCasparServer.Text; // Properties.Settings.Default.Hostname;
                caspar_.Settings.Port = 5250;
                caspar_.Connect();
            }
            else
            {
                caspar_.Disconnect();
            }
        }



        //caspar event - data retrieved
        void caspar__UpdatedDataFiles(object sender, EventArgs e)
        {
        }

        private bool IsTableDataFile(string dataFile)
        {
            return true;
        }

        void caspar__UpdatedMediaFiles(object sender, EventArgs e)
        {

            List<CasparItem> lstCasparItems = new List<CasparItem>();
            //var ffMpeg = new NReco.VideoConverter.FFMpegConverter();

            String mediaLocation = "D:\\Users\\Tom\\CasparCG\\CasparCG Server\\Server\\media";
            setMediaPlayer(ax1, mediaLocation, "AMB.mp4");
            setMediaPlayer(ax2, mediaLocation, "example2.mp4");
            setMediaPlayer(ax3, mediaLocation, "go1080p25.mp4");

            this.Invoke((MethodInvoker)delegate
            {
                ax1_Enter(ax1, null);
            });
        }

        private void setMediaPlayer(AxWindowsMediaPlayer ax, String mediaLocation, String fileName)
        {
            this.Invoke((MethodInvoker)delegate
            {
                ax.URL = mediaLocation + "\\" + fileName;
                ax.settings.mute = true;
                ax.settings.setMode("loop", true);
                ax.Ctlcontrols.play();
            });
        }

        private void showVolumeBar(object sender, EventArgs e)
        {
            Panel panel = (Panel)sender;
            changeVolumeBar(panel, true);
        }

        private void hideVolumeBar(object sender, EventArgs e)
        {
            Panel panel = (Panel)sender;
            changeVolumeBar(panel, false);
        }

        static int s_sameMessageCounter = 0;
        private void showChatMessages()
        {
            try
            {

                if (this.caspar_.IsConnected)
                {
                    // Clear old data
                    cgData = this.getChatComment();
                    if (cgData == null)
                    {
                        s_sameMessageCounter++;
                        // Check if to clear the chat.
                        if (s_sameMessageCounter > 3)
                        {
                            StopTickerExternal();
                        }

                        return;
                    }

                    s_sameMessageCounter = 0;
                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                    this.caspar_.Channels[0].CG.Add(Properties.Settings.Default.GraphicsLayerExternalTicker, Properties.Settings.Default.TemplateNameFacebookBottom, true, cgData);
                }
                else
                {
                    MessageBox.Show("There is no Caspar connected.", "Caspar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static string[] s_chatMessage = {};
        private CasparCGDataCollection getChatComment()
        {
            string[] message = this.getLastChatMessage();
            if (message == null)
            {
                return null;
            }

            // Check if it's a new message.
            if (message.SequenceEqual(s_chatMessage))
            {
                return null;
            }

            s_chatMessage = (string[])message.Clone();

            CasparCGDataCollection cgCollection = new CasparCGDataCollection();
            cgCollection.SetData("f0", message[0]);
            cgCollection.SetData("f1", message[1]);

            return cgCollection;
        }

        private void changeVolumeBar(Panel panel, bool visible)
        {
            String barName = panel.Tag.ToString();

            // Change the currect player.
            switch (barName)
            {
                case "bar1":
                    bar1.Visible = visible;
                    break;
                case "bar2":
                    bar2.Visible = visible;
                    break;
                case "bar3":
                    bar3.Visible = visible;
                    break;
                default:
                    break;
            }
        }

        //private void ChangeTabColor(object sender, DrawItemEventArgs e)
        //{
        //    Font TabFont;
        //    Brush BackBrush = new SolidBrush(Color.Green); //Set background color
        //    Brush ForeBrush = new SolidBrush(Color.Yellow);//Set foreground color
        //    if (e.Index == this.tabControl1.SelectedIndex)
        //    {
        //        TabFont = new Font(e.Font, FontStyle.Italic | FontStyle.Bold);
        //    }
        //    else
        //    {
        //        TabFont = e.Font;
        //    }
        //    string TabName = this.tabControl1.TabPages[e.Index].Text;
        //    StringFormat sf = new StringFormat();
        //    sf.Alignment = StringAlignment.Center;
        //    e.Graphics.FillRectangle(BackBrush, e.Bounds);
        //    Rectangle r = e.Bounds;
        //    r = new Rectangle(r.X, r.Y + 3, r.Width, r.Height - 3);
        //    e.Graphics.DrawString(TabName, TabFont, ForeBrush, r, sf);
        //    //Dispose objects
        //    sf.Dispose();
        //    if (e.Index == this.tabControl1.SelectedIndex)
        //    {
        //        TabFont.Dispose();
        //        BackBrush.Dispose();
        //    }
        //    else
        //    {
        //        BackBrush.Dispose();
        //        ForeBrush.Dispose();
        //    }
        //}

        //caspar event - data retrieved
        void caspar__DataRetrieved(object sender, DataEventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnDataRetrieved), e);
            else
                OnDataRetrieved(e);
        }
        void OnDataRetrieved(object param)
        {
            DataEventArgs d = param as DataEventArgs;
            System.Diagnostics.Debug.WriteLine(d.Data);
        }

        private void SetValueOnControl(Control container, string cName, string value)
        {
            Control[] cc;

            cc = container.Controls.Find(cName, true);
            if (cc.Length > 0)
            {
                if (cc[0] is TextBox)
                {
                    TextBox tb = cc[0] as TextBox;
                    tb.Text = value;
                }
                else if (cc[0] is ComboBox)
                {
                    ComboBox cb = cc[0] as ComboBox;
                    cb.SelectedItem = value;
                }
            }
        }

        //caspar event - connected
        void caspar__Connected(object sender, NetworkEventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnCasparConnected), e);
            else
                OnCasparConnected(e);
        }

        public static void SimpleListenerExample(string[] prefixes)
        {
            HttpListener listener = new HttpListener();
            // Add the prefixes. 
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();

            // Note: The GetContext method blocks while waiting for a request. 
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response. 
            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
            listener.Stop();
        }

        void OnCasparConnected(object param)
        {
            buttonConnect.Enabled = true;
            updateConnectButtonText();
            
            caspar_.RefreshMediafiles();
            caspar_.RefreshDatalist();

            NetworkEventArgs e = (NetworkEventArgs)param;
            enableControls();
           
            //Thread thread = new Thread(NetworkFilter.Begin);
            //thread.Start();
            //Thread.Sleep(5000);
        }

        //caspar event - failed connect
        void caspar__FailedConnected(object sender, NetworkEventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnCasparFailedConnect), e);
            else
                OnCasparFailedConnect(e);
        }
        void OnCasparFailedConnect(object param)
        {
            buttonConnect.Enabled = true;
            updateConnectButtonText();

            NetworkEventArgs e = (NetworkEventArgs)param;

            disableControls();
        }

        //caspar event - disconnected
        void caspar__Disconnected(object sender, NetworkEventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnCasparDisconnected), e);
            else
                OnCasparDisconnected(e);
        }
        void OnCasparDisconnected(object param)
        {
            buttonConnect.Enabled = true;
            updateConnectButtonText();

            NetworkEventArgs e = (NetworkEventArgs)param;

            disableControls();
        }

        // update text on button
        private void updateConnectButtonText()
        {
            if (!caspar_.IsConnected)
            {
                buttonConnect.Text = "Connect";// to " + Properties.Settings.Default.Hostname;
            }
            else
            {
                buttonConnect.Text = "Disconnect"; // from " + Properties.Settings.Default.Hostname;
            }
        }

        #endregion



        #region control enabling
        private void disableControls()
        {
            tabControl1.Enabled = false;
        }

        private void enableControls()
        {
            tabControl1.Enabled = true;
        }
        #endregion


        #region file handling
        string currentFilename_;
        string CurrentFilename
        {
            get
            {
                return currentFilename_;
            }
            set
            {
                currentFilename_ = value;
                //UpdateTitle();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO
            if (CheckSaveBefore() != DialogResult.Cancel)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                ofd.Filter = "World Cup 2010 Caspar Controller (*.vmc)|*.vmc";
                if (ofd.ShowDialog() == DialogResult.OK)
                    if (LoadFile(ofd.FileName))
                        CurrentFilename = ofd.FileName;
            }
        }

        DialogResult CheckSaveBefore()
        {
            //if (IsDirty)
            //{
            DialogResult result = MessageBox.Show("Do you want to save first?", "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            return result;
            // }
            // else
            //    return DialogResult.No;
        }

        private bool LoadFile(string filename)
        {
            bool returnvalue = true;
            try
            {
                this.clearAllData();
                XPathDocument xpDoc = new XPathDocument(filename);
                XPathNavigator xpNav = xpDoc.CreateNavigator();
                XPathNodeIterator items = xpNav.Select("/vmklocka/ticker");
                while (items.MoveNext())
                {
                    try
                    {
                        String tickerText = (string)items.Current.Evaluate("string(@Text)");
                        String tickerName = (string)items.Current.Evaluate("string(@Name)");

                        Control[] cc = Controls.Find(tickerName, true);
                        if (cc.Length > 0)
                        {
                            TextBox tbTicker = cc[0] as TextBox;
                            tbTicker.Text = tickerText;
                        }
                    }
                    catch
                    {

                    }
                }

            }
            catch (Exception ex)
            {
                returnvalue = false;
                MessageBox.Show(ex.Message, "There was an error when loading rundown");
            }

            //IsDirty = !returnvalue;
            return returnvalue;
        }

        private void clearAllData()
        {
            //extratidstext
        }

        private void WriteTickerXML(XmlWriter writer, TextBox tbTicker1)
        {
            writer.WriteStartElement("ticker");
            writer.WriteAttributeString("Text", tbTicker1.Text);
            writer.WriteAttributeString("Name", tbTicker1.Name);
            writer.WriteEndElement();
        }


        #endregion

        private bool hasValidClockData()
        {
            return true;
        }


        #region game time


        private void tbTimeMin_Leave(object sender, EventArgs e)
        {
            //this.updateGameTime();
        }

        private void tbTimeSec_Leave(object sender, EventArgs e)
        {
            //this.updateGameTime();
        }
      
        #endregion


        private void btnShowHideClock_Click(object sender, EventArgs e)
        {
            this.showHideClock();
        }

        private void showHideClock()
        {
            try
            {

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {

                    //caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Invoke(Properties.Settings.Default.GraphicsLayerClock, "clockShowHide");
                    //System.Diagnostics.Debug.WriteLine("Invoke");
                    //System.Diagnostics.Debug.WriteLine("clockShowHide");
                }
            }
        }


        private void btnSetGameTime_Click(object sender, EventArgs e)
        {
        }

        private void btnSetOvertime_Click(object sender, EventArgs e)
        {
        }

        private void btnTimeStartStop_Click(object sender, EventArgs e)
        {
            this.gameTimeStartStop();
        }

        private void gameTimeStartStop()
        {
            try
            {

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Invoke(Properties.Settings.Default.GraphicsLayerClock, "gameTimeStartStop");
                    System.Diagnostics.Debug.WriteLine("Invoke");
                    System.Diagnostics.Debug.WriteLine("gameTimeStartStop");
                }
            }
        }

        private void nyttToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckSaveBefore() != DialogResult.Cancel)
            {
                this.clearAllData();
                //IsDirty = false;
            }
        }

        private void avslutaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckSaveBefore() != DialogResult.Cancel)
            {
                Application.Exit();
            }

        }

        private void btnBottomFlarpShowHide_Click(object sender, EventArgs e)
        {
        }

        private void btnSaveHalfNum_Click(object sender, EventArgs e)
        {
        }

        private void ShowNameExternal(string templateName)
        {
            /*
              TODO: Check for valid field values
              */


            try
            {
                // Clear old data
                cgData.Clear();

                // build data

                String nameText = "";

                String[] nameTextArray = nameText.Split(new Char[] { '#' });
                if (nameTextArray.Length == 2)
                {
                    cgData.SetData("f0", nameTextArray[0]);
                    cgData.SetData("f1", nameTextArray[1]);
                }
                else
                {
                    cgData.SetData("f0", nameText);
                }

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerExternalTicker, templateName, true, cgData);
                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerExternalTicker);
                    System.Diagnostics.Debug.WriteLine(templateName);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }

        private void StopTickerExternal()
        {
            try
            {

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[0].CG.Stop(Properties.Settings.Default.GraphicsLayerExternalTicker);
                    System.Diagnostics.Debug.WriteLine("Stop");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerExternalTicker);
                }
            }
        }

        private void btnName1RowDownShow_Click(object sender, EventArgs e)
        {
            this.ShowNameExternal(Properties.Settings.Default.Name1RowDown);
        }

        private void btnName1RowDownHide_Click(object sender, EventArgs e)
        {
            this.StopNameExternal();
        }

        private void StopNameExternal()
        {
            // same layer as ticker..
            System.Diagnostics.Debug.WriteLine("Stopping name on same layer as ticker...");
        }

        private void btnName1RowUpShow_Click(object sender, EventArgs e)
        {
            this.ShowNameExternal(Properties.Settings.Default.Name1RowUp);
        }

        private void btnName1RowUpHide_Click(object sender, EventArgs e)
        {
            // same layer as ticker..
            System.Diagnostics.Debug.WriteLine("Stopping name on same layer as ticker...");
        }

        private void btnClearGraphics_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Clear on: " + Properties.Settings.Default.CasparChannel);
            try
            {
                this.caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Clear();
                this.caspar_.Channels[Properties.Settings.Default.CasparChannel].Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }

        }

        private void ax1_Enter(object sender, EventArgs e)
        {
            AxWindowsMediaPlayer ax = (AxWindowsMediaPlayer)sender;

            String fileName = System.IO.Path.GetFileName(ax.URL);
            String boxName = ax.Tag.ToString();
            box1.Visible = false;
            box2.Visible = false;
            box3.Visible = false;

            switch (boxName)
            {
                case "box1":
                    box1.Visible = true;
                    break;
                case "box2":
                    box2.Visible = true;
                    break;
                case "box3":
                    box3.Visible = true;
                    break;
                default:
                    break;
            }

            caspar_.Channels[0].LoadBG(new CasparItem(fileName));
            caspar_.Channels[0].Play();
        }

        private void bar1_Scroll(object sender, EventArgs e)
        {
            TrackBar bar = (TrackBar)sender;

            String playerName = bar.Tag.ToString();

            // Change the currect player.
            switch (playerName)
            {
                case "ax1":
                    ax1.settings.volume = bar.TabIndex;
                    break;
                case "ax2":
                    ax2.settings.volume = bar.TabIndex;
                    break;
                case "ax3":
                    ax3.settings.volume = bar.TabIndex;
                    break;
                default:
                    break;
            }
        }

        private string[] getLastChatMessage()
        {
            var json = new WebClient().DownloadString("http://embed.tlk.io/api/chats/643046/messages");
            var messages = new JavaScriptSerializer().Deserialize<List<Dictionary<string, string>>>(json);
            
            // There are no messags.
            if (messages.Count == 0)
            {
                return null;
            }

            var recentMessage = messages.Last();

            bool deleted;
            bool.TryParse(recentMessage["deleted"], out deleted);
            if (deleted)
            {
                return null;
            }

            string name = recentMessage["nickname"];
            string text = recentMessage["body"];
            string[] result = {name, text};
            return result;
        }

        private void chatTimer_Tick(object sender, EventArgs e)
        {
            this.showChatMessages();
        }
    }
}

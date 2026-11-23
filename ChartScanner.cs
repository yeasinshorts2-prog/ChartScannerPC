using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace ChartScannerPC
{
    public class ScannerForm : Form
    {
        private Panel iconPanel;
        private Panel popupPanel;
        private Label lblIcon;
        private Label lblTitle;
        private Label lblMin;
        private Label lblClose;
        private TextBox txtApiKey;
        private Label lblSignal;
        private Label lblReason;
        private Button btnAnalyze;

        private bool isDragging = false;
        private bool hasMoved = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private string keyFilePath;

        public ScannerForm()
        {
            keyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "apikey.txt");

            this.Text = "ChartScanner AI";
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 150);
            this.BackColor = Color.FromArgb(16, 28, 22);
            this.DoubleBuffered = true;

            SetupIconUI();
            SetupPopupUI();

            LoadApiKey();
            SwitchToIconMode();
        }

        private void SetupIconUI()
        {
            iconPanel = new Panel();
            iconPanel.Size = new Size(58, 58);
            iconPanel.Location = new Point(0, 0);
            iconPanel.BackColor = Color.FromArgb(20, 90, 50);
            iconPanel.Cursor = Cursors.Hand;

            lblIcon = new Label();
            lblIcon.Text = "AI";
            lblIcon.ForeColor = Color.White;
            lblIcon.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.Dock = DockStyle.Fill;

            lblIcon.MouseDown += Icon_MouseDown;
            lblIcon.MouseMove += Icon_MouseMove;
            lblIcon.MouseUp += Icon_MouseUp;

            iconPanel.Controls.Add(lblIcon);
            this.Controls.Add(iconPanel);
        }

        private void SetupPopupUI()
        {
            popupPanel = new Panel();
            popupPanel.Size = new Size(270, 265);
            popupPanel.Location = new Point(0, 0);
            popupPanel.BackColor = Color.FromArgb(16, 28, 22);
            popupPanel.Paint += PopupPanel_Paint;

            // হেডার বার (এটা ধরে ড্র্যাগ করা যাবে)
            Panel header = new Panel();
            header.Size = new Size(266, 34);
            header.Location = new Point(2, 2);
            header.BackColor = Color.FromArgb(22, 42, 32);
            header.Cursor = Cursors.SizeAll;
            header.MouseDown += Header_MouseDown;
            header.MouseMove += Header_MouseMove;
            header.MouseUp += Header_MouseUp;

            lblTitle = new Label();
            lblTitle.Text = "Quotex 1m AI";
            lblTitle.ForeColor = Color.FromArgb(46, 204, 113);
            lblTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTitle.Location = new Point(10, 7);
            lblTitle.AutoSize = true;
            lblTitle.MouseDown += Header_MouseDown;
            lblTitle.MouseMove += Header_MouseMove;
            lblTitle.MouseUp += Header_MouseUp;

            lblMin = new Label();
            lblMin.Text = "—";
            lblMin.ForeColor = Color.LightGray;
            lblMin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblMin.Size = new Size(26, 26);
            lblMin.Location = new Point(208, 4);
            lblMin.TextAlign = ContentAlignment.MiddleCenter;
            lblMin.Cursor = Cursors.Hand;
            lblMin.Click += delegate { SwitchToIconMode(); };

            lblClose = new Label();
            lblClose.Text = "✕";
            lblClose.ForeColor = Color.FromArgb(255, 107, 107);
            lblClose.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblClose.Size = new Size(26, 26);
            lblClose.Location = new Point(236, 4);
            lblClose.TextAlign = ContentAlignment.MiddleCenter;
            lblClose.Cursor = Cursors.Hand;
            lblClose.Click += delegate { Application.Exit(); };

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblMin);
            header.Controls.Add(lblClose);

            // API Key বক্স
            Label lblApi = new Label();
            lblApi.Text = "Gemini API Key:";
            lblApi.ForeColor = Color.Silver;
            lblApi.Font = new Font("Segoe UI", 8.5f);
            lblApi.Location = new Point(14, 44);
            lblApi.AutoSize = true;

            txtApiKey = new TextBox();
            txtApiKey.Location = new Point(14, 62);
            txtApiKey.Size = new Size(242, 24);
            txtApiKey.BackColor = Color.FromArgb(30, 48, 38);
            txtApiKey.ForeColor = Color.White;
            txtApiKey.BorderStyle = BorderStyle.FixedSingle;
            txtApiKey.UseSystemPasswordChar = true;

            // সিগন্যাল রেজাল্ট
            lblSignal = new Label();
            lblSignal.Text = "READY";
            lblSignal.ForeColor = Color.White;
            lblSignal.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblSignal.TextAlign = ContentAlignment.MiddleCenter;
            lblSignal.Location = new Point(10, 100);
            lblSignal.Size = new Size(250, 38);

            // কারণ / স্ট্যাটাস
            lblReason = new Label();
            lblReason.Text = "Open Quotex chart & click Analysis";
            lblReason.ForeColor = Color.FromArgb(207, 216, 220);
            lblReason.Font = new Font("Segoe UI", 9);
            lblReason.TextAlign = ContentAlignment.MiddleCenter;
            lblReason.Location = new Point(10, 142);
            lblReason.Size = new Size(250, 48);

            // ANALYSIS বাটন
            btnAnalyze = new Button();
            btnAnalyze.Text = "ANALYSIS";
            btnAnalyze.BackColor = Color.FromArgb(39, 174, 96);
            btnAnalyze.ForeColor = Color.White;
            btnAnalyze.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnAnalyze.FlatStyle = FlatStyle.Flat;
            btnAnalyze.FlatAppearance.BorderSize = 0;
            btnAnalyze.Location = new Point(14, 202);
            btnAnalyze.Size = new Size(242, 44);
            btnAnalyze.Cursor = Cursors.Hand;
            btnAnalyze.Click += BtnAnalyze_Click;

            popupPanel.Controls.Add(header);
            popupPanel.Controls.Add(lblApi);
            popupPanel.Controls.Add(txtApiKey);
            popupPanel.Controls.Add(lblSignal);
            popupPanel.Controls.Add(lblReason);
            popupPanel.Controls.Add(btnAnalyze);

            this.Controls.Add(popupPanel);
        }

        private void PopupPanel_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.FromArgb(46, 204, 113), 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, popupPanel.Width - 2, popupPanel.Height - 2);
            }
        }

        private void SwitchToIconMode()
        {
            popupPanel.Visible = false;
            iconPanel.Visible = true;
            this.Size = new Size(58, 58);

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, 58, 58);
            this.Region = new Region(path);
        }

        private void SwitchToPopupMode()
        {
            this.Region = null;
            iconPanel.Visible = false;
            popupPanel.Visible = true;
            this.Size = new Size(270, 265);
        }

        // আইকন ড্র্যাগ এবং ক্লিক
        private void Icon_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                hasMoved = false;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void Icon_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                if (Math.Abs(diff.X) > 6 || Math.Abs(diff.Y) > 6)
                {
                    hasMoved = true;
                    this.Location = Point.Add(dragFormPoint, new Size(diff));
                }
            }
        }

        private void Icon_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            if (!hasMoved && e.Button == MouseButtons.Left)
            {
                SwitchToPopupMode();
            }
        }

        // পপ-আপ ড্র্যাগ
        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void Header_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void LoadApiKey()
        {
            try
            {
                if (File.Exists(keyFilePath))
                {
                    txtApiKey.Text = File.ReadAllText(keyFilePath).Trim();
                }
            }
            catch { }
        }

        private void SaveApiKey(string key)
        {
            try
            {
                File.WriteAllText(keyFilePath, key);
            }
            catch { }
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            string apiKey = txtApiKey.Text.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                lblSignal.ForeColor = Color.Yellow;
                lblSignal.Text = "NO API KEY";
                lblReason.Text = "Please paste your Gemini API Key";
                return;
            }

            SaveApiKey(apiKey);

            btnAnalyze.Enabled = false;
            lblSignal.ForeColor = Color.Yellow;
            lblSignal.Text = "SCANNING...";
            lblReason.Text = "Capturing screen...";

            // স্ক্রিনশট পরিষ্কার নেওয়ার জন্য উইন্ডো সাময়িক লুকানো
            this.Hide();
            Application.DoEvents();
            Thread.Sleep(250);

            Bitmap screenshot = null;
            try
            {
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                screenshot = new Bitmap(bounds.Width, bounds.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
            }
            catch (Exception ex)
            {
                this.Show();
                lblSignal.ForeColor = Color.Red;
                lblSignal.Text = "ERROR";
                lblReason.Text = ex.Message;
                btnAnalyze.Enabled = true;
                return;
            }

            this.Show();
            lblReason.Text = "AI analyzing candlesticks...";

            Thread worker = new Thread(delegate()
            {
                AnalyzeWithGemini(screenshot, apiKey);
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void AnalyzeWithGemini(Bitmap bmp, string apiKey)
        {
            try
            {
                // Windows 7-এ TLS 1.2 চালু করা
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                // ছবি রিসাইজ করে হালকা করা
                int targetW = 900;
                int targetH = (int)((float)bmp.Height * targetW / bmp.Width);
                Bitmap scaled = new Bitmap(targetW, targetH);
                using (Graphics g = Graphics.FromImage(scaled))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, 0, 0, targetW, targetH);
                }
                bmp.Dispose();

                string base64Img;
                using (MemoryStream ms = new MemoryStream())
                {
                    scaled.Save(ms, ImageFormat.Jpeg);
                    base64Img = Convert.ToBase64String(ms.ToArray());
                }
                scaled.Dispose();

                string prompt = "You are an expert 1-minute binary options candlestick chart analyst. " +
                    "Analyze the latest candles, trend, and patterns (Hammer, Shooting Star, Doji, Marubozu, Engulfing, Morning/Evening Star) on this trading chart. " +
                    "Predict the NEXT 1-minute candle direction. " +
                    "Reply STRICTLY in 2 short lines only:\n" +
                    "Line 1: UP or DOWN\n" +
                    "Line 2: Short reason (max 8 words).";

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;

                var payload = new Dictionary<string, object>
                {
                    {
                        "contents", new object[]
                        {
                            new Dictionary<string, object>
                            {
                                {
                                    "parts", new object[]
                                    {
                                        new Dictionary<string, object> { { "text", prompt } },
                                        new Dictionary<string, object>
                                        {
                                            {
                                                "inline_data", new Dictionary<string, object>
                                                {
                                                    { "mime_type", "image/jpeg" },
                                                    { "data", base64Img }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                string jsonBody = serializer.Serialize(payload);
                string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
req.Method = "POST";
req.ContentType = "application/json";
req.Headers.Add("X-goog-api-key", apiKey);
req.Timeout = 25000;

                byte[] bytes = Encoding.UTF8.GetBytes(jsonBody);
                req.ContentLength = bytes.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                }

                string responseText;
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                }

                Dictionary<string, object> jsonResp = serializer.Deserialize<Dictionary<string, object>>(responseText);
                IList candidates = (IList)jsonResp["candidates"];
                Dictionary<string, object> firstCand = (Dictionary<string, object>)candidates[0];
                Dictionary<string, object> contentObj = (Dictionary<string, object>)firstCand["content"];
                IList parts = (IList)contentObj["parts"];
                Dictionary<string, object> firstPart = (Dictionary<string, object>)parts[0];
                string aiText = firstPart["text"].ToString().Trim();

                string[] lines = aiText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string signal = lines.Length > 0 ? lines[0].Trim().ToUpper() : "WAIT";
                string reason = lines.Length > 1 ? lines[1].Trim() : "Pattern analyzed";

                this.Invoke((MethodInvoker)delegate
                {
                    if (signal.Contains("UP"))
                    {
                        lblSignal.ForeColor = Color.FromArgb(46, 204, 113);
                        lblSignal.Text = "▲ UP (CALL)";
                    }
                    else if (signal.Contains("DOWN"))
                    {
                        lblSignal.ForeColor = Color.FromArgb(255, 82, 82);
                        lblSignal.Text = "▼ DOWN (PUT)";
                    }
                    else
                    {
                        lblSignal.ForeColor = Color.Yellow;
                        lblSignal.Text = signal;
                    }
                    lblReason.Text = reason;
                    btnAnalyze.Enabled = true;
                });
            }
            catch (WebException wex)
            {
                string errMsg = wex.Message;
                if (wex.Response != null)
                {
                    HttpWebResponse errResp = (HttpWebResponse)wex.Response;
                    errMsg = "API Error (" + (int)errResp.StatusCode + ") - Check Key";
                }
                this.Invoke((MethodInvoker)delegate
                {
                    lblSignal.ForeColor = Color.Red;
                    lblSignal.Text = "API ERROR";
                    lblReason.Text = errMsg;
                    btnAnalyze.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblSignal.ForeColor = Color.Red;
                    lblSignal.Text = "ERROR";
                    lblReason.Text = ex.Message;
                    btnAnalyze.Enabled = true;
                });
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ScannerForm());
        }
    }
}

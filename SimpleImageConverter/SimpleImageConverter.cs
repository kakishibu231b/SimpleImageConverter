using ImageMagick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleImageConverter
{
    public partial class SimpleImageConverter : Form
    {
        public SimpleImageConverter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 出力対象拡張子とフォーマット番号
        /// </summary>
        Dictionary<string, MagickFormat> dicOutputExtentions = new Dictionary<string, MagickFormat>();

        /// <summary>
        /// 起動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimpleImageConverter_Load(object sender, EventArgs e)
        {
            for (int ii = 0, count = checkedListBoxExt1.Items.Count; ii < count; ++ii)
            {
                checkedListBoxExt1.SetItemChecked(ii, true);
            }

            toolStripStatusLabel1.Text = "ファイルまたはフォルダを画面にドラッグ＆ドロップしてください";

            foreach (string strTargetExtention in checkedListBoxExt2.Items)
            {
                string strExtentionLower = strTargetExtention.ToLower();
                MagickFormat magickFormat = 0;
                if (strExtentionLower == "png")
                {
                    magickFormat = MagickFormat.Png32;
                }
                else if (strExtentionLower == "jpg")
                {
                    magickFormat = MagickFormat.Jpg;
                }
                else if (strExtentionLower == "webp")
                {
                    magickFormat = MagickFormat.WebP;
                }
                dicOutputExtentions.Add("." + strExtentionLower, magickFormat);
            }
        }

        /// <summary>
        /// ドラッグ＆ドロップ開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimpleImageConverter_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// ドラッグ＆ドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimpleImageConverter_DragDrop(object sender, DragEventArgs e)
        {
            // ファイル、フォルダを処理対象とする。
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                // 処理対象拡張子取得
                List<string> lstExtentions = new List<string>();
                foreach (string strTargetExtention in checkedListBoxExt1.Items)
                {
                    lstExtentions.Add(strTargetExtention.ToLower());
                }

                // ドロップされたファイル、フォルダのパス取得
                string[] strPathNames = e.Data.GetData(DataFormats.FileDrop, false) as string[];
                foreach(string strPathName in strPathNames)
                {
                    // ファイル属性取得
                    FileAttributes fileAttributes = File.GetAttributes(strPathName);

                    // フォルダは無条件に追加する。
                    if((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        checkedListBoxTarget.Items.Add(strPathName);
                        checkedListBoxTarget.SetItemChecked(checkedListBoxTarget.Items.Count - 1, true);
                    }
                    else
                    {
                        // ファイルは処理対象拡張子のみ追加する。
                        string strExtention = Path.GetExtension(strPathName);
                        strExtention = strExtention.Replace(".", "");
                        strExtention = strExtention.ToLower();
                        if (lstExtentions.Contains(strExtention))
                        {
                            checkedListBoxTarget.Items.Add(strPathName);
                            checkedListBoxTarget.SetItemChecked(checkedListBoxTarget.Items.Count - 1, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ファイルフォルダ一覧全件チェックON
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemCheckOnAll_Click(object sender, EventArgs e)
        {
            for (int ii = 0, count = checkedListBoxTarget.Items.Count; ii < count; ++ii)
            {
                checkedListBoxTarget.SetItemChecked(ii, true);
            }
        }

        /// <summary>
        /// ファイルフォルダ一覧全件チェックOFF
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemCheckOffAll_Click(object sender, EventArgs e)
        {
            for (int ii = 0, count = checkedListBoxTarget.Items.Count; ii < count; ++ii)
            {
                checkedListBoxTarget.SetItemChecked(ii, false);
            }
        }

        /// <summary>
        /// 選択行削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemRemoveSelectedItems_Click(object sender, EventArgs e)
        {
            int sel = checkedListBoxTarget.SelectedIndex;
            if (sel == -1)
            {
                return;
            }

            checkedListBoxTarget.Items.RemoveAt(sel);
        }

        // 入力対象拡張子
        List<string> lstInputExtentions = new List<string>();

        // 出力対象拡張子
        List<string> lstOutputExtentions = new List<string>();

        /// <summary>
        /// 変換
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStart_Click(object sender, EventArgs e)
        {
            // 入力対象拡張子取得
            lstInputExtentions.Clear();
            foreach (string strTargetExtention in checkedListBoxExt1.CheckedItems)
            {
                string strExtentionLower = strTargetExtention.ToLower();
                lstInputExtentions.Add("." + strExtentionLower);
            }
            if (lstInputExtentions.Count == 0)
            {
                return;
            }

            // 出力対象拡張子取得
            lstOutputExtentions.Clear();
            foreach (string strTargetExtention in checkedListBoxExt2.CheckedItems)
            {
                string strExtentionLower = strTargetExtention.ToLower();
                lstOutputExtentions.Add("." + strExtentionLower);
            }
            if (lstOutputExtentions.Count == 0)
            {
                return;
            }

            // 出力対象ファイルフォルダ取得
            List<string> strFileNames = new List<string>();
            foreach (var item in checkedListBoxTarget.CheckedItems)
            {
                strFileNames.Add(item.ToString().Trim());
            }

            toolStripProgressBar1.Maximum = strFileNames.Count;

            // 処理開始
            backgroundWorker1.RunWorkerAsync(strFileNames);
        }

        /// <summary>
        /// 中止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStop_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        /// <summary>
        /// 進捗状況
        /// </summary>
        class ProgressInfo
        {
            public ProgressInfo(string level, string message)
            {
                DateTime = DateTime.Now;
                Level = level;
                Message = message;
            }

            public DateTime DateTime { get; }
            public string Level { get; }
            public string Message { get; }
        }

        /// <summary>
        /// 処理対象ファイル一覧
        /// </summary>
        List<string> lstTargetFiles = new List<string>();

        /// <summary>
        /// 変換処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // 処理対象ファイルフォルダ
            List<string> strFileNames = e.Argument as List<string>;

            // 処理開始メッセージ
            int intCount = 0;
            backgroundWorker1.ReportProgress(intCount, new ProgressInfo("A", $"変換対象のファイルを検索しています...({intCount++}/{strFileNames.Count})"));

            // 変換対象ファイル抽出
            lstTargetFiles.Clear();
            foreach (string strFileName in strFileNames)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    backgroundWorker1.ReportProgress(0, new ProgressInfo("A", $"変換対象のファイル検索を中止しました({intCount}/{strFileNames.Count})"));
                    break;
                }

                FileAttributes fileAttributes = File.GetAttributes(strFileName);
                switch (fileAttributes & FileAttributes.Directory)
                {
                    case FileAttributes.Directory:
                        searchDirectory(strFileName);
                        break;
                    default:
                        searchFile(strFileName);
                        break;
                }

                backgroundWorker1.ReportProgress(intCount, new ProgressInfo("A", $"変換対象のファイルを検索しています...({intCount++}/{strFileNames.Count})"));
            }
            if (backgroundWorker1.CancellationPending)
            {
               return;
            }

            backgroundWorker1.ReportProgress(lstTargetFiles.Count, new ProgressInfo("X", ""));

            intCount = 0;
            backgroundWorker1.ReportProgress(intCount, new ProgressInfo("A", $"ファイルを変換しています...({intCount++}/{lstTargetFiles.Count})"));

            // 変換処理
            foreach (string strFileName in lstTargetFiles)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    backgroundWorker1.ReportProgress(0, new ProgressInfo("A", $"ファイルの変換を中止しました({intCount}/{lstTargetFiles.Count})"));
                    break;
                }

                foreach (string strOutputExtention in lstOutputExtentions)
                {
                    if ( dicOutputExtentions.TryGetValue(strOutputExtention, out MagickFormat magickFormat))
                    {
                        convertFile(strFileName, strOutputExtention, magickFormat);
                    }
                }

                backgroundWorker1.ReportProgress(intCount, new ProgressInfo("A", $"ファイルを変換しています...({intCount++}/{lstTargetFiles.Count})"));
            }
            if (backgroundWorker1.CancellationPending)
            {
                return;
            }

            backgroundWorker1.ReportProgress(0, new ProgressInfo("A", "ファイルの変換が完了しました"));
        }

        /// <summary>
        /// ファイル抽出(ディレクトリ)
        /// </summary>
        /// <param name="strDirectory"></param>
        private void searchDirectory(string strDirectory)
        {
            // サブフォルダチェックONの場合下位フォルダを処理する。
            if (checkBoxSubFolder.Checked)
            {
                string[] strDirectoryNames = Directory.GetDirectories(strDirectory);
                foreach (string strDirectoryName in strDirectoryNames)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        break;
                    }
                    searchDirectory(strDirectoryName);
                }
            }
            if (backgroundWorker1.CancellationPending)
            {
                return;
            }

            // フォルダ内のファイルを処理する。
            string[] strFileNames = Directory.GetFiles(strDirectory);
            foreach (string strFileName in strFileNames)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    break;
                }
                searchFile(strFileName);
            }
            if (backgroundWorker1.CancellationPending)
            {
                return;
            }
        }

        /// <summary>
        /// ファイル抽出(ファイル)
        /// </summary>
        /// <param name="strFileName"></param>
        private void searchFile(string strFileName)
        {
            string strExtensionName = Path.GetExtension(strFileName);
            strExtensionName = strExtensionName.ToLower();
            if (lstInputExtentions.Contains(strExtensionName))
            {
                lstTargetFiles.Add(strFileName);
                backgroundWorker1.ReportProgress(0, new ProgressInfo("B", $"処理対象ファイルに追加しました {strFileName}"));
            }
        }

        /// <summary>
        /// 画像ファイル変換
        /// </summary>
        /// <param name="strInFilePath"></param>
        /// <param name="strOutExtension"></param>
        /// <param name="mfOutMagickFormat"></param>
        private void convertFile(string strInFilePath, string strOutExtension, MagickFormat mfOutMagickFormat)
        {
            // 入力ファイル拡張子
            string strInputExtension = Path.GetExtension(strInFilePath);

            // 入力ファイルと出力ファイルの拡張子が同じ場合処理対象外とする。
            if (strInputExtension.ToLower() == strOutExtension.ToLower())
            {
                return;
            }

            // 入力ファイル格納フォルダパス取得
            string strDirecotryName = Path.GetDirectoryName(strInFilePath);

            // 拡張子なしファイル名取得
            string strFileNameWithoutExtension = Path.GetFileNameWithoutExtension(strInFilePath);

            // 出力ファイルパス構築
            string strOutFileName = strFileNameWithoutExtension + strOutExtension;
            string strOutFilePath = Path.Combine(strDirecotryName, strOutFileName);

            if (lstTargetFiles.Contains(strOutFilePath))
            {
                // 入力ファイルは上書きしない
                backgroundWorker1.ReportProgress(0, new ProgressInfo("B", $"出力ファイルパスが入力ファイルと同じになるため変換をスキップしました 出力ファイル名:{strOutFilePath}"));
            }
            else
            {
                // 変換処理起動
                SaveMultiFrame(strInFilePath, strOutFilePath, mfOutMagickFormat);
            }
        }

        public void SaveMultiFrame(string strInFilePath, string strOutFilePath, MagickFormat format)
        {
            MagickImageCollection magickImages = new MagickImageCollection(strInFilePath);

            try
            {
                string strOutFileName = Path.GetFileName(strOutFilePath);

                if (magickImages.Count == 1)
                {
                    backgroundWorker1.ReportProgress(0, new ProgressInfo("B", $"変換を開始します 変換前:{strInFilePath} 変換後:{strOutFilePath}"));

                    ImageMagickAdapter.Save(strInFilePath, strOutFilePath, format);

                    backgroundWorker1.ReportProgress(0, new ProgressInfo("B", $"変換に成功しました 変換前:{strInFilePath} 変換後:{strOutFilePath}"));
                }
                else
                {
                    string strOutDirectoryName = Path.GetDirectoryName(strOutFilePath);
                    string strOutFileNameWithoutExtension = Path.GetFileNameWithoutExtension(strOutFilePath);
                    string strOutExtension = Path.GetExtension(strOutFilePath);
                    string strOutDirectoryName2 = Path.Combine(strOutDirectoryName, strOutFileName.Replace(".", "_"));

                    if (!File.Exists(strOutDirectoryName2))
                    {
                        Directory.CreateDirectory(strOutDirectoryName2);
                    }

                    int intFrameNumber = 1;
                    foreach (MagickImage magickImage in magickImages)
                    {
                        string strOutFileName2 = $"{strOutFileNameWithoutExtension}_{intFrameNumber:00}{strOutExtension}";
                        string strOutFilePath2 = Path.Combine(strOutDirectoryName2, strOutFileName2);
                        backgroundWorker1.ReportProgress(0, new ProgressInfo("B", $"変換を開始します 変換前:{strInFilePath} フレーム番号:{intFrameNumber} 変換後:{strOutFilePath2}"));

                        ImageMagickAdapter.Save(magickImage, strOutFilePath2, format);

                        backgroundWorker1.ReportProgress(0, new ProgressInfo("B", $"変換に成功しました 変換前:{strInFilePath} フレーム番号:{intFrameNumber} 変換後:{strOutFilePath2}"));
                        intFrameNumber++;

                        magickImage.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                magickImages.Dispose();
            }
        }

        /// <summary>
        /// 変換処理進捗報告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressInfo progressInfo = e.UserState as ProgressInfo;

            string strMessage = string.Format("{0} {1}", progressInfo.DateTime.ToString("yyyy/MM/dd hh:mm:ss"), progressInfo.Message);

            switch (progressInfo.Level)
            {
                case "X":
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Maximum = e.ProgressPercentage;
                    toolStripStatusLabel1.Text = "";
                    break;
                case "A":
                    toolStripProgressBar1.Value = e.ProgressPercentage;
                    toolStripStatusLabel1.Text = progressInfo.Message;
                    listBoxLog.Items.Add(strMessage);
                    break;
                case "B":
                    listBoxLog.Items.Add(strMessage);
                    break;
                default:
                    break;
            }
            listBoxLog.TopIndex = listBoxLog.Items.Count - 1;
        }

        /// <summary>
        /// 変換処理終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lstInputExtentions.Clear();
            lstOutputExtentions.Clear();
        }
    }
}

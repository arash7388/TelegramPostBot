using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NReco.VideoConverter;

namespace Converter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            NReco.VideoConverter.FFMpegConverter wrap = new FFMpegConverter();
            var path = @"C:\Users\Arash\Desktop\Convert";
            var file = Directory.GetFiles(path).FirstOrDefault();
            
            var s = $" -i {file} -vf \"drawtext=text='SaraSamet':x=10:y=H-th-10:" +
            $"fontfile=c:\\windows\\fonts\\arial.ttf:fontsize=16:fontcolor=white:" +
            $"shadowcolor=black:shadowx=1:shadowy=1\" {path}\\output.mp4";

            //wrap.Invoke("-i D:\\d.mp4 -i D:\\w.txt -filter_complex \"overlay=10:10\" D:\\O.mp4");
            wrap.Invoke(s);
        }

        private void btnCambridge_Click(object sender, EventArgs e)
        {
            PostBot.CambridgeJob.DoPost();
        }

        private void btnPhotoWithText_Click(object sender, EventArgs e)
        {
            PostBot.PhotoWithTextJob.DoPost();
        }

        private void btnEslFast_Click(object sender, EventArgs e)
        {
            PostBot.EslFastJob.DoPost();
        }

        private void btnListenAMin_Click(object sender, EventArgs e)
        {
            PostBot.ListenAMinJob.DoPost();
        }
    }
}

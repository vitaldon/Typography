﻿//MIT, 2016-2017, WinterDev
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
//
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Rendering;


namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        //for this sample code,
        //create text printer env for developer.
        DevGdiTextPrinter currentTextPrinter = new DevGdiTextPrinter();

        public Form1()
        {
            InitializeComponent();

            //choose Thai script for 'complex script' testing.
            //you can change this to test other script.
            currentTextPrinter.ScriptLang = Typography.OpenFont.ScriptLangs.Thai;
            //----------
            button1.Click += (s, e) => UpdateRenderOutput();
            //simple load test fonts from local test dir
            //and send it into test list
            chkFillBackground.Checked = true;
            chkBorder.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkFillBackground.CheckedChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbPositionTech.Items.Add(PositionTechnique.OpenFont);
            cmbPositionTech.Items.Add(PositionTechnique.Kerning);
            cmbPositionTech.Items.Add(PositionTechnique.None);
            cmbPositionTech.SelectedIndex = 0;
            cmbPositionTech.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //----------
            lstHintList.Items.Add(HintTechnique.None);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction_VerticalOnly);
            lstHintList.Items.Add(HintTechnique.CustomAutoFit);
            lstHintList.SelectedIndex = 0;
            lstHintList.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //---------- 

            txtInputChar.TextChanged += (s, e) => UpdateRenderOutput();
            //
            int selectedFileIndex = -1;
            //string selectedFontFileName = "pala.ttf";
            string selectedFontFileName = "tahoma.ttf";
            //string selectedFontFileName="cambriaz.ttf";
            //string selectedFontFileName="CompositeMS2.ttf"; 
            int fileIndexCount = 0;

            foreach (string file in Directory.GetFiles("..\\..\\..\\TestFonts", "*.ttf"))
            {
                var tmpLocalFile = new TempLocalFontFile(file);
                lstFontList.Items.Add(tmpLocalFile);
                if (selectedFileIndex < 0 && tmpLocalFile.OnlyFileName == selectedFontFileName)
                {
                    selectedFileIndex = fileIndexCount;
                    currentTextPrinter.FontFilename = file;

                }
                fileIndexCount++;
            }
            if (selectedFileIndex < 0) { selectedFileIndex = 0; }
            lstFontList.SelectedIndex = selectedFileIndex;
            lstFontList.SelectedIndexChanged += (s, e) =>
            {
                currentTextPrinter.FontFilename = ((TempLocalFontFile)lstFontList.SelectedItem).actualFileName;
                UpdateRenderOutput();
            };
            //----------
            lstFontSizes.Items.AddRange(
                new object[]{
                    8, 9,
                    10,11,
                    12,
                    14,
                    16,
                    18,20,22,24,26,28,36,48,72,240,300,360
                });
            lstFontSizes.SelectedIndexChanged += (s, e) =>
            {
                //new font size
                currentTextPrinter.FontSizeInPoints = (int)lstFontSizes.SelectedItem;
                UpdateRenderOutput();
            };
            lstFontSizes.SelectedIndex = 0;
            this.Text = "Gdi+ Sample";
        }
        void UpdateRenderOutput()
        {
            //render glyph with gdi path
            if (g == null)
            {
                g = this.CreateGraphics();
            }
            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }
            //-----------------------  
            //set some props ...
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            //credit:
            //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly   

            //-----------------------  
            currentTextPrinter.HintTechnique = (HintTechnique)lstHintList.SelectedItem;
            currentTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
            //render at specific pos
            float x_pos = 0, y_pos = 100;
            char[] textBuffer = txtInputChar.Text.ToCharArray();

            //test draw multiple lines
            float lineSpacingPx = currentTextPrinter.FontLineSpacingPx;
            for (int i = 0; i < 3; ++i)
            {
                currentTextPrinter.DrawString(g,
                 textBuffer,
                 0,
                 textBuffer.Length,
                 x_pos,
                 y_pos
                );
                //draw top to bottom 
                y_pos -= lineSpacingPx;
            }
            //
            //-----------------------  
            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly            
        }

        //=========================================================================
        //msdf texture generator example
        private void cmdBuildMsdfTexture_Click(object sender, System.EventArgs e)
        {
            string sampleFontFile = @"..\..\..\TestFonts\tahoma.ttf";
            CreateSampleMsdfTextureFont(
                sampleFontFile,
                18,
                0,
                255,
                "d:\\WImageTest\\sample_msdf.png");
        }
        static void CreateSampleMsdfTextureFont(string fontfile, float sizeInPoint, ushort startGlyphIndex, ushort endGlyphIndex, string outputFile)
        {
            //sample
            var reader = new OpenFontReader();

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeface = reader.Read(fs);
                //sample: create sample msdf texture 
                //-------------------------------------------------------------
                var builder = new GlyphPathBuilder(typeface);
                //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
                //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
                //-------------------------------------------------------------
                var atlasBuilder = new SimpleFontAtlasBuilder();


                for (ushort n = startGlyphIndex; n <= endGlyphIndex; ++n)
                {
                    //build glyph
                    builder.BuildFromGlyphIndex(n, sizeInPoint);
                    var glyphToContour = new GlyphTranslatorToContour();
                    builder.ReadShapes(glyphToContour);
                    //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours()); 

                    GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour);
                    atlasBuilder.AddGlyph(n, glyphImg);

                    //using (Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    //{
                    //    var bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
                    //    bmp.UnlockBits(bmpdata);
                    //    bmp.Save("d:\\WImageTest\\a001_xn2_" + n + ".png");
                    //}
                }

                var glyphImg2 = atlasBuilder.BuildSingleImage();
                using (Bitmap bmp = new Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    int[] intBuffer = glyphImg2.GetImageBuffer();

                    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                    bmp.UnlockBits(bmpdata);
                    bmp.Save("d:\\WImageTest\\a_total.png");
                }
                atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");
            }
        }

        private void cmdMeasureTextSpan_Click(object sender, System.EventArgs e)
        {
            //set some Gdi+ props... 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            //credit:
            //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly   

            //--------------------------------
            //textspan measurement sample
            //--------------------------------  
            currentTextPrinter.HintTechnique = (HintTechnique)lstHintList.SelectedItem;
            currentTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
            //render at specific pos
            float x_pos = 0, y_pos = 100;
            char[] textBuffer = txtInputChar.Text.ToCharArray();

            //Example 1: this is a basic draw sample
            currentTextPrinter.FillColor = Color.Black;
            currentTextPrinter.DrawString(g,
                 textBuffer,
                 0,
                 textBuffer.Length,
                 x_pos,
                 y_pos
                );
            //
            //--------------------------------------------------
            //Example 2: print glyph plan to 'user' list-> then draw it (or hold it/ not draw)                         
            //you can create you own class to hold userGlyphPlans.***
            //2.1
            List<GlyphPlan> userGlyphPlans = new List<GlyphPlan>();
            currentTextPrinter.PrintGlyphPlans(userGlyphPlans, textBuffer, 0, textBuffer.Length);
            //2.2
            //and we can print the formatted glyph plan later.
            y_pos -= currentTextPrinter.FontLineSpacingPx;
            currentTextPrinter.FillColor = Color.Red;
            currentTextPrinter.DrawString(g,
                  userGlyphPlans,
                  x_pos,
                  y_pos
             );
            //Example 3: MeasureString
            //3.1
            SizeF sizeF = currentTextPrinter.MeasureString(textBuffer, 0, textBuffer.Length);
            //draw rect 
            g.DrawRectangle(Pens.Red, x_pos, y_pos, sizeF.Width, sizeF.Height);
            //3.2
            MeasuredStringBox strBox;
            currentTextPrinter.MeasureString(textBuffer, 0, textBuffer.Length, out strBox);
            //draw line mark
            float x_pos2 = x_pos + sizeF.Width + 10;
            g.DrawLine(Pens.Blue, x_pos, y_pos, x_pos2, y_pos); //baseline
            g.DrawLine(Pens.Green, x_pos, y_pos + strBox.descending, x_pos2, y_pos + strBox.descending);//descending
            g.DrawLine(Pens.Magenta, x_pos, y_pos + strBox.ascending, x_pos2, y_pos + strBox.ascending);//ascending

            //
            //Example 4:wrap glyph plan to the 'textrun'
            y_pos -= currentTextPrinter.FontLineSpacingPx;
            currentTextPrinter.FillColor = Color.Blue;
            TextRun textRun = new TextRun(
                textBuffer,
                0,
                textBuffer.Length);

            //set presentation elements
            textRun.SetGlyphPlan(userGlyphPlans, 0, userGlyphPlans.Count);




            //-------------------------------------------------- 
            currentTextPrinter.FillColor = Color.Black;
            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly   
        }




        class TextRun
        {
            char[] _srcTextBuffer;
            int _startAt;
            int _len;

            GlyphPlanListCache _glyphPlanListCache;

            public TextRun(char[] srcTextBuffer, int startAt, int len)
            {
                this._srcTextBuffer = srcTextBuffer;
                this._startAt = startAt;
                this._len = len;
            }
            public void SetGlyphPlan(List<GlyphPlan> glyphPlans, int startAt, int len)
            {
                _glyphPlanListCache = new GlyphPlanListCache(glyphPlans, startAt, len);
            }

            struct GlyphPlanListCache
            {
                public readonly List<GlyphPlan> glyphPlans;
                public readonly int startAt;
                public readonly int len;
                public GlyphPlanListCache(List<GlyphPlan> glyphPlans, int startAt, int len)
                {
                    this.glyphPlans = glyphPlans;
                    this.startAt = startAt;
                    this.len = len;
                }

            }

        }
    }
}

namespace WindowsFormsApplication2
{
    partial class frmMatome
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.picSVG = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picSVG)).BeginInit();
            this.SuspendLayout();
            // 
            // picSVG
            // 
            this.picSVG.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picSVG.Location = new System.Drawing.Point(0, -1);
            this.picSVG.Name = "picSVG";
            this.picSVG.Size = new System.Drawing.Size(960, 540);
            this.picSVG.TabIndex = 1;
            this.picSVG.TabStop = false;
            this.picSVG.Paint += new System.Windows.Forms.PaintEventHandler(this.picSVG_Paint);
            this.picSVG.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picSVG_MouseDown);
            //this.picSVG.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.picSVG_DoubleClick);
            // 
            // frmMatome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(942, 493);
            this.Controls.Add(this.picSVG);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmMatome";
            this.Text = "まとめ";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picSVG)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picSVG;
    }
}


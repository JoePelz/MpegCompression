namespace NodeShop {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.viewLeft = new NodeShop.Viewport(this.components);
            this.viewRight = new NodeShop.Viewport(this.components);
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.viewNodes = new NodeShop.NodeView(this.components);
            this.viewProperties = new NodeShop.PropertyPage(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(1181, 603);
            this.splitContainer1.SplitterDistance = 393;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.viewLeft);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.viewRight);
            this.splitContainer2.Size = new System.Drawing.Size(1181, 393);
            this.splitContainer2.SplitterDistance = 576;
            this.splitContainer2.TabIndex = 0;
            // 
            // viewLeft
            // 
            this.viewLeft.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.viewLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewLeft.Location = new System.Drawing.Point(0, 0);
            this.viewLeft.Name = "viewLeft";
            this.viewLeft.Size = new System.Drawing.Size(576, 393);
            this.viewLeft.TabIndex = 0;
            this.viewLeft.TabStop = true;
            // 
            // viewRight
            // 
            this.viewRight.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.viewRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewRight.Location = new System.Drawing.Point(0, 0);
            this.viewRight.Name = "viewRight";
            this.viewRight.Size = new System.Drawing.Size(601, 393);
            this.viewRight.TabIndex = 0;
            this.viewRight.TabStop = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.viewNodes);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.viewProperties);
            this.splitContainer3.Size = new System.Drawing.Size(1181, 206);
            this.splitContainer3.SplitterDistance = 819;
            this.splitContainer3.TabIndex = 0;
            // 
            // viewNodes
            // 
            this.viewNodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewNodes.Location = new System.Drawing.Point(0, 0);
            this.viewNodes.Name = "viewNodes";
            this.viewNodes.Size = new System.Drawing.Size(819, 206);
            this.viewNodes.TabIndex = 0;
            this.viewNodes.TabStop = true;
            // 
            // viewProperties
            // 
            this.viewProperties.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.viewProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 358F));
            this.viewProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewProperties.Location = new System.Drawing.Point(0, 0);
            this.viewProperties.Name = "viewProperties";
            this.viewProperties.Size = new System.Drawing.Size(358, 206);
            this.viewProperties.TabIndex = 0;
            this.viewProperties.TabStop = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1181, 603);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "NodeShop - Node-based DSP application";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private Viewport viewLeft;
        private Viewport viewRight;
        private PropertyPage viewProperties;
        private NodeView viewNodes;
    }
}


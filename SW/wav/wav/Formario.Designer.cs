namespace wav
{
    partial class Formario
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button4 = new System.Windows.Forms.Button();
            this.txtCantidadMuestrasPorSeg = new System.Windows.Forms.TextBox();
            this.txtTiempoEnSeg = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtArchivoSeleccionado = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(12, 78);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(139, 83);
            this.button4.TabIndex = 3;
            this.button4.Text = "Procesar!";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // txtCantidadMuestrasPorSeg
            // 
            this.txtCantidadMuestrasPorSeg.Location = new System.Drawing.Point(175, 91);
            this.txtCantidadMuestrasPorSeg.Name = "txtCantidadMuestrasPorSeg";
            this.txtCantidadMuestrasPorSeg.Size = new System.Drawing.Size(100, 20);
            this.txtCantidadMuestrasPorSeg.TabIndex = 4;
            this.txtCantidadMuestrasPorSeg.Text = "3100";
            // 
            // txtTiempoEnSeg
            // 
            this.txtTiempoEnSeg.Location = new System.Drawing.Point(178, 138);
            this.txtTiempoEnSeg.Name = "txtTiempoEnSeg";
            this.txtTiempoEnSeg.Size = new System.Drawing.Size(100, 20);
            this.txtTiempoEnSeg.TabIndex = 5;
            this.txtTiempoEnSeg.Text = "22";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(175, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(171, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Cantidad de muestras por segundo";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(175, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Tiempo (en segundos)";
            // 
            // txtArchivoSeleccionado
            // 
            this.txtArchivoSeleccionado.Location = new System.Drawing.Point(175, 31);
            this.txtArchivoSeleccionado.Name = "txtArchivoSeleccionado";
            this.txtArchivoSeleccionado.Size = new System.Drawing.Size(171, 20);
            this.txtArchivoSeleccionado.TabIndex = 8;
            this.txtArchivoSeleccionado.Text = "prueba.txt";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(175, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Archivo selecccionado";
            // 
            // Formario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 262);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtArchivoSeleccionado);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtTiempoEnSeg);
            this.Controls.Add(this.txtCantidadMuestrasPorSeg);
            this.Controls.Add(this.button4);
            this.Name = "Formario";
            this.Text = "Formulario procesamiento";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox txtCantidadMuestrasPorSeg;
        private System.Windows.Forms.TextBox txtTiempoEnSeg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtArchivoSeleccionado;
    }
}


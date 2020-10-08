﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using System.IO;

using HRC.Common.Data;
using HRC.Common.Exceptions;
using HRC.Framework.BL;
using HRC.IRIS.BL;

namespace Harness 
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = string.Empty;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                path = dialog.FileName;
            }

            byte[] bytes = System.IO.File.ReadAllBytes(path);

            Save(path);            
        }

        private void TestSaveFile()
        {
            Save(@"\\file\scanner\IRIS\0\Test test test.doc");
        }

        private void Save(string path)
        {
            Document doc = new Document();

            Guid guid = Guid.NewGuid();

            doc.IrisId = 2209;
            
            doc.Name = "this is a test";
            doc.DocumentFullPath = path;
            doc.Owner = "dave";
            doc.Status = DocumentStatus.Open;

            DocumentVersion version = new DocumentVersion();
            
            version.VersionId = guid;
            version.CreatedDate = DateTime.Now;
            version.CreatedBy = "dave";
            version.ModifiedDate = version.CreatedDate;
            version.ModifiedBy = "dave";

            version.Document = System.IO.File.ReadAllBytes(path);

            doc.Versions.Add(version);

            doc.Save(version); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Document document = Document.Load(35);

            DocumentVersion version = document.CurrentVersion;
            if (version != null)
            {
                Document.LoadDocument(ref version);

                string path = @"c:\temp\blah.docx";
                System.IO.File.WriteAllBytes(path, version.Document);

                Process.Start(path);
            }
        }
    }
}

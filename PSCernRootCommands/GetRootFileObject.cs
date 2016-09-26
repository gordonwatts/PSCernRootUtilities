using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using ROOTNET.Interface;
using System.IO;

namespace PSCernRootCommands
{
    /// <summary>
    /// Fetch a clone of the object in the file. File is not kept open unless otherwise requested... and then it is kept open forever.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "RootFileObject")]
    public class GetRootFileObject : PSCmdlet
    {
        [Parameter(HelpMessage ="Path to the ROOT file", Position = 1, Mandatory = true)]
        public string ROOTFilePath { get; set; }

        [Parameter(HelpMessage = "Path inside ROOT file, include directory names, to the object", Position = 2, Mandatory = true, ValueFromPipeline = true)]
        public string ObjectPath { get; set; }

        /// <summary>
        /// Cache the opened file incase we need more than one object from it.
        /// </summary>
        private NTFile _openedFile = null;

        /// <summary>
        /// Open the file
        /// </summary>
        protected override void BeginProcessing()
        {
            _openedFile = ROOTNET.NTFile.Open(ROOTFilePath, "READ");
            if (_openedFile == null || !_openedFile.IsOpen())
            {
                WriteError(new ErrorRecord(new FileNotFoundException($"Unable to open file '{ROOTFilePath}' as a ROOT file"), "FileNotFound", ErrorCategory.InvalidData, null));
            }
            base.BeginProcessing();
        }

        protected override void EndProcessing()
        {
            if (_openedFile != null)
            {
                _openedFile.Close();
            }
            base.EndProcessing();
        }

        /// <summary>
        /// Return a object
        /// </summary>
        protected override void ProcessRecord()
        {
            // see if we can fetch the object.
            var o = _openedFile.Get(ObjectPath);
            var h = new ROOTNET.NTH1F();
            if (o == null)
            {
                WriteError(new ErrorRecord(new ArgumentException($"Unable to find ROOT object '{ObjectPath}' in file '{ROOTFilePath}'"), "ObjectNotFound", ErrorCategory.ResourceUnavailable, null));
            } else
            {
                var c = o.Clone();
                WriteObject(c);
            }
            base.ProcessRecord();
        }
    }
}

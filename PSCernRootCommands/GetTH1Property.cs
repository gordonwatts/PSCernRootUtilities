using ROOTNET.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PSCernRootCommands
{
    [Cmdlet(VerbsCommon.Get, "TH1Property")]
    public class GetTH1Property : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the ROOT file", Position = 1, Mandatory = true)]
        public string ROOTFilePath { get; set; }

        [Parameter(HelpMessage = "Name of .NET Property to fetch", Position = 2, Mandatory = true)]
        public string PropertyName { get; set; }

        [Parameter(HelpMessage = "Path inside ROOT file, include directory names, to the object", Position = 3, Mandatory = true, ValueFromPipeline = true)]
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
            if (o == null)
            {
                WriteError(new ErrorRecord(new ArgumentException($"Unable to find ROOT object '{ObjectPath}' in file '{ROOTFilePath}'"), "ObjectNotFound", ErrorCategory.ResourceUnavailable, null));
                return;
            }
            var h = o as NTH1;
            if (h == null)
            {
                WriteError(new ErrorRecord(new ArgumentException($"ROOT object '{ObjectPath}' in file '{ROOTFilePath}' is not of type NTH1"), "ObjectNotProperType", ErrorCategory.InvalidArgument, null));
                return;
            }

            // Next, see if the property makes sense.
            var p = h.GetType().GetProperty(PropertyName);
            if (p == null)
            {
                WriteError(new ErrorRecord(new ArgumentException($"ROOT NTH1 object '{ObjectPath}' in file '{ROOTFilePath}' does not have a property of type '{PropertyName}'"), "PropertyNotFound", ErrorCategory.InvalidArgument, null));
                return;
            }

            // Get get it.
            WriteObject(p.GetValue(h));

            base.ProcessRecord();
        }

    }
}

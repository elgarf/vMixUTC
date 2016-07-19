using NCrash.Core;
using NCrash.Sender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Classes
{

        public class NCrashLocalSender : ISender
        {
            FileStream filew;
            byte[] output;
            public bool Send(Stream data, string fileName, Report report)
            {

                using (filew = new FileStream(fileName, FileMode.Create))
                {
                    output = new byte[data.Length];
                    data.Read(output, 0, output.Length);
                    filew.Write(output, 0, output.Length);
                    filew.Close();
                }
                return true;
            }

        }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SelfSignCertReqTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			// https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.x509findtype(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-2
			// Loop through certs
			X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
			//X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

			X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
			X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
			// https://stackoverflow.com/questions/15617409/name-x509certificate2ui-does-not-exist-in-the-current-context
			X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "Test Certificate Select", "Select a certificate from the following list to get information on that certificate", X509SelectionFlag.MultiSelection);
			Console.WriteLine("Number of certificates: {0}{1}", scollection.Count, Environment.NewLine);

			foreach (X509Certificate2 x509 in scollection)
			{
				try
				{
					//*/
					string endPointUrl = "https://server.cryptomix.com/secure/";
					HttpWebRequest req = null;
					HttpWebResponse rsp = null;
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

					var uri = endPointUrl;
					req = (HttpWebRequest)WebRequest.Create(uri);
					req.PreAuthenticate = true;
					req.AllowAutoRedirect = true;
					req.Method = "GET";
					req.KeepAlive = false;
					req.UserAgent = null;
					req.Timeout = 99999;
					req.ReadWriteTimeout = 99999;
					req.ServicePoint.MaxIdleTime = 99999;
					req.ClientCertificates.Add(x509);
					rsp = (HttpWebResponse)req.GetResponse();
					var reader = new StreamReader(rsp.GetResponseStream());
					var retData = reader.ReadToEnd();
					Console.WriteLine(retData);
					//*/
				}
				catch (CryptographicException)
				{
					Console.WriteLine("Information could not be written out for this certificate.");
				}
			}
			store.Close();
		}
	}
}

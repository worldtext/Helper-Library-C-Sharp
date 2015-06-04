using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;


// Disclaimer
//
// World Text offer this client provided source code under a three clause BSD licence.
//
// All sample code is provided by World Text for education purposes only.
// These examples have not been thoroughly tested under all conditions.
// World Text, therefore, cannot guarantee or imply reliability, serviceability,
// or function of this code.
//


namespace WorldText.SMSInterface
{
	public class Outgoing
	{
		//default values for account and api amd default URI - can be changed by calling applications
		private static string baseURL = "https://sms.world-text.com/v2.0/";
		private static string accountNumber = "nnnnn";		// Put your account number here
		private static string apiKey = "xxxxxxxxxxxxxxxxx";	// API key here.
		
		//the default national code (if a number is input starting with 0)
		private static string defaultNatCode = "44";
		
		//Clean destination numbers remove leading + / or swap leading 0 for the national code
		private static void GetProperPhoneNumber(ref string InputNumber)
		{
			if (InputNumber.IndexOf('+') == 0)
			{
				InputNumber = InputNumber.Substring(1);
			}
			if (InputNumber.Substring(0, 1) == "0")
			{
				InputNumber = defaultNatCode + InputNumber.Substring(1);
			}
		}
		
		
		public static string AccountNumber
		{
			get
			{
				return accountNumber;
			}
			set
			{
				accountNumber = value;
			}
		}
		
		//put the credential parameters onto the URI - every call will need them
		public static void AppendCredentialParameters(ref string parameters)
		{
			if (string.IsNullOrEmpty(parameters))
				parameters += "id=" + accountNumber + "&key=" + apiKey;
			else
				parameters += "&id=" + accountNumber + "&key=" + apiKey;
		}
		
		public static string BaseURL
		{
			get
			{
				return baseURL;
			}
			set
			{
				baseURL = value;
			}
		}
		
		public static string APIKey
		{
			get
			{
				return apiKey;
			}
			set
			{
				apiKey = value;
			}
		}
		
		
		//all calls to the service use these 4 functions makes the calls easier
		public interface Request
		{
			string ResourceURI { get; }
			string Params { get; }
			string Method { get; }
			bool Simulation { get; }
		}
		
		//set of classes holding the structure of the calls for individual SMS messages
		public class SMS
		{
			
			public class SMSMessage : Request
			{
				private string text;
				private string destination;
				private string sourceSystemRef;
				private string sourceNumber;
				private bool allowMultipart = false;
				private string resourceURI = "sms/send";
				private bool simulation = false;
				
				
				public bool Simulation
				{
					get
					{
						return simulation;
					}
					set
					{
						simulation = value;
					}
				}
				
				public string ResourceURI
				{
					get
					{
						return resourceURI;
					}
				}
				
				public bool AllowMultipart
				{
					get { return allowMultipart; }
					set { allowMultipart = value; }
				}
				public string Destination
				{
					get { return destination; }
					set
					{
						GetProperPhoneNumber(ref value);
						destination = value;
					}
				}
				public string Text
				{
					get { return text; }
					set { text = value; }
				}
				private int MultipartAmount
				{
					get
					{
						var textLength = text.Length;
						if (textLength < 160)
							return 1;
						
						//each additional holds 146 chars
						var addLen = 146;
						
						textLength = textLength - 160;
						
						var Add = 1;
						
						do
						{
							Add++;
						} while ((addLen * (Add - 1)) < textLength);
						
						return Add;
					}
				}
				
				public string Params
				{
					get
					{
						var postingInfo = "txt=" + Uri.EscapeDataString (Text);
						postingInfo += "&dstaddr=" + Destination;
						if (AllowMultipart & MultipartAmount > 1)
							postingInfo += "&multipart=" + MultipartAmount.ToString();
						if (!string.IsNullOrEmpty(sourceSystemRef))
							postingInfo += "&clientref=" + sourceSystemRef;
						if (!string.IsNullOrEmpty(sourceNumber))
							postingInfo += "&srcaddr=" + sourceNumber;
						Outgoing.AppendCredentialParameters(ref postingInfo);
						return postingInfo;
					}
				}
				
				public string Method
				{
					get
					{ return "PUT"; }
				}
				
				public string SourceSystemRef
				{
					get
					{
						return sourceSystemRef;
					}
					set
					{
						sourceSystemRef = value;
					}
				}
				
				public string SourceNumber
				{
					get
					{
						return sourceNumber;
					}
					set
					{
						sourceNumber = value;
					}
				}
				
				public string Send()
				{
					return HttpHost.MakeRequest(this);
				}
				
			}
			
			public class Cost : Request
			{
				private string dstaddr;
				private bool simulation = false;
				
				public string Destination
				{
					get { return dstaddr; }
					set
					{
						GetProperPhoneNumber(ref value);
						dstaddr = value;
					}
				}
				
				#region Request Members
				
				public bool Simulation
				{
					get
					{
						return simulation;
					}
					set
					{
						simulation = value;
					}
				}
				
				public string ResourceURI
				{
					get { return "sms/cost"; }
				}
				
				public string Params
				{
					get
					{
						string par = "dstaddr=" + dstaddr;
						Outgoing.AppendCredentialParameters(ref par);
						return par;
					}
				}
				
				public string Method
				{
					get { return "GET"; }
				}
				
				#endregion
				
				public string GetCost()
				{
					return HttpHost.MakeRequest(this);
				}
			}
			
			public class SMSStatusQuery : Request
			{
				
				private bool simulation = false;
				private string messageID;
				
				public string MessageID
				{
					get { return messageID; }
					set { messageID = value; }
				}
				
				public string SendQuery()
				{
					return HttpHost.MakeRequest(this);
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "sms/query"; }
				}
				
				public string Params
				{
					get
					{
						string para = "msgid=" + messageID;
						Outgoing.AppendCredentialParameters(ref para);
						return para;
					}
				}
				
				public string Method
				{
					get { return "GET"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
		}
		
		//set of classes holding the structure of calls regarding groups
		public class Group
		{
			public class ClearContants : Request
			{
				private bool simulation = false;
				private string grpid;
				
				public string GroupID
				{
					get { return grpid; }
					set { grpid = value; }
				}
				
				public string ClearContents()
				{
					return HttpHost.MakeRequest(this);
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/contents"; }
				}
				
				public string Params
				{
					get
					{
						string par = "grpid=" + grpid;
						Outgoing.AppendCredentialParameters(ref par);
						return par;
					}
				}
				
				public string Method
				{
					get { return "DELETE"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
			public class Create : Request
			{
				private string srcAddr, name;
				private string pin = "0000";
				private bool simulation = false;
				
				public string SourceNumber
				{
					get { return srcAddr; }
					set { srcAddr = value; }
				}
				
				public string Name
				{
					get { return name; }
					set { name = value; }
				}
				
				public string Pin
				{
					get { return pin; }
					set { pin = value; }
				}
				
				public string CreateGroup()
				{
					return HttpHost.MakeRequest(this);
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/create"; }
				}
				
				public string Params
				{
					get
					{
						string param = "srcaddr=" + srcAddr + "&name=" + name + "&pin=" + pin;
						Outgoing.AppendCredentialParameters(ref param);
						return param;
					}
				}
				
				public string Method
				{
					get { return "PUT"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
			public class Cost : Request
			{
				
				private string grpid;
				private bool simulation = false;
				
				
				public string GroupID
				{
					get { return grpid; }
					set { grpid = value; }
				}
				
				public string GetCost()
				{
					return HttpHost.MakeRequest(this);
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/cost"; }
				}
				
				public string Params
				{
					get
					{
						string para = "grpid=" + grpid;
						Outgoing.AppendCredentialParameters(ref para);
						return para;
					}
				}
				
				public string Method
				{
					get { return "GET"; }
				}
				
				public bool Simulation
				{
					get
					{
						return simulation;
					}
					set
					{
						simulation = value;
					}
				}
				
				#endregion
			}
			
			public class Destroy : Request
			{
				private string grpid;
				private bool simulation = false;
				
				public string GroupID
				{
					get { return grpid; }
					set { grpid = value; }
				}
				
				public string DestroyGroup()
				{
					return HttpHost.MakeRequest(this);
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/destroy"; }
				}
				
				public string Params
				{
					get
					{
						string param = "grpid=" + grpid;
						Outgoing.AppendCredentialParameters(ref param);
						return param;
					}
				}
				
				public string Method
				{
					get { return "DELETE"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
			public class Details : Request
			{
				private string grpid;
				private bool simulation = false;
				
				public string GetDetails()
				{
					return HttpHost.MakeRequest(this);
				}
				
				public string GroupID
				{
					get { return grpid; }
					set { grpid = value; }
				}
				
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/details"; }
				}
				
				public string Params
				{
					get
					{
						string param = "grpid=" + grpid;
						Outgoing.AppendCredentialParameters(ref param);
						return param;
					}
				}
				
				public string Method
				{
					get { return "GET"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
			public class Entries : Request
			{
				private struct Member
				{
					private string name, number;
					
					public Member(string Name, string Number)
					{
						name = Name;
						number = Number;
					}
					
					public string Name
					{
						get { return name; }
						set { name = value; }
					}
					
					public string Number
					{
						get { return number; }
						set { number = value; }
					}
					
					
				}
				
				private bool simulation = false;
				private List<Member> members = new List<Member>();
				private string grpid;
				
				public string GroupID
				{
					get { return grpid; }
					set { grpid = value; }
				}
				
				private string getMemberParam()
				{
					if (members != null)
					{
						var mems = from Member m in members select m;
						string memParam = string.Empty;
						foreach (Member m in mems)
						{
							memParam += "," + m.Number + "," + m.Name;
						}
						
						if (memParam.Length > 0)
							memParam = memParam.TrimStart(new char[] { ',' });
						
						return memParam;
					}
					else
						return string.Empty;
				}
				
				public void AddMember(string Name, string PhoneNumber)
				{
					GetProperPhoneNumber(ref PhoneNumber);
					int exists = (from Member m in members where m.Number == PhoneNumber select m).Count();
					if (exists == 0)
					{
						Member m = new Member(Name, PhoneNumber);
						members.Add(m);
					}
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/entries"; }
				}
				
				public string Params
				{
					get
					{
						string param = "grpid=" + grpid + "&members=" + getMemberParam();
						Outgoing.AppendCredentialParameters(ref param);
						return param;
					}
				}
				
				public string Method
				{
					get { return "PUT"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
			public class Entry : Request
			{
				private bool simulation = false;
				private string grpid, dstaddr, name;
				
				public string GroupID
				{
					get { return grpid; }
					set { grpid = value; }
				}
				
				public string Number
				{
					get { return dstaddr; }
					set { dstaddr = value; }
				}
				
				public string Name
				{
					get { return name; }
					set { name = value; }
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/entry"; }
				}
				
				public string Params
				{
					get
					{
						string p = string.Empty;
						Outgoing.AppendCredentialParameters(ref p);
						p += "&grpid=" + grpid + "&dstaddr=" + dstaddr + "&name=" + name;
						return p;
					}
				}
				
				public string Method
				{
					get { return "PUT"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
			public class List : Request
			{
				private bool simulation = false;
				
				public string ListGroup()
				{
					return HttpHost.MakeRequest(this);
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/list"; }
				}
				
				public string Params
				{
					get
					{
						string p = string.Empty;
						Outgoing.AppendCredentialParameters(ref p);
						return p;
					}
				}
				
				public string Method
				{
					get { return "GET"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
			public class Numbers : Request
			{
				private string grpid;
				private bool simulation = false;
				
				public string GroupID
				{
					get { return grpid; }
					set { grpid = value; }
				}
				
				public string ListNumbers()
				{
					return HttpHost.MakeRequest(this);
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/members"; }
				}
				
				public string Params
				{
					get
					{
						string param = "grpid=" + grpid;
						Outgoing.AppendCredentialParameters(ref param);
						return param;
					}
				}
				
				public string Method
				{
					get { return "GET"; }
				}
				
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
			public class Send : Request
			{
				private string grpid, txt;
				private string srcaddr = string.Empty;
				private bool allowMultipart = false;
				private bool simulation = false;
				
				public string Text
				{
					get { return txt; }
					set { txt = value; }
				}
				
				public string GroupID
				{
					get { return grpid; }
					set { grpid = value; }
				}
				
				public string SourceAddress
				{
					get { return srcaddr; }
					set { srcaddr = value; }
				}
				
				public bool AllowMultipart
				{
					get { return allowMultipart; }
					set { allowMultipart = value; }
				}
				
				public string SendMessage()
				{
					return HttpHost.MakeRequest(this);
				}
				
				#region Request Members
				
				public string ResourceURI
				{
					get { return "group/send"; }
				}
				public string Params
				{
					get
					{
						string param = "grpid=" + grpid + "&txt=" + txt;
						if (allowMultipart && MultipartAmount > 1)
							param += "&multipart=" + MultipartAmount.ToString();
						if (!string.IsNullOrEmpty(srcaddr))
							param += "&srcaddr=" + srcaddr;
						
						Outgoing.AppendCredentialParameters(ref param);
						
						return param;
					}
				}

				// MRH: Incomplete def of msg len...
				private int MultipartAmount
				{
					get
					{
						var textLength = txt.Length;
						if (textLength < 160)
							return 1;
						
						//each additional holds 146 chars
						var addLen = 146;
						
						textLength = textLength - 160;
						
						var Add = 1;
						
						do
						{
							Add++;
						} while ((addLen * (Add - 1)) < textLength);
						
						return Add;
					}
				}
				public string Method
				{
					get { return "PUT"; }
				}
				public bool Simulation
				{
					get { return simulation; }
					set { simulation = value; }
				}
				
				#endregion
			}
			
		}
		
		//All messages go through here using the request interface
		private class HttpHost
		{
			
			public static string MakeRequest(Request r)
			{
				//set up the http request object
				var fullRequest = Outgoing.baseURL + r.ResourceURI;
				var meth = r.Method;
				var parameters = r.Params;
				if (r.Simulation)
				{
					parameters += "&sim";
				}
				HttpWebRequest req = null;
				fullRequest += "?" + parameters;
				req = (HttpWebRequest)HttpWebRequest.Create(fullRequest);
				req.AllowAutoRedirect = true;
				req.Method = meth;
				
				//if outgoing comms requires a proxy call this and set the up in function below as needed
//				SetProxy(ref req);

				Console.WriteLine (req.Address);
				Console.WriteLine ("Call method:");
				Console.WriteLine (meth);


				
				try
				{
					//get the response 
					var resp = req.GetResponse() as HttpWebResponse;
					
					//if the responding host is not the host we asked - update the main server so all remaining calls using this object go to the responding server
					//.net will automatically follow the temp redirect so this will have been done if needed
					if (resp.ResponseUri.Host.ToLower() != req.RequestUri.Host.ToLower())
					{
						var respStr = resp.ResponseUri.ToString();
						if (respStr.IndexOf("/") > -1)
						{
							respStr = respStr.Substring(0, respStr.LastIndexOf("/"));
						}
						Outgoing.BaseURL = respStr;
					}
					using (resp)
					{
						StreamReader read = new StreamReader(resp.GetResponseStream());
						string rv = read.ReadToEnd();
						return rv;
					}
				}
				catch (WebException ex)
				{
					//an http error has occured
					HttpWebResponse resp = (HttpWebResponse)ex.Response;
					//a slow down has been sent - pause for 20 seconds and try again
					if (resp.StatusCode.Equals("429"))
					{
						System.Threading.Thread.Sleep(1000 * 20);
						return MakeRequest(r);
					}
					//another error type - read the response and return to calling application
					StreamReader rdr = new StreamReader(ex.Response.GetResponseStream());
					string rv = rdr.ReadToEnd();
					return rv;
				}
			}
			
			
			private static void SetProxy(ref HttpWebRequest req)
			{
				//this default impleemtation uses a proxy server which uses windows process credentials to autherise
				//will need to be set based upon specific network circumstances
				req.Proxy = new System.Net.WebProxy("proxyservername", true);
				req.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
			}
			
			
		}
		
	}
}

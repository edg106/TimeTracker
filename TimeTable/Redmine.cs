using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using Version = Redmine.Net.Api.Types.Version;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;


namespace TimeTable
{
      /// <summary>
      /// 
      /// </summary>
      public class OwnRedmineManager
      {
         private const string REQUESTFORMAT = "{0}/{1}/{2}.xml";
         private const string FORMAT = "{0}/{1}.xml";
         private const string CURRENT_USER_URI = "current";
         private readonly string host, apiKey, basicAuthorization;
         private readonly CredentialCache cache;

         private readonly Dictionary<Type, String> urls = new Dictionary<Type, string>
                                                             {
                                                                {typeof (Issue), "issues"},
                                                                {typeof (Project), "projects"},
                                                                {typeof (User), "users"},
                                                                {typeof (News), "news"},
                                                                {typeof (Query), "queries"},
                                                                {typeof (Version), "versions"},
                                                                {typeof (Attachment), "attachments"},
                                                                {typeof (IssueRelation), "relations"},
                                                                {typeof (TimeEntry), "time_entries"},
                                                                {typeof (IssueStatus), "issue_statuses"},
                                                                {typeof (Tracker), "trackers"},
                                                                {typeof (IssueCategory), "issue_categories"},
                                                                {typeof (Role), "roles"},
                                                                {typeof (ProjectMembership), "memberships"}
                                                             };

         /// <summary>
         /// Initializes a new instance of the <see cref="RedmineManager"/> class.
         /// </summary>
         /// <param name="host">The host.</param>
         /// <param name="verifyServerCert">if set to <c>true</c> [verify server cert].</param>
         public OwnRedmineManager(string host, bool verifyServerCert = true)
         {
            this.host = host;

            if (!verifyServerCert)
            {
               ServicePointManager.ServerCertificateValidationCallback += RemoteCertValidate;
            }
         }

         /// <summary>
         /// Initializes a new instance of the <see cref="RedmineManager"/> class.
         /// </summary>
         /// <param name="host">The host.</param>
         /// <param name="apiKey">The API key.</param>
         /// <param name="verifyServerCert">if set to <c>true</c> [verify server cert].</param>
         public OwnRedmineManager(string host, string apiKey, bool verifyServerCert = true)
            : this(host, verifyServerCert)
         {
            this.apiKey = apiKey;
         }

         /// <summary>
         /// Initializes a new instance of the <see cref="RedmineManager"/> class.
         /// </summary>
         /// <param name="host">The host.</param>
         /// <param name="login">The login.</param>
         /// <param name="password">The password.</param>
         /// <param name="verifyServerCert">if set to <c>true</c> [verify server cert].</param>
         public OwnRedmineManager(string host, string login, string password, bool verifyServerCert = true)
            : this(host, verifyServerCert)
         {
            cache = new CredentialCache {{new Uri(host), "Basic", new NetworkCredential(login, password)}};
            basicAuthorization = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(login + ":" + password));
         }

         /// <summary>
         /// Returns the user whose credentials are used to access the API.
         /// </summary>
         /// <param name="parameters">The parameters.</param>
         /// <returns></returns>
         public User GetCurrentUser(NameValueCollection parameters = null)
         {
            using (var wc = CreateWebClient(parameters))
            {
               var xml = wc.DownloadString(string.Format(REQUESTFORMAT, host, urls[typeof (User)], CURRENT_USER_URI));
               return Deserialize<User>(xml);
            }
         }

         /// <summary>
         /// Gets the object list.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="parameters">The parameters.</param>
         /// <returns></returns>
         public IList<T> GetObjectList<T>(NameValueCollection parameters) where T : class
         {
            if (!urls.ContainsKey(typeof (T))) return null;

            using (var wc = CreateWebClient(parameters))
            {
               var type = typeof (T);
               string result;

               if (type == typeof (Version) || type == typeof (IssueCategory) || type == typeof (ProjectMembership))
               {
                  string projectId = null;
                  if (parameters != null)
                     projectId = parameters.Get("project_id");

                  result = wc.DownloadString(string.Format("{0}/projects/{1}/{2}.xml", host, projectId, urls[type]));
               }
               else
               {
                  result = wc.DownloadString(string.Format(FORMAT, host, urls[type]));
               }

               using (var text = new StringReader(result))
               {
                  using (var xmlReader = new XmlTextReader(text))
                  {
                     xmlReader.Read();
                     xmlReader.Read();
                     return xmlReader.ReadElementContentAsCollection<T>();
                  }
               }
            }
         }

         /// <summary>
         /// Gets the object list.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="parameters">The parameters.</param>
         /// <param name="totalCount">The total count.</param>
         /// <returns></returns>
         public IList<T> GetObjectList<T>(NameValueCollection parameters, out int totalCount) where T : class
         {
            totalCount = -1;
            if (!urls.ContainsKey(typeof(T))) return null;

            using (var wc = CreateWebClient(parameters))
            {
               var xml = wc.DownloadString(string.Format(FORMAT, host, urls[typeof(T)]));
               using (var text = new StringReader(xml))
               {
                  using (var xmlReader = new XmlTextReader(text))
                  {
                     xmlReader.Read();
                     xmlReader.Read();

                     totalCount = xmlReader.ReadAttributeAsInt("total_count");

                     return xmlReader.ReadElementContentAsCollection<T>();
                  }
               }
            }
         }

         /// <summary>
         /// Gets the object.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="id">The id.</param>
         /// <param name="parameters">The parameters.</param>
         /// <returns></returns>
         public T GetObject<T>(string id, NameValueCollection parameters) where T : class
         {
            if (!urls.ContainsKey(typeof(T))) return null;

            using (var wc = CreateWebClient(parameters))
            {
               var xml = wc.DownloadString(string.Format(REQUESTFORMAT, host, urls[typeof(T)], id));
               return Deserialize<T>(xml);
            }
         }

         /// <summary>
         /// Gets the object.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="id">The id.</param>
         /// <param name="parameters">The parameters.</param>
         /// <returns></returns>
         public T GetReport<T>() where T : class
         {
            if (!urls.ContainsKey(typeof(T))) return null;

            using (var wc = CreateWebClient(null))
            {
               try
               {
                  var xml = wc.DownloadString(string.Format(REQUESTFORMAT, host, urls[typeof(T)], @"report?key="+apiKey+"criterias%5B%5D=member&period_type=2&from=2012-03-19&to=2012-04-01&columns=day&criterias%5B%5D="));
                  return Deserialize<T>(xml);
               }
               catch (Exception e)
               {
                  Console.WriteLine(e.Data);
                  throw;
               }
            }
         }

         /// <summary>
         /// Creates the object.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="obj">The obj.</param>
         public string CreateObject<T>(T obj) where T : class
         {
            if (!urls.ContainsKey(typeof (T))) return "Fehler1";

            var xml = Serialize(obj);

            if (string.IsNullOrEmpty(xml)) return "Fehler2";

            using (var wc = CreateWebClient(null))
            {
               return wc.UploadString(string.Format(FORMAT, host, urls[typeof (T)]), xml);
            }
         }

         ///// <summary>
         ///// Upload data on server.
         ///// </summary>
         ///// <param name="data">Data which will be uploaded on server</param>
         ///// <returns>Returns 'Upload' object with inialized 'Token' by server response.</returns>
         //public Upload UploadData(byte[] data)
         //{
         //   using (WebClient wc = new WebClient())
         //   {
         //      wc.UseDefaultCredentials = false;

         //      wc.Headers.Add("Content-Type", "application/octet-stream");
         //      // Workaround - it seems that WebClient doesn't send credentials in each POST request
         //      wc.Headers.Add("Authorization", basicAuthorization);

         //      byte[] response = wc.UploadData(string.Format(FORMAT, host, "uploads"), data);

         //      string responseString = Encoding.ASCII.GetString(response);

         //      return Deserialize<Upload>(responseString);
         //   }
         //}

         /// <summary>
         /// Updates the object.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="id">The id.</param>
         /// <param name="obj">The obj.</param>
         public string UpdateObject<T>(string id, T obj) where T : class
         {
            if (!urls.ContainsKey(typeof (T))) return "Fehler3";

            var xml = Serialize(obj);

            if (string.IsNullOrEmpty(xml)) return "Fehler4";

            using (var wc = CreateWebClient(null))
            {
                return  wc.UploadString(string.Format(REQUESTFORMAT, host, urls[typeof (T)], id), "PUT", xml);
            }
         }

         /// <summary>
         /// Deletes the object.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="id">The id.</param>
         /// <param name="parameters">The parameters.</param>
         public void DeleteObject<T>(string id, NameValueCollection parameters) where T : class
         {
            if (!urls.ContainsKey(typeof (T))) return;

            using (var wc = CreateWebClient(parameters))
            {
               wc.UploadString(string.Format(REQUESTFORMAT, host, urls[typeof (T)], id), "DELETE", string.Empty);
            }
         }

         /// <summary>
         /// Creates the web client.
         /// </summary>
         /// <param name="parameters">The parameters.</param>
         /// <returns></returns>
         protected WebClient CreateWebClient(NameValueCollection parameters)
         {
            var webClient = new WebClient();

            if (parameters != null)
               webClient.QueryString = parameters;

            if (!string.IsNullOrEmpty(apiKey))
            {
               webClient.QueryString["key"] = apiKey;
            }
            else if (cache != null)
            {
               webClient.Credentials = cache;
            }

            webClient.Headers.Add("Content-Type", "text/xml; charset=utf-8");
            webClient.Encoding = Encoding.UTF8;
            return webClient;
         }

         //This is to take care of SSL certification validation which are not issued by Trusted Root CA. Recommended for testing  only.
         protected bool RemoteCertValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
         {
            //Cert Validation Logic
            return true;
         }

         /// <summary>
         /// Serializes the specified obj.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="obj">The obj.</param>
         /// <returns></returns>
         protected static string Serialize<T>(T obj) where T : class
         {
            var xws = new XmlWriterSettings {OmitXmlDeclaration = true};

            using (var stringWriter = new StringWriter())
            {
               using (var xmlWriter = XmlWriter.Create(stringWriter, xws))
               {
                  var sr = new XmlSerializer(typeof (T));
                  sr.Serialize(xmlWriter, obj);
                  return stringWriter.ToString();
               }
            }
         }

         /// <summary>
         /// Deserializes the specified XML.
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="xml">The XML.</param>
         /// <returns></returns>
         public static T Deserialize<T>(string xml) where T : class
         {
            using (var text = new StringReader(xml))
            {
               var sr = new XmlSerializer(typeof (T));
               return sr.Deserialize(text) as T;
            }
         }

         public Stream GetStream<T>(NameValueCollection parameters)
         {
            if (!urls.ContainsKey(typeof(T))) return null;

            using (var wc = CreateWebClient(parameters))
            {
               var xml = wc.DownloadString(string.Format(FORMAT, host, urls[typeof(T)]));
               using (var text = new StringReader(xml))
               {
                 return new MemoryStream(text.Read());
               }
            }
         }
      }
   }
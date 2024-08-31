using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using EBILLSPAY_V2.Models;
using System.ServiceModel;

/// <summary>
/// Summary description for Authentication
/// </summary>
/// 
namespace EBILLSPAY_V2
{

 
    public class Authentication
    {

        private readonly IConfiguration _configuration;
        public Authentication(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<UserInfo> Authenticate(UserInfo sValue, String uid, String upass, Boolean isEncrypt, String ucode)
        {

            AppDevService.AppDevServiceSoapClient client = new AppDevService.AppDevServiceSoapClient(new BasicHttpBinding(BasicHttpSecurityMode.Transport), new EndpointAddress(new Uri(_configuration.GetConnectionString("AppDevService"))));
            
            
            String eoneRetVal = null;
            string userdetails = null;


            XmlDocument document = null;
            XPathNavigator navigator = null;
            XPathNodeIterator snodes = null;
            XPathNodeIterator sDetails = null;
            String retcode = null;
            int appid = 0;

            appid = Convert.ToInt32(_configuration.GetConnectionString("ApplicationID"));

            if (isEncrypt == true)
            {
                eoneRetVal = await client.ValidateEncryptedAdminUserAsync(uid, upass, ucode, appid);
            }

            else
                eoneRetVal = await client.ValidateAdminUserOffSiteAsync
                    (uid, upass, appid);


            document = new XmlDocument();
            document.LoadXml(eoneRetVal);
            navigator = document.CreateNavigator();

            snodes = navigator.Select("/Response/CODE");
            snodes.MoveNext();
            retcode = snodes.Current.Value;

            if (retcode != "1000")
            {
               
                sDetails = navigator.Select("/Response/Error");
                sDetails.MoveNext();
                retcode = sDetails.Current.Value;

                sValue = null;
                return sValue;
            }
            DataSet dsMenus = new DataSet();
            StringReader dr = new StringReader(eoneRetVal);
            dsMenus.ReadXml(dr);

            snodes = navigator.Select("/Response/USER/ID");
            snodes.MoveNext();
            sValue.userId = snodes.Current.Value;


            snodes = navigator.Select("/Response/USER/DOMAINID");
            snodes.MoveNext();
         

            snodes = navigator.Select("/Response/USER/NAME");
            snodes.MoveNext();
            sValue.muserName = snodes.Current.Value;

            snodes = navigator.Select("/Response/USER/BRANCH");
            snodes.MoveNext();
            sValue.mbranchCode = snodes.Current.Value;

            snodes = navigator.Select("/Response/USER/EMAIL");
            snodes.MoveNext();
            sValue.muserEmail = snodes.Current.Value;

            snodes = navigator.Select("/Response/USER/LASTLOGINDATE");
            snodes.MoveNext();
            sValue.lastLoginDate = snodes.Current.Value;

            snodes = navigator.Select("/Response/ROLE/RID");
            snodes.MoveNext();
            sValue.mroleId = snodes.Current.Value;

            return sValue;
        }


    }

}

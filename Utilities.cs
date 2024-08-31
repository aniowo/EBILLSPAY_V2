using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Oracle.ManagedDataAccess.Client;


namespace EBILLSPAY_V2
{
    public class Utilities
    {
		private readonly ILogger<Utilities> _logger;
		private readonly IConfiguration _configuration;
		private int keySize = 1024;
	

		public Utilities(ILogger<Utilities> logger, IConfiguration configuration)
        {

			_logger = logger;
			_configuration = configuration;
		}
		public string GetXMLEncryptionKey(string channel)
		{
			_logger.LogInformation("About to retrieve PublicKey using GetPublicKey");
			SqlDataReader reader;
			SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Eone"));
			string fieldSet = string.Empty;

			SqlCommand comm = new SqlCommand("proc_getPublicKeyXML", conn);
			comm.Parameters.AddWithValue("@channel", channel);
			comm.CommandType = CommandType.StoredProcedure;
			//open connetion
			try
			{
				if (conn.State != ConnectionState.Open)
				{
					conn.Open();
				}
				reader = comm.ExecuteReader();

				if (reader.HasRows)
				{
					reader.Read();
					fieldSet = reader.GetString(0).Trim();
				}
				reader.Close();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error Encountered: ");
			}
			finally
			{
				conn.Close();
			}
			return fieldSet;
		}

		public string Encryption(string textToBeEncrypted, string publicEncryptionKey)
		{

			var testString = Encoding.UTF8.GetBytes(textToBeEncrypted);

			using (var rsa = new RSACryptoServiceProvider(keySize))
			{
				try
				{
					if (!string.IsNullOrEmpty(publicEncryptionKey.Trim()))
					{
						rsa.FromXmlString(publicEncryptionKey);
						var encryptedData = rsa.Encrypt(testString, false);
						var base64EncryptedString = Convert.ToBase64String(encryptedData);
						return base64EncryptedString;
					}
					else
					{
						return string.Empty;
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error with Encryption method ");
					return string.Empty;
				}
				finally
				{
					rsa.PersistKeyInCsp = false;
				}
			}

		}

		public string GetUserId(string nuban)
		{
			using (OracleConnection OraConn = new OracleConnection(_configuration.GetConnectionString("BasisConnectionString")))
			{

				using (OracleCommand OraSelect = new OracleCommand())
				{
					OracleDataReader OraDrSelect;
					string bra_code = null;
					string cus_num = null;

					try
					{
						if (OraConn.State == ConnectionState.Closed)
						{
							OraConn.Open();
						}
						OraSelect.Connection = OraConn;
						string selectquery = "SELECT * FROM map_acct WHERE MAP_ACC_NO = '" + nuban + "'";
						OraSelect.CommandText = selectquery;
						OraSelect.CommandType = CommandType.Text;
						using (OraDrSelect = OraSelect.ExecuteReader(CommandBehavior.CloseConnection))
						{
							if (OraDrSelect.HasRows == true)
							{
								OraDrSelect.Read();
								bra_code = OraDrSelect["BRA_CODE"].ToString().Trim();
								cus_num = OraDrSelect["CUS_NUM"].ToString().Trim();

								return bra_code + cus_num;
							}
							else
							{
								return "-2";
							}
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "GetUserId");
						return "-1";
					}
					finally
					{
						if (OraConn.State == ConnectionState.Open)
						{
							OraConn.Close();
						}
						OraConn.Dispose();
					}
				}
			}

		}

		public string GetCustNameUserId(string userId)
		{
			string phoneNum = string.Empty;
			string email = string.Empty;
			string bra_code = userId.Substring(0, 3).Trim();
			string cus_num = userId.Substring(3, 6).Trim();
			string custName = string.Empty;
			OracleCommand cmd;
			try
			{
				using (OracleConnection OraConn = new OracleConnection(_configuration.GetConnectionString("BasisConnectionString")))
				{
					using (OracleCommand OraSelect = new OracleCommand())
					{
						OracleDataReader OraDrSelect;

						try
						{
							if (OraConn.State == ConnectionState.Closed)
							{
								OraConn.Open();
							}
							OraSelect.Connection = OraConn;

							string selectquery = @"SELECT CUS_SHO_NAME FROM SEC_ADD WHERE BRA_CODE = " + bra_code + " AND CUS_NUM = " + cus_num;
							OraSelect.CommandText = selectquery;
							cmd = new OracleCommand(selectquery, OraConn);


							OraSelect.CommandType = CommandType.Text;


							using (OraDrSelect = OraSelect.ExecuteReader(CommandBehavior.CloseConnection))
							{
								if (OraDrSelect.HasRows == true)
								{
									OraDrSelect.Read();
									custName = Convert.ToString(OraDrSelect["CUS_SHO_NAME"].ToString());
								}
								else
								{
									custName = "";
								}
							}

						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Could not get customer name ");
							custName = "";
						}
						finally
						{
							if (OraConn.State == ConnectionState.Open)
							{
								OraConn.Close();
							}
							OraConn.Dispose();
						}
					}
				}

			}
			catch (Exception ex)
			{

				_logger.LogError(ex, "Error getting customer Name: ");
			}
			return custName;
		}

        public bool hasSpecialChar(string input)
        {
            string specialChar = @"\|!#$%&/()=?»«£§€{};'<>_,";
            foreach (var item in specialChar)
            {
                if (input.Contains(item)) 
				return true;
            }

            return false;
        }



    }
}

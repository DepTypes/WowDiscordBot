using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using SshNet.Security.Cryptography;
using System.Linq;
using System.Numerics;
using System.Globalization;

namespace WowBot
{
    class AccountService
    {
        private MySqlConnection connection = null;

        private static readonly AccountService instance = new AccountService();

        public static AccountService Instance
        {
            get
            {
                instance.CheckMysqlConnection();
                return instance;
            }
        }

        private AccountService()
        {
            ConnectMysql();
        }

        ~AccountService()
        {
            try
            {
                connection.Close();
            }
            catch (Exception e)
            {
                Log.Error("SQL", e.Message);
            }
        }

        private void ConnectMysql()
        {
            try
            {
                string connectionString = $"server={Config.HOSTNAME};port={Config.PORT};userid={Config.DB_USER};password={Config.DB_PASS};";
                connection = new MySqlConnection(connectionString);
                connection.Open();

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    Log.Debug("SQL", $"Connected. MySQL ver: {connection.ServerVersion}");
                }
                else // problem
                {
                    Log.Error("SQL", "System.Data." + connection.State);
                }
            }
            catch (Exception e)
            {
                Log.Error("SQL", e.Message);
            }
        }

        private void CheckMysqlConnection()
        {
            if (connection.State != System.Data.ConnectionState.Open)
                ConnectMysql();
        }

        private int GetGMLevel(string username)
        {
            string query = $"SELECT gmlevel FROM {Config.AUTH_DB}.account_access INNER JOIN account USING (id) WHERE username='{username}';";
            Log.Info("SQL", query);

            try
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                int result = Convert.ToInt32(command.ExecuteScalar());

                return result;
            }
            catch (Exception e)
            {
                Log.Error("SQL", e.Message);
            }

            return -1;
        }

        public int SetGMLevel(string username, int level)
        {
            int lastLevel = GetGMLevel(username);

            if (lastLevel > 3)
                return 1;

            string query = $"INSERT INTO {Config.AUTH_DB}.account_access (id, gmlevel) VALUES ((SELECT id FROM account WHERE username='{username}'), 3) ON DUPLICATE KEY UPDATE gmlevel=3;";

            Log.Info("SQL", query);
            try
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                int numAffectedRows = command.ExecuteNonQuery();

                Log.Info("SQL", $"Affected rows: {numAffectedRows}");
            }
            catch (Exception e)
            {
                Log.Error("SQL", e.Message);
                return -1;
            }

            return 0;
        }

        public bool CreateAccount(string username, string password)
        {
            username = username.ToUpper();
            byte[] salt = GetRandomSalt();
            byte[] verifier = CalculateHash(username, password, salt);

            Log.Info("SQL", "Inserting account");

            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"INSERT INTO {Config.AUTH_DB}.account (username, salt, verifier, joindate) VALUES ('{username}', @salt_sql, @verifier_sql, NOW());", connection))
                {
                    cmd.Parameters.Add("@salt_sql", MySqlDbType.Blob).Value = salt;
                    cmd.Parameters.Add("@verifier_sql", MySqlDbType.Blob).Value = verifier;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Log.Error("SQL", e.Message);
                return false;
            }

            return true;
        }

        public int GetOnlineAccounts()
        {
            string query = $"SELECT COUNT(id) AS total FROM {Config.AUTH_DB}.account WHERE online=1;";
            Log.Info("SQL", query);
            try
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                int numOnline = Convert.ToInt32(command.ExecuteScalar());

                Log.Info("SQL", $"Query result: {numOnline}");
                return numOnline;
            }
            catch (Exception e)
            {
                Log.Error("SQL", e.Message);
                return -1;
            }
        }

        public WowCharacter[] GetOnlineCharacters()
        {
            string query = $"SELECT name, race, class, gender, level, map FROM {Config.CHAR_DB}.characters WHERE ONLINE='1';";
            Log.Info("SQL", query);
            try
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                List<WowCharacter> charList = new List<WowCharacter>();

                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    int race = reader.GetInt32(1);
                    int wowClass = reader.GetInt32(2);
                    int gender = reader.GetInt32(3);
                    int level = reader.GetInt32(4);
                    int mapId = reader.GetInt32(5);

                    charList.Add(new WowCharacter(name, race, wowClass, gender, level, mapId));
                }

                reader.Close();

                WowCharacter[] wowCharacters = charList.ToArray();

                Log.Info("SQL", $"Who returned {wowCharacters.Length} online characters");
                return wowCharacters;
            }
            catch (Exception e)
            {
                Log.Error("SQL", e.Message);
                return null;
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static byte[] GetRandomSalt()
        {
            Random rnd = new Random();
            byte[] bytes = new byte[32];
            rnd.NextBytes(bytes);

            return bytes;
        }

        public static byte[] CalculateHash(string username, string password, byte[] salt)
        {
            // SHA 1
            string passString = username.ToUpper() + ":" + password.ToUpper();
            byte[] passBytes = Encoding.ASCII.GetBytes(passString);
            SHA1 sha1 = new SHA1();
            sha1.Initialize();
            byte[] h1 = sha1.ComputeHash(passBytes);

            // Compute salt + h1
            byte[] secondPassBytes = salt.Concat(h1).ToArray();
            SHA1 sha1_2 = new SHA1();
            sha1_2.Initialize();
            byte[] h2Bytes = sha1_2.ComputeHash(secondPassBytes);

            BigInteger h2 = new BigInteger(h2Bytes, isUnsigned: true);

            BigInteger g = 7;
            BigInteger N = BigInteger.Parse("0894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.AllowHexSpecifier);
            BigInteger GtPModuloN = BigInteger.ModPow(g, h2, N);

            byte[] finalBytes = GtPModuloN.ToByteArray();
            return finalBytes;
        }
    }
}

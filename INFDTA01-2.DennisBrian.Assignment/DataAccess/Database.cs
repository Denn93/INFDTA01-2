using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace INFDTA01_2.DennisBrian.Assignment.DataAccess
{
    internal class Database
    {
        private readonly MySqlConnection _connection;
        private ILog _log = LogManager.GetLogger(typeof(Database));

        public Database()
        {
            _connection = new MySqlConnection(Configuration.MySqlConnectionString);
            OpenConnection();
        }

        /// <summary>
        /// Opens the databaseconnection
        /// </summary>
        /// <returns>Succesfull opened</returns>
        public bool OpenConnection()
        {
            if (_connection.State == ConnectionState.Open)
                return true;

            try
            {
                _connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                _log.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Closes the Database connection
        /// </summary>
        /// <returns>Successfull closed</returns>
        public bool CloseConnection()
        {
            try
            {
                _connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                _log.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Get Method. Return the data from the Database in a DataTable
        /// </summary>
        /// <param name="query">The performend query</param>
        /// <returns>Database Result</returns>
        public DataTable Get(string query)
        {
            DataTable dt = new DataTable();

            try
            {
                if (OpenConnection())
                {
                    MySqlCommand command = new MySqlCommand(query, _connection);

                    MySqlDataReader reader = command.ExecuteReader();
                    dt.Load(reader);
                }
            }
            catch (MySqlException ex)
            {
                _log.Error(ex.ToString());
            }
            finally
            {
                CloseConnection();
            }

            return dt;
        }

        /// <summary>
        /// Save method. Performes query and returns lastinsertedId if given
        /// </summary>
        /// <param name="query">Performed query</param>
        /// <returns>Last inserted id if given. else -1</returns>
        public int SaveToReturnId(string query)
        {
            try
            {
                if (OpenConnection())
                {
                    MySqlCommand command = new MySqlCommand(query, _connection);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch (MySqlException ex)
            {
                _log.Error(ex.ToString());
            }

            return -1;
        }
    }
}
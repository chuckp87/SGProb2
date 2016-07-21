using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Data;

namespace SGProb2
{
    /// <summary>
    /// Summary description for SGProb2WebSvc
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    //[System.Web.Script.Services.ScriptService]
    public class SGProb2WebSvc : System.Web.Services.WebService
    {
        /// <summary>
        /// GetSlotMachineSpinResult, using the supplied parameters, will call the GetSpinResult stored
        /// procedure in the MySQL database to update the player's statistics.  A JSON string is returned
        /// to the caller with the player's name, current credit total, number of lifetime spins, and 
        /// lifetime average return per spin.
        /// </summary>
        /// <param name="playerId">The Player's ID number</param>
        /// <param name="coinsBet">Number of coins wagered by the player</param>
        /// <param name="coinsWon">Number of coins won by the player</param>
        /// <param name="hashValue">Hash security value</param>
        [WebMethod]
        public void GetSlotMachineSpinResult( uint playerId, uint coinsBet, uint coinsWon, string hashValue )
        {
            try
            {
               using( MySqlConnection sqlConn = new MySqlConnection())
               {
                    // Get the DB connection string
                    sqlConn.ConnectionString = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
                    
                    // Open the connection to the MySQL database
                    sqlConn.Open();

                    // Build the command to execute the GetSpinResult stored procedure
                    using ( MySqlCommand cmd = new MySqlCommand("GetSpinResult", sqlConn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Set up the stored procedure INPUT parameters
                        cmd.Parameters.AddWithValue("playerIdVal", playerId);
                        cmd.Parameters["playerIdVal"].Direction = ParameterDirection.Input;

                        cmd.Parameters.AddWithValue("coinsBet", coinsBet);
                        cmd.Parameters["coinsBet"].Direction = ParameterDirection.Input;

                        cmd.Parameters.AddWithValue("coinsWon", coinsWon);
                        cmd.Parameters["coinsWon"].Direction = ParameterDirection.Input;

                        cmd.Parameters.AddWithValue("hashValue", hashValue);
                        cmd.Parameters["hashValue"].Direction = ParameterDirection.Input;

                        // Set up the stored procedure OUTPUT parameters
                        cmd.Parameters.Add(new MySqlParameter("pName", MySqlDbType.VarChar));
                        cmd.Parameters["pName"].Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(new MySqlParameter("pCredits", MySqlDbType.UInt32));
                        cmd.Parameters["pCredits"].Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(new MySqlParameter("pLifetimeSpins", MySqlDbType.UInt32));
                        cmd.Parameters["pLifetimeSpins"].Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(new MySqlParameter("pLifetimeAvgReturn", MySqlDbType.Decimal));
                        cmd.Parameters["pLifetimeAvgReturn"].Direction = ParameterDirection.Output;

                        // Execute the stored procedure
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        // Close the connection to the database
                        sqlConn.Close();

                        // Create an instance of SpinResult to hold the values to be
                        // passed back to the caller.
                        SpinResult sr = new SpinResult();

                        // Populate the SpinResult properties
                        sr.PlayerName = Convert.ToString(cmd.Parameters["pName"].Value);
                        sr.PlayerCredits = Convert.ToInt32(cmd.Parameters["pCredits"].Value);
                        sr.LifetimeSpins = Convert.ToInt32(cmd.Parameters["pLifetimeSpins"].Value);
                        sr.LifetimeAvgReturn = Convert.ToDecimal(cmd.Parameters["pLifetimeAvgReturn"].Value);

                        // Return a JSON string to the caller with the SpinResult values
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        Context.Response.Write(js.Serialize(sr));
                    }
               }
            }
            catch (MySqlException ex)
            {
                // Any MySQL errors will be caught in this catch block
                string msg = string.Format("MySql ERROR number {0}, msg = {1}", ex.Number, ex.Message);
                throw new HttpException(500, msg);
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.Message);
            }
        }
    }
}

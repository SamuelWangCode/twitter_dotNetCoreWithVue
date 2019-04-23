// C#

using System;
using System.Data;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;

class GetSchemaSample
{
    static void Main(string[] args)
    {
        string constr = "User Id=; Password=; Data Source=";
        

        using (OracleConnection conn = new OracleConnection(constr))
        {
            try
            {
                conn.ConnectionString = constr;
                conn.Open();

                //function FUNC_SEARCH_TOPIC_BY_HEAT(heat in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procedurename = "FUNC_SEARCH_TOPIC_BY_HEAT";
                OracleCommand cmd = new OracleCommand(procedurename, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32 );
                p1.Direction = ParameterDirection.ReturnValue;

                //Add first parameter heat
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("heat", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = 100;
                OracleParameter p3 = new OracleParameter();

                //Add second parameter search_result
                p3 = cmd.Parameters.Add("result", OracleDbType.RefCursor);
                p3.Direction = ParameterDirection.Output;

                //Get the result table
                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                DataAdapter.Fill(dt);

                for(int i = 0; i < dt.Rows.Count; ++i)
                {
                    for(int j = 0; j < 3; ++j)
                    {
                        Console.Write(dt.Rows[i][j].ToString() + " ");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}


using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace sqlapp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var st = new Stopwatch();
            while (true)
            {
                var sqlConnection = System.Environment.GetEnvironmentVariable("SQL_CONNECTION");

                using (var conn = new SqlConnection(sqlConnection))
                {

                    st.Start();
                    var ds = new DataSet();
                    try
                    {
                        await conn.OpenAsync();
                        Console.WriteLine("Connected to database");
                        using (var cmd = new SqlCommand("select top 1000 * from [master].[sys].[all_columns]", conn))
                        {
                            using (var da = new SqlDataAdapter())
                            {
                                da.SelectCommand = cmd;
                                da.Fill(ds);
                            }
                        }
                        st.Stop();
                        Console.WriteLine($"Rows: {ds.Tables[0].Rows.Count} Time: {st.Elapsed}");
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                }
                await Task.Delay(100);
            }
        }
    }
}

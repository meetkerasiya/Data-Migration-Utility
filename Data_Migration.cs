using System.Data;
using System.Data.SqlClient;


namespace Data_Migration_Utility
{
    public class Data_Migration
    {
        static string connectionString;
        static SqlConnection cnn;
        public static void DbConnect()
        {
            
            connectionString = @"Data Source=DESKTOP-OS86G42;Initial Catalog=MigrationDB;Integrated Security=True;Pooling=False";
            cnn=new SqlConnection(connectionString);
            try
            {
                cnn.Open();
                
                Console.WriteLine("Conncetion established");
                
                //To clear all records from table which where previously stored
                SqlCommand command = new SqlCommand("DELETE FROM DestinationTable Where Id>=0", cnn);
                command.ExecuteNonQuery();
                command = new SqlCommand("DELETE FROM SourceTable", cnn);
                command.ExecuteNonQuery();
                cnn.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("Could not establish sql connection");
                Console.WriteLine(e.ToString());
                Environment.Exit(1);
            }

        }
        public static void AddMillionRecord()
        {
            
            DataTable sourceTable = new DataTable();
            sourceTable.Columns.Add(new DataColumn("Id", typeof(Int32)));
            sourceTable.Columns.Add(new DataColumn("FirstNumber",typeof(Double)));
            sourceTable.Columns.Add(new DataColumn("SecondNumber",typeof(Double)));
            Random random=new Random();

            for (int i=0;i<1000000;i++)
            {
                DataRow row = sourceTable.NewRow();
                row["Id"] = i;
                row["FirstNumber"] =random.Next(1, 1000000);
                row["SecondNumber"] =random.Next(1, 1000000);

                sourceTable.Rows.Add(row);
            }
            cnn.Open();
            SqlBulkCopy objBulk = new SqlBulkCopy(cnn);
            objBulk.DestinationTableName = "SourceTable";
            objBulk.ColumnMappings.Add("Id", "Id");
            objBulk.ColumnMappings.Add("FirstNumber", "FirstNumber");
            objBulk.ColumnMappings.Add("SecondNumber", "SecondNumber");
            try
            {
                objBulk.WriteToServer(sourceTable);
                Console.WriteLine("1 Million records added successfully to SourceTable");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Could not add data to source table");
            }
            finally 
            {
                objBulk.Close();
                cnn.Close(); 
            }
        }

        public static double Sum(double firstNum, double secondNum)
        {
            Thread.Sleep(50);
            return firstNum + secondNum;
        }
        
        public static void Migration()
        {
            //lock (Data_Migration.listen)
            //{
                while (true)
                {
                    Console.WriteLine("Please enter a range to migrate a batch.");
                    Console.Write("start number = ");
                    int start = int.Parse(Console.ReadLine());
                    Console.Write("end number = ");
                    int end = int.Parse(Console.ReadLine());
                    if (start > end)
                    {
                        Console.WriteLine("Please enter a range correctly");
                        continue;
                    }
                    cnn.Open();

                    var dataInRange = new SqlCommand($"SELECT * FROM SourceTable WHERE Id>={start} AND Id<={end} ORDER BY Id", cnn);

                    //data reader to show table 
                    SqlDataReader reader = dataInRange.ExecuteReader();

                    /*
                    while(reader.Read())
                    {
                        Console.Write(reader.GetInt32(0)+" ");
                        Console.Write(reader.GetSqlSingle(1)+" ");
                        Console.Write(reader.GetSqlSingle(2)+"\n");
                    }
                    */
                    DataTable destTable = new DataTable();
                    destTable.Columns.Add("Id", typeof(int));
                    destTable.Columns.Add("Sum", typeof(double));

                   // Monitor.Wait(Data_Migration.listen, 1);

                    for (int i = start; i <= end; i++)
                    {
                      
                        DataRow row = destTable.NewRow();
                        reader.Read();
                        row["Id"] = reader.GetInt32(0);
                        row["sum"] = Sum((double)reader.GetSqlSingle(1), (double)reader.GetSqlSingle(2));
                        destTable.Rows.Add(row);
                        if (i == end || ((start - i) % 100 == 0 && i != start))
                        {
                            //if(Console.KeyAvailable)
                            //{
                            //    string action=Console.ReadLine();
                            //    if(action.ToLower()=="cancel")
                            //    {
                            //        break;
                            //    }
                            //}
                        Console.WriteLine($"Start={start}- i={i} - End={end}");
                            WriteInDestinationTable(destTable);

                            destTable.Clear();
                        }
                    }

                    cnn.Close();
                }
            //}
        }
        public static void WriteInDestinationTable(DataTable destTable)
        {
            SqlConnection destCnn = new SqlConnection(connectionString);
            destCnn.Open();
            SqlBulkCopy copy = new SqlBulkCopy(destCnn);
            copy.DestinationTableName = "DestinationTable";
            copy.ColumnMappings.Add("Id", "Id");
            copy.ColumnMappings.Add("Sum", "Sum");
            try
            {
                copy.WriteToServer(destTable);
                copy.Close();
                Console.WriteLine("Successfully migrate data to DestinationTable");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write data to DestinationTable");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                destCnn.Close();
            }
        }
        
    }
}

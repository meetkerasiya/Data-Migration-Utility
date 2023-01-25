using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace Data_Migration_Utility
{
    public class Data_Migration
    {
        static string connectionString;
        static SqlConnection cnn;
        static AutoResetEvent signal=new AutoResetEvent(false);
        volatile static bool interruptFlag;
        static bool cancelFlag;
        static bool overlapFlag;
        static int start;
        static int end;
        static int current;
        static int count = 0;
        public static void DbConnect()
        {
            
            connectionString = @"Data Source=DESKTOP-OS86G42;Initial Catalog=MigrationDB;Integrated Security=True;Pooling=False";
            cnn=new SqlConnection(connectionString);
            try
            {
                cnn.Open();
                
                Console.WriteLine("Conncetion established");
                
                //To clear all records from table which where previously stored
                SqlCommand command = new SqlCommand("DELETE FROM DestinationTable", cnn);
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
            sourceTable.Columns.Add(new DataColumn("FirstNumber", typeof(Int32)));
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
            
                while (true)
                {
                    Console.WriteLine("Please enter a range to migrate a batch.");
                    Console.Write("start number = ");
                    try
                    {
                        start = int.Parse(Console.ReadLine());
                        Console.Write("end number = ");
                        end = int.Parse(Console.ReadLine());
                    count = 0;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Please enter range correctly");
                        continue;
                    }
                    
                    if (start > end || start<0 || end<1 || start>=1000000 || end>=1000000)
                    {
                        Console.WriteLine("Please enter a range correctly");
                        continue;
                    }
                    cnn.Open();

                    var dataInRange = new SqlCommand($"SELECT * FROM SourceTable WHERE Id>={start} AND Id<={end} ORDER BY Id", cnn);

                    //data reader to show table 
                    SqlDataReader reader = dataInRange.ExecuteReader();

                  
                    DataTable destTable = new DataTable();
                    destTable.Columns.Add("Id", typeof(int));
                    destTable.Columns.Add("Sum", typeof(double));


                    for (int i = start; i <= end; i++)
                    {
                        current = i;
                        DataRow row = destTable.NewRow();
                        reader.Read();
                        row["Id"] = reader.GetInt32(0);
                        row["sum"] = Sum((double)reader.GetSqlSingle(1), (double)reader.GetSqlSingle(2));
                        destTable.Rows.Add(row);
                        if(overlapFlag)
                        {
                            break;
                        }
                        if(interruptFlag)
                        {
                            if(signal.WaitOne())
                            {

                            }
                            if(cancelFlag)
                            {
                            //if we don't change than when in future we ask for status it will stop program
                                cancelFlag= false;
                                break;
                            }
                        }
                        if (i == end || ((start - i) % 100 == 0 && i != start))
                        {
                        count += 100;
                        WriteInDestinationTable(destTable);
                        
                            
                            destTable.Clear();
                        }
                    }
                    if(current== end && !overlapFlag)
                    {
                        Console.WriteLine("Data Migration completed successfully!!");
                    }
                overlapFlag = false;
                cnn.Close();
                }
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write data to DestinationTable");
                if(ex.Message.Contains("The duplicate key value is"))
                {
                    Console.WriteLine("Please enter a range which do not overlap previous migration");
                    overlapFlag = true;
                }
                

                
            }
            finally
            {
                destCnn.Close();
            }
        }
        public static void Interupt()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    interruptFlag= true;
                    string action = Console.ReadLine();
                    if (action.ToLower() == "cancel")
                    {
                        Console.WriteLine("Migration cancelled");
                        Console.WriteLine($"{start} to {start+count} were completed");

                        cancelFlag = true;
                        
                    }
                    if(action.ToLower()=="status")
                    {
                        Console.WriteLine("Migration status:");
                        Console.WriteLine($"{start} to {start+count} are completed");
                        Console.WriteLine($"Ongoing: {start + count + 1} to {current} ");
                        Console.WriteLine($"{current+1} to {end} are remianing");
                    }
                    interruptFlag= false;
                    signal.Set();
                }
            }
        }
    }
}

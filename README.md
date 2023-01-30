# Data-Migration-Utility
23 Jan
Practice session
Data migration utility

->download System.Data.SqlClient from NuGet

->Create one database with two table as defined in problem statement

->go to solution explorer and add new data->database-> 
after adding new database connect it using connection string
and pass it to SqlConnection constructor

->we need to open connection using
sqlConnection.open();

->we can use sql queries using 

	SqlCommand command=new SqlCommand("query",sqlConnection);

->and execute using

	command.ExecuteNonQuery()
	
->but to read data from DB

	SqlDataReader reader=command.ExecuteReader()
	
->To get these outputs
reader.Read(); //advances one record
for int reader.GetInt32(0); // 0 if column index like 0-> first , 1-> second
if does not work for any perticular type then try

	reader.GetSqlSingle(1);

Note:
We can't add data while using reader
so either we have to close the reader
or we can use different connection object to achieve that

-----------------------------------------------------
=>To add data in database 
->we use class named DataTable

            DataTable sourceTable = new DataTable();
            sourceTable.Columns.Add(new DataColumn("Id", typeof(Int32)));
            sourceTable.Columns.Add(new DataColumn("Id", typeof(Int32)));
this is how we set dataTable
then to add rows

		DataRow row = sourceTable.NewRow();
                row["Id"] = i;
                row["FirstNumber"] =num;
                sourceTable.Rows.Add(row);

=>SqlBulkCopy  
->If the source and destination tables are in the same SQL Server instance, 
it is easier and faster to use a Transact-SQL INSERT ... SELECT statement to copy the data.
-> Also use SqlBulkCopy for large amount of data not for few rows or row by row data insertion

	    SqlBulkCopy objBulk = new SqlBulkCopy(cnn);
            objBulk.DestinationTableName = "SourceTable";
            objBulk.ColumnMappings.Add("Id", "Id");
            objBulk.ColumnMappings.Add("FirstNumber", "FirstNumber");

objBulk.WriteToServer(sourceTable);

------------------------------------------------------------------------

=>Working with thread

There is one thread called "Main thread" which always runs in program

-> We can create threads and also create for specific method
->here i have to read user input for cancel and status 
while doing migration task
so i used

	Thread migration = new Thread(Data_Migration.Migration);
        migration.Start();
        Thread interrupt = new Thread(Data_Migration.Interupt);
        interrupt.Start();

=>Note:
To check if user has press any key to console i used this in Interrupt


while (true)
            {
                if (Console.KeyAvailable)
                {...

and then i set Interrupt flag and read that line to check if it's cancel or status

--------------------------------------------------------------------------
=>Syncronization using AutoResetEvent

->To make sure that when migration task is taking the range user won't give cancel or status input
->also when user ask for status or cancel, to pause migration
we use this michanism for intercommunication 

        static AutoResetEvent signal=new AutoResetEvent(false);

-> in Migration()

				if(interruptFlag)
                        {
                            if(signal.WaitOne()) //this will make this thread till it get signal to start
                            {

                            }


->in Interupt()
when work is done
		interruptFlag= false;
		signal.Set(); //It gives other thread signal to continue


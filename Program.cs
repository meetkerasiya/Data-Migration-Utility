using Data_Migration_Utility;

class Myclass
{
    public static void Main(string[] args)
    {
        Data_Migration.DbConnect();
        Data_Migration.AddMillionRecord();
        Thread migration = new Thread(Data_Migration.Migration);
        migration.Start();
        Thread interrupt = new Thread(Data_Migration.Interupt);
        interrupt.Start();
        

        migration.Join();
        interrupt.Join();
       

    }
}
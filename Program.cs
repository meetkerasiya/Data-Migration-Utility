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
        //while(true)
        //{
        //    Thread.Sleep(1000);
        //    if(Console.KeyAvailable)
        //    {
        //        string action = Console.ReadLine();
        //        if (action.ToLower() == "cancel")
        //        {
        //            Console.WriteLine("Migration cancelled");
        //        }
        //    }
                
            
        //}

        migration.Join();
        interrupt.Join();
        //Data_Migration.Migration();
        //ThreadStart s1 = Data_Migration.Migration;
        //ThreadStart s2 = Listener.listen;
        //Thread thread1= new Thread(s1);
        //Thread thread2= new Thread(s2);
        //thread1.Start();
        //thread2.Start();
         

    }
}
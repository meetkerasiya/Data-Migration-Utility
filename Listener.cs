namespace Data_Migration_Utility
{
    public class Listener
    {
       
        public static void  listen()
        {
            while(true)
            {
             
                Console.WriteLine("Thread in Listener");

                //Data_Migration.action = Console.ReadLine();
            }
            
            /*lock(Data_Migration.listen)
            {
                while (true)
                {
                    Console.WriteLine("Thread in Listener");
                    Monitor.Wait(Data_Migration.listen);
                    Data_Migration.action = Console.ReadLine();
                    Monitor.PulseAll(Data_Migration.listen);
                }
            }*/
            
        }
    }
}

using System;
using System.ServiceModel;

namespace ProjectSNUS
{
    class Program
    {
        static void Main(string[] args)
      {
  Console.WriteLine("================================================");
            Console.WriteLine("  WCF Server - Causal Broadcast System");
       Console.WriteLine("================================================");
 Console.WriteLine();

          ServiceHost host = null;

          try
            {
    // Create service host
            host = new ServiceHost(typeof(Service1));

   // Open the service
  host.Open();

           Console.WriteLine("? Server started successfully!");
     Console.WriteLine();
    Console.WriteLine("Service Information:");
         Console.WriteLine($"  Base Address: {host.BaseAddresses[0]}");
          Console.WriteLine($"  State: {host.State}");
 Console.WriteLine();
        Console.WriteLine("Server is ready to accept sensor connections...");
         Console.WriteLine();
                Console.WriteLine("================================================");
       Console.WriteLine("Connected Sensors will appear below:");
           Console.WriteLine("================================================");
     Console.WriteLine();

         // Keep the service running
      Console.WriteLine("Press <ENTER> to stop the server...");
             Console.ReadLine();

    // Close the service
        host.Close();
        Console.WriteLine();
                Console.WriteLine("? Server stopped successfully!");
      }
      catch (Exception ex)
   {
                Console.WriteLine();
                Console.WriteLine("? ERROR: Failed to start server!");
       Console.WriteLine($"  {ex.Message}");
         Console.WriteLine();

  if (ex.InnerException != null)
              {
Console.WriteLine("Inner Exception:");
         Console.WriteLine($"  {ex.InnerException.Message}");
         }

        if (host != null && host.State == CommunicationState.Faulted)
            {
  host.Abort();
       }
         }

            Console.WriteLine();
  Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

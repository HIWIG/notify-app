using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Net.Mail;



namespace Notify_Framework
{
    class Program
    {
        //tablica na ścieżkę i użytkowników
        static string[] config { get; set; }

        //tablica na nazwy plików
        static string[] files { get; set; }
        //zmienna przetrzymująca ścieżkę
        static string path { get; set; }

        static DateTime lastRead = DateTime.MinValue;

        //static string server = "mail.polmotors.com.pl";
        static string server;


        // static string login = "gfaber.polmotors";
        static string login;

        static string password;


        //funkcja wysyłająca maila
        public static void SendEmail(string server, string user, string fpath)
        {
            var x = DateTime.Now;
            


            string to = config[9];
            // string from = "gfaber@polmotors.com.pl";
            string from = config[7];
            MailMessage message = new MailMessage(from, to);
            message.Subject = config[11];
            message.Body =$"Uzytkownik: {user} zmienil plik: {fpath} dnia {x}.";
            int y = int.Parse(config[13]);
            SmtpClient client = new SmtpClient(server, y);
            //SmtpClient client = new SmtpClient(server, 587);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(login, password);


            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                    ex.ToString());
            }
        }

        static void Main(string[] args)
        {
            //odczyt  configu i nazw plików
            config = System.IO.File.ReadAllLines(@"cfg.txt");
            files = System.IO.File.ReadAllLines(@"pliki.txt");
            string[] filters = File.ReadAllLines(@"rozszerzenia.txt");
            List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
            path = config[15];
            var info = new DirectoryInfo(path);
            bool exit = true;
            server = config[1];
            login = config[3];
            password = config[5];

            //dodawanie filtrów na każdy typ pliku
            foreach (var field in filters)
            {
                var watcher = new FileSystemWatcher(path);

                watcher.NotifyFilter = NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size;

                //dodanie eventu do obserwowania
                watcher.Changed += OnChanged;
                watcher.Filter = field;
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
                watchers.Add(watcher);
            }

            Console.WriteLine("Application is running. Press q to exit...");


            //read aby utrzymać program w oczekiwaniu
            while (exit)
            {
                var x = Console.Read();
                switch (x)
                {

                    case 'l':
                        Console.WriteLine("okokok");
                        break;


                    case 'q':
                        exit = false;
                        Console.WriteLine("quit");
                        break;
                    default:
                        break;
                }




            }
        }

        //funkcja wywałana po zdarzeniu zapisu
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            var sec = DateTime.MinValue.AddSeconds(1);


            DateTime lastWriteTime = File.GetLastWriteTime(path);

            //naprawa buga, przez którego zdarzenie było wywoływane 2 razy
            if (lastWriteTime.Subtract(lastRead).TotalSeconds > 2)
            {
                lastRead = lastWriteTime;

                if (e.ChangeType != WatcherChangeTypes.Changed)
                {
                    return;
                }
                var directory = new DirectoryInfo(path);
                var lastModified = directory.GetFiles().OrderByDescending(fi => fi.LastWriteTime).First();
                string modifiedBy = lastModified.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();

                //odrzucanie tymczasowego pliku
                if (e.FullPath.ToCharArray()[path.Length + 1] != '~')
                {
                    foreach (var fileName in files)
                    {

                        //sprawdzanie pliku czy jest na liście do powiadamiania
                        if (path + @"\" + fileName == e.FullPath)
                        {

                            foreach (var line in config)
                            {

                                //sprawdzenie użytkownika czy jest na liście do sprawdzania
                                if (modifiedBy == line)
                                {
                                    Console.WriteLine($"zmiana pliku: {e.FullPath} przez: " + line);
                                    SendEmail(server, line, e.FullPath);

                                }
                            }
                        }
                    }


                }
                // else discard the (duplicated) OnChanged event
            }



        }
    }
}


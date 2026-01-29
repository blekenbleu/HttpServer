using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// https://github.com/Peteri-git/simple-HttpServer/tree/master
namespace HttpServer
{
    internal class Program
    {
        delegate void SimpleDelegate();
        static readonly Dictionary<string, SimpleDelegate> dic = new Dictionary<string, SimpleDelegate>();
        static string path;
        static void Main(/*string[] args*/)
        {
            // Console.WriteLine("Zadejte IP");
            string ip = "127.0.0.1"; //  Console.ReadLine();
            // Console.WriteLine("Zadejte Port");
            string port = "7734";  Console.ReadLine();
            // Console.WriteLine("Zadej cestu k složce");
            path = "R:\\Temp";   // Console.ReadLine();
            Console.WriteLine(ip+":"+port+" for "+path+" files\n");
/*
            dic.Add("/On", new SimpleDelegate(On));
            dic.Add("/Blinking", Blinking);
            dic.Add("/Off", new SimpleDelegate(Off));
            dic.Add("/Switch", new SimpleDelegate(Switch));
 */
            TcpListener server = new TcpListener(IPAddress.Parse(ip), Convert.ToInt32(port));
            server.Start();
            while (true)
            {
                var client = server.AcceptTcpClient();
                Thread th = new Thread(Process);
                th.Start(client);
            }
        }

        static void Process(object param)
        {
            try
            {
                var client = (TcpClient)param;
                var stream = client.GetStream();
                var sr = new StreamReader(stream);
                var sw = new StreamWriter(stream, Encoding.UTF8);

                string[] actionLine = sr.ReadLine()?.Split(new char[] { ' ' }, 3);
                int contentLength = 0;
                while (true)
                {
                    string line = sr.ReadLine();
                    string[] headLine = line?.Split(new char[] { ':' }, 2);
                    if (headLine[0] == "Content-Length")
                    {
                        contentLength = int.Parse(headLine[1].Trim());
                    }
                    Console.WriteLine(line);
                    if (string.IsNullOrWhiteSpace(line))
                        break;
                }
                string[] filePaths = Directory.GetFiles(path);
                if (actionLine[0] == "POST")
                {
                    char[] postData = new char[contentLength];
                    sr.Read(postData, 0, contentLength);
                    Console.WriteLine(new string(postData));
                    string tmp = new string(postData);
                    string[] input = tmp.Split('=');
                    List<string> files = new List<string>();
                    foreach (var item in filePaths)
                    {
                        string[] items = item.Split('\\');
                        files.Add(items.Last());
                    }
                    foreach (var file in files)
                    {
                        if (input[1] == file)
                        {
                            foreach (var item in MimeTypes._mappings)
                            {
                                if (file.EndsWith(item.Key))
                                {
                                    if (File.Exists(path + "\\" + file.ToString()))
                                    {
                                        sw.WriteLine("HTTP/1.1 200 OK");
                                        sw.WriteLine($"Content-Type:{item.Value}; charset=utf-8");
                                        sw.WriteLine();
                                        byte[] filee = File.ReadAllBytes(path + "\\" + file.ToString());
                                        sw.BaseStream.Write(filee, 0, filee.Length);
                                        sw.BaseStream.Flush();
                                    }
                                    else
                                    {
                                        sw.WriteLine("HTTP/1.1 404 NOT FOUND");
                                        sw.WriteLine();
                                    }
                                }
                            }
                        }
                    }
                    sw.Flush();
                    client.Close();
                }
                /*                foreach (var fgt in dic)
                                {
                                    if (actionLine[1] == fgt.Key)
                                    {
                                        SimpleDelegate tmp = fgt.Value;
                                        tmp();
                                    }
                                }
                                var test = actionLine[1].Split('/');
                                if (test[1] == "jas")
                                {
                                    bulb.Brightness = Convert.ToInt32(test[2]);
                                }
*/
                                if (actionLine[1] == "/Menu" || actionLine[1] == "/")
                
                {
                    Menu(sw, filePaths);
                } else
                        /*
                                        foreach (var item in MimeTypes._mappings)
                                        {
                                            if (actionLine[1].EndsWith(item.Key))
                                            {
                         */
                        if (File.Exists(path + actionLine[1].ToString()))
                {
                    sw.WriteLine("HTTP/1.1 200 OK");
                    sw.WriteLine($"Content-Type:text/html; charset=utf-8");
                    sw.WriteLine();
                    byte[] file = File.ReadAllBytes(path + actionLine[1].ToString());
                    sw.BaseStream.Write(file, 0, file.Length);
                    sw.BaseStream.Flush();
                }
                else
                {
                    sw.WriteLine("HTTP/1.1 404 NOT FOUND");
                    sw.WriteLine();
                }

//                    }
//                }
                sw.Flush();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Menu(StreamWriter sw, string[] files)
        {
            sw.WriteLine("HTTP/1.1 200 OK");
            sw.WriteLine($"Content-Type:text/html; charset=utf-8");
            sw.WriteLine();
            sw.WriteLine("<html>");
            sw.WriteLine("<head>");
            sw.WriteLine("</head>");
            sw.WriteLine("<body><h2>HttpServer by TcpListener</h2>Enter filename from "+path);
            sw.WriteLine("  <form action=\"/\" method=\"POST\">");
            sw.WriteLine("   <input type=\"text\" name=\"something\" />");
            sw.WriteLine("   <input type=\"submit\" value=\"send\" />");
            sw.WriteLine("  </form>");
            sw.WriteLine("<ul>");
            foreach (var item in files)
            {
                string[] tmp = item.Split('\\');
                sw.WriteLine($"<li>{item}</li><a href=\"{tmp.Last()}\">Open</a>");
            }
            sw.WriteLine("</ul>");
            sw.WriteLine("</body>");
            sw.WriteLine("</html>");
        }
    }
}

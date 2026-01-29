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
		static StreamWriter sw;
		static string path;
		static void Main(/*string[] args*/)
		{
			string ip = "127.0.0.1";
			string port = "7734";
			path = "R:\\Temp";
			Console.WriteLine(ip+":"+port+" for "+path+" files\n");
			TcpListener server = new TcpListener(IPAddress.Parse(ip), Convert.ToInt32(port));
			server.Start();
			while (true)
			{
				var client = server.AcceptTcpClient();
				Thread th = new Thread(Process);
				th.Start(client);
			}
		}

		static string MimeType(string line)
		{
			string type = "text/*";
			foreach (var item in MimeTypes._mappings)
				if (line.EndsWith(item.Key))
				{
					type = item.Value;
					break;
				}
			return type;
		}

		static void SwWrite(string foo)
		{
			string poo;
									
			if (File.Exists(poo = path + "\\" + foo))
			{
				Console.WriteLine(poo+"\n"+$"Content-Type:{MimeType(foo)}; charset=utf-8");
				sw.WriteLine("HTTP/1.1 200 OK");
				sw.WriteLine($"Content-Type:{MimeType(foo)}; charset=utf-8");
				sw.WriteLine();
				byte[] file = File.ReadAllBytes(poo);
				sw.BaseStream.Write(file, 0, file.Length);
				sw.BaseStream.Flush();
			}
			else
			{
				Console.WriteLine(poo + " not found");
				sw.WriteLine("HTTP/1.1 404 NOT FOUND");
			}
		}

		static void Process(object param)
		{
			try
			{
				var client = (TcpClient)param;
				var stream = client.GetStream();
				var sr = new StreamReader(stream);
				sw = new StreamWriter(stream, Encoding.UTF8);

				string first = sr.ReadLine();
				Console.WriteLine(first);
				string[] actionLine = first?.Split(new char[] { ' ' }, 3);
				int contentLength = 0;
				while (true)
				{
					string line = sr.ReadLine();
					string[] headLine = line?.Split(new char[] { ':' }, 2);
					if (null != headLine && headLine[0] == "Content-Length")
						contentLength = int.Parse(headLine[1].Trim());

//					Console.WriteLine(line);
					if (string.IsNullOrWhiteSpace(line))
						break;
				}
				string[] filePaths = Directory.GetFiles(path);

				if (null != actionLine && actionLine[0] == "POST")
				{
					char[] postData = new char[contentLength];
					sr.Read(postData, 0, contentLength);
//					Console.WriteLine(new string(postData));
					string tmp = new string(postData);
					string[] input = tmp.Split('=');
					List<string> files = new List<string>();
					foreach (var item in filePaths)
					{
						string[] items = item.Split('\\');
						files.Add(items.Last());
					}
					foreach (var file in files)
						if (input[1] == file)
							SwWrite(file.ToString());

				}
				else if (actionLine[1] == "/Menu" || actionLine[1] == "/")
					Menu(sw, filePaths);
				else SwWrite(actionLine[1].ToString());
				sw.WriteLine("\n");
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

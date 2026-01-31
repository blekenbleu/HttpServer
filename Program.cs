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

		static void W(string s)
		{
//			Console.WriteLine("\t"+s);
			sw.WriteLine(s);
		}

		static void W()
		{
			W("");
		}

		static void SwWrite(string foo)
		{
			string poo;
									
			if (File.Exists(poo = path + "\\" + foo))
			{
				string line, type = MimeType(foo);
				W("HTTP/1.1 200 OK");
				using (var fr = new StreamReader(poo))
				{
					W($"Content-Length: {(int)fr.BaseStream.Length}");
					W($"Content-Type: {type}; charset=UTF-8");
					W();
					while ((line = fr.ReadLine()) != null)
						W(line);
				}
			}
			else
			{
				W("HTTP/1.1 404 NOT FOUND");
			}
		}

		static void Process(object param)
		{
			bool post;

			try
			{
				var client = (TcpClient)param;
				var stream = client.GetStream();
				var sr = new StreamReader(stream);
				sw = new StreamWriter(stream, Encoding.UTF8);

				string first = sr.ReadLine();
				Console.WriteLine("first: " + first);
				string[] actionLine = first?.Split(new char[] { ' ' }, 3);
				post = (null != actionLine && "POST" == actionLine[0]);
				int contentLength = 0;
				while (true)
				{
					string line = sr.ReadLine();
					if (string.IsNullOrWhiteSpace(line))
						break;

					string[] headLine = line?.Split(new char[] { ':' }, 2);
					if (null != headLine && headLine[0] == "Content-Length")
						contentLength = int.Parse(headLine[1].Trim());
				}
				string[] filePaths = Directory.GetFiles(path);

				if (post)
				{
					char[] postData = new char[contentLength];
					sr.Read(postData, 0, contentLength);
					string tmp = new string(postData);
					string[] input = tmp.Split('=');
					foreach (var item in filePaths)
					{
						string[] items = item.Split('\\');
						if (input[1] == items.Last())
							SwWrite(input[1]);
					}
				}
				else if (null == actionLine || actionLine[1] == "/Menu" || actionLine[1] == "/")
					Menu(filePaths);
				else SwWrite(actionLine[1].ToString());
				W();
				sw.Flush();
				client.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		static void Menu(string[] files)
		{
			W("HTTP/1.1 200 OK");
			W($"Content-Type:text/html; charset=UTF-8");
			W();
			W("<html>");
			W("<head>");
			W("</head>");
			W("<body><h2>HttpServer by TcpListener</h2>Enter filename from "+path);
			W("  <form action=\"/\" method=\"POST\">");
			W("   <input type=\"text\" name=\"something\" />");
			W("   <input type=\"submit\" value=\"send\" />");
			W("  </form>");
			W("<ul>");
			foreach (var item in files)
			{
				string[] tmp = item.Split('\\');
				W($"<li>{item}</li><a href=\"{tmp.Last()}\">Open</a>");
			}
			W("</ul>");
			W("</body>");
			W("</html>");
		}
	}
}

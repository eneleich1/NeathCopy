using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NeathCopyEngine.Helpers
{
    public enum RquestContent
    {
        None, Sources, Operation,Destiny, All,FromList
    }
    /// <summary>
    /// Represent a operation request information.
    /// </summary>
    public class RequestInfo
    {
        public string Operation { get; set; }
        public string Destiny { get; set; }
        public string SourceArg { get; set; }
        public bool Container { get; set; }
        public List<string> Sources { get; set; }
        public RquestContent Content { get; set; }
        public string[] Arguments = new string[3];

        public RequestInfo()
        {
        }
        public RequestInfo(string operation, string sourceArg, string destiny)
            :this(new string[] { operation, sourceArg, destiny })
        { }

        public RequestInfo(string operation, List<string> sources, string destiny)
        {
            Operation = operation;
            Sources = sources;
            Destiny = destiny;

            //Detecting FastMove
            if (Operation == "move" && Path.GetPathRoot(Sources[0]) == Path.GetPathRoot(Destiny))
                Operation = "fastmove";
        }

        public RequestInfo(string[] args)
        {
            if (args == null || args.Length != 3) 
                throw new ArgumentException(string.Format("The array of arguments must contain 3 elements: operation, source, destiny"));

            var lines = new char[] { '\n','\r','\t'};
            Arguments[0] = args[0].RemoveChars(lines);
            Arguments[1] = args[1].RemoveChars(lines);
            Arguments[2] = args[2].RemoveChars(lines);

            Operation = args[0].ToLower();

            if (Arguments[1][0] == '*')
            {
                SourceArg = Arguments[1].Remove(0, 1);
                Container = true;
            }
            else
            {
                SourceArg = Arguments[1];
                Container = false;
            }

            var fixArg2 = Arguments[2][Arguments[2].Length-1]=='"'? Arguments[2].Remove(Arguments[2].Length-1,1): Arguments[2];
            Destiny = fixArg2.Length == 2 || fixArg2.Length == 3 ? string.Format("{0}\\", fixArg2) : fixArg2;

            Sources = GetSources(SourceArg, Destiny, Container);

            //Detecting FastMove
            if (Operation == "move" && Path.GetPathRoot(Sources[0]) == Path.GetPathRoot(Destiny))
                Operation = "fastmove";
        }

        /// <summary>
        /// Since request operation is based on multiples sources specifics in FileData.dat file,
        /// This method read all lines in this files and return the lines(sources) list.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSources(string SourceArg,string Destiny,bool Container)
        {
            var list = new List<string>();

            if (!File.Exists(SourceArg) && !Directory.Exists(SourceArg))
                throw new InvalidOperationException(string.Format("The File od Directory {0} not exist", SourceArg));

            if (Container)
            {
                var reader = new StreamReader(SourceArg,Encoding.Unicode);

                char first= (char)reader.Read();

                //Is Separate By | => From NeathCopyShellExt
                if (first == '|')
                {
                    var content = reader.ReadToEnd();
                    list = content.Split(new char[] { '|' }).ToList();
                    list.Remove(list.Last());
                }
                //Came from TeraCopyShellExt
                else
                {
                    reader = new StreamReader(SourceArg, Encoding.Default);
                    while (!reader.EndOfStream)
                        list.Add(reader.ReadLine());
                }

                reader.Close();
                reader.Dispose();
            }
            else list.Add(SourceArg);

            return list;
        }

    }
}

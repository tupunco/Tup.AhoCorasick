using System;

namespace Tup.AhoCorasick
{
    class Program
    {
        static void Main(string[] args)
        {
            var search = new AhoCorasickSearch();
            var keywords = new string[] { "he", "she", "his", "hers" };
            search.Build(keywords);
            var res = search.SearchAll("ushers" );
            Console.Write(res);
            Console.Read();
        }
    }
}

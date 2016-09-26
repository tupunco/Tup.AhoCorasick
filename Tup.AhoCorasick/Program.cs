using System;
using System.Text;

namespace Tup.AhoCorasick
{
    class Program
    {
        static void Main(string[] args)
        {
            var search = new AhoCorasickSearch();
            var keywords = new string[] { "he", "she", "his", "hers" };
            search.Build(keywords);
            
            searchTest(search);

            searchTest(search);
            replaceTest2();
            replaceTest3();

            Console.Read();
        }

        private static void searchTest(AhoCorasickSearch search)
        {
            var res = search.SearchAll("ushers");
            Console.WriteLine(res);
        }

        private static void replaceTest(AhoCorasickSearch search)
        {
            var text = "ushers";
            var res = search.Replace(text, "-");
            Console.WriteLine(res);

            text = "shersx";
            res = search.Replace(text, "-");
            Console.WriteLine(res);

            text = "her";
            res = search.Replace(text, "-");
            Console.WriteLine(res);

            text = "she";
            res = search.Replace(text, "-");
            Console.WriteLine(res);
        }

        private static void replaceTest2()
        {
            var search = new AhoCorasickSearch();
            var keywords = new string[] { "伟大","特色主义","公园" };
            search.Build(keywords);

            var text = "从这里建设伟大的特色主义主题公园";
            var res = search.Replace(text, "-");
            Console.WriteLine(res);

            text = "主题公园";
            res = search.Replace(text, "-");
            Console.WriteLine(res);

            text = "伟大的特色主义主题公园";
            res = search.Replace(text, "-");
            Console.WriteLine(res);

            text = "伟大特色主义公园";
            res = search.Replace(text, "-");
            Console.WriteLine(res);
        }

        private static void replaceTest3()
        {
            var search = new AhoCorasickSearch();
            var keywords = new string[] { "一边", "刀锋", "烽火" };
            search.Build(keywords);

            var text = "一边烽火, 一边烽火天";
            var res = search.Replace(text, "-");
            Console.WriteLine(res);

            text = "一边刀锋很犀利";
            res = search.Replace(text, "-");
            Console.WriteLine(res);
        }
    }
}

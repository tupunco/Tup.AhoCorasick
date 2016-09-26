using System;
using System.Collections.Generic;
using System.Text;

namespace Tup.AhoCorasick
{
    /// <summary>
    /// Aho-Corasick Search
    /// </summary>
    /// <remarks>
    /// Aho-Corasick exact set matching algorithm
    /// Search time: O(n + m + z), where z is number of pattern occurrences
    ///
    /// This implementation is loosely based on http://www.codeproject.com/useritems/ahocorasick.asp
    /// 
    /// String Matching: An Aid to Bibliographic Search - Alfred V. Aho and Margaret J. Corasick (Bell Laboratories) (http://hidden.dankirsh.com/CS549/paper.pdf)
    /// 
    /// Set Matching and Aho-Corasick Algorithm - Pekka Kilpeläinen (University of Kuopio) (www.cs.uku.fi/~kilpelai/BSA05/lectures/slides04.pdf)
    /// http://www-sr.informatik.uni-tuebingen.de/~buehler/AC/AC.html
    /// </remarks>
    public class AhoCorasickSearch
    {
        /// <summary>
        /// AC Tree Root
        /// </summary>
        private Node TreeRoot = null;

        /// <summary>
        /// search all and replace text
        /// </summary>
        /// <param name="ac"></param>
        /// <param name="content"></param>
        /// <param name="newWord"></param>
        /// <returns></returns>
        public string Replace(string text, string newWord)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = this.SearchAll(text);
            if (result == null || result.Length <= 0)
                return text;

            if (newWord == null)
                newWord = "";

            var startIndex = 0;
            var endIndex = 0;
            var resContent = new StringBuilder();
            foreach (var v in result)
            {
                endIndex = v.Index;
                if (endIndex > startIndex)
                    resContent.Append(text.Substring(startIndex, endIndex - startIndex));

                resContent.Append(newWord);
                startIndex = v.Index + v.Match.Length;
            }

            var rcLen = text.Length;
            if (startIndex < rcLen)
            {
                endIndex = rcLen;
                resContent.Append(text.Substring(startIndex, endIndex - startIndex));
            }
            return resContent.ToString();
        }

        /// <summary>
        /// Search All
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual SearchResult[] SearchAll(string text)
        {
            return SearchAll(text, 0, int.MaxValue);
        }

        public virtual SearchResult[] SearchAll(string text, int start)
        {
            return SearchAll(text, start, int.MaxValue);
        }

        public virtual SearchResult SearchFirst(string text)
        {
            return SearchFirst(text, 0);
        }

        public SearchResult[] SearchAll(string text, int start, int count)
        {
            CheckArguments(text, start, count);

            List<SearchResult> results = null;
            if (count == int.MaxValue)
                results = new List<SearchResult>();
            else
                results = new List<SearchResult>(count);

            foreach (SearchResult result in SearchIterator(text, start))
            {
                results.Add(result);
                if (results.Count == count)
                    break;
            }

            return results.ToArray();
        }

        public SearchResult SearchFirst(string text, int start)
        {
            CheckArguments(text, start, int.MaxValue);

            IEnumerator<SearchResult> iter = SearchIterator(text, start).GetEnumerator();
            if (iter.MoveNext())
                return iter.Current;
            return SearchResult.Empty;
        }

        protected IEnumerable<SearchResult> SearchIterator(string text, int start)
        {
            var root = this.TreeRoot;
            if (root == null)
                throw new ArgumentNullException("root", "need search.Build()");

            var ptr = root;
            int index = 0;
            if (start > 0)
                text = text.Substring(start);

            while (index < text.Length)
            {
                Node trans = null;

                while (trans == null)
                {
                    trans = ptr.GetTransition(text[index]);

                    if (ptr == root)
                        break;

                    if (trans == null)
                        ptr = ptr.Failure;
                }

                if (trans != null)
                    ptr = trans;

                if (ptr.Outputs != null)
                {
                    foreach (string found in ptr.Outputs)
                        yield return new SearchResult(index - found.Length + 1, found);

                }
                index++;
            }
        }
        /// <summary>
        /// Build AC Tree
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public bool Build(params string[] keywords)
        {
            CheckKeywords(keywords);

            this.TreeRoot = BuildTree(keywords);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        protected static Node BuildTree(string[] keywords)
        {
            Node root = new Node(null, ' ');

            #region build trie tree
            {
                Node cNode = root;
                Node newNode = null;
                foreach (string keyword in keywords)
                {
                    cNode = root;
                    // add pattern to tree
                    foreach (char c in keyword)
                    {
                        newNode = null;

                        if ((newNode = cNode.GetTransition(c)) == null)
                        {
                            newNode = new Node(cNode, c);
                            cNode.AddTransition(newNode);
                        }
                        cNode = newNode;
                    }
                    cNode.AddResult(keyword);
                }
            }
            #endregion

            // Find failure functions

            var nodesQueue = new Queue<Node>();
            // level 1 nodes - fail to root node
            foreach (Node cNode in root.Transition.Values)
            {
                cNode.Failure = root;

                QueueAddRange(nodesQueue, cNode.Transition.Values);
            }

            {
                Node cNode = null, r = null, nNode = null;
                // other nodes - using BFS
                while (nodesQueue.Count != 0)
                {
                    cNode = nodesQueue.Dequeue();
                    r = cNode.Parent.Failure;

                    while (r != null && (nNode = r.GetTransition(cNode.Char)) == null)
                        r = r.Failure;

                    if (r == null)
                    {
                        cNode.Failure = root;
                    }
                    else
                    {
                        cNode.Failure = nNode;
                        cNode.AddResults(cNode.Failure.Outputs);
                    }

                    //add child nodes to BFS list 
                    QueueAddRange(nodesQueue, cNode.Transition.Values);
                }
            }
            root.Failure = root;
            return root;
        }

        protected class Node
        {
            internal char Char;
            internal Node Parent;
            internal Node Failure;

            internal HashSet<string> Outputs;
            internal Dictionary<char, Node> Transition;

            public Node(Node parent, char c)
            {
                Char = c;
                Parent = parent;

                Transition = new Dictionary<char, Node>();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="results"></param>
            public void AddResult(string result)
            {
                if (string.IsNullOrEmpty(result))
                    return;
                if (Outputs == null)
                    Outputs = new HashSet<string>();

                Outputs.Add(result);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="results"></param>
            public void AddResults(IEnumerable<string> results)
            {
                if (results == null)
                    return;
                if (Outputs == null)
                    Outputs = new HashSet<string>();

                foreach (var result in results)
                {
                    Outputs.Add(result);
                }
            }

            public void AddTransition(Node node)
            {
                Transition.Add(node.Char, node);
            }

            public Node GetTransition(char c)
            {
                Node node = null;
                if (Transition.TryGetValue(c, out node))
                    return node;
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="collection"></param>
        private static void QueueAddRange(Queue<Node> queue, IEnumerable<Node> collection)
        {
            if (queue == null || collection == null)
                return;

            foreach (var item in collection)
            {
                queue.Enqueue(item);
            }
        }

        [System.Diagnostics.DebuggerHidden]
        protected static void CheckKeywords(params string[] keywords)
        {
            if (keywords == null)
                throw new ArgumentNullException("keywords");
            if (keywords.Length == 0)
                throw new ArgumentException("keywords");

            foreach (string keyword in keywords)
            {
                if (string.IsNullOrEmpty(keyword))
                    throw new ArgumentException("The keyword set cannot contain null references or empty strings.");
            }
        }

        [System.Diagnostics.DebuggerHidden]
        protected static void CheckArguments(string text, int start, int count)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (text.Length == 0)
                throw new ArgumentException("text");

            if (start < 0)
                throw new ArgumentOutOfRangeException("start");
            if (start >= text.Length)
                throw new ArgumentOutOfRangeException("start");

            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");
        }
    }

    /// <summary>
    /// Container class for <see cref="ISetSearchAlgorithm"/> search results.
    /// </summary>
    public struct SearchResult : IEquatable<SearchResult>
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly SearchResult Empty = new SearchResult(-1, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResult"/> struct.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="match">The match.</param>
        internal SearchResult(int index, string match)
            : this()
        {
            Index = index;
            Match = match;
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; internal set; }

        /// <summary>
        /// Gets the length of the Matched keyword.
        /// </summary>
        /// <value>The length.</value>
        public int Length { get; internal set; }

        /// <summary>
        /// Gets the matched keyword.
        /// </summary>
        /// <value>The matched keyword.</value>
        public string Match { get; internal set; }

        public override string ToString()
        {
            return string.Format("[SearchResult Index:{0}, Length:{1}, Match:{2}]", this.Index, this.Length, this.Match);
        } 

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            SearchResult sr = (SearchResult)obj;
            return Index == sr.Index && Match == sr.Match;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SearchResult other)
        {
            return Index == other.Index && Match == other.Match;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ Match.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="sr1">A SearchResult.</param>
        /// <param name="sr2">A SearchResult.</param>
        /// <returns>The result of the comparison.</returns>
        public static bool operator ==(SearchResult sr1, SearchResult sr2)
        {
            return sr1.Index == sr2.Index && sr1.Match == sr2.Match;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="sr1">A SearchResult.</param>
        /// <param name="sr2">A SearchResult.</param>
        /// <returns>The result of the comparison.</returns>
        public static bool operator !=(SearchResult sr1, SearchResult sr2)
        {
            return sr1.Index != sr2.Index || sr1.Match != sr2.Match;
        }
    }
}

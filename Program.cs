using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace serialize_RyndychRD
{

    public static class Globals
    {
        public static Random rand = new Random();
        public static int rand1 = 0;
        public const string SEPARATOR = "|";
        public const string CYCLE_FLAG = "Cycle";

    }

    class ListNode
    {
        public ListNode Previous;
        public ListNode Next;
        public ListNode Random; // произвольный элемент внутри списка
        public string Data;

        public ListNode()
        {
            //this.Data = Globals.rand.Next().ToString();
            this.Data = Globals.rand1++.ToString();
        }

        public ListNode(string data)
        {
            Data = data;
        }


    }



    class ListRandom
    {
        public ListNode Head;
        public ListNode Tail;
        public int Count;

        //fill random node in current node
        public void addRandomInNode(ListNode node)
        {
            int rand = Globals.rand.Next() % Count;
            ListNode current = Head;
            for (int i = 1; i < rand; i++)
            {
                current = current.Next;
            }
            node.Random = current;
        }

        public void addNode(ListNode node)
        {
            if (Count == 0)
            {
                Head = node;
                Tail = node;
            }
            else
            {
                node.Previous = Tail;
                node.Next = null;
                Tail.Next = node;
                Tail = node;
            }
            Count++;
        }

        //print raw data of list. Made for demonstration purpose
        public void print_assosiated_data()
        {
            ListNode current = Head;
            if (Head != null)
            {
                do
                {
                    Console.WriteLine("Node " + current.Data + " assosiate with node " + current.Random.Data);
                    current = current.Next;
                } while (current != null && current != Head);
                if (current == Head)
                {
                    Console.WriteLine("Cycled");
                }
            }
        }

        //make double linked list 
        public void simple_list_fill(int length)
        {
            for (int i = 0; i < length; i++)
            {
                this.addNode(new ListNode());
            }
            ListNode current = Head;
            while (current != null)
            {
                addRandomInNode(current);
                current = current.Next;
            }

        }

        //add circular for double linked list
        public void make_cycle()
        {
            Head.Previous = Tail;
            Tail.Next = Head;
        }


        public void Serialize(Stream s)
        {
            if (Head != null)
            {
                List<ListNode> list_of_nodes = new List<ListNode>();
                ListNode current = Head;
                do
                {
                    list_of_nodes.Add(current);
                    current = current.Next;
                } while (current != Head && current != null);
                using (StreamWriter stream_writer = new StreamWriter(s))
                {
                    foreach (ListNode node in list_of_nodes)
                        stream_writer.WriteLine(node.Data.ToString() + Globals.SEPARATOR + list_of_nodes.IndexOf(node.Random).ToString());
                    if (current == Head)
                    {
                        stream_writer.WriteLine(Globals.CYCLE_FLAG);
                    }
                }
            }
        }

        public void Deserialize(Stream s)
        {
            List<ListNode> list_of_nodes = new List<ListNode>();
            List<int> list_of_random_nodes = new List<int>();


            Count = 0;
            string stream_line;

            using (StreamReader stream_reader = new StreamReader(s))
            {
                while (!stream_reader.EndOfStream)
                {
                    stream_line = stream_reader.ReadLine();
                    if (stream_line.Equals(Globals.CYCLE_FLAG))
                    {
                        make_cycle();
                    }
                    else
                    {
                        ListNode current = new ListNode(stream_line.Split(Globals.SEPARATOR)[0]);
                        addNode(current);
                        list_of_nodes.Add(current);
                        list_of_random_nodes.Add(Convert.ToInt32(stream_line.Split(Globals.SEPARATOR)[1]));
                    }
                }
            }
            for (int i = 0; i < list_of_random_nodes.Count; i++)
            {
                list_of_nodes[i].Random = list_of_nodes[list_of_random_nodes[i]];
            }

        }
    }

    class Program
    {
        //Check if 2 lists are identical
        static bool is_identical(ListRandom list1, ListRandom list2)
        {
            ListNode current1 = list1.Head;
            ListNode current2 = list2.Head;
            bool result = true;
            while (current1 != null && current2 != null)
            {
                result = current1.Data == current2.Data;
                current1 = current1.Next;
                current2 = current2.Next;
            }
            result = list1.Count == list2.Count;
            return result;
        }

        public static void generate_sequence(ListRandom nodeList )
        {
            //Change here for shorter/bigger generating sequence
            int length = 5;

            if (length < 1)
            {
                Console.WriteLine("You should input int > 0");
                Environment.Exit(-1);
            }

            nodeList.simple_list_fill(length);

            //Unlock if you want circular double linked list

            //nodeList.make_cycle();
        }

        static void Main(string[] args)
        {
            ListRandom nodeList = new ListRandom();
            ListRandom nodeList2 = new ListRandom();

            generate_sequence(nodeList);
            
            Console.WriteLine("__Generated list__");
            nodeList.print_assosiated_data();
            
            FileStream fs;
            try
            {
                fs = new FileStream("serialized_node_list", FileMode.Create);
                nodeList.Serialize(fs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot create new file or serialize stream");
                Console.WriteLine(e);
                Environment.Exit(-1);
            }

            try
            {
                Console.WriteLine("__Serialized list__");
                Console.Write(File.ReadAllText("serialized_node_list"));
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot access to serialized node list file");
                Console.WriteLine(e);
            }


            try
            {
                fs = new FileStream("serialized_node_list", FileMode.Open);
                nodeList2.Deserialize(fs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot create open data file or deserialize stream");
                Console.WriteLine(e);
                Environment.Exit(-1);
            }


            Console.WriteLine("__Deserialized list__");

            nodeList2.print_assosiated_data();

            if (is_identical(nodeList, nodeList2))
            {
                Console.WriteLine("Success");
            }


        }
    }
}

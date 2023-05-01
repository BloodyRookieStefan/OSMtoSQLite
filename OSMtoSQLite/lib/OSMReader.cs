using OSMConverter.lib;
using OSMConverter.OSMTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OSMConverter
{
    internal class OSMReader
    {
        internal static List<Node> Nodes = new List<Node>();
        internal static List<Way> Ways = new List<Way>();
        internal static List<Relation> Relations = new List<Relation>();

        internal static TimeSpan ElapsedTime;                                   // Elapsed time for reading OSM document

        /// <summary>
        /// Read OSM file
        /// </summary>
        /// <param name="input">Path to OSM file</param>
        internal static void Read(string input)
        {
            // Start time
            DateTime start = DateTime.UtcNow;

            // Get in clear state
            Nodes.Clear();
            Ways.Clear();
            Relations.Clear();
            ElapsedTime = TimeSpan.Zero;

            // Read XML styled file
            using (XmlReader reader = XmlReader.Create(input))
            {
                Node? node = null;
                Way? way = null;
                Relation? relation = null;  

                List<NodeRef> nodeRefs = new List<NodeRef>();
                List<Tag> tags = new List<Tag>();
                List<Member> members = new List<Member>();

                // Run trough all lines
                reader.MoveToContent();
                while (reader.Read())
                {
                    // Start tag XML node
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // Store value before open new one
                        if((reader.Name == "node" || reader.Name == "way" || reader.Name == "relation") &&
                            (node != null || way != null || relation != null))
                        {
                            StoreToList(node, way, relation, nodeRefs, tags, members);

                            // Reset
                            node = null;
                            way = null;
                            relation = null;

                            nodeRefs.Clear();
                            tags.Clear();
                            members.Clear();    
                        }

                        // Parent nodes
                        if (reader.Name == "node")
                        {
                            node = ReadNode(reader);
                            continue;
                        }
                        else if(reader.Name == "way")
                        {
                            way = ReadWay(reader);
                            continue;
                        }
                        else if (reader.Name == "relation")
                        {
                            relation = ReadRelation(reader);
                            continue;
                        }

                        // Sub nodes
                        if (reader.Name == "tag")
                        {
                            tags.Add(ReadTag(reader));
                            continue;
                        }
                        else if(reader.Name == "nd")
                        {
                            nodeRefs.Add(ReadNodeRef(reader));
                            continue;
                        }
                        else if(reader.Name == "member")
                        {
                            members.Add(ReadMember(reader));
                            continue;
                        }
                    }
                    // Last element - End of document
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "osm")
                            StoreToList(node, way, relation, nodeRefs, tags, members);
                    }
                }
            }

            // End time
            ElapsedTime = DateTime.UtcNow - start;
        }

        /// <summary>
        /// Store struct in list
        /// </summary>
        /// <param name="node">Node struct</param>
        /// <param name="way">Way struct</param>
        /// <param name="relation">Relation struct</param>
        /// <param name="nodeRefs">List of node references</param>
        /// <param name="tags">List of tags</param>
        /// <param name="members">List of members</param>
        /// <exception cref="Exception">Invalid handover to store XML node</exception>
        private static void StoreToList(Node? node, Way? way, Relation? relation, List<NodeRef> nodeRefs, List<Tag> tags, List<Member> members)
        {
            int i = 0;

            // Check which node has been recived => Store it in list
            if (node != null)
            {
                Node n = (Node)node;
                n.TAGS = new List<Tag>(tags);

                Nodes.Add(n);
                i++;
            }
            else if(way != null)
            {
                Way w = (Way)way;
                w.TAGS = new List<Tag>(tags);
                w.NODEREFERENCES = new List<NodeRef>(nodeRefs);

                Ways.Add(w);
                i++;
            }
            else if(relation != null)
            {
                Relation r = (Relation)relation;
                r.TAGS = new List<Tag>(tags);
                r.MEMBERS = new List<Member>(members);

                Relations.Add(r);
                i++;
            }
            else
            {
                throw new Exception("Could not store parent node. No XML node recived");
            }

            // Check only one XML node was stored
            if(i != 1)
            {
                throw new Exception("Could not store parent node. Multiple XML nodes recived");
            }
        }

        #region Read functions
        /// <summary>
        /// Read OSM node type
        /// </summary>
        /// <param name="reader">Current XML reader</param>
        /// <returns>Node struct</returns>
        /// <exception cref="InvalidOperationException">Invalid xml format</exception>
        private static Node ReadNode(XmlReader reader)
        {
            // Create new node
            Node node = new Node();
            // Read attributes
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Name == "id")
                {
                    node.ID = ReadFormatedValue(reader);
                }
                else if (reader.Name == "version")
                {
                    node.VERSION = ReadFormatedValue(reader);
                }
                else if (reader.Name == "timestamp")
                {
                    node.TIMESTAMP = ReadFormatedValue(reader);
                }
                else if (reader.Name == "lat")
                {
                    node.LAT = ReadFormatedValue(reader);
                }
                else if (reader.Name == "lon")
                {
                    node.LONG = ReadFormatedValue(reader);
                }
                else
                {
                    Console.WriteLine($"WARNING: Unknown attribute in ReadNode {reader.Value}");
                }
            }

            // Get back to element
            reader.MoveToElement();

            return node;
        }

        /// <summary>
        /// Read OSM way type
        /// </summary>
        /// <param name="reader">Current XML reader</param>
        /// <returns>Way struct</returns>
        /// <exception cref="InvalidOperationException">Invalid xml format</exception>
        private static Way ReadWay(XmlReader reader)
        {
            // Create new node
            Way way = new Way();
            // Read attributes
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Name == "id")
                {
                    way.ID = ReadFormatedValue(reader);
                }
                else if (reader.Name == "version")
                {
                    way.VERSION = ReadFormatedValue(reader);
                }
                else if (reader.Name == "timestamp")
                {
                    way.TIMESTAMP = ReadFormatedValue(reader);
                }
                else
                {
                    Console.WriteLine($"WARNING: Unknown attribute in ReadWay {reader.Value}");
                }
            }

            // Get back to element
            reader.MoveToElement();

            return way;
        }

        /// <summary>
        /// Read OSM relation type
        /// </summary>
        /// <param name="reader">Current XML reader</param>
        /// <returns>Relation struct</returns>
        /// <exception cref="InvalidOperationException">Invalid xml format</exception>
        private static Relation ReadRelation(XmlReader reader)
        {
            // Create new node
            Relation relation = new Relation();
            // Read attributes
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Name == "id")
                {
                    relation.ID = ReadFormatedValue(reader);
                }
                else if (reader.Name == "version")
                {
                    relation.VERSION = ReadFormatedValue(reader);
                }
                else if (reader.Name == "timestamp")
                {
                    relation.TIMESTAMP = ReadFormatedValue(reader);
                }
                else
                {
                    Console.WriteLine($"WARNING: Unknown attribute in ReadRelation {reader.Value}");
                }
            }

            // Get back to element
            reader.MoveToElement();

            return relation;
        }

        /// <summary>
        /// Read OSM NodeRef type
        /// </summary>
        /// <param name="reader">Current XML reader</param>
        /// <returns>NodeRef struct</returns>
        /// <exception cref="InvalidOperationException">Invalid xml format</exception>
        private static NodeRef ReadNodeRef(XmlReader reader)
        {
            // Create new tag
            NodeRef nodeRef = new NodeRef();
            // Read attributes
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Name == "ref")
                {
                    nodeRef.ID = ReadFormatedValue(reader);
                }
                else
                {
                    Console.WriteLine($"WARNING: Unknown attribute in ReadNodeRef {reader.Value}");
                }
            }

            // Get back to element
            reader.MoveToElement();

            return nodeRef;
        }

        /// <summary>
        /// Read OSM Tag type
        /// </summary>
        /// <param name="reader">Current XML reader</param>
        /// <returns>Tag struct</returns>
        /// <exception cref="InvalidOperationException">Invalid xml format</exception>
        private static Tag ReadTag(XmlReader reader)
        {
            // Create new tag
            Tag tag = new Tag();
            // Read attributes
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Name == "k")
                {
                    tag.KEY = ReadFormatedValue(reader);
                }
                else if (reader.Name == "v")
                {
                    tag.VALUE = ReadFormatedValue(reader);
                }
                else
                {
                    Console.WriteLine($"WARNING: Unknown attribute in ReadTag {reader.Value}");
                }
            }

            // Get back to element
            reader.MoveToElement();

            return tag;
        }

        /// <summary>
        /// Read OSM Member type
        /// </summary>
        /// <param name="reader">Current XML reader</param>
        /// <returns>Member struct</returns>
        /// <exception cref="InvalidOperationException">Invalid xml format</exception>
        private static Member ReadMember(XmlReader reader)
        {
            // Create new tag
            Member member = new Member();
            // Read attributes
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Name == "type")
                {
                    member.TYPE = ReadFormatedValue(reader);
                }
                else if (reader.Name == "ref")
                {
                    member.REF = ReadFormatedValue(reader);
                }
                else if (reader.Name == "role")
                {
                    member.ROLE = ReadFormatedValue(reader);
                }
                else
                {
                    Console.WriteLine($"WARNING: Unknown attribute in ReadMember {reader.Value}");
                }
            }

            // Get back to element
            reader.MoveToElement();

            return member;
        }
        #endregion

        /// <summary>
        /// Read value
        /// </summary>
        /// <param name="reader">Current XML reader</param>
        /// <returns>Formatted string</returns>
        private static string ReadFormatedValue(XmlReader reader)
        {
            string val = reader.Value;
            return val.Trim().Replace("\"", string.Empty).Replace("\\", string.Empty);
        }
    }
}

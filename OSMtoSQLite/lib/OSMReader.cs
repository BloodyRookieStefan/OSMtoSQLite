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
    /// <summary>
    /// Reads OpenStreetMap XML files and populates in-memory collections of OSM elements.
    /// </summary>
    /// <remarks>
    /// This reader is a simple streaming XML reader that parses <c>node</c>, <c>way</c> and <c>relation</c> elements
    /// and their child elements (<c>tag</c>, <c>nd</c>, <c>member</c>), storing the parsed structures into the
    /// static lists exposed by this class: <see cref="Nodes"/>, <see cref="Ways"/> and <see cref="Relations"/>.
    /// The collections are cleared at the start of each <see cref="Read(string)"/> call.
    /// </remarks>
    internal class OSMReader
    {
        /// <summary>
        /// Collection of parsed OSM nodes from the last <see cref="Read(string)"/> invocation.
        /// </summary>
        /// <remarks>
        /// Each entry is an instance of <see cref="Node"/>. Consumers should treat the list as the result of the last read operation.
        /// The list is cleared when <see cref="Read(string)"/> is called.
        /// </remarks>
        internal static List<Node> Nodes = new List<Node>();

        /// <summary>
        /// Collection of parsed OSM ways from the last <see cref="Read(string)"/> invocation.
        /// </summary>
        /// <remarks>
        /// Each entry is an instance of <see cref="Way"/>. Way objects include their <see cref="Way.NODEREFERENCES"/> and <see cref="Way.TAGS"/>.
        /// </remarks>
        internal static List<Way> Ways = new List<Way>();

        /// <summary>
        /// Collection of parsed OSM relations from the last <see cref="Read(string)"/> invocation.
        /// </summary>
        /// <remarks>
        /// Each entry is an instance of <see cref="Relation"/>. Relation objects include their <see cref="Relation.MEMBERS"/> and <see cref="Relation.TAGS"/>.
        /// </remarks>
        internal static List<Relation> Relations = new List<Relation>();

        /// <summary>
        /// Read an OSM XML file and populate the static node/way/relation collections.
        /// </summary>
        /// <param name="input">Path to the OSM XML file to read. The file must be accessible to the running process.</param>
        /// <remarks>
        /// This method clears the current contents of <see cref="Nodes"/>, <see cref="Ways"/> and <see cref="Relations"/> before reading.
        /// It performs a streaming read using <see cref="XmlReader"/> to minimize memory overhead relative to DOM parsing,
        /// but still stores all parsed entities in memory.
        /// </remarks>
        internal static void Read(string input)
        {

            // Get in clear state
            Nodes.Clear();
            Ways.Clear();
            Relations.Clear();

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
        }

        /// <summary>
        /// Store a parsed parent element (<c>node</c>, <c>way</c> or <c>relation</c>) into the corresponding static collection.
        /// </summary>
        /// <param name="node">Node struct to store, or <c>null</c> if not applicable.</param>
        /// <param name="way">Way struct to store, or <c>null</c> if not applicable.</param>
        /// <param name="relation">Relation struct to store, or <c>null</c> if not applicable.</param>
        /// <param name="nodeRefs">List of node references to assign to the way (copied into the way's <c>NODEREFERENCES</c>).</param>
        /// <param name="tags">List of tags to assign to the element (copied into the element's <c>TAGS</c>).</param>
        /// <param name="members">List of members to assign to the relation (copied into the relation's <c>MEMBERS</c>).</param>
        /// <exception cref="Exception">
        /// Thrown when neither <paramref name="node"/>, <paramref name="way"/> nor <paramref name="relation"/> is provided
        /// or when more than one parent element is provided. These indicate an internal parsing state error.
        /// </exception>
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
        /// Parse a <c>node</c> element's attributes into a <see cref="Node"/> struct.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned on a <c>node</c> start element.</param>
        /// <returns>A <see cref="Node"/> populated with attribute values found on the element.</returns>
        /// <exception cref="InvalidOperationException">May be thrown by <see cref="XmlReader"/> operations if the reader is in an invalid state.</exception>
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
        /// Parse a <c>way</c> element's attributes into a <see cref="Way"/> struct.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned on a <c>way</c> start element.</param>
        /// <returns>A <see cref="Way"/> populated with attribute values found on the element.</returns>
        /// <exception cref="InvalidOperationException">May be thrown by <see cref="XmlReader"/> operations if the reader is in an invalid state.</exception>
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
        /// Parse a <c>relation</c> element's attributes into a <see cref="Relation"/> struct.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned on a <c>relation</c> start element.</param>
        /// <returns>A <see cref="Relation"/> populated with attribute values found on the element.</returns>
        /// <exception cref="InvalidOperationException">May be thrown by <see cref="XmlReader"/> operations if the reader is in an invalid state.</exception>
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
        /// Parse an <c>nd</c> element (node reference) into a <see cref="NodeRef"/> struct.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned on an <c>nd</c> start element.</param>
        /// <returns>A <see cref="NodeRef"/> that contains the referenced node id.</returns>
        /// <exception cref="InvalidOperationException">May be thrown by <see cref="XmlReader"/> operations if the reader is in an invalid state.</exception>
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
        /// Parse a <c>tag</c> element into a <see cref="Tag"/> struct.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned on a <c>tag</c> start element.</param>
        /// <returns>A <see cref="Tag"/> with <see cref="Tag.KEY"/> and <see cref="Tag.VALUE"/> set from the element's attributes.</returns>
        /// <exception cref="InvalidOperationException">May be thrown by <see cref="XmlReader"/> operations if the reader is in an invalid state.</exception>
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
        /// Parse a <c>member</c> element into a <see cref="Member"/> struct.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> positioned on a <c>member</c> start element.</param>
        /// <returns>A <see cref="Member"/> with <see cref="Member.TYPE"/>, <see cref="Member.REF"/> and <see cref="Member.ROLE"/> populated.</returns>
        /// <exception cref="InvalidOperationException">May be thrown by <see cref="XmlReader"/> operations if the reader is in an invalid state.</exception>
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
        /// Read and sanitize an attribute value from the current <see cref="XmlReader"/> attribute.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> positioned on an attribute (after <c>MoveToAttribute</c>).</param>
        /// <returns>
        /// The attribute value trimmed of whitespace and with double quotes and backslashes removed.
        /// This method does not validate specific formats (e.g., numeric ids or timestamps).
        /// </returns>
        private static string ReadFormatedValue(XmlReader reader)
        {
            string val = reader.Value;
            return val.Trim().Replace("\"", string.Empty).Replace("\\", string.Empty);
        }
    }
}

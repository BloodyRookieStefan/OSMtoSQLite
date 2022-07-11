using OSMConverter.lib;
using OSMConverter.OSMTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OSMConverter
{
    internal class OSMReader
    {
        private static Node? Node = null;
        private static Way? Way = null;
        private static Relation? Relation = null;
        private static List<NodeRef> NodeRefs = new List<NodeRef>();
        private static List<Tag> Tags = new List<Tag>();
        private static List<Member> Members = new List<Member>();

        internal static TimeSpan ElapsedTime = TimeSpan.Zero;
        internal static long TotalNodes = 0;
        internal static long TotalWays = 0;
        internal static long TotalRelations = 0;

        /// <summary>
        /// Read OSM file
        /// </summary>
        /// <param name="input">Path to OSM file</param>
        internal static void Read(string input)
        {
            // Start time
            DateTime start = DateTime.UtcNow;

            // Read XML styled file
            using (XmlReader reader = XmlReader.Create(input))
            {
                // Run trough all lines
                reader.MoveToContent();
                while (reader.Read())
                {
                    // Start node
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // Handle type
                        switch(reader.Name)
                        {
                            case "node":
                                // Check if single xml node without sub nodes
                                if (Node != null)
                                    SendNODEtoSQL(Node, null);
                                // Read xml node
                                Node = ReadNode(reader);
                                break;
                            case "way":
                                // Check if single xml node without sub nodes
                                if (Way != null)
                                    SendWAYtoSQL(Way, null, null);
                                // Read xml node
                                Way = ReadWay(reader);
                                break;
                            case "relation":
                                // Check if single xml node without sub nodes
                                if (Relation != null)
                                    SendRELATIONtoSQL(Relation, null, null);
                                // Read xml node
                                Relation = ReadRelation(reader);
                                break;
                            case "nd":
                                // Read xml node
                                NodeRefs.Add(ReadNodeRef(reader));
                                break;
                            case "tag":
                                // Read xml node
                                Tags.Add(ReadTag(reader));
                                break;
                            case "member":
                                // Read xml node
                                Members.Add(ReadMember(reader));
                                break;
                        }
                    }
                    // End node
                    else if(reader.NodeType == XmlNodeType.EndElement)
                    {
                        // Check type of parent xml node and
                        // send to SQLite database
                        if(Node != null)
                            SendNODEtoSQL(Node, Tags);
                        else if (Way != null)
                            SendWAYtoSQL(Way, NodeRefs, Tags);
                        else if (Relation != null)
                            SendRELATIONtoSQL(Relation, Members, Tags);
                    }
                }
            }

            // End time
            ElapsedTime = DateTime.UtcNow - start;

            // Do a cleanup after we are done
            TempFolder.CleanUpTempFolder();
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

                switch (i)
                {
                    case 0:
                        node.ID = reader.Value;
                        break;
                    case 1:
                        node.VISIBLE = reader.Value;
                        break;
                    case 2:
                        node.VERSION = reader.Value;
                        break;
                    case 3:
                        node.CHANGESET = reader.Value;
                        break;
                    case 4:
                        node.TIMESTAMP = reader.Value;
                        break;
                    case 5:
                        node.USER = reader.Value;
                        break;
                    case 6:
                        node.UID = reader.Value;
                        break;
                    case 7:
                        node.LAT = reader.Value;
                        break;
                    case 8:
                        node.LONG = reader.Value;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid number of attributes for NODE");
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

                switch (i)
                {
                    case 0:
                        way.ID = reader.Value;
                        break;
                    case 1:
                        way.VISIBLE = reader.Value;
                        break;
                    case 2:
                        way.VERSION = reader.Value;
                        break;
                    case 3:
                        way.CHANGESET = reader.Value;
                        break;
                    case 4:
                        way.TIMESTAMP = reader.Value;
                        break;
                    case 5:
                        way.USER = reader.Value;
                        break;
                    case 6:
                        way.UID = reader.Value;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid number of attributes for WAY");
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

                switch (i)
                {
                    case 0:
                        relation.ID = reader.Value;
                        break;
                    case 1:
                        relation.VISIBLE = reader.Value;
                        break;
                    case 2:
                        relation.VERSION = reader.Value;
                        break;
                    case 3:
                        relation.CHANGESET = reader.Value;
                        break;
                    case 4:
                        relation.TIMESTAMP = reader.Value;
                        break;
                    case 5:
                        relation.USER = reader.Value;
                        break;
                    case 6:
                        relation.UID = reader.Value;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid number of attributes for RELATION");
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

                switch (i)
                {
                    case 0:
                        nodeRef.ID = reader.Value;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid number of attributes for NODE REFERENCE");
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

                switch (i)
                {
                    case 0:
                        tag.KEY = reader.Value;
                        break;
                    case 1:
                        tag.VALUE = reader.Value;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid number of attributes for TAG");
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

                switch (i)
                {
                    case 0:
                        member.TYPE = reader.Value;
                        break;
                    case 1:
                        member.REF = reader.Value;
                        break;
                    case 2:
                        member.ROLE = reader.Value;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid number of attributes for TAG");
                }
            }

            // Get back to element
            reader.MoveToElement();

            return member;
        }
        #endregion

        /// <summary>
        /// Cleanup global data
        /// </summary>
        private static void CleanUp()
        {
            Node = null;
            Way = null;
            Relation = null;
            NodeRefs.Clear();
            Tags.Clear();
            Members.Clear();
        }

        private static void SendNODEtoSQL(Node? node, List<Tag> tags)
        {
            TotalNodes++;

            // TODO
            if (tags != null && tags.Count > 0)
                Console.WriteLine($"DEBUG: Send NODE: {node.Value.ID} - Number of tags: {tags.Count}");
            else
                Console.WriteLine($"DEBUG: Send NODE: {node.Value.ID}");

            CleanUp();
        }

        private static void SendWAYtoSQL(Way? way, List<NodeRef> nodeRefs, List<Tag> tags)
        {
            TotalWays++;

            // TODO

            if (tags != null && tags.Count > 0 && nodeRefs != null && nodeRefs.Count > 0)
                Console.WriteLine($"DEBUG: Send WAY: {way.Value.ID} - Number of nodeRefs: {nodeRefs.Count} - Number of tags: {tags.Count}");
            else if (tags != null && tags.Count > 0 )
                Console.WriteLine($"DEBUG: Send WAY: {way.Value.ID} - Number of tags: {tags.Count}");
            else if ( nodeRefs != null && nodeRefs.Count > 0)
                Console.WriteLine($"DEBUG: Send WAY: {way.Value.ID} - Number of nodeRefs: {nodeRefs.Count}");
            else
                Console.WriteLine($"DEBUG: Send WAY: {way.Value.ID}");

            CleanUp();
        }

        private static void SendRELATIONtoSQL(Relation? way, List<Member> members, List<Tag> tags)
        {
            TotalRelations++;

            // TODO

            if (tags != null && tags.Count > 0 && members != null && members.Count > 0)
                Console.WriteLine($"DEBUG: Send RELATION: {way.Value.ID} - Number of members: {members.Count} - Number of tags: {tags.Count}");
            else if (tags != null && tags.Count > 0)
                Console.WriteLine($"DEBUG: Send RELATION: {way.Value.ID} - Number of tags: {tags.Count}");
            else if (members != null && members.Count > 0)
                Console.WriteLine($"DEBUG: Send RELATION: {way.Value.ID} - Number of members: {members.Count}");
            else
                Console.WriteLine($"DEBUG: Send RELATION: {way.Value.ID}");

            CleanUp();
        }
    }
}

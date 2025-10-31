using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter.OSMTypes
{
    /// <summary>
    /// Represents an OpenStreetMap node element.
    /// Contains a unique identifier, version and timestamp metadata, geographic coordinates and optional tags.
    /// </summary>
    internal struct Node
    {
        /// <summary>
        /// The node's unique identifier as provided by OSM (usually a numeric string).
        /// </summary>
        public string ID;

        /// <summary>
        /// The version string of the node (OSM version number as string).
        /// </summary>
        public string VERSION;

        /// <summary>
        /// The timestamp of the node's last modification (ISO 8601 string as provided by OSM).
        /// </summary>
        public string TIMESTAMP;

        /// <summary>
        /// The latitude of the node in decimal degrees (string representation).
        /// </summary>
        public string LAT;

        /// <summary>
        /// The longitude of the node in decimal degrees (string representation).
        /// </summary>
        public string LONG;

        /// <summary>
        /// A list of tags associated with the node. May be null or empty if the node has no tags.
        /// Each element is a <see cref="Tag"/>.
        /// </summary>
        public List<Tag> TAGS; 
    }

    /// <summary>
    /// Represents an OpenStreetMap way element.
    /// Contains identifier and metadata, associated tags and an ordered list of node references.
    /// </summary>
    internal struct Way
    {
        /// <summary>
        /// The way's unique identifier as provided by OSM (usually a numeric string).
        /// </summary>
        public string ID;

        /// <summary>
        /// The version string of the way (OSM version number as string).
        /// </summary>
        public string VERSION;

        /// <summary>
        /// The timestamp of the way's last modification (ISO 8601 string as provided by OSM).
        /// </summary>
        public string TIMESTAMP;

        /// <summary>
        /// A list of tags associated with the way. May be null or empty if the way has no tags.
        /// Each element is a <see cref="Tag"/>.
        /// </summary>
        public List<Tag> TAGS;

        /// <summary>
        /// Ordered list of node references that define the geometry of the way.
        /// Each element is a <see cref="NodeRef"/>. May be null or empty for malformed/empty ways.
        /// </summary>
        public List<NodeRef> NODEREFERENCES;
    }

    /// <summary>
    /// Represents an OpenStreetMap relation element.
    /// Contains identifier and metadata, associated tags and a list of members.
    /// </summary>
    internal struct Relation
    {
        /// <summary>
        /// The relation's unique identifier as provided by OSM (usually a numeric string).
        /// </summary>
        public string ID;

        /// <summary>
        /// The version string of the relation (OSM version number as string).
        /// </summary>
        public string VERSION;

        /// <summary>
        /// The timestamp of the relation's last modification (ISO 8601 string as provided by OSM).
        /// </summary>
        public string TIMESTAMP;

        /// <summary>
        /// A list of tags associated with the relation. May be null or empty if the relation has no tags.
        /// Each element is a <see cref="Tag"/>.
        /// </summary>
        public List<Tag> TAGS;

        /// <summary>
        /// A list of members that make up the relation. Each element is a <see cref="Member"/>.
        /// May be null or empty for empty relations.
        /// </summary>
        public List<Member> MEMBERS;
    }

    /// <summary>
    /// Represents a key/value tag associated with an OSM element.
    /// </summary>
    internal struct Tag
    {
        /// <summary>
        /// The tag's key (e.g., "highway", "name").
        /// </summary>
        public string KEY;

        /// <summary>
        /// The tag's value corresponding to <see cref="KEY"/>.
        /// </summary>
        public string VALUE;
    }

    /// <summary>
    /// Represents a reference to a node by identifier (used by ways).
    /// </summary>
    internal struct NodeRef
    {
        /// <summary>
        /// The referenced node's identifier (usually a numeric string).
        /// </summary>
        public string ID;
    }

    /// <summary>
    /// Represents a member of a relation, including its type, reference id and role.
    /// </summary>
    internal struct Member
    {
        /// <summary>
        /// The member type (e.g., "node", "way", "relation").
        /// </summary>
        public string TYPE;

        /// <summary>
        /// The referenced element's identifier (usually a numeric string).
        /// </summary>
        public string REF;

        /// <summary>
        /// The role of the member within the relation (may be empty string).
        /// </summary>
        public string ROLE;
    }
}

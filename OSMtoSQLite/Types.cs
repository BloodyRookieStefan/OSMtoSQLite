using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter.OSMTypes
{
    internal struct Node
    {
        public string ID;
        public string VISIBLE;
        public string VERSION;
        public string CHANGESET;
        public string TIMESTAMP;
        public string USER;
        public string UID;
        public string LAT;
        public string LONG;
    }

    internal struct Tag
    {
        public string KEY;
        public string VALUE;
    }

    internal struct Way
    {
        public string ID;
        public string VISIBLE;
        public string VERSION;
        public string CHANGESET;
        public string TIMESTAMP;
        public string USER;
        public string UID;
    }

    internal struct NodeRef
    {
        public string ID;
    }

    internal struct Relation
    {
        public string ID;
        public string VISIBLE;
        public string VERSION;
        public string CHANGESET;
        public string TIMESTAMP;
        public string USER;
        public string UID;
    }

    internal struct Member
    {
        public string TYPE;
        public string REF;
        public string ROLE;
    }
}

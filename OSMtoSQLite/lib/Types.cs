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
        public string VERSION;
        public string TIMESTAMP;
        public string LAT;
        public string LONG;

        public List<Tag> TAGS; 
    }

    internal struct Way
    {
        public string ID;
        public string VERSION;
        public string TIMESTAMP;

        public List<Tag> TAGS;
        public List<NodeRef> NODEREFERENCES;
    }

    internal struct Relation
    {
        public string ID;
        public string VERSION;
        public string TIMESTAMP;

        public List<Tag> TAGS;
        public List<Member> MEMBERS;
    }

    internal struct Tag
    {
        public string KEY;
        public string VALUE;
    }

    internal struct NodeRef
    {
        public string ID;
    }

    internal struct Member
    {
        public string TYPE;
        public string REF;
        public string ROLE;
    }
}

﻿using System.Collections.Generic;
using System.Linq;

namespace GenogramGenerator.Core
{
    public class Family
    {
        private readonly List<Relationship> _relationships = new List<Relationship>();

        public IEnumerable<Person> Members()
        {
            return _relationships.Select(r => r.A).Union(_relationships.Select(r => r.B));
        }

        public IEnumerable<Person> SiblingsOf(Person person)
        {
            var parents = ParentsOf(person);
            return ChildrenOf(parents).Where(s => s != person);
        }

        public IEnumerable<Person> ParentsOf(Person person)
        {
            return _relationships.Where(r => r.B == person && r.Type == RelationshipType.Parent).Select(r => r.A);
        }

        public IEnumerable<Person> ChildrenOf(IEnumerable<Person> parents)
        {
            if (!parents.Any())
                return Enumerable.Empty<Person>();

            return parents.Skip(1)
                .Aggregate(ChildrenOf(parents.First()),
                           (current, parent) => current.Intersect(ChildrenOf(parent)));
        }

        public IEnumerable<Person> ChildrenOf(Person parent)
        {
            return _relationships.Where(r => r.Type == RelationshipType.Parent && r.A == parent).Select(r => r.B);
        }

        public Person Add(string name)
        {
            return new Person { Name = name, Family = this };
        }

        public Relationship Add(Person person)
        {
            var relationship = new Relationship { A = person };
            _relationships.Add(relationship);
            return relationship;
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public Family Family { get; set; }

        public Relationship Is { get { return Family.Add(this); } }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Relationship
    {
        public Person A { get; set; }
        public Person B { get; set; }
        public RelationshipType Type { get; set; }

        public IEnumerable<Person> Relatives
        {
            get
            {
                yield return A;
                yield return B;
            }
        }

        public void TheParentOf(Person person)
        {
            Set(person, RelationshipType.Parent);
        }

        public void TheChildOf(Person parent1, Person parent2)
        {
            var child = A;
            A = parent1;
            Set(child, RelationshipType.Parent);
            parent2.Is.TheParentOf(child);
        }

        public void MarriedTo(Person person)
        {
            Set(person, RelationshipType.Spouse);
        }

        public void DivorcedFrom(Person person)
        {
            Set(person, RelationshipType.Divorced);
        }

        public void TheRomanticPartnerOf(Person person)
        {
            Set(person, RelationshipType.Romance);
        }

        private void Set(Person b, RelationshipType type)
        {
            B = b;
            Type = type;
        }
    }

    public enum RelationshipType
    {
        Parent,
        Spouse,
        Divorced,
        Romance
    }
}
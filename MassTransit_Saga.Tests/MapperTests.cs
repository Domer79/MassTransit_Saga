using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NUnit.Framework;

namespace MassTransit_Saga.Tests
{
    [TestFixture]
    public class MapperTests
    {
        [Test]
        public void MemberAssignTest()
        {
            Mapper.Initialize(c =>
            {
                c.CreateMap<Foo, Bar>();
            });

            var source = new Foo { Notes = "source" };

            var destination = new Bar { Notes = "destination" };

            Mapper.Map<Foo, Bar>(source, destination);

            Console.WriteLine(destination.Notes);
        }

        [Test]
        public void CoreCountInTheComp()
        {
            Console.WriteLine(Environment.ProcessorCount);
        }
    }

    public class Foo
    {
        public string Notes { get; set; }
    }

    public class Bar
    {
        public string Notes { get; set; }
    }
}

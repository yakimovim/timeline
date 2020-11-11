using AutoFixture.Xunit2;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class HierarchyTests
    {
        [Theory]
        [AutoData]
        public void Empty_hierarchy(string nodeId)
        {
            var hierarchy = new Hierarchy<string>();

            hierarchy.TopNodes.Count.ShouldBe(0);
            hierarchy.ContainsNodeWithId(nodeId).ShouldBeFalse();
            hierarchy.GetNodeById(nodeId).ShouldBeNull();
        }

        [Theory]
        [AutoData]
        public void Add_top_level_node(string nodeId, string content)
        {
            var hierarchy = new Hierarchy<string>();

            hierarchy.AddTopNode(nodeId, content).ShouldBeTrue();

            hierarchy.ContainsNodeWithId(nodeId).ShouldBeTrue();

            hierarchy.TopNodes.Count.ShouldBe(1);

            var node = hierarchy.TopNodes[0];

            node.ShouldNotBeNull();
            node.Id.ShouldBe(nodeId);
            node.Content.ShouldBe(content);

            hierarchy.GetNodeById(nodeId).ShouldBeSameAs(node);
        }
    }
}

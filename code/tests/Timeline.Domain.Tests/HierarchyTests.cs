using AutoFixture.Xunit2;
using Shouldly;
using System.Linq;
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
            hierarchy.Count().ShouldBe(1);

            var node = hierarchy.TopNodes[0];

            node.ShouldNotBeNull();
            node.Id.ShouldBe<StringId>(nodeId);
            node.Content.ShouldBe(content);
            node.Left.ShouldBe(0);
            node.Right.ShouldBe(1);

            hierarchy.GetNodeById(nodeId).ShouldBeSameAs(node);
        }

        [Theory]
        [AutoData]
        public void Unable_to_add_several_top_level_nodes_with_same_id(string nodeId, string content)
        {
            var hierarchy = new Hierarchy<string>();

            hierarchy.AddTopNode(nodeId, content).ShouldBeTrue();

            hierarchy.AddTopNode(nodeId, "something").ShouldBeFalse();

            hierarchy.TopNodes.Count.ShouldBe(1);
            hierarchy.Count().ShouldBe(1);
        }

        [Theory]
        [AutoData]
        public void Add_several_top_level_nodes(string nodeIdPrefix, string content)
        {
            var hierarchy = new Hierarchy<string>();

            hierarchy.AddTopNode(nodeIdPrefix + "_1", content).ShouldBeTrue();
            hierarchy.AddTopNode(nodeIdPrefix + "_2", content).ShouldBeTrue();
            hierarchy.AddTopNode(nodeIdPrefix + "_3", content).ShouldBeTrue();

            hierarchy.TopNodes.Count.ShouldBe(3);
            hierarchy.Count().ShouldBe(3);

            hierarchy.ContainsNodeWithId(nodeIdPrefix + "_1").ShouldBeTrue();
            hierarchy.ContainsNodeWithId(nodeIdPrefix + "_2").ShouldBeTrue();
            hierarchy.ContainsNodeWithId(nodeIdPrefix + "_3").ShouldBeTrue();

            hierarchy.TopNodes[0].Left.ShouldBe(0);
            hierarchy.TopNodes[0].Right.ShouldBe(1);
            hierarchy.TopNodes[1].Left.ShouldBe(2);
            hierarchy.TopNodes[1].Right.ShouldBe(3);
            hierarchy.TopNodes[2].Left.ShouldBe(4);
            hierarchy.TopNodes[2].Right.ShouldBe(5);
        }

        [Theory]
        [AutoData]
        public void Add_sub_nodes(string nodeIdPrefix)
        {
            var hierarchy = new Hierarchy<string>();

            hierarchy.AddTopNode(nodeIdPrefix + "_1", "1").ShouldBeTrue();
            hierarchy.GetNodeById(nodeIdPrefix + "_1")
                .AddSubNode(nodeIdPrefix + "_2", "2").ShouldBeTrue();
            hierarchy.GetNodeById(nodeIdPrefix + "_2")
                .AddSubNode(nodeIdPrefix + "_3", "3").ShouldBeTrue();

            hierarchy.TopNodes.Count.ShouldBe(1);
            hierarchy.Count().ShouldBe(3);


            hierarchy.ContainsNodeWithId(nodeIdPrefix + "_1").ShouldBeTrue();
            hierarchy.ContainsNodeWithId(nodeIdPrefix + "_2").ShouldBeTrue();
            hierarchy.ContainsNodeWithId(nodeIdPrefix + "_3").ShouldBeTrue();

            hierarchy.GetNodeById(nodeIdPrefix + "_1").Left.ShouldBe(0);
            hierarchy.GetNodeById(nodeIdPrefix + "_2").Left.ShouldBe(1);
            hierarchy.GetNodeById(nodeIdPrefix + "_3").Left.ShouldBe(2);
            hierarchy.GetNodeById(nodeIdPrefix + "_3").Right.ShouldBe(3);
            hierarchy.GetNodeById(nodeIdPrefix + "_2").Right.ShouldBe(4);
            hierarchy.GetNodeById(nodeIdPrefix + "_1").Right.ShouldBe(5);
        }

        [Fact]
        public void Remove_top_node()
        {
            // Arrange

            var hierarchy = new Hierarchy<string>();
            hierarchy.AddTopNode("universe", "Universe");
            hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            // Act

            hierarchy.RemoveNode("universe").ShouldBeTrue();

            // Assert

            hierarchy.ShouldBeEmpty();
        }

        [Fact]
        public void Remove_sub_node()
        {
            // Arrange

            var hierarchy = new Hierarchy<string>();
            hierarchy.AddTopNode("universe", "Universe");
            hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            // Act

            hierarchy.RemoveNode("solar_system").ShouldBeTrue();

            // Assert

            hierarchy.Count().ShouldBe(1);
            hierarchy.ContainsNodeWithId("universe").ShouldBeTrue();
            hierarchy.GetNodeById("universe").Left.ShouldBe(0);
            hierarchy.GetNodeById("universe").Right.ShouldBe(1);
        }

        [Fact]
        public void Remove_leaf_node()
        {
            // Arrange

            var hierarchy = new Hierarchy<string>();
            hierarchy.AddTopNode("universe", "Universe");
            hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            // Act

            hierarchy.RemoveNode("mars").ShouldBeTrue();

            // Assert

            hierarchy.Count().ShouldBe(3);
            hierarchy.ContainsNodeWithId("universe").ShouldBeTrue();
            hierarchy.ContainsNodeWithId("solar_system").ShouldBeTrue();
            hierarchy.ContainsNodeWithId("earth").ShouldBeTrue();

            hierarchy.GetNodeById("universe").Left.ShouldBe(0);
            hierarchy.GetNodeById("solar_system").Left.ShouldBe(1);
            hierarchy.GetNodeById("earth").Left.ShouldBe(2);
            hierarchy.GetNodeById("earth").Right.ShouldBe(3);
            hierarchy.GetNodeById("solar_system").Right.ShouldBe(4);
            hierarchy.GetNodeById("universe").Right.ShouldBe(5);
        }

        [Fact]
        public void Remove_absent_node()
        {
            // Arrange

            var hierarchy = new Hierarchy<string>();
            hierarchy.AddTopNode("universe", "Universe");
            hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            // Act

            hierarchy.RemoveNode("aaa").ShouldBeFalse();

            // Assert

            hierarchy.Count().ShouldBe(4);
            hierarchy.ContainsNodeWithId("universe").ShouldBeTrue();
            hierarchy.ContainsNodeWithId("solar_system").ShouldBeTrue();
            hierarchy.ContainsNodeWithId("earth").ShouldBeTrue();
            hierarchy.ContainsNodeWithId("mars").ShouldBeTrue();
        }
    }
}

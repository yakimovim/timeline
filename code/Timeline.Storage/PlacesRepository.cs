using EdlinSoftware.Timeline.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Timeline.Storage
{
    /// <summary>
    /// Repository for work with places.
    /// </summary>
    public class PlacesRepository
    {
        private readonly TimelineContext _db;

        public PlacesRepository(TimelineContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Gets places hierarchy.
        /// </summary>
        public async Task<Hierarchy<string>> GetPlacesAsync()
        {
            var placesHierarchyNodes = await _db.Places.OrderBy(p => p.Left).ToArrayAsync();

            var placesHierarchy = new Hierarchy<string>();

            if (placesHierarchyNodes.Length == 0) return placesHierarchy;

            var minLeft = placesHierarchyNodes.Min(n => n.Left);

            while (true)
            {
                var node = placesHierarchyNodes.FirstOrDefault(n => n.Left == minLeft);

                if (node == null) break;

                placesHierarchy.AddTopNode(node.Id, node.Content);

                AddSubNodes(
                    placesHierarchy,
                    node.Id,
                    placesHierarchyNodes
                        .Where(n => n.Left > node.Left && n.Right < node.Right)
                        .ToArray()
                );

                minLeft = node.Right + 1;
            }

            return placesHierarchy;
        }

        private void AddSubNodes(
            Hierarchy<string> placesHierarchy, 
            string parentNodeId, 
            PlaceHierarchy[] subNodes)
        {
            if (subNodes.Length == 0) return;

            var parentNode = placesHierarchy.GetNodeById(parentNodeId);

            var minLeft = subNodes.Min(n => n.Left);

            while (true)
            {
                var node = subNodes.FirstOrDefault(n => n.Left == minLeft);

                if (node == null) break;

                parentNode.AddSubNode(node.Id, node.Content);

                AddSubNodes(
                    placesHierarchy,
                    node.Id,
                    subNodes
                        .Where(n => n.Left > node.Left && n.Right < node.Right)
                        .ToArray()
                );

                minLeft = node.Right + 1;
            }
        }

        /// <summary>
        /// Saves places hierarchy.
        /// </summary>
        /// <param name="places">Places hierarchy.</param>
        public async Task SavePlacesAsync(Hierarchy<string> places)
        {
            if (places is null)
            {
                throw new ArgumentNullException(nameof(places));
            }

            _db.RemoveRange(_db.Places);

            await _db.AddRangeAsync(
                places.Select(node => new PlaceHierarchy
                {
                    Id = node.Id,
                    Content = node.Content,
                    Left = node.Left,
                    Right = node.Right
                })
            );

            await _db.SaveChangesAsync();
        }
    }
}

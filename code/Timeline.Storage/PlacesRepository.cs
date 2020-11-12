using EdlinSoftware.Timeline.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<Hierarchy<string>> GetPlaces()
        {
            var placesHierarchyNodes = await _db.Places.OrderBy(p => p.Left).ToArrayAsync();

            var placesHierarchy = new Hierarchy<string>();

            foreach (var node in placesHierarchyNodes)
            {
                placesHierarchy.AddTopNode(node.Id, node.Content);
            }

            return placesHierarchy;
        }

        /// <summary>
        /// Saves places hierarchy.
        /// </summary>
        /// <param name="places">Places hierarchy.</param>
        public async Task SavePlaces(Hierarchy<string> places)
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

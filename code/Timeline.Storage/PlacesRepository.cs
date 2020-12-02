using CSharpFunctionalExtensions;
using EdlinSoftware.Timeline.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EdlinSoftware.Timeline.Storage
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

            using var _ = placesHierarchy.PosponeRenumeration();

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
            Place[] subNodes)
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
        public async Task<Result> SavePlacesAsync(Hierarchy<string> places)
        {
            if (places is null)
            {
                throw new ArgumentNullException(nameof(places));
            }

            var oldPlaces = await GetPlacesAsync();

            var oldPlacesIds = new HashSet<StringId>(oldPlaces.Select(n => n.Id));
            oldPlacesIds.ExceptWith(places.Select(n => n.Id));
            if (oldPlacesIds.Count > 0)
                return Result.Failure("Unable to remove some places as there can be events referencing them.");

            await ResetPlaces(places);

            return Result.Success();
        }

        private async Task ResetPlaces(Hierarchy<string> places)
        {
            var oldPlacesMap = await _db.Places.ToDictionaryAsync(p => p.Id, p => p);

            foreach (var place in places)
            {
                if(oldPlacesMap.ContainsKey(place.Id))
                {
                    var oldPlace = oldPlacesMap[place.Id];
                    oldPlace.Content = place.Content;
                    oldPlace.Left = place.Left;
                    oldPlace.Right = place.Right;

                    _db.Entry(oldPlace).State = EntityState.Modified;

                    oldPlacesMap.Remove(place.Id);
                }
                else
                {
                    var newPlace = new Place
                    {
                        Id = place.Id,
                        Content = place.Content,
                        Left = place.Left,
                        Right = place.Right
                    };

                    _db.Places.Add(newPlace);
                }
            }

            foreach (var place in oldPlacesMap.Values)
            {
                _db.Places.Remove(place);
            }

            await _db.SaveChangesAsync();
        }

        public async Task ChangePlaceId(StringId oldPlaceId, StringId newPlaceId)
        {
            if (oldPlaceId is null)
            {
                throw new ArgumentNullException(nameof(oldPlaceId));
            }

            if (newPlaceId is null)
            {
                throw new ArgumentNullException(nameof(newPlaceId));
            }

            var place = await _db.Places.FindAsync(oldPlaceId.Id);
            if (place == null) return;

            var eventsInPlace = await _db.Events
                .Where(e => e.PlaceId == oldPlaceId.Id)
                .ToArrayAsync();

            foreach (var @event in eventsInPlace)
            {
                @event.PlaceId = newPlaceId.Id;
            }

            _db.Places.Remove(place);

            await _db.SaveChangesAsync();

            place.Id = newPlaceId.Id;

            _db.Places.Add(place);

            await _db.SaveChangesAsync();
        }

        public async Task RemovePlaceAsync(StringId placeId)
        {
            if (placeId is null)
            {
                throw new ArgumentNullException(nameof(placeId));
            }

            var places = await GetPlacesAsync();

            var place = places.GetNodeById(placeId);
            if (place == null) return;

            var subPlaces = places
                .Where(p => p.Left >= place.Left && p.Right <= place.Right)
                .ToArray();

            var placeIdsToRemove = subPlaces.Select(p => p.Id.Id).ToArray();

            var eventsInPlaces = await _db.Events
                .Where(e => placeIdsToRemove.Contains(e.PlaceId))
                .ToArrayAsync();

            foreach (var @event in eventsInPlaces)
            {
                @event.PlaceId = null;
            }

            await _db.SaveChangesAsync();

            places.RemoveNode(placeId);

            await ResetPlaces(places);
        }
    }
}
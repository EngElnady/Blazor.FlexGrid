using Blazor.FlexGrid.DataSet;
using Blazor.FlexGrid.DataSet.Options;
using Blazor.FlexGrid.Demo.Shared;
using Blazor.FlexGrid.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Blazor.Components.Demo.FlexGrid.Services
{
    public class WeatherForecastService : ILazyDataSetLoader<WeatherForecast>, ILazyDataSetItemManipulator<WeatherForecast>
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly StaticRepositoryCollections staticRepositoryCollections;

        public WeatherForecastService(StaticRepositoryCollections staticRepositoryCollections)
        {
            this.staticRepositoryCollections = staticRepositoryCollections;
        }

        public Task<WeatherForecast> DeleteItem(WeatherForecast item, ILazyLoadingOptions lazyLoadingOptions)
        {
            if (staticRepositoryCollections.Forecasts.TryRemove(item.Id, out var value))
            {
                return Task.FromResult(value);
            }

            return Task.FromResult(default(WeatherForecast));
        }

        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            var rng = new Random();
            return Task.FromResult(staticRepositoryCollections.Forecasts
                //.Take(20)
                .Select(kv => kv.Value)
                .ToArray()
            );
        }

        public Task<LazyLoadingDataSetResult<WeatherForecast>> GetTablePageData(
            ILazyLoadingOptions lazyLoadingOptions,
            IPagingOptions pageableOptions,
            ISortingOptions sortingOptions,
            IReadOnlyCollection<IFilterDefinition> filters = null)
        {
            var items = staticRepositoryCollections.Forecasts.Values.AsQueryable();

            var sortExp = sortingOptions?.SortExpression;
            if (!string.IsNullOrEmpty(sortExp))
            {
                if (sortingOptions.SortDescending)
                {
                    sortExp += " descending";
                }
                items = items.OrderBy(sortExp);
            }

            items = items
                .Skip(pageableOptions.PageSize * pageableOptions.CurrentPage)
                .Take(pageableOptions.PageSize);

            return Task.FromResult(new LazyLoadingDataSetResult<WeatherForecast>
            {
                Items = items.ToList(),
                TotalCount = staticRepositoryCollections.Forecasts.Count
            });
        }

        public Task<WeatherForecast> SaveItem(WeatherForecast item, ILazyLoadingOptions lazyLoadingOptions)
        {
            var id = item.Id;
            if (staticRepositoryCollections.Forecasts.TryGetValue(id, out var value))
            {
                if (staticRepositoryCollections.Forecasts.TryUpdate(id, item, value))
                {
                    // Update Success
                    return Task.FromResult(item);
                }
            }
            else if (staticRepositoryCollections.Forecasts.TryAdd(id, item))
            {
                // Create Success
                return Task.FromResult(item);
            }

            // Conflict
            return Task.FromResult(default(WeatherForecast));
        }
    }
}
